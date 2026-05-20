using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class EnemigoDistancia : MonoBehaviour
{
    private enum Estado { Patrullar, Detectando, Combate }

    [Header("Detección")]
    [SerializeField] private float rangoDeteccion = 15f;
    [SerializeField] private float distanciaDisparo = 8f;

    [Header("Disparo")]
    [SerializeField] private GameObject prefabProyectil;
    [SerializeField] private Transform puntoDisparo;
    [SerializeField] private float cadenciaDisparo = 2f;

    [Header("Movimiento de combate")]
    [SerializeField] private float distanciaMinima = 6f;
    [SerializeField] private float velocidadRotacion = 8f;
    [Tooltip("Tiempo entre decisiones de moverse en combate")]
    [SerializeField] private float intervaloDecision = 1.5f;
    [Tooltip("Distancia que strafea lateralmente")]
    [SerializeField] private float distanciaStrafe = 3f;

    [Header("Patrulla")]
    [SerializeField] private Transform[] puntosRuta;

    [Header("Detección Visual")]
    [SerializeField] private GameObject prefabExclamacion;
    [SerializeField] private Transform puntoExclamacion;

    private Transform jugador;
    private NavMeshAgent agente;
    private Animator animator;

    private Estado estadoActual = Estado.Patrullar;
    private float tiempoUltimoDisparo;
    private float tiempoUltimaDecision;
    private int puntoActual = 0;

    void Start()
    {
        agente = GetComponent<NavMeshAgent>();
        agente.updateRotation = false;
        animator = GetComponentInChildren<Animator>();

        var jugadorObj = FindFirstObjectByType<MovimientoAlastor>();
        if (jugadorObj != null) jugador = jugadorObj.transform;

        IrAlSiguientePunto();
    }

    void Update()
    {
        if (jugador == null) return;

        ActualizarAnimacionMovimiento();

        float distancia = Vector3.Distance(transform.position, jugador.position);

        switch (estadoActual)
        {
            case Estado.Patrullar:
                EstadoPatrullar(distancia);
                break;
            case Estado.Detectando:
                // La corrutina se encarga
                break;
            case Estado.Combate:
                EstadoCombate(distancia);
                break;
        }
    }

    private void ActualizarAnimacionMovimiento()
    {
        if (agente == null || animator == null) return;
        animator.SetBool("correr", agente.velocity.magnitude > 0.1f);
    }

    // ---------- ESTADO: PATRULLAR ----------

    private void EstadoPatrullar(float distancia)
    {
        if (distancia < rangoDeteccion)
        {
            StartCoroutine(SecuenciaDeteccion());
            return;
        }

        if (puntosRuta == null || puntosRuta.Length == 0) return;

        if (!agente.pathPending && agente.remainingDistance <= 0.5f)
            IrAlSiguientePunto();
    }

    private void IrAlSiguientePunto()
    {
        if (puntosRuta == null || puntosRuta.Length == 0) return;
        if (agente == null || !agente.isOnNavMesh) return;

        agente.SetDestination(puntosRuta[puntoActual].position);
        puntoActual = (puntoActual + 1) % puntosRuta.Length;
    }

    // ---------- ESTADO: DETECTANDO ----------

    private IEnumerator SecuenciaDeteccion()
    {
        estadoActual = Estado.Detectando;
        agente.isStopped = true;

        GameObject exclamacion = null;
        if (prefabExclamacion != null && puntoExclamacion != null)
        {
            exclamacion = Instantiate(prefabExclamacion, puntoExclamacion.position, Quaternion.identity);
            exclamacion.transform.SetParent(puntoExclamacion);
        }

        yield return new WaitForSeconds(1f);

        if (exclamacion != null) Destroy(exclamacion);

        agente.isStopped = false;
        estadoActual = Estado.Combate;
    }

    // ---------- ESTADO: COMBATE ----------

    private void EstadoCombate(float distancia)
    {
        // Si pierde al jugador, vuelve a patrullar
        if (distancia > rangoDeteccion * 1.3f)
        {
            estadoActual = Estado.Patrullar;
            IrAlSiguientePunto();
            return;
        }

        // Rotar suavemente hacia el jugador
        Vector3 direccion = jugador.position - transform.position;
        direccion.y = 0;
        if (direccion.sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(direccion);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, velocidadRotacion * Time.deltaTime);
        }

        // Decidir movimiento cada cierto tiempo
        if (Time.time - tiempoUltimaDecision >= intervaloDecision)
        {
            DecidirMovimientoCombate(distancia);
            tiempoUltimaDecision = Time.time;
        }

        // Disparar si cumple cadencia y mira hacia el jugador
        if (Time.time - tiempoUltimoDisparo >= cadenciaDisparo)
        {
            Disparar();
            tiempoUltimoDisparo = Time.time;
        }
    }

    private void DecidirMovimientoCombate(float distancia)
    {
        Vector3 destino;

        if (distancia > distanciaDisparo)
        {
            // Demasiado lejos: acercarse
            destino = jugador.position;
        }
        else if (distancia < distanciaMinima)
        {
            // Demasiado cerca: huir
            Vector3 huir = (transform.position - jugador.position).normalized;
            destino = transform.position + huir * (distanciaDisparo - distancia);
        }
        else
        {
            // En rango: strafear lateralmente (izquierda o derecha aleatoriamente)
            Vector3 lateral = transform.right * (Random.value > 0.5f ? 1f : -1f);
            destino = transform.position + lateral * distanciaStrafe;
        }

        // Asegurar que el destino sea válido en el NavMesh
        if (NavMesh.SamplePosition(destino, out NavMeshHit hit, 2f, NavMesh.AllAreas))
            agente.SetDestination(hit.position);
    }

    // ---------- DISPARO ----------

    private void Disparar()
    {
        if (prefabProyectil == null || puntoDisparo == null) return;

        if (animator != null)
        {
            animator.speed = 1f;
            animator.SetTrigger("Disparar");
        }
    }

    // Llamado por Animation Event
    public void LanzarProyectil()
    {
        if (prefabProyectil == null || puntoDisparo == null) return;

        GameObject bola = Instantiate(prefabProyectil, puntoDisparo.position, Quaternion.identity);
        Proyectil proy = bola.GetComponent<Proyectil>();

        if (proy != null)
        {
            Vector3 direccion = (jugador.position + Vector3.up * 0.5f - puntoDisparo.position).normalized;
            proy.Inicializar(direccion);
        }
    }

    public void RecibirDanioAnimacion()
    {
        if (animator != null)
            animator.SetTrigger("danio");
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, rangoDeteccion);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, distanciaDisparo);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, distanciaMinima);
    }
}