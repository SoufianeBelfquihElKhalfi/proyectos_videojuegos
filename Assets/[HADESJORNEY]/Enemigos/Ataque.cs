using Enemy.FSM;
using UnityEngine;
using UnityEngine.AI;

public class Ataque : EstadoFSM
{
    Transform player;
    NavMeshAgent agent;

    [SerializeField] private float distanciaAtaque = 2f;
    [SerializeField] private float tiempoEspera = 1.5f;
    [SerializeField] private float velocidadRotacion = 10f;

    private float tiempoUltimoAtaque;
    private bool esperando = false;

    void OnEnable()
    {
        var jugador = FindFirstObjectByType<MovimientoAlastor>();
        if (jugador != null)
            player = jugador.transform;

        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

        if (agent != null)
        {
            agent.isStopped = false;
            agent.updateRotation = false;
        }

        esperando = false;
    }

    void Update()
    {
        if (player == null || agent == null) return;
        if (!agent.isOnNavMesh) return;

        Vector3 direccion = (player.position - transform.position);
        direccion.y = 0;
        if (direccion != Vector3.zero)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direccion), velocidadRotacion * Time.deltaTime);

        float distancia = Vector3.Distance(transform.position, player.position);

        if (esperando)
        {
            agent.isStopped = true;

            if (Time.time - tiempoUltimoAtaque >= tiempoEspera)
            {
                esperando = false;
                agent.isStopped = false;
            }
            return;
        }

        if (distancia > distanciaAtaque)
        {
            agent.SetDestination(player.position);
        }
        else
        {
            agent.isStopped = true;
            tiempoUltimoAtaque = Time.time;
            esperando = true;
        }
    }
}