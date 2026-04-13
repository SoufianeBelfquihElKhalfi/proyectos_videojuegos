using UnityEngine;

public class MuerteLava : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // Buscamos el script MuerteJugador en Alastor (o en sus padres si choca la cápsula)
        MuerteJugador scriptMuerte = other.GetComponentInParent<MuerteJugador>();

        if (scriptMuerte != null)
        {
            Debug.Log("Jugador detectado en lava. Ejecutando muerte...");
            scriptMuerte.Morir();
        }
    }
}
