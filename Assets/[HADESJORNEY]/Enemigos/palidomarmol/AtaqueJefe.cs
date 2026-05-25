using Enemy.FSM;
using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class AtaqueJefe : EstadoFSM
{
    [Header("Configuración de Combo de Ataques")]
    private string[] ordenAtaques = { "embestida", "mazazo", "embestida", "salto", "embestida" };
    private int indiceAtaqueActual = 0;

    [Header("Distancias")]
    [SerializeField] private float rangoMazazo = 3.5f;
    [SerializeField] private float rangoLargo = 18f;
    [SerializeField] private float rangoDeteccionIntro = 10f;

    [Header("Tiempos")]
    [SerializeField] private float tiempoEntreAtaques = 1.5f;

    [Header("Rotación Base")]
    [SerializeField] private float velocidadRotacion = 8f;

    [Header("Intro caída")]
    [SerializeField] private float alturaInicio = 15f;
    [SerializeField] private float duracionCaida = 2.0f;
    [SerializeField] private bool hacerIntro = true;
    [SerializeField] private EstadoFSM estadoPerseguir;

    private Transform player;
    private NavMeshAgent agent;
    private InfligirDanio infligirDanio;
    private Animator animator;
    private Rigidbody rb;

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
        rb = GetComponent<Rigidbody>();

        if (agent != null)
        {
            agent.updateRotation = false; // Desactivamos la rotación automática del NavMesh
            agent.updateUpAxis = false;   // Evita tirones en el eje vertical
        }

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

        // 1. CONTROL DE LA INTRO
        if (hacerIntro && !introTerminada)
        {
            if (!cayendo)
            {
                float distanciaAlJugador = Vector3.Distance(new Vector3(transform.position.x, player.position.y, transform.position.z), player.position);
                if (distanciaAlJugador <= rangoDeteccionIntro)
                {
                    StartCoroutine(IntroCaida());
                }
            }
            return;
        }

        // 2. COMPROBACIONES DE SEGURIDAD (Si está atacando, bloqueamos el resto del Update)
        if (atacando) return;
        if (agent == null || !agent.enabled || !agent.isOnNavMesh) return;

        // 3. ROTACIÓN Y ANIMACIÓN BASE
        RotarHaciaJugador();
        ControlarAnimacionMovimiento();

        // 4. CAMBIO DE ESTADO (Perseguir)
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

        // Evaluamos si el agente se está desplazando realmente
        bool seEstaMoviendo = (agent.hasPath && agent.remainingDistance > 0.1f) || agent.velocity.sqrMagnitude > 0.1f;

        if (seEstaMoviendo)
        {
            animator.SetBool("caminando", true);
            animator.SetBool("girando", false); // Si camina, no está estático girando
        }
        else
        {
            animator.SetBool("caminando", false);
        }
    }

    private void RotarHaciaJugador()
    {
        if (player == null)
        {
            if (animator != null) animator.SetBool("girando", false);
            return;
        }

        Vector3 dir = (player.position - transform.position);
        dir.y = 0; // Evita inclinaciones verticales del jefe

        if (dir.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(dir.normalized);
            float anguloRestante = Quaternion.Angle(transform.rotation, targetRotation);

            // Si el ángulo es amplio, activamos animación de giro (solo si no se está desplazando)
            if (anguloRestante > 5f && (agent != null && agent.velocity.sqrMagnitude <= 0.1f))
            {
                if (animator != null) animator.SetBool("girando", true);
            }
            else
            {
                if (animator != null) animator.SetBool("girando", false);
            }

            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, velocidadRotacion * Time.deltaTime);
        }
        else
        {
            if (animator != null) animator.SetBool("girando", false);
        }
    }

    private void EjecutarAtaque(string nombre)
    {
        switch (nombre)
        {
            case "mazazo": StartCoroutine(Mazazo()); break;
            case "salto": StartCoroutine(Salto()); break;
            case "embestida": StartCoroutine(Embestida()); break;
        }
    }

    private void ResetearParametrosMovimiento()
    {
        if (animator != null)
        {
            animator.SetBool("caminando", false);
            animator.SetBool("girando", false);
            // Dejamos que las transiciones del Animator manejen la vuelta a Idle de forma fluida
        }
    }

    private IEnumerator IntroCaida()
    {
        cayendo = true;
        atacando = true;

        // Evitamos interferencias físicas del Rigidbody durante el Lerp manual
        if (rb != null) rb.isKinematic = true;
        if (agent != null) agent.enabled = false;

        // Forzamos el inicio del estado de caída (Asegúrate de que se llame exactamente "Caída" en tu Animator)
        if (animator != null) animator.Play("Caída", 0, 0f);

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

        if (rb != null) rb.isKinematic = false;

        OndaChoque(8f);
        yield return new WaitForSeconds(0.6f);

        if (agent != null)
        {
            agent.enabled = true;
            yield return new WaitForEndOfFrame();
            agent.isStopped = false;

            if (agent.isOnNavMesh && player != null)
            {
                agent.SetDestination(player.position);
            }
        }

        atacando = false;
        introTerminada = true;

        if (animator != null) animator.SetBool("caminando", true);
    }

    private IEnumerator Mazazo()
    {
        atacando = true;
        ResetearParametrosMovimiento();

        float tiempoMaximoPersecucion = 2.0f;
        if (agent != null && agent.isOnNavMesh) agent.isStopped = false;

        while (Vector3.Distance(transform.position, player.position) > rangoMazazo && tiempoMaximoPersecucion > 0)
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
        if (animator != null) animator.Play("mazazo");

        yield return new WaitForSeconds(0.4f);

        if (Vector3.Distance(transform.position, player.position) <= rangoMazazo + 1.0f)
        {
            if (infligirDanio != null) infligirDanio.IntentarGolpear(player);
        }

        yield return new WaitForSeconds(0.8f);
        FinAtaque();
    }

    private IEnumerator Salto()
    {
        atacando = true;
        ResetearParametrosMovimiento();

        Vector3 dirAlJugador = (player.position - transform.position);
        dirAlJugador.y = 0;
        if (dirAlJugador != Vector3.zero) transform.rotation = Quaternion.LookRotation(dirAlJugador.normalized);

        Vector3 inicio = transform.position;
        Vector3 destino = player.position;

        if (agent != null) agent.enabled = false;
        if (animator != null) animator.Play("salto");

        float t = 0f;
        float duracionSalto = 1.2f;

        while (t < duracionSalto)
        {
            t += Time.deltaTime;
            float porcentaje = t / duracionSalto;
            float altura = Mathf.Sin(porcentaje * Mathf.PI) * 6f;

            transform.position = Vector3.Lerp(inicio, destino, porcentaje) + Vector3.up * altura;
            yield return null;
        }

        if (agent != null)
        {
            agent.enabled = true;
            yield return new WaitForEndOfFrame();
        }

        AplicarDañoSiCerca(6f);
        OndaChoque(7f);

        yield return new WaitForSeconds(0.6f);
        FinAtaque();
    }

    private IEnumerator Embestida()
    {
        atacando = true;
        ResetearParametrosMovimiento();

        Vector3 dirAlJugador = (player.position - transform.position);
        dirAlJugador.y = 0;
        if (dirAlJugador != Vector3.zero) transform.rotation = Quaternion.LookRotation(dirAlJugador.normalized);

        if (animator != null) animator.Play("embestir");
        yield return new WaitForSeconds(0.3f);

        float t = 0f;
        float duracionCarga = 0.6f;
        Vector3 direccionCarga = transform.forward;

        while (t < duracionCarga)
        {
            transform.Translate(direccionCarga * 18f * Time.deltaTime, Space.World);
            AplicarDañoSiCerca(2.5f);
            t += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(0.5f);
        FinAtaque();
    }

    private void AplicarDañoSiCerca(float radio)
    {
        if (player == null || infligirDanio == null) return;
        if (Vector3.Distance(transform.position, player.position) <= radio)
        {
            infligirDanio.IntentarGolpear(player);
        }
    }

    private void EmpujarJugador(float fuerza)
    {
        Vector3 direccion = (player.position - transform.position).normalized;
        direccion.y = 0;
        player.position += direccion * (fuerza * Time.deltaTime);
    }

    private void OndaChoque(float radio)
    {
        if (Vector3.Distance(transform.position, player.position) <= radio) EmpujarJugador(12f);
    }

    private void FinAtaque()
    {
        atacando = false;
        ResetearParametrosMovimiento();

        if (agent != null && agent.enabled && agent.isOnNavMesh)
        {
            agent.isStopped = false;
            agent.ResetPath();
        }
    }
}