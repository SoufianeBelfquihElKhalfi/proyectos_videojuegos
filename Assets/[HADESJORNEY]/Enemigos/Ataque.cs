using Enemy.FSM;
using UnityEngine;
using UnityEngine.AI;
using System.Collections;

/** Se ha cambiado la manera de funcionar de este script:
 * ANTES el enemigo se movía hacia el jugador y hacía daño por proximidad constantemente,
 * AHORA:
 *  - el enemigo busca al jugador,
 *  - gira hacia él,
 *  - si está lejos, le persigue,
 *  - si está en rango, se para, espera un poco, intenta golpear una vez, espera recuperación, y vuelve a estar libre para actuar.
 */
public class Ataque : EstadoFSM
{
    [SerializeField] private float distanciaAtaque = 2.2f;
    [SerializeField] private float tiempoPreparacion = 0.25f;
    [SerializeField] private float tiempoRecuperacion = 0.45f;
    [SerializeField] private float velocidadRotacion = 10f;

    private Transform player;
    private NavMeshAgent agent;
    private InfligirDanio infligirDanio;

    private bool atacando = false;

    private void OnEnable()
    {
        var jugador = FindFirstObjectByType<MovimientoAlastor>();
        if (jugador != null)
        {
            player = jugador.transform;
        }

        if (agent == null)
        {
            agent = GetComponent<NavMeshAgent>();
        }

        if (infligirDanio == null)
        {
            infligirDanio = GetComponent<InfligirDanio>();
        }

        if (agent != null)
        {
            agent.isStopped = false;
            agent.updateRotation = false;
        }

        atacando = false;
    }

    private void Update()
    {
        if (player == null || agent == null || infligirDanio == null)
        {
            return;
        }

        if (!agent.isOnNavMesh)
        {
            return;
        }

        Vector3 direccion = player.position - transform.position;
        direccion.y = 0f;

        if (direccion != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direccion);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, velocidadRotacion * Time.deltaTime);
        }

        if (atacando)
        {
            return;
        }

        float distancia = Vector3.Distance(transform.position, player.position);

        if (distancia > distanciaAtaque)
        {
            agent.isStopped = false;
            agent.SetDestination(player.position);
        }
        else
        {
            StartCoroutine(RealizarAtaque());
        }
    }

    private IEnumerator RealizarAtaque()
    {
        atacando = true;
        agent.isStopped = true;

        yield return new WaitForSeconds(tiempoPreparacion);

        if (player != null)
        {
            infligirDanio.IntentarGolpear(player);
        }

        yield return new WaitForSeconds(tiempoRecuperacion);

        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = false;
        }

        atacando = false;
    }
}