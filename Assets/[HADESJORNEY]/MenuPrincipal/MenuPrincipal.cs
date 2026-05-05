using UnityEngine;

public class MenuPrincipal : MonoBehaviour
{
    public GameObject canvasMenuPrincipal;
    public GameObject canvasOpciones;

    private void Start()
    {
        canvasMenuPrincipal.SetActive(true);
        canvasOpciones.SetActive(false);
    }

    public void CargarEscena()
    {
        ReiniciarPartida();

        SceneLoader.Load(
            "Sala_Inicial",
            "Abriendo las puertas del Inframundo..."
        );
    }

    private void ReiniciarPartida()
    {
        if (InventarioAlmas.Instancia != null)
        {
            InventarioAlmas.Instancia.ResetearInventario();
        }

        if (CheckpointData.Instancia != null)
        {
            CheckpointData.Instancia.BorrarCheckpoint();
        }

        DatosGlobales.hayDatosVida = false;
        DatosGlobales.vidaActual = 0;
        DatosGlobales.vidaMaxima = 0;
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