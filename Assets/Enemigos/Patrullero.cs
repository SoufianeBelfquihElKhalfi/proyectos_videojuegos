using Enemy.FSM;

using UnityEngine;

public class Patrullero : MaquinaFSM
{
    [SerializeField] GameString patrulla;
    [SerializeField] GameString ataque;
    Transform player;

    void Start()
    {
        var jugador = FindFirstObjectByType<MovimientoAlastor>();
        if (jugador != null)
            player = jugador.transform;

        //Debug.Log("Valor Patrulla = [" + patrulla.Value + "]");
        //Debug.Log("Valor Ataque = [" + ataque.Value + "]");
    }

    void Update()
    {
        if (player == null) return;

        float distanciaAlPlayer = Vector3.Distance(transform.position, player.position);
        //Debug.Log("Distancia al jugador: " + distanciaAlPlayer);

        if (distanciaAlPlayer < 5f)
        {
           // Debug.Log("Intento cambiar a: " + ataque.Value);
            SetEstado(ataque.Value);
        }
        else
        {
            //Debug.Log("Intento cambiar a: " + patrulla.Value);
            SetEstado(patrulla.Value);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 5f);
    }
}