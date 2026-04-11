using UnityEngine;
using UnityEngine.SceneManagement;

public class MuerteJugador : MonoBehaviour
{
    [SerializeField] private string escenaInicio = "SplashScreen";
    public GameObject panelGameOver;

    public void Morir()
    {
        Debug.Log("Jugador muerto. Cargando: " + escenaInicio);
        SceneManager.LoadScene(escenaInicio);
    }
}