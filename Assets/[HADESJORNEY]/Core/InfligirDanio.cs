using UnityEngine;

public class InfligirDanio : MonoBehaviour
{
    [Tooltip("1 = medio corazón, 2 = un corazón, 3 = corazón y medio...")]
    public int danioMitadCorazones = 1;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Flecha tocó: " + other.gameObject.name);

        SistemaVida vida = other.GetComponent<SistemaVida>();
        if (vida != null)
        {
            vida.RecibirDanio(danioMitadCorazones);
            Debug.Log("Daño aplicado a: " + other.gameObject.name);
        }
        else
        {
            Debug.Log("Sin SistemaVida en: " + other.gameObject.name);
        }
    }
}