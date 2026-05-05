using UnityEngine;

public class MenuPrincipal : MonoBehaviour
{
    public GameObject canvasMenuPrincipal;
    public GameObject canvasOpciones;

    void Start()
    {
        canvasMenuPrincipal.SetActive(true);
        canvasOpciones.SetActive(false);
    }

    public void CargarEscena()
    {
        DatosGlobales.ReiniciarPartida();
        SceneLoader.Load("Sala_Inicial", "Abriendo las puertas del Inframundo...");
    }

    public void AbrirOpciones()
    {
        canvasMenuPrincipal.SetActive(false);
        canvasOpciones.SetActive(true);
    }

    public void VolverAlMenu()
    {
        canvasOpciones.SetActive(false);
        canvasMenuPrincipal.SetActive(true);
    }

    public void SalirDelJuego()
    {
        Application.Quit();
        Debug.Log("Saliendo del juego...");
    }
}