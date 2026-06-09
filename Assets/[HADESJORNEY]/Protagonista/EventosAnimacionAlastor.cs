using UnityEngine;

public class EventosAnimacionAlastor : MonoBehaviour
{
    private MovimientoAlastor movimiento;

    private void Awake()
    {
        movimiento = GetComponentInParent<MovimientoAlastor>();

        if (movimiento == null)
        {
            Debug.LogWarning("EventosAnimacionAlastor: no se encontró MovimientoAlastor en el padre.", this);
        }
    }

    public void EventoPisada()
    {
        if (movimiento != null)
            movimiento.EventoPisada();
    }
}