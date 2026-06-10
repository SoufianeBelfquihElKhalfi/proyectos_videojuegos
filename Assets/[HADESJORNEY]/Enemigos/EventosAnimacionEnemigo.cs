using UnityEngine;
using UnityEngine.AI;

public class EventosAnimacionEnemigo : MonoBehaviour
{
    [SerializeField] private float velocidadMinimaParaPisada = 0.1f;

    private SonidosEnemigos sonidos;
    private NavMeshAgent agent;

    private void Awake()
    {
        sonidos = GetComponentInParent<SonidosEnemigos>();
        agent = GetComponentInParent<NavMeshAgent>();

        if (sonidos == null)
        {
            throw new MissingComponentException($"{name}: no se encontró SonidosEnemigos en el enemigo padre.");
        }

        if (agent == null)
        {
            throw new MissingComponentException($"{name}: no se encontró NavMeshAgent en el enemigo padre.");
        }
    }

    public void EventoPisada()
    {
        if (agent.velocity.sqrMagnitude < velocidadMinimaParaPisada * velocidadMinimaParaPisada)
            return;

        if (agent.isStopped)
            return;

        sonidos.ReproducirPisada();
    }
}