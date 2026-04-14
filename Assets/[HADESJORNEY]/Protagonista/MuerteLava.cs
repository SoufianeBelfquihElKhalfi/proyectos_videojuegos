using UnityEngine;

public class MuerteLava : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        MuerteJugador scriptMuerte = other.GetComponentInParent<MuerteJugador>();

        if (scriptMuerte != null)
        {
            Debug.Log("Jugador detectado en lava. Ejecutando muerte...");
            scriptMuerte.Morir();
        }
    }
}
