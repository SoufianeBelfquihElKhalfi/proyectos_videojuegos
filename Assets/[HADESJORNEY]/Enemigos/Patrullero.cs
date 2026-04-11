using Enemy.FSM;
using UnityEngine;

public class Patrullero : MaquinaFSM
{
    [SerializeField] GameString patrulla;
    [SerializeField] GameString ataque;
    [SerializeField] private float rangoDeteccion = 10f;
    Transform player;

    void Start()
    {
        var jugador = FindFirstObjectByType<MovimientoAlastor>();
        if (jugador != null)
            player = jugador.transform;
    }

    void Update()
    {
        if (player == null) return;

        float distanciaAlPlayer = Vector3.Distance(transform.position, player.position);

        if (distanciaAlPlayer < rangoDeteccion)
        {
            SetEstado(ataque.Value);
        }
        else
        {
            SetEstado(patrulla.Value);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, rangoDeteccion);
    }
}