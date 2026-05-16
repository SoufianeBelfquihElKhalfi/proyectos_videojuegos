using UnityEngine;
using UnityEngine.AI;
using System.Collections;

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

    [SerializeField] private GameObject prefabExclamacion;
    [SerializeField] private Transform puntoExclamacion;
    private bool detectando = false;

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
        if (animator != null)
            Debug.Log("Estado actual: " + animator.GetCurrentAnimatorStateInfo(0).IsName("Correr"));

        float distancia = Vector3.Distance(transform.position, jugador.position);

        // Animación de correr - ANTES del return
        if (agente != null)
        {
            bool seEstaMoviendo = agente.velocity.magnitude > 0.1f;
            if (animator != null)
                animator.SetBool("correr", seEstaMoviendo);
        }

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
        if (distancia < rangoDeteccion && !jugadorDetectado && !detectando)
        {
            detectando = true;
            StartCoroutine(SecuenciaDeteccion());
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
        Debug.Log("Disparar llamado");
        if (prefabProyectil == null || puntoDisparo == null) return;
        Debug.Log("Posición puntoDisparo Y: " + puntoDisparo.position.y);
        if (animator != null) { 
            Debug.Log("Activando trigger Disparar");
        animator.SetTrigger("Disparar");
    }
        else{
            Debug.Log("Animator null");
        }
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
    public void RecibirDanioAnimacion()
    {
        Debug.Log("RecibirDanioAnimacion llamado");
        if (animator != null)
        {
            Debug.Log("Trigger Daño activado");
            animator.SetTrigger("danio");
        }
        else
            Debug.Log("Animator null en RecibirDanioAnimacion");
    }
    IEnumerator SecuenciaDeteccion()
    {
        agente.isStopped = true;

        GameObject exclamacion = null;
        if (prefabExclamacion != null && puntoExclamacion != null)
        {
            exclamacion = Instantiate(prefabExclamacion, puntoExclamacion.position, Quaternion.identity);
            exclamacion.transform.SetParent(puntoExclamacion);
        }

        yield return new WaitForSeconds(1f);

        if (exclamacion != null)
            Destroy(exclamacion);

        agente.isStopped = false;
        jugadorDetectado = true;
        detectando = false;
    }
}