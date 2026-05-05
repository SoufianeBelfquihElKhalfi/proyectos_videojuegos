using UnityEngine;

public class InfligirDanio : MonoBehaviour
{
    [Tooltip("1 = medio corazon, 2 = un corazon")]
    [SerializeField] private int danioMitadCorazones = 1;

    [Header("Golpe")]
    [SerializeField] private float distanciaGolpe = 2f;

    [Header("Retroceso")]
    [SerializeField] private float fuerzaRetroceso = 2f;
    [SerializeField] private float duracionRetroceso = 0.15f;

    public bool IntentarGolpear(Transform jugador)
    {
        if (jugador == null)
        {
            return false;
        }

        float distancia = Vector3.Distance(transform.position, jugador.position);

        if (distancia > distanciaGolpe)
        {
            return false;
        }

        SistemaVida vida = jugador.GetComponentInParent<SistemaVida>();

        if (vida == null)
        {
            return false;
        }

        vida.RecibirDanio(danioMitadCorazones);

        MovimientoAlastor movimientoJugador = jugador.GetComponentInParent<MovimientoAlastor>();

        if (movimientoJugador != null)
        {
            Vector3 direccion = jugador.position - transform.position;
            direccion.y = 0f;

            movimientoJugador.AplicarRetroceso(
                direccion,
                fuerzaRetroceso,
                duracionRetroceso
            );
        }

        return true;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, distanciaGolpe);
    }
}