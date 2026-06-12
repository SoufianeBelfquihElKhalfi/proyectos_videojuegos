using Enemy.FSM;
using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using Dapasa.Audio;

public class AtaqueJefe : EstadoFSM
{
    [Header("Configuración de Combo de Ataques")]
    private string[] ordenAtaques = { "embestida", "mazazo", "embestida", "mazazo", "salto" };
    private int indiceAtaqueActual = 0;

    [Header("Distancias")]
    [SerializeField] private float rangoMazazo = 3.5f;
    [SerializeField] private float rangoLargo = 18f;
    [SerializeField] private float rangoDeteccionIntro = 10f;

    [Header("Tiempos")]
    [SerializeField] private float tiempoEntreAtaques = 1.5f;

    [Header("Rotación")]
    [SerializeField] private float velocidadRotacion = 540f;
    [SerializeField] private float anguloParaGirarEnSitio = 60f;
    [SerializeField] private float anguloFinGiroEnSitio = 15f;

    [Header("Audio")]
    [SerializeField] private string idSonidoGolpeJefe = "golpe_jefe";
    [SerializeField] private string idSonidoCaida = "caida";
    [Tooltip("Segundos ANTES de que el salto toque el suelo a los que suena 'caida'.")]
    [SerializeField] private float anticipoCaidaSalto = 0.15f;

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
    private SistemaVida sistemaVida;
    private bool muerteDetectada = false;
    private bool combateAvisado = false;

    private bool atacando = false;
    private bool introTerminada = false;
    private bool cayendo = false;
    private bool girandoEnSitio = false;
    private bool introInicializada = false;
    private float cooldown = 0f;

    private Vector3 ultimaDireccionCarga;

    [Header("Orientación del modelo")]
    [SerializeField] private float offsetModeloY = 180f;

    private Quaternion OffsetModelo => Quaternion.Euler(0f, offsetModeloY, 0f);
    private Vector3 ForwardVisual => transform.rotation * Quaternion.Euler(0f, -offsetModeloY, 0f) * Vector3.forward;

    public void MarcarIntroCompletada()
    {
        introInicializada = true;
        introTerminada = true;
        cayendo = false;
        atacando = false;
    }

    private void OnEnable()
    {
        player = FindFirstObjectByType<MovimientoAlastor>()?.transform;
        agent = GetComponent<NavMeshAgent>();
        infligirDanio = GetComponent<InfligirDanio>();
        animator = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody>();
        sistemaVida = GetComponent<SistemaVida>();

        if (agent != null)
        {
            // Rotación 
            agent.updateRotation = false;
            agent.updateUpAxis = false;
            agent.angularSpeed = 0f;
        }

        if (hacerIntro && !introInicializada)
        {
    
            introInicializada = true;
            introTerminada = false;
            cayendo = false;
            if (agent != null) agent.enabled = false;
            transform.position += Vector3.up * alturaInicio;
        }
        else if (!hacerIntro)
        {
            introTerminada = true;
            ResetearParametrosMovimiento();
        }
    }

    private void Update()
    {
    
        if (sistemaVida != null && sistemaVida.EstaMuerto)
        {
            if (!muerteDetectada)
            {
                muerteDetectada = true;
                atacando = true;        // bloquea cualquier ataque en cola
                StopAllCoroutines();    // corta coroutines de Mazazo/Embestida/Salto en curso
                ParaAgente();
                ResetearParametrosMovimiento();

                
                if (combateAvisado && GestorMusicaCombate.Instance != null)
                {
                    GestorMusicaCombate.Instance.SalirCombate(this);
                    combateAvisado = false;
                }
            }
            return;
        }

        if (player == null) return;

        if (!combateAvisado && introTerminada && GestorMusicaCombate.Instance != null)
        {
            combateAvisado = true;
            GestorMusicaCombate.Instance.EntrarCombate(this);
        }

        // intro
        if (hacerIntro && !introTerminada)
        {
            if (!cayendo)
            {
                Vector3 posXZ = new Vector3(transform.position.x, player.position.y, transform.position.z);
                if (Vector3.Distance(posXZ, player.position) <= rangoDeteccionIntro)
                {
                    StartCoroutine(IntroCaida());
                }
            }
            return;
        }

        if (atacando) return;
        if (agent == null || !agent.enabled || !agent.isOnNavMesh) return;

        // distancia con el jugador
        float distancia = Vector3.Distance(transform.position, player.position);

        // girar hacia el jugador
        Vector3 dirAlJugador = player.position - transform.position;
        dirAlJugador.y = 0f;
        float anguloAlJugador = dirAlJugador.sqrMagnitude > 0.001f
            ? Vector3.Angle(ForwardVisual, dirAlJugador)
            : 0f;

        bool enRangoCorto = distancia <= rangoMazazo * 0.9f;

        if (enRangoCorto)
        {
            ParaAgente();
            GirarHacia(dirAlJugador);
            ActualizarAnimaciones(moviendose: false, anguloAlJugador);
        }
        else
        {
            // perseguir al jugador
            agent.isStopped = false;
            agent.SetDestination(player.position);

            Vector3 dirMov = agent.velocity;
            dirMov.y = 0f;
            if (dirMov.sqrMagnitude > 0.04f) GirarHacia(dirMov);
            else GirarHacia(dirAlJugador);

            ActualizarAnimaciones(moviendose: true, anguloAlJugador);
        }

        // ataques
        cooldown -= Time.deltaTime;
        if (cooldown <= 0f && distancia <= rangoLargo)
        {
            EjecutarAtaque(ordenAtaques[indiceAtaqueActual]);
            indiceAtaqueActual = (indiceAtaqueActual + 1) % ordenAtaques.Length;
            cooldown = tiempoEntreAtaques;
        }
    }


    private void ReproducirSonidoSFX(string id)
    {
        if (AudioManager.Instance == null) return;
        if (string.IsNullOrEmpty(id)) return;
        AudioManager.Instance.ReproducirSFX2D(id);
    }

    private void ParaAgente()
    {
        if (agent == null || !agent.isOnNavMesh) return;
        agent.isStopped = true;
        agent.velocity = Vector3.zero;
    }

    private void GirarHacia(Vector3 direccion)
    {
        direccion.y = 0f;
        if (direccion.sqrMagnitude < 0.001f) return;
        transform.rotation = Quaternion.LookRotation(direccion.normalized) * OffsetModelo;
    }

    private void ActualizarAnimaciones(bool moviendose, float anguloAlJugador)
    {
        if (animator == null) return;

        animator.SetBool("caminando", moviendose);
        animator.SetBool("girando", !moviendose && anguloAlJugador > anguloFinGiroEnSitio);
    }

    private void EjecutarAtaque(string nombre)
    {
        switch (nombre)
        {
            case "mazazo": StartCoroutine(Mazazo()); break;
            case "embestida": StartCoroutine(Embestida()); break;
            case "salto": StartCoroutine(Salto()); break;
        }
    }

    private void ResetearParametrosMovimiento()
    {
        if (animator == null) return;
        animator.SetBool("caminando", false);
        animator.SetBool("girando", false);
    }

    private IEnumerator IntroCaida()
    {
        cayendo = true;
        atacando = true;

        if (rb != null) rb.isKinematic = true;
        if (agent != null) agent.enabled = false;

        if (animator == null) animator = GetComponentInChildren<Animator>();
        if (animator == null) animator = GetComponent<Animator>();

        if (animator != null)
        {
            animator.enabled = true;
            animator.speed = 1f;

            float caerLength = ObtenerDuracionClip("caer");
            if (caerLength > 0.01f && duracionCaida > 0.05f)
                animator.speed = caerLength / duracionCaida;

            int hashCaer = Animator.StringToHash("Caer");
            animator.ResetTrigger("caer");
            animator.Play(hashCaer, 0, 0f);
            animator.SetTrigger("caer");
            animator.Update(0f);

            Debug.Log($"[Jefe] IntroCaida → state Caer activado. animator.enabled={animator.enabled}, " +
                      $"speed={animator.speed}, clipCaerLen={caerLength}, duracionCaida={duracionCaida}, " +
                      $"currentState.fullPathHash={animator.GetCurrentAnimatorStateInfo(0).fullPathHash}, " +
                      $"esperadoCaerHash={hashCaer}");
        }
        else
        {
            Debug.LogWarning("[Jefe] IntroCaida: no se encontró Animator. La caída se hará sin animación.");
        }

        Vector3 suelo = new Vector3(transform.position.x, transform.position.y - alturaInicio, transform.position.z);
        Vector3 inicio = transform.position;
        float t = 0f;

        while (t < duracionCaida)
        {
            t += Time.deltaTime;
            transform.position = Vector3.Lerp(inicio, suelo, t / duracionCaida);

            if (player != null)
            {
                Vector3 dir = player.position - transform.position;
                dir.y = 0f;
                if (dir.sqrMagnitude > 0.01f)
                    transform.rotation = Quaternion.LookRotation(dir.normalized) * OffsetModelo;
            }

            yield return null;
        }

        transform.position = suelo;
        if (rb != null) rb.isKinematic = false;

        if (player != null)
        {
            Vector3 dirFin = player.position - transform.position;
            dirFin.y = 0f;
            if (dirFin.sqrMagnitude > 0.01f)
                transform.rotation = Quaternion.LookRotation(dirFin.normalized) * OffsetModelo;
        }

        if (animator != null) animator.speed = 1f;

        OndaChoque(8f);
        yield return new WaitForSeconds(0.6f);

        if (agent != null)
        {
            agent.enabled = true;
            yield return new WaitForEndOfFrame();
            agent.isStopped = false;
            if (agent.isOnNavMesh && player != null)
                agent.SetDestination(player.position);
        }

        atacando = false;
        introTerminada = true;
        ResetearParametrosMovimiento();
    }

    private float ObtenerDuracionClip(string nombre)
    {
        if (animator == null || animator.runtimeAnimatorController == null) return 0f;
        var clips = animator.runtimeAnimatorController.animationClips;
        for (int i = 0; i < clips.Length; i++)
        {
            if (clips[i] != null && clips[i].name == nombre) return clips[i].length;
        }
        return 0f;
    }

    private IEnumerator Mazazo()
    {
        atacando = true;
        ResetearParametrosMovimiento();

        float tiempoMaximoPersecucion = 2.0f;
        if (agent != null && agent.isOnNavMesh) agent.isStopped = false;

        while (Vector3.Distance(transform.position, player.position) > rangoMazazo
               && tiempoMaximoPersecucion > 0f)
        {
            if (agent != null && agent.isOnNavMesh)
            {
                agent.isStopped = false;
                agent.SetDestination(player.position);
            }

            Vector3 dirMov = agent != null ? agent.velocity : Vector3.zero;
            dirMov.y = 0f;
            if (dirMov.sqrMagnitude > 0.04f)
            {
                GirarHacia(dirMov);
            }
            else
            {
                Vector3 d = player.position - transform.position;
                d.y = 0f;
                if (d.sqrMagnitude > 0.001f) GirarHacia(d);
            }

            if (animator != null)
            {
                animator.SetBool("caminando", true);
                animator.SetBool("girando", false);
            }

            tiempoMaximoPersecucion -= Time.deltaTime;
            yield return null;
        }

        ParaAgente();
        Vector3 dirFinal = player.position - transform.position;
        dirFinal.y = 0f;
        if (dirFinal.sqrMagnitude > 0.01f)
            transform.rotation = Quaternion.LookRotation(dirFinal.normalized) * OffsetModelo;

        ResetearParametrosMovimiento();
        if (animator != null) animator.Play("mazazo");

        yield return new WaitForSeconds(0.4f);

        if (Vector3.Distance(transform.position, player.position) <= rangoMazazo + 1.0f
            && infligirDanio != null
            && infligirDanio.IntentarGolpear(player))
        {
            ReproducirSonidoSFX(idSonidoGolpeJefe);
        }

        yield return new WaitForSeconds(0.8f);
        FinAtaque();
    }

    private IEnumerator Embestida()
    {
        atacando = true;
        ResetearParametrosMovimiento();

        ParaAgente();

        Vector3 dir = player.position - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude > 0.01f)
            transform.rotation = Quaternion.LookRotation(dir.normalized) * OffsetModelo;

        if (animator != null) animator.Play("embestir");

        yield return new WaitForSeconds(0.3f);

        ultimaDireccionCarga = ForwardVisual;

        if (agent != null && agent.isOnNavMesh) agent.isStopped = false;

        float t = 0f;
        float duracionCarga = 0.6f;
        float velocidadCarga = 18f;
        bool golpeEmbestidaSonado = false;

        while (t < duracionCarga)
        {
            Vector3 paso = ultimaDireccionCarga * velocidadCarga * Time.deltaTime;
            if (agent != null && agent.enabled && agent.isOnNavMesh)
                agent.Move(paso);
            else
                transform.Translate(paso, Space.World);

            if (AplicarDañoSiCerca(2.5f) && !golpeEmbestidaSonado)
            {
                golpeEmbestidaSonado = true;
                ReproducirSonidoSFX(idSonidoGolpeJefe);
            }

            t += Time.deltaTime;
            yield return null;
        }
        if (agent != null && agent.isOnNavMesh)
        {
            agent.velocity = Vector3.zero;
            agent.isStopped = true;
            if (player != null) agent.SetDestination(player.position);
        }

        yield return new WaitForSeconds(0.25f);

        if (agent != null && agent.isOnNavMesh && player != null)
            agent.SetDestination(player.position);

        FinAtaque();
    }
    private IEnumerator Salto()
    {
        atacando = true;
        ResetearParametrosMovimiento();

        Vector3 destino = player.position;

        // encarar al jugador 
        Vector3 dirInicial = destino - transform.position;
        dirInicial.y = 0f;
        if (dirInicial.sqrMagnitude > 0.01f)
            transform.rotation = Quaternion.LookRotation(dirInicial.normalized) * OffsetModelo;

        if (agent != null)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
            agent.enabled = false;
        }

        if (animator != null) animator.Play("salto", 0, 0f);

        Vector3 inicio = transform.position;
        float t = 0f;
        float duracionSalto = 1.2f;
        float alturaArco = 6f;

        bool caidaSaltoSonada = false;

        while (t < duracionSalto)
        {
            t += Time.deltaTime;
            float p = t / duracionSalto;
            float altura = Mathf.Sin(p * Mathf.PI) * alturaArco;
            transform.position = Vector3.Lerp(inicio, destino, p) + Vector3.up * altura;

            Vector3 dirVuelo = destino - transform.position;
            dirVuelo.y = 0f;
            if (dirVuelo.sqrMagnitude > 0.01f)
                transform.rotation = Quaternion.LookRotation(dirVuelo.normalized) * OffsetModelo;

            if (!caidaSaltoSonada && t >= duracionSalto - anticipoCaidaSalto)
            {
                caidaSaltoSonada = true;
                ReproducirSonidoSFX(idSonidoCaida);
            }

            yield return null;
        }

        transform.position = new Vector3(destino.x, inicio.y, destino.z);

        if (!caidaSaltoSonada) ReproducirSonidoSFX(idSonidoCaida);

        if (agent != null)
        {
            agent.enabled = true;
            yield return new WaitForEndOfFrame();
            if (agent.isOnNavMesh)
            {
                agent.Warp(transform.position);
                agent.isStopped = false;
                if (player != null) agent.SetDestination(player.position);
            }
        }

        AplicarDañoSiCerca(3f);

        yield return new WaitForSeconds(0.5f);
        FinAtaque();
    }

    private bool AplicarDañoSiCerca(float radio)
    {
        if (player == null || infligirDanio == null) return false;
        if (Vector3.Distance(transform.position, player.position) > radio) return false;
        return infligirDanio.IntentarGolpear(player);
    }

    private void OndaChoque(float radio)
    {
        AplicarDañoSiCerca(radio);
    }

    private void FinAtaque()
    {
        atacando = false;
        girandoEnSitio = false;
        ResetearParametrosMovimiento();
        
        if (agent != null && agent.enabled && agent.isOnNavMesh)
            agent.isStopped = false;
    }
}