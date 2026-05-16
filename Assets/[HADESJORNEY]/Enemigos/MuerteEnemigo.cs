using UnityEngine;

public class MuerteEnemigo : MonoBehaviour
{
    public void Morir()
    {
        ControladorSala sala = FindFirstObjectByType<ControladorSala>();
        if (sala != null)
            sala.EnemigoMuerto();
        Destroy(gameObject);
    }
}