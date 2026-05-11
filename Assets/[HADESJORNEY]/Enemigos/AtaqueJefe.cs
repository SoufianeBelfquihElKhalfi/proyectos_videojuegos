using Enemy.FSM;
using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

public class AtaqueJefe : EstadoFSM
{
    [Header("Configuración de Rotación")]
   
    private string[] ordenAtaques = { "embestida", "mazazo", "embestida", "salto", "embestida", "barrido" };
    private int indiceAtaqueActual = 0;

    [Header("Distancias")]
    [SerializeField] private float rangoMazazo = 3.5f;
    [SerializeField] private float rangoLargo = 18f;
    [SerializeField] private float rangoDeteccionIntro = 10f;

    [Header("Tiempos")]
    [SerializeField] private float tiempoEntreAtaques = 1.2f;

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
    private bool cayendo = false;
    private float cooldown = 0f;

    private void OnEnable()
    {
        player = FindFirstObjectByType<MovimientoAlastor>()?.transform;
        agent = GetComponent<NavMeshAgent>();
        infligirDanio = GetComponent<InfligirDanio>();

        if (agent != null) agent.updateRotation = false;

        if (hacerIntro)
        {
            introTerminada = false;
            cayendo = false;
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

        if (hacerIntro && !introTerminada && !cayendo)
        {
            float distanciaAlJugador = Vector3.Distance(new Vector3(transform.position.x, player.position.y, transform.position.z), player.position);
            if (distanciaAlJugador <= rangoDeteccionIntro) StartCoroutine(IntroCaida());
            return;
        }

        if (!introTerminada || atacando) return;
        if (!agent.enabled || !agent.isOnNavMesh) return;

        RotarHaciaJugador();

        float distancia = Vector3.Distance(transform.position, player.position);
        if (distancia > rangoLargo)
        {
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
            EjecutarAtaque(ordenAtaques[indiceAtaqueActual]);
            indiceAtaqueActual = (indiceAtaqueActual + 1) % ordenAtaques.Length;
            cooldown = tiempoEntreAtaques;
        }
    }

    private void EjecutarAtaque(string nombre)
    {
        switch (nombre)
        {
            case "mazazo": StartCoroutine(Mazazo()); break;
            case "barrido": StartCoroutine(Barrido()); break;
            case "embestida": StartCoroutine(Embestida()); break;
            case "salto": StartCoroutine(Salto()); break;
        }
    }

    private IEnumerator Mazazo()
    {
        atacando = true;
        // Se acerca al jugador si está lejos
        while (Vector3.Distance(transform.position, player.position) > rangoMazazo)
        {
            agent.isStopped = false;
            agent.SetDestination(player.position);
            RotarHaciaJugador();
            yield return null;
        }

        agent.isStopped = true;
        yield return new WaitForSeconds(0.4f);
        AplicarDañoSiCerca(rangoMazazo + 1f);
        yield return new WaitForSeconds(0.8f);
        FinAtaque();
    }

    private IEnumerator Barrido()
    {
        atacando = true;
        if (agent.isOnNavMesh) agent.isStopped = true;
        yield return new WaitForSeconds(0.4f);

        AplicarDañoSiCerca(5f);
        EmpujarJugador(8f);

        yield return new WaitForSeconds(0.7f);
        FinAtaque();
    }

    private IEnumerator Embestida()
    {
        atacando = true;
        float t = 0f;
        Vector3 direccionCarga = transform.forward;
        while (t < 0.8f) 
        {
            transform.Translate(direccionCarga * 14f * Time.deltaTime, Space.World);
            t += Time.deltaTime;
            yield return null;
        }
        yield return new WaitForSeconds(0.3f);
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
            float altura = Mathf.Sin(t * Mathf.PI) * 5f;
            transform.position = Vector3.Lerp(inicio, destino, t) + Vector3.up * altura;
            t += Time.deltaTime;
            yield return null;
        }

        agent.enabled = true;
        AplicarDañoSiCerca(6f);
        OndaChoque(7f);

        yield return new WaitForSeconds(0.8f);
        FinAtaque();
    }

    private void AplicarDañoSiCerca(float radio)
    {
        if (player == null || infligirDanio == null) return;

        float distancia = Vector3.Distance(transform.position, player.position);
        if (distancia <= radio)
        {
            infligirDanio.IntentarGolpear(player);
        }
    }

    private IEnumerator IntroCaida()
    {
        cayendo = true;
        atacando = true;
        Vector3 suelo = new Vector3(transform.position.x, transform.position.y - alturaInicio, transform.position.z);
        Vector3 inicio = transform.position;
        float t = 0f;
        while (t < duracionCaida)
        {
            transform.position = Vector3.Lerp(inicio, suelo, t / duracionCaida);
            t += Time.deltaTime;
            yield return null;
        }
        transform.position = suelo;
        OndaChoque(8f);
        yield return new WaitForSeconds(0.5f);
        if (agent != null) { agent.enabled = true; agent.isStopped = false; }
        atacando = false;
        introTerminada = true;
    }

    private void RotarHaciaJugador()
    {
        Vector3 dir = (player.position - transform.position).normalized;
        dir.y = 0;
        if (dir != Vector3.zero)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), velocidadRotacion * Time.deltaTime);
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