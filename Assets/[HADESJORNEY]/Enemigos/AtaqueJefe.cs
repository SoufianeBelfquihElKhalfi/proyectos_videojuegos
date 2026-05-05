using Enemy.FSM;
using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

public class AtaqueJefe : EstadoFSM
{
    [Header("Distancias")]
    [SerializeField] private float rangoCorto = 3f;
    [SerializeField] private float rangoMedio = 7f;
    [SerializeField] private float rangoLargo = 12f;
    [SerializeField] private float rangoDeteccionIntro = 10f; // Área para activar la caída

    [Header("Tiempos")]
    [SerializeField] private float tiempoEntreAtaques = 0.5f;

    [Header("Rotación")]
    [SerializeField] private float velocidadRotacion = 8f;

    [Header("Intro caída")]
    [SerializeField] private float alturaInicio = 15f;
    [SerializeField] private float duracionCaida = 3.0f;
    [SerializeField] private bool hacerIntro = true;
    [SerializeField] private EstadoFSM estadoPerseguir;

    private Transform player;
    private NavMeshAgent agent;
    private InfligirDanio infligirDanio;

    private bool atacando = false;
    private bool introTerminada = false;
    private bool cayendo = false; // Control de estado de caída

    private float cooldown = 0f;

    private Dictionary<string, float> cdAtaques = new Dictionary<string, float>();
    private string ultimoAtaque = "";

    private void OnEnable()
    {
        player = FindFirstObjectByType<MovimientoAlastor>()?.transform;
        agent = GetComponent<NavMeshAgent>();
        infligirDanio = GetComponent<InfligirDanio>();

        if (agent != null) agent.updateRotation = false;

        cdAtaques["mazazo"] = 0;
        cdAtaques["barrido"] = 0;
        cdAtaques["embestida"] = 0;
        cdAtaques["salto"] = 0;

        if (hacerIntro)
        {
            introTerminada = false;
            cayendo = false;
            // Posicionar al jefe en el aire desde el inicio
            if (agent != null) agent.enabled = false;
            transform.position += Vector3.up * alturaInicio;
        }
        else
        {
            introTerminada = true;
        }
    }

    private void Update()
    {
        if (player == null || agent == null) return;

        // Lógica de detección para la caída inicial
        if (hacerIntro && !introTerminada && !cayendo)
        {
            float distanciaAlJugador = Vector3.Distance(new Vector3(transform.position.x, player.position.y, transform.position.z), player.position);

            if (distanciaAlJugador <= rangoDeteccionIntro)
            {
                StartCoroutine(IntroCaida());
            }
            return; // No ejecutar ataques hasta caer
        }

        if (!introTerminada) return;

        if (!agent.enabled || !agent.isOnNavMesh) return;

        ActualizarCooldowns();

        if (atacando) return;

        RotarHaciaJugador();

        float distancia = Vector3.Distance(transform.position, player.position);

        if (distancia > rangoLargo)
        {
            agent.isStopped = false;
            if (estadoPerseguir != null)
            {
                this.enabled = false;
                estadoPerseguir.enabled = true;
            }
            return;
        }

        cooldown -= Time.deltaTime;
        if (cooldown <= 0f)
        {
            DecidirAtaque(distancia);
        }
    }

    private IEnumerator IntroCaida()
    {
        cayendo = true;
        atacando = true;

        Vector3 suelo = transform.position - Vector3.up * alturaInicio;
        Vector3 inicio = transform.position;

        float t = 0f;
        while (t < duracionCaida)
        {
            float progreso = t / duracionCaida;
            // Caída lineal
            transform.position = Vector3.Lerp(inicio, suelo, progreso);

            t += Time.deltaTime;
            yield return null;
        }

        transform.position = suelo;
        OndaChoque(8f);

        yield return new WaitForSeconds(0.5f);

        if (agent != null)
        {
            agent.enabled = true;
            if (agent.isOnNavMesh) agent.isStopped = false;
        }

        atacando = false;
        introTerminada = true;
    }

    // ... (El resto de funciones de ataque y decisión se mantienen igual que en la versión anterior) ...

    private void DecidirAtaque(float distancia)
    {
        List<string> opciones = new List<string>();

        if (distancia <= rangoCorto)
        {
            if (cdAtaques["mazazo"] <= 0) opciones.Add("mazazo");
            if (cdAtaques["barrido"] <= 0) opciones.Add("barrido");
        }
        else if (distancia <= rangoMedio)
        {
            if (cdAtaques["embestida"] <= 0) opciones.Add("embestida");
            if (cdAtaques["mazazo"] <= 0) opciones.Add("mazazo");
        }
        else
        {
            if (cdAtaques["salto"] <= 0) opciones.Add("salto");
            if (cdAtaques["embestida"] <= 0) opciones.Add("embestida");
        }

        opciones.Remove(ultimoAtaque);
        if (opciones.Count == 0) return;

        string elegido = opciones[Random.Range(0, opciones.Count)];
        ultimoAtaque = elegido;
        EjecutarAtaque(elegido);
        cooldown = tiempoEntreAtaques;
    }

    private void EjecutarAtaque(string nombre)
    {
        switch (nombre)
        {
            case "mazazo": cdAtaques["mazazo"] = 3f; StartCoroutine(Mazazo()); break;
            case "barrido": cdAtaques["barrido"] = 4f; StartCoroutine(Barrido()); break;
            case "embestida": cdAtaques["embestida"] = 5f; StartCoroutine(Embestida()); break;
            case "salto": cdAtaques["salto"] = 6f; StartCoroutine(Salto()); break;
        }
    }

    private void ActualizarCooldowns()
    {
        List<string> keys = new List<string>(cdAtaques.Keys);
        foreach (string k in keys)
        {
            if (cdAtaques[k] > 0) cdAtaques[k] -= Time.deltaTime;
        }
    }

    private IEnumerator Mazazo()
    {
        atacando = true;
        if (agent.isOnNavMesh) agent.isStopped = true;
        yield return new WaitForSeconds(0.4f);
        if (infligirDanio != null) infligirDanio.IntentarGolpear(player);
        yield return new WaitForSeconds(0.8f);
        FinAtaque();
    }

    private IEnumerator Barrido()
    {
        atacando = true;
        if (agent.isOnNavMesh) agent.isStopped = true;
        yield return new WaitForSeconds(0.3f);
        if (infligirDanio != null) infligirDanio.IntentarGolpear(player);
        EmpujarJugador(6f);
        yield return new WaitForSeconds(0.7f);
        FinAtaque();
    }

    private IEnumerator Embestida()
    {
        atacando = true;
        float t = 0f;
        while (t < 1f)
        {
            transform.Translate(Vector3.forward * 12f * Time.deltaTime);
            t += Time.deltaTime;
            yield return null;
        }
        yield return new WaitForSeconds(0.4f);
        FinAtaque();
    }

    private IEnumerator Salto()
    {
        atacando = true;
        agent.enabled = false;
        Vector3 inicio = transform.position;
        Vector3 destino = player.position;
        float t = 0f;
        while (t < 1f)
        {
            float altura = Mathf.Sin(t * Mathf.PI) * 4f;
            transform.position = Vector3.Lerp(inicio, destino, t) + Vector3.up * altura;
            t += Time.deltaTime;
            yield return null;
        }
        agent.enabled = true;
        if (infligirDanio != null) infligirDanio.IntentarGolpear(player);
        OndaChoque(7f);
        yield return new WaitForSeconds(0.6f);
        FinAtaque();
    }

    private void RotarHaciaJugador()
    {
        Vector3 dir = (player.position - transform.position).normalized;
        dir.y = 0;
        if (dir != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), velocidadRotacion * Time.deltaTime);
        }
    }

    private void EmpujarJugador(float fuerza)
    {
        Vector3 direccion = (player.position - transform.position).normalized;
        direccion.y = 0;
        player.position += direccion * (fuerza * 0.2f);
    }

    private void OndaChoque(float radio)
    {
        if (Vector3.Distance(transform.position, player.position) <= radio) EmpujarJugador(10f);
    }

    private void FinAtaque()
    {
        if (agent.enabled && agent.isOnNavMesh) agent.isStopped = false;
        atacando = false;
    }
}