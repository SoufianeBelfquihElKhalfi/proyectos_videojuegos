using Enemy.FSM;
using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class AtaqueJefe : EstadoFSM
{
    [Header("Configuración de Combo de Ataques")]
    // Salto cada ~5 ataques. Si quieres más/menos, cambia la lista.
    private string[] ordenAtaques = { "embestida", "mazazo", "embestida", "mazazo", "salto" };
    private int indiceAtaqueActual = 0;

    [Header("Distancias")]
    [SerializeField] private float rangoMazazo = 3.5f;
    [SerializeField] private float rangoLargo = 18f;
    [SerializeField] private float rangoDeteccionIntro = 10f;

    [Header("Tiempos")]
    [SerializeField] private float tiempoEntreAtaques = 1.5f;

    [Header("Rotación")]
    // Grados por segundo. 540 = giro rápido y "sólido" (3/4 de vuelta por segundo).
    [SerializeField] private float velocidadRotacion = 540f;
    // Si el ángulo al objetivo supera esto, paramos y giramos en sitio
    // (evita el "andar hacia atrás" y el drift al cambiar de dirección).
    [SerializeField] private float anguloParaGirarEnSitio = 60f;
    // Mientras giramos en sitio, terminamos cuando el ángulo restante baja de esto.
    [SerializeField] private float anguloFinGiroEnSitio = 15f;

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
    private bool girandoEnSitio = false;
    private bool introInicializada = false;
    private float cooldown = 0f;

    private Vector3 ultimaDireccionCarga;

    // Offset de rotación del modelo. Si el frente visual no coincide con +Z
    // del transform, ajusta este valor en el Inspector hasta que el bicho
    // mire al jugador. Valores típicos: 0 (frente en +Z), 180 (frente en -Z),
    // 90 / -90 (frente en X). El offset NO se aplica durante la caída (el
    // clip "caer" puede tener orientación distinta al resto).
    [Header("Orientación del modelo")]
    [SerializeField] private float offsetModeloY = 180f;

    private Quaternion OffsetModelo => Quaternion.Euler(0f, offsetModeloY, 0f);
    private Vector3 ForwardVisual => transform.rotation * Quaternion.Euler(0f, -offsetModeloY, 0f) * Vector3.forward;

    private void OnEnable()
    {
        player = FindFirstObjectByType<MovimientoAlastor>()?.transform;
        agent = GetComponent<NavMeshAgent>();
        infligirDanio = GetComponent<InfligirDanio>();
        animator = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody>();

        if (agent != null)
        {
            // Rotación la gestionamos nosotros (rápida y con "girar en sitio").
            agent.updateRotation = false;
            agent.updateUpAxis = false;
            agent.angularSpeed = 0f;
        }

        if (hacerIntro && !introInicializada)
        {
            // Sólo inicializamos la intro UNA vez. Si algún FSM maestro vuelve
            // a hacer enable de este componente, no queremos volver a subir al
            // jefe 15 unidades y disparar otra caída.
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
        if (player == null) return;

        // 1. INTRO
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

        // 2. DISTANCIA AL JUGADOR
        //    Antes, si pasaba de rangoLargo soltábamos el control a estadoPerseguir.
        //    Pero ese estado no perseguía y el jefe se quedaba parado al perder el
        //    rastro. Ahora seguimos persiguiendo nosotros siempre: sólo dejamos de
        //    atacar cuando está fuera de rango.
        float distancia = Vector3.Distance(transform.position, player.position);

        // 3. MOVIMIENTO Y ORIENTACIÓN
        Vector3 dirAlJugador = player.position - transform.position;
        dirAlJugador.y = 0f;
        // Usamos ForwardVisual (no transform.forward) porque el modelo va girado 180°.
        float anguloAlJugador = dirAlJugador.sqrMagnitude > 0.001f
            ? Vector3.Angle(ForwardVisual, dirAlJugador)
            : 0f;

        bool enRangoCorto = distancia <= rangoMazazo * 0.9f;

        if (enRangoCorto)
        {
            // Pegado al jugador: parar y mirarle.
            ParaAgente();
            GirarHacia(dirAlJugador);
            ActualizarAnimaciones(moviendose: false, anguloAlJugador);
        }
        else
        {
            // Perseguir. NO comprobamos un "ángulo demasiado grande para andar":
            // como la rotación sigue a agent.velocity, el cuerpo encara siempre
            // la dirección de movimiento → no hay marcha atrás posible. Antes
            // ese check oscilaba (para-rota-anda-para-rota-anda) y se veía como
            // un TP a tirones.
            agent.isStopped = false;
            agent.SetDestination(player.position);

            Vector3 dirMov = agent.velocity;
            dirMov.y = 0f;
            if (dirMov.sqrMagnitude > 0.04f) GirarHacia(dirMov);
            else                              GirarHacia(dirAlJugador);

            ActualizarAnimaciones(moviendose: true, anguloAlJugador);
        }

        // 4. ATAQUES
        cooldown -= Time.deltaTime;
        if (cooldown <= 0f && distancia <= rangoLargo)
        {
            EjecutarAtaque(ordenAtaques[indiceAtaqueActual]);
            indiceAtaqueActual = (indiceAtaqueActual + 1) % ordenAtaques.Length;
            cooldown = tiempoEntreAtaques;
        }
    }

    // LateUpdate eliminado: con "Bake Into Pose" activado en los clips las
    // animaciones son in-place y no hace falta cancelar deriva manualmente.

    private void ParaAgente()
    {
        if (agent == null || !agent.isOnNavMesh) return;
        agent.isStopped = true;
        agent.velocity = Vector3.zero;
        // No ResetPath: conservamos el path para reanudar sin tener que recomputarlo
        // (recomputar es asíncrono y produce ese parón que parece un TP).
    }

    private void GirarHacia(Vector3 direccion)
    {
        direccion.y = 0f;
        if (direccion.sqrMagnitude < 0.001f) return;
        // Rotación instantánea + offset del modelo (frente del bicho está en -Z).
        transform.rotation = Quaternion.LookRotation(direccion.normalized) * OffsetModelo;
    }

    private void ActualizarAnimaciones(bool moviendose, float anguloAlJugador)
    {
        if (animator == null) return;

        animator.SetBool("caminando", moviendose);
        // "girando" solo si estamos parados y el ángulo es notable.
        animator.SetBool("girando", !moviendose && anguloAlJugador > anguloFinGiroEnSitio);
    }

    private void EjecutarAtaque(string nombre)
    {
        switch (nombre)
        {
            case "mazazo":    StartCoroutine(Mazazo());    break;
            case "embestida": StartCoroutine(Embestida()); break;
            case "salto":     StartCoroutine(Salto());     break;
        }
    }

    private void ResetearParametrosMovimiento()
    {
        if (animator == null) return;
        animator.SetBool("caminando", false);
        animator.SetBool("girando", false);
    }

    // ─────────────────────────────────────────────
    //  INTRO
    // ─────────────────────────────────────────────
    private IEnumerator IntroCaida()
    {
        cayendo = true;
        atacando = true;

        if (rb != null) rb.isKinematic = true;
        if (agent != null) agent.enabled = false;

        if (animator != null)
        {
            // Reset por si quedó modificado en una pasada anterior.
            animator.speed = 1f;

            // Estiramos/encogemos el clip "caer" para que dure exactamente duracionCaida.
            float caerLength = ObtenerDuracionClip("caer");
            if (caerLength > 0.01f && duracionCaida > 0.05f)
                animator.speed = caerLength / duracionCaida;

            // Forzamos la entrada al state Caer. Update() inmediato para que
            // el animator empiece a sample-ar el clip en el frame actual y
            // no haya un frame "muerto" en Idle.
            animator.Play("Caer", 0, 0f);
            animator.Update(0f);
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

        // Al salir de "Caída" volvemos a la orientación normal del modelo.
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

    // ─────────────────────────────────────────────
    //  MAZAZO
    // ─────────────────────────────────────────────
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
        // Girada final instantánea hacia el jugador (con offset del modelo).
        Vector3 dirFinal = player.position - transform.position;
        dirFinal.y = 0f;
        if (dirFinal.sqrMagnitude > 0.01f)
            transform.rotation = Quaternion.LookRotation(dirFinal.normalized) * OffsetModelo;

        ResetearParametrosMovimiento();
        if (animator != null) animator.Play("mazazo");

        yield return new WaitForSeconds(0.4f);

        if (Vector3.Distance(transform.position, player.position) <= rangoMazazo + 1.0f)
            if (infligirDanio != null) infligirDanio.IntentarGolpear(player);

        yield return new WaitForSeconds(0.8f);
        FinAtaque();
    }

    // ─────────────────────────────────────────────
    //  EMBESTIDA
    // ─────────────────────────────────────────────
    private IEnumerator Embestida()
    {
        atacando = true;
        ResetearParametrosMovimiento();

        ParaAgente();

        // Encarar al jugador (con offset del modelo).
        Vector3 dir = player.position - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude > 0.01f)
            transform.rotation = Quaternion.LookRotation(dir.normalized) * OffsetModelo;

        if (animator != null) animator.Play("embestir");

        // Anticipación quieta.
        yield return new WaitForSeconds(0.3f);

        // Dirección de carga = frente VISUAL del bicho (transform.-Z porque el
        // modelo está girado 180° respecto al transform). La fijamos una sola
        // vez para que el dash sea recto, sin curva.
        ultimaDireccionCarga = ForwardVisual;

        // Para el dash queremos que sea el AGENTE quien se mueva (manteniéndose
        // en el NavMesh). Si usamos transform.Translate con el agente activo,
        // el agente reescribe la posición cada frame (updatePosition = true por
        // defecto) y se ve a saltos en vez de deslizarse.
        if (agent != null && agent.isOnNavMesh) agent.isStopped = false;

        float t = 0f;
        float duracionCarga = 0.6f;
        float velocidadCarga = 18f;

        while (t < duracionCarga)
        {
            Vector3 paso = ultimaDireccionCarga * velocidadCarga * Time.deltaTime;
            if (agent != null && agent.enabled && agent.isOnNavMesh)
                agent.Move(paso);
            else
                transform.Translate(paso, Space.World);

            AplicarDañoSiCerca(2.5f);
            t += Time.deltaTime;
            yield return null;
        }

        // Tras la carga: paramos la inercia y, MIENTRAS hacemos la recuperación,
        // dejamos al agente precomputando el path al jugador. Así cuando vuelva
        // a moverse no hay parón ni acelerón abrupto que parezca un TP.
        if (agent != null && agent.isOnNavMesh)
        {
            agent.velocity = Vector3.zero;
            agent.isStopped = true;
            if (player != null) agent.SetDestination(player.position);
        }

        yield return new WaitForSeconds(0.25f);

        // Refrescamos el destino justo antes de reanudar (el jugador habrá
        // seguido moviéndose) y soltamos al agente sin tocar el path.
        if (agent != null && agent.isOnNavMesh && player != null)
            agent.SetDestination(player.position);

        FinAtaque();
    }

    // ─────────────────────────────────────────────
    //  SALTO
    // ─────────────────────────────────────────────
    private IEnumerator Salto()
    {
        atacando = true;
        ResetearParametrosMovimiento();

        // Capturamos destino ANTES de tocar el agente, por si el player se mueve.
        Vector3 destino = player.position;

        // Encarar al jugador instantáneamente.
        Vector3 dirInicial = destino - transform.position;
        dirInicial.y = 0f;
        if (dirInicial.sqrMagnitude > 0.01f)
            transform.rotation = Quaternion.LookRotation(dirInicial.normalized) * OffsetModelo;

        // Durante el vuelo controlamos la posición manualmente → agente fuera.
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

        while (t < duracionSalto)
        {
            t += Time.deltaTime;
            float p = t / duracionSalto;
            float altura = Mathf.Sin(p * Mathf.PI) * alturaArco;
            transform.position = Vector3.Lerp(inicio, destino, p) + Vector3.up * altura;

            // Mantenemos la cara orientada al destino (no al jugador, para que
            // no curvee mientras vuela).
            Vector3 dirVuelo = destino - transform.position;
            dirVuelo.y = 0f;
            if (dirVuelo.sqrMagnitude > 0.01f)
                transform.rotation = Quaternion.LookRotation(dirVuelo.normalized) * OffsetModelo;

            yield return null;
        }

        // Aterrizaje exacto sobre el destino.
        transform.position = new Vector3(destino.x, inicio.y, destino.z);

        // Reactivar agente y reanclarlo al NavMesh por si el aterrizaje cayó
        // un pelín fuera. Warp ajusta la posición interna del agente.
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

        // Impacto al aterrizar.
        AplicarDañoSiCerca(3f);

        yield return new WaitForSeconds(0.5f);
        FinAtaque();
    }

    // ─────────────────────────────────────────────
    //  HELPERS
    // ─────────────────────────────────────────────
    private void AplicarDañoSiCerca(float radio)
    {
        if (player == null || infligirDanio == null) return;
        if (Vector3.Distance(transform.position, player.position) <= radio)
            infligirDanio.IntentarGolpear(player);
    }

    // Onda de choque: solo daño, sin empujar al jugador (el empujón disparaba
    // RetrocesoConColisiones del player con el CharacterController desactivado y petaba).
    private void OndaChoque(float radio)
    {
        AplicarDañoSiCerca(radio);
    }

    private void FinAtaque()
    {
        atacando = false;
        girandoEnSitio = false;
        ResetearParametrosMovimiento();

        // NO hacemos ResetPath: si la coroutina dejó un destino preparado
        // (caso de la embestida), queremos seguirlo sin esperar a recomputar.
        if (agent != null && agent.enabled && agent.isOnNavMesh)
            agent.isStopped = false;
    }
}
