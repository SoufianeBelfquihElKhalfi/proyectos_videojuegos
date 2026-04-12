using UnityEngine;

public class InfligirDanioFlecha : MonoBehaviour
{
    [Tooltip("1 = medio corazón, 2 = un corazón, 3 = corazón y medio...")]
    public int danioMitadCorazones = 1;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Ghost") )
            return;

        SistemaVida vida = other.GetComponent<SistemaVida>();
        if (vida != null)
        {
            vida.RecibirDanio(danioMitadCorazones);
        }
    }
}