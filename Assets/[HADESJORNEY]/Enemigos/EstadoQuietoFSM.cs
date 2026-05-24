using Enemy.FSM;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EstadoQuietoFSM : EstadoFSM
{
    private NavMeshAgent agent;

    protected override void OnAwake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    private void OnEnable()
    {
        agent.isStopped = true;
        agent.ResetPath();
        agent.velocity = Vector3.zero;
    }
}