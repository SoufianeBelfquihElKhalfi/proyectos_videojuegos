using UnityEngine;

public class InfligirDanio : MonoBehaviour
{
    [Tooltip("1 = medio corazón, 2 = un corazón, 3 = corazón y medio...")]
    public int danioMitadCorazones = 1;

    private void OnTriggerEnter(Collider other)
    {
        SistemaVida vida = other.GetComponent<SistemaVida>();

        if (vida != null)
        {
            vida.RecibirDanio(danioMitadCorazones);
        }
    }
}