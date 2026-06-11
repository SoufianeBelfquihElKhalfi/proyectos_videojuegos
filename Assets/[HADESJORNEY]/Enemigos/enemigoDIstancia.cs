using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using Dapasa.Audio;

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
    [SerializeField] private float intervaloDecisionMin = 0.6f;
    [SerializeField] private float intervaloDecisionMax = 1.2f;
    [SerializeField] private float distanciaStrafeMin = 2f;
    [SerializeField] private float distanciaStrafeMax = 4f;

    [Header("Patrulla")]
    [SerializeField] private Transform[] puntosRuta;

    [Header("Aviso de detección")]
    [SerializeField] private AvisoDeteccionEnemigo avisoDeteccion;

    [Header("Música")]
    [SerializeField] private string idMusicaDeteccion = "musica_combate";

    private Transform jugador;
    private NavMeshAgent agente;
    private Animator animator;

    private Estado estadoActual = Estado.Patrullar;
    private float tiempoUltimoDisparo;
    private float tiempoUltimaDecision;
    private float intervaloActual = 1f;
    private int puntoActual = 0;
    private bool estaDisparando = false;

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
                break;
            case Estado.Combate:
                EstadoCombate(distancia);
                break;
        }
    }

    private void ActualizarAnimacionMovimiento()
    {
        if (agente == null || animator == null) return;
    }

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

        RotarHaciaMovimiento();
    }

    private void RotarHaciaMovimiento()
    {
        if (agente.velocity.sqrMagnitude < 0.01f) return;

        Vector3 direccion = agente.velocity;
        direccion.y = 0;

        Quaternion targetRot = Quaternion.LookRotation(direccion);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, velocidadRotacion * Time.deltaTime);
    }

    private void IrAlSiguientePunto()
    {
        if (puntosRuta == null || puntosRuta.Length == 0) return;
        if (agente == null || !agente.isOnNavMesh) return;

        agente.SetDestination(puntosRuta[puntoActual].position);
        puntoActual = (puntoActual + 1) % puntosRuta.Length;
    }

    private IEnumerator SecuenciaDeteccion()
    {
        estadoActual = Estado.Detectando;
        agente.isStopped = true;

        AudioManager.Instance.ReproducirMusica(idMusicaDeteccion);

        yield return avisoDeteccion.MostrarYEsperar();

        agente.isStopped = false;
        estadoActual = Estado.Combate;
    }

    private void EstadoCombate(float distancia)
    {
        if (distancia > rangoDeteccion * 1.3f)
        {
            estadoActual = Estado.Patrullar;
            IrAlSiguientePunto();
            return;
        }

        RotarHaciaJugador();
        if (estaDisparando) return;

        if (Time.time - tiempoUltimaDecision >= intervaloActual)
        {
            DecidirMovimientoCombate(distancia);
            tiempoUltimaDecision = Time.time;
            intervaloActual = Random.Range(intervaloDecisionMin, intervaloDecisionMax);
        }

        if (Time.time - tiempoUltimoDisparo >= cadenciaDisparo)
        {
            Disparar();
            tiempoUltimoDisparo = Time.time;
        }
    }

    private void RotarHaciaJugador()
    {
        Vector3 direccion = jugador.position - transform.position;
        direccion.y = 0;
        if (direccion.sqrMagnitude < 0.01f) return;

        Quaternion targetRot = Quaternion.LookRotation(direccion);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, velocidadRotacion * Time.deltaTime);
    }

    private void DecidirMovimientoCombate(float distancia)
    {
        Vector3 destino;

        if (distancia > distanciaDisparo)
        {
            Vector3 lateral = transform.right * Random.Range(-2f, 2f);
            destino = jugador.position + lateral;
        }
        else if (distancia < distanciaMinima)
        {
            Vector3 huir = (transform.position - jugador.position).normalized;
            Vector3 lateral = transform.right * Random.Range(-1.5f, 1.5f);
            destino = transform.position + huir * (distanciaDisparo - distancia) + lateral;
        }
        else
        {
            float decision = Random.value;
            Vector3 lateral = transform.right * (Random.value > 0.5f ? 1f : -1f);
            float distanciaStrafe = Random.Range(distanciaStrafeMin, distanciaStrafeMax);

            if (decision < 0.6f)
            {
                destino = transform.position + lateral * distanciaStrafe;
            }
            else if (decision < 0.85f)
            {
                Vector3 haciaJugador = (jugador.position - transform.position).normalized;
                float acercarse = Random.Range(-1.5f, 1.5f);
                destino = transform.position + lateral * distanciaStrafe + haciaJugador * acercarse;
            }
            else
            {
                return;
            }
        }

        if (NavMesh.SamplePosition(destino, out NavMeshHit hit, 2f, NavMesh.AllAreas))
            agente.SetDestination(hit.position);
    }

    private void Disparar()
    {
        if (prefabProyectil == null || puntoDisparo == null) return;

        estaDisparando = true;
        agente.ResetPath();
        agente.velocity = Vector3.zero;

        if (animator != null)
        {
            animator.speed = 1f;
            animator.SetTrigger("Disparar");
        }
    }

    public void LanzarProyectil()
    {
        estaDisparando = false;
        tiempoUltimaDecision = 0f;
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
        estaDisparando = false;
        tiempoUltimaDecision = 0f;

        animator.ResetTrigger("Disparar");

        tiempoUltimoDisparo = Time.time;
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