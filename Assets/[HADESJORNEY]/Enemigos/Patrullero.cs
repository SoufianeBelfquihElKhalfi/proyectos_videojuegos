using Enemy.FSM;
using UnityEngine;

public class Patrullero : MaquinaFSM
{
    [SerializeField] GameString patrulla;
    [SerializeField] GameString ataque;
    [SerializeField] private float rangoDeteccion = 10f;
    Transform player;
    private Animator animator;
    private bool jugadorDetectado = false;

    void Start()
    {
        var jugador = FindFirstObjectByType<MovimientoAlastor>();
        if (jugador != null)
            player = jugador.transform;
        animator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        if (player == null) return;

        float distanciaAlPlayer = Vector3.Distance(transform.position, player.position);

        if (distanciaAlPlayer < rangoDeteccion)
        {
            if (!jugadorDetectado)
            {
                jugadorDetectado = true;
                Debug.Log("Trigger Deteccion activado");
                if (animator != null)
                    animator.SetTrigger("Deteccion");
                else
                    Debug.Log("Animator null en Patrullero");
            }
            SetEstado(ataque.Value);
        }
        else
        {
            jugadorDetectado = false;
            SetEstado(patrulla.Value);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, rangoDeteccion);
    }
}