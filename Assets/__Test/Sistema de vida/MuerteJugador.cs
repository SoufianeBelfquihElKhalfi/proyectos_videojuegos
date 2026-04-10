using UnityEngine;

public class MuerteJugador : MonoBehaviour
{
    public GameObject panelGameOver;

    public void Morir()
    {
        Debug.Log("Jugador muerto");

        if (panelGameOver != null)
            panelGameOver.SetActive(true);
    }
}