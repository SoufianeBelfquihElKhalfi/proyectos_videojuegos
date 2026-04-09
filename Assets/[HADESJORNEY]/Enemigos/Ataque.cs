using Enemy.FSM;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class Ataque : EstadoFSM
{
    Transform player;
    NavMeshAgent agent;

    [SerializeField] float distanciaCaptura = 1.5f;
    [SerializeField] string nombreEscena = "SplashScreen";

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

        if (distanciaAlPlayer <= distanciaCaptura)
        {
            Debug.Log("¡Jugador capturado! Cargando escena: " + nombreEscena);
            SceneManager.LoadScene(nombreEscena);
            return;
        }

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

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, distanciaCaptura);
    }
}
