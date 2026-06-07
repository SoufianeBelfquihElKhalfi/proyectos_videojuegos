using UnityEngine;
using Dapasa.Audio;

public class MenuPrincipal : MonoBehaviour
{
    public GameObject canvasMenuPrincipal;
    public GameObject canvasOpciones;

    [Header("Audio UI")]
    [SerializeField] private string idSonidoSeleccion = "ui_select";
    [SerializeField] private string idSonidoVolver = "ui_back";

    private void Start()
    {
        canvasMenuPrincipal.SetActive(true);
        canvasOpciones.SetActive(false);
    }

    public void CargarEscena()
    {
        ReproducirSonidoUI(idSonidoSeleccion);

        ReiniciarPartida();

        SceneLoader.Load(
            "EscenaInicial",
            "Abriendo las puertas del Inframundo..."
        );
    }

    private void ReiniciarPartida()
    {
        Time.timeScale = 1f;

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

        EstadisticasJugador.ResetearEstadisticasGlobales();

        Debug.Log("Partida reiniciada completamente.");
    }

    public void AbrirOpciones()
    {
        ReproducirSonidoUI(idSonidoSeleccion);

        canvasMenuPrincipal.SetActive(false);
        canvasOpciones.SetActive(true);
    }

    public void VolverAlMenu()
    {
        ReproducirSonidoUI(idSonidoVolver);

        canvasOpciones.SetActive(false);
        canvasMenuPrincipal.SetActive(true);
    }

    public void SalirDelJuego()
    {
        ReproducirSonidoUI(idSonidoSeleccion);

        Application.Quit();
        Debug.Log("Saliendo del juego...");
    }

    private void ReproducirSonidoUI(string idSonido)
    {
        if (string.IsNullOrWhiteSpace(idSonido))
            return;

        if (AudioManager.Instance == null)
        {
            Debug.LogWarning($"{name}: no hay AudioManager en la escena.");
            return;
        }

        AudioManager.Instance.ReproducirSFX2D(idSonido);
    }
}