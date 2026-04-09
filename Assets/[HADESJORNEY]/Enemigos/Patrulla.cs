using Enemy.FSM;

using UnityEngine;
using UnityEngine.AI;

public class Patrulla : EstadoFSM
{
    public Transform[] ruta;

    NavMeshAgent agent;
    int contador = 0;

    void OnEnable()
    {
        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

        if (agent == null) return;
        if (ruta == null || ruta.Length == 0) return;
        if (!agent.isOnNavMesh) return;

        agent.isStopped = false;
        agent.SetDestination(ruta[contador % ruta.Length].position);

        Debug.Log("Estado PATRULLA activado");
    }

    void Update()
    {
        if (agent == null) return;
        if (ruta == null || ruta.Length == 0) return;
        if (!agent.isOnNavMesh) return;
        if (agent.pathPending) return;

        if (agent.remainingDistance <= 0.1f)
        {
            contador++;
            agent.SetDestination(ruta[contador % ruta.Length].position);
        }
    }
}