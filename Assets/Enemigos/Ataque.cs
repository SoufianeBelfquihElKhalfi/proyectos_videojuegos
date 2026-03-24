using Enemy.FSM;

using UnityEngine;
using UnityEngine.AI;

public class Ataque : EstadoFSM
{
    Transform player;
    NavMeshAgent agent;

    void OnEnable()
    {
        var jugador = FindFirstObjectByType<MovimientoAlastor>();
        if (jugador != null)
            player = jugador.transform;

        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

        if (agent != null)
            agent.isStopped = false;

        Debug.Log("Estado ATAQUE activado");
    }

    void Update()
    {
        if (player == null || agent == null) return;
        if (!agent.isOnNavMesh) return;

        float distanciaAlPlayer = Vector3.Distance(transform.position, player.position);

        if (distanciaAlPlayer < 5f)
        {
            agent.SetDestination(player.position);
        }
        else
        {
            agent.ResetPath();
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 5f);
    }
}