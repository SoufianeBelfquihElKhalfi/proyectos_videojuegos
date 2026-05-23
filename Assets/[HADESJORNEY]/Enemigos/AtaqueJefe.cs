using Enemy.FSM;
using UnityEngine;
using UnityEngine.AI;
using System.Collections;

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
    [SerializeField] private bool hacerIntro = false; // ← desactivado por defecto, la cinemática se encarga
    [SerializeField] private EstadoFSM estadoPerseguir;

    private Transform player;
    private NavMeshAgent agent;
    private InfligirDanio infligirDanio;
    private Animator animator;

    private bool atacando = false;
    private bool introTerminada = false;
    private bool cayendo = false;
    private float cooldown = 0f;

    private void OnEnable()
    {
        player = FindFirstObjectByType<MovimientoAlastor>()?.transform;
        agent = GetComponent<NavMeshAgent>();
        infligirDanio = GetComponent<InfligirDanio>();
        animator = GetComponentInChildren<Animator>();

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
            ResetearParametrosMovimiento();
        }
    }

    private void Update()
    {
        if (player == null) return;

        // 1. CONTROL DE LA INTRO (Caída inicial)
        if (hacerIntro && !introTerminada)
        {
            if (!cayendo)
            {
                float distanciaAlJugador = Vector3.Distance(
                    new Vector3(transform.position.x, player.position.y, transform.position.z),
                    player.position);

                if (distanciaAlJugador <= rangoDeteccionIntro)
                    StartCoroutine(IntroCaida());
            }
            return;
        }

        // 2. COMPROBACIONES DE SEGURIDAD
        if (agent == null || !agent.enabled || !agent.isOnNavMesh) return;
        if (atacando) return;

        // 3. MOVIMIENTO Y ROTACIÓN BASE
        RotarHaciaJugador();
        ControlarAnimacionMovimiento();

        // 4. CAMBIO DE ESTADO
        float distancia = Vector3.Distance(transform.position, player.position);
        if (distancia > rangoLargo)
        {
            if (estadoPerseguir != null)
            {
                ResetearParametrosMovimiento();
                this.enabled = false;
                estadoPerseguir.enabled = true;
            }
            return;
        }

        // 5. BUCLE DE ATAQUES
        cooldown -= Time.deltaTime;
        if (cooldown <= 0f)
        {
            EjecutarAtaque(ordenAtaques[indiceAtaqueActual]);
            indiceAtaqueActual = (indiceAtaqueActual + 1) % ordenAtaques.Length;
            cooldown = tiempoEntreAtaques;
        }
    }

    private void ControlarAnimacionMovimiento()
    {
        if (animator == null || agent == null) return;

        if (agent.remainingDistance > 0.1f || agent.velocity.sqrMagnitude > 0.1f)
        {
            animator.SetBool("caminando", true);
            animator.SetBool("girando", false);
        }
        else
        {
            Vector3 direccionAlJugador = (player.position - transform.position).normalized;
            float angulo = Vector3.Angle(transform.forward, direccionAlJugador);

            if (angulo > 15f)
            {
                animator.SetBool("caminando", false);
                animator.SetBool("girando", true);
            }
            else
            {
                animator.SetBool("caminando", false);
                animator.SetBool("girando", false);
            }
        }
    }

    private void EjecutarAtaque(string nombre)
    {
        switch (nombre)
        {
            case "mazazo": StartCoroutine(Mazazo()); break;
            case "salto": StartCoroutine(Salto()); break;
            case "embestida": StartCoroutine(Embestida()); break;
            case "barrido": StartCoroutine(Barrido()); break;
        }
    }

    private void ResetearParametrosMovimiento()
    {
        if (animator != null)
        {
            animator.SetBool("caminando", false);
            animator.SetBool("girando", false);
        }
    }

    private IEnumerator IntroCaida()
    {
        cayendo = true;
        atacando = true;

        if (animator != null) animator.SetTrigger("caer");

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

        if (agent != null)
        {
            agent.enabled = true;
            yield return new WaitForEndOfFrame();
            agent.isStopped = false;
        }

        atacando = false;
        introTerminada = true;
        ResetearParametrosMovimiento();
    }

    private IEnumerator Mazazo()
    {
        atacando = true;
        ResetearParametrosMovimiento();

        float tiempoMaximoPersecucion = 2.5f;
        float distanciaParaGolpear = rangoMazazo;

        if (agent != null) agent.isStopped = false;

        while (Vector3.Distance(transform.position, player.position) > distanciaParaGolpear && tiempoMaximoPersecucion > 0)
        {
            if (agent != null && agent.isOnNavMesh) agent.SetDestination(player.position);
            RotarHaciaJugador();
            if (animator != null) animator.SetBool("caminando", true);

            tiempoMaximoPersecucion -= Time.deltaTime;
            yield return null;
        }

        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
        }

        ResetearParametrosMovimiento();
        if (animator != null) animator.SetTrigger("mazazo");

        yield return new WaitForSeconds(0.4f);

        if (Vector3.Distance(transform.position, player.position) <= rangoMazazo + 1.5f)
        {
            if (infligirDanio != null) infligirDanio.IntentarGolpear(player);
        }

        yield return new WaitForSeconds(1.0f);
        FinAtaque();
    }

    private IEnumerator Salto()
    {
        atacando = true;
        ResetearParametrosMovimiento();

        Vector3 inicio = transform.position;
        Vector3 destino = player.position;

        if (agent != null) agent.enabled = false;
        if (animator != null) animator.SetTrigger("salto");

        float t = 0f;
        while (t < 1f)
        {
            float altura = Mathf.Sin(t * Mathf.PI) * 5f;
            transform.position = Vector3.Lerp(inicio, destino, t) + Vector3.up * altura;
            t += Time.deltaTime;
            yield return null;
        }

        if (agent != null)
        {
            agent.enabled = true;
            yield return new WaitForEndOfFrame();
        }

        AplicarDanioSiCerca(6f);
        OndaChoque(7f);

        yield return new WaitForSeconds(0.8f);
        FinAtaque();
    }

    private IEnumerator Embestida()
    {
        atacando = true;
        ResetearParametrosMovimiento();

        float t = 0f;
        Vector3 direccionCarga = transform.forward;
        if (animator != null) animator.SetBool("girando", true);

        while (t < 0.8f)
        {
            transform.Translate(direccionCarga * 14f * Time.deltaTime, Space.World);
            t += Time.deltaTime;
            yield return null;
        }
        yield return new WaitForSeconds(0.3f);
        FinAtaque();
    }

    private IEnumerator Barrido()
    {
        atacando = true;
        ResetearParametrosMovimiento();

        if (agent != null && agent.isOnNavMesh) agent.isStopped = true;
        if (animator != null) animator.SetBool("girando", true);

        yield return new WaitForSeconds(0.4f);
        AplicarDanioSiCerca(5f);
        EmpujarJugador(8f);
        yield return new WaitForSeconds(0.7f);
        FinAtaque();
    }

    private void AplicarDanioSiCerca(float radio)
    {
        if (player == null || infligirDanio == null) return;
        if (Vector3.Distance(transform.position, player.position) <= radio)
            infligirDanio.IntentarGolpear(player);
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
        atacando = false;
        if (agent != null && agent.enabled && agent.isOnNavMesh) agent.isStopped = false;
        ResetearParametrosMovimiento();
    }
}