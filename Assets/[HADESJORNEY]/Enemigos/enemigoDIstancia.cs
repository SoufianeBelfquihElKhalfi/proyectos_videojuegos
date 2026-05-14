using UnityEngine;
using UnityEngine.AI;

public class EnemigoDistancia : MonoBehaviour
{
    [Header("Detecci�n")]
    [SerializeField] private float rangoDeteccion = 15f;
    [SerializeField] private float distanciaDisparo = 8f;

    [Header("Disparo")]
    [SerializeField] private GameObject prefabProyectil;
    [SerializeField] private Transform puntoDisparo;
    [SerializeField] private float cadenciaDisparo = 2f;

    [Header("Movimiento")]
    [SerializeField] private float distanciaMinima = 6f;

    [Header("Patrulla")]
    [SerializeField] private Transform[] puntosRuta;

    private Transform jugador;
    private NavMeshAgent agente;
    private float tiempoUltimoDisparo;
    private int puntoActual = 0;
    private Animator animator;
    private bool jugadorDetectado = false;

    void Start()
    {
        agente = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
        var jugadorObj = FindFirstObjectByType<MovimientoAlastor>();
        if (jugadorObj != null)
            jugador = jugadorObj.transform;
        if (animator == null)
            Debug.Log("Animator null en EnemigoDistancia");
        else
            Debug.Log("Animator encontrado en EnemigoDistancia");
        IrAlSiguientePunto();
    }

    void Update()
    {
        if (jugador == null) return;

        float distancia = Vector3.Distance(transform.position, jugador.position);

        if (distancia > rangoDeteccion)
        {
            Patrullar();
            return;
        }

        // Mirar al jugador
        Vector3 direccion = (jugador.position - transform.position);
        direccion.y = 0;
        if (direccion != Vector3.zero)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direccion), 5f * Time.deltaTime);

        if (distancia > distanciaDisparo)
        {
            if (agente != null) agente.SetDestination(jugador.position);
        }
        else if (distancia < distanciaMinima)
        {
            Vector3 huir = transform.position - jugador.position;
            huir.y = 0;
            Vector3 destino = transform.position + huir.normalized * (distanciaDisparo - distancia);

            if (agente != null && (!agente.hasPath || agente.remainingDistance < 0.5f))
            {
                agente.SetDestination(destino);
            }
        }
        else
        {
            if (agente != null) agente.ResetPath();
        }

        if (distancia <= distanciaDisparo && Time.time - tiempoUltimoDisparo >= cadenciaDisparo)
        {
            Disparar();
            tiempoUltimoDisparo = Time.time;
        }
        if (distancia < rangoDeteccion && !jugadorDetectado)
        {
            jugadorDetectado = true;
            Debug.Log("Jugador detectado - activando trigger Deteccion");
            if (animator != null)
                animator.SetTrigger("Deteccion");
        }

        if (distancia > rangoDeteccion)
        {
            jugadorDetectado = false;
        }
    }

    void Patrullar()
    {
        if (agente == null || puntosRuta == null || puntosRuta.Length == 0)
        {
            if (agente != null) agente.ResetPath();
            return;
        }

        if (!agente.pathPending && agente.remainingDistance <= 0.5f)
        {
            IrAlSiguientePunto();
        }
    }

    void IrAlSiguientePunto()
    {
        if (puntosRuta == null || puntosRuta.Length == 0) return;
        if (agente == null || !agente.isOnNavMesh) return;

        agente.SetDestination(puntosRuta[puntoActual].position);
        puntoActual = (puntoActual + 1) % puntosRuta.Length;
    }

    void Disparar()
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