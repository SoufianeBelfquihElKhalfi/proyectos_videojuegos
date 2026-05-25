using Enemy.FSM;
using UnityEngine;
using UnityEngine.AI;

public class Patrulla : EstadoFSM
{
    public Transform[] ruta;
    [SerializeField] private EstadoFSM estadoAtaque;
    [SerializeField] private float rangoDeteccion = 18f;

    private NavMeshAgent agent;
    private Transform jugador;
    private int contador = 0;

    void OnEnable()
    {
        if (agent == null)
            agent = GetComponent<NavMeshAgent>();
        if (jugador == null)
        {
            var jugadorObj = FindFirstObjectByType<MovimientoAlastor>();
            if (jugadorObj != null) jugador = jugadorObj.transform;
        }

        if (agent == null) return;
        if (ruta == null || ruta.Length == 0) return;
        if (!agent.isOnNavMesh) return;

        agent.isStopped = false;
        agent.SetDestination(ruta[contador % ruta.Length].position);
        Debug.Log("Estado PATRULLA activado");
    }

    void Update()
    {
        if (agent == null || jugador == null) return;
        if (!agent.isOnNavMesh) return;

        // Cambiar a ataque si el jugador está cerca
        float distancia = Vector3.Distance(transform.position, jugador.position);
        if (distancia < rangoDeteccion && estadoAtaque != null)
        {
            this.enabled = false;
            estadoAtaque.enabled = true;
            return;
        }

        if (ruta == null || ruta.Length == 0) return;
        if (agent.pathPending) return;
        if (agent.remainingDistance <= 0.1f)
        {
            contador++;
            agent.SetDestination(ruta[contador % ruta.Length].position);
        }
    }
}