using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MuerteJugador : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject panelGameOver;

    [Header("Control")]
    [SerializeField] private MonoBehaviour[] componentesADesactivar;

    [Header("Escenas")]
    [SerializeField] private string escenaMenu = "MAIN";
    [SerializeField] private bool usarLoadingScreen = true;
    [SerializeField] private string escenaInicial = "EscenaInicial";
    [SerializeField] private string escenaMercader = "EscenaMercader";

    [SerializeField]
    private string[] escenasDespuesDelMercader =
    {
        "EscenaCombate02",
        "SalaJefe"
    };

    private bool haMuerto = false;

    private void Awake()
    {
        ValidarConfiguracion();
    }

    private void ValidarConfiguracion()
    {
        ValidarEscena(escenaMenu, "escenaMenu");
        ValidarEscena(escenaInicial, "escenaInicial");
        ValidarEscena(escenaMercader, "escenaMercader");

        for (int i = 0; i < escenasDespuesDelMercader.Length; i++)
        {
            ValidarEscena(escenasDespuesDelMercader[i], $"escenasDespuesDelMercader[{i}]");
        }

        for (int i = 0; i < componentesADesactivar.Length; i++)
        {
            if (componentesADesactivar[i] == null)
            {
                throw new MissingReferenceException($"{name}: hay una entrada vacia en componentesADesactivar[{i}].");
            }
        }
    }

    private void ValidarEscena(string nombreEscena, string nombreCampo)
    {
        if (string.IsNullOrWhiteSpace(nombreEscena))
        {
            throw new InvalidOperationException($"{name}: el campo {nombreCampo} esta vacio.");
        }

        if (!Application.CanStreamedLevelBeLoaded(nombreEscena))
        {
            throw new InvalidOperationException($"{name}: la escena '{nombreEscena}' indicada en {nombreCampo} no esta en Build Settings o el nombre no coincide.");
        }
    }

    public void Morir()
    {
        if (haMuerto)
        {
            return;
        }

        if (panelGameOver == null)
        {
            throw new MissingReferenceException($"{name}: falta asignar Panel Game Over en MuerteJugador.");
        }

        haMuerto = true;

        foreach (MonoBehaviour componente in componentesADesactivar)
        {
            componente.enabled = false;
        }

        Time.timeScale = 0f;
        panelGameOver.SetActive(true);
    }

    public void Reintentar()
    {
        PrepararCambioDeEscena();

        string escenaActual = SceneManager.GetActiveScene().name;
        bool volverAlMercader = EstaDespuesDelMercader(escenaActual);

        string escenaDestino;
        string mensajeCarga;

        if (volverAlMercader)
        {
            CheckpointData datos = CheckpointData.Instancia;

            if (datos == null)
            {
                throw new MissingReferenceException("No existe CheckpointData. Debe existir desde MAIN y mantenerse entre escenas.");
            }

            if (!datos.HayCheckpoint)
            {
                throw new InvalidOperationException($"{name}: el jugador ha muerto despues del mercader, pero no hay checkpoint guardado. Revisa el CambioDeEscena que sale de EscenaMercader.");
            }

            if (string.IsNullOrWhiteSpace(datos.EscenaCheckpoint))
            {
                throw new InvalidOperationException($"{name}: hay checkpoint guardado, pero EscenaCheckpoint esta vacia.");
            }

            if (!Application.CanStreamedLevelBeLoaded(datos.EscenaCheckpoint))
            {
                throw new InvalidOperationException($"{name}: la escena de checkpoint '{datos.EscenaCheckpoint}' no esta en Build Settings o el nombre no coincide.");
            }

            escenaDestino = datos.EscenaCheckpoint;
            mensajeCarga = "Volviendo al checkpoint...";
        }
        else
        {
            ReiniciarRunDesdeElPrincipio();

            escenaDestino = escenaInicial;
            mensajeCarga = "Reiniciando partida...";
        }

        CargarEscena(escenaDestino, mensajeCarga);
    }

    public void VolverAlMenu()
    {
        PrepararCambioDeEscena();
        ReiniciarRunDesdeElPrincipio();
        CargarEscena(escenaMenu, "Volviendo al menu...");
    }

    private void PrepararCambioDeEscena()
    {
        Time.timeScale = 1f;
        panelGameOver.SetActive(false);

        foreach (MonoBehaviour componente in componentesADesactivar)
        {
            componente.enabled = true;
        }

        haMuerto = false;
    }

    private void ReiniciarRunDesdeElPrincipio()
    {
        EstadisticasJugador.ResetearEstadisticasGlobales();
        DatosGlobales.ReiniciarPartida();

        CheckpointData datos = CheckpointData.Instancia;

        if (datos == null)
        {
            throw new MissingReferenceException("No existe CheckpointData. Debe existir desde MAIN y mantenerse entre escenas.");
        }

        datos.BorrarCheckpoint();

        InventarioAlmas inventario = InventarioAlmas.Instancia;

        if (inventario == null)
        {
            throw new MissingReferenceException("No existe InventarioAlmas.");
        }

        inventario.ResetearInventario();
    }

    private void CargarEscena(string escenaDestino, string mensajeCarga)
    {
        if (usarLoadingScreen)
        {
            SceneLoader.Load(escenaDestino, mensajeCarga);
        }
        else
        {
            SceneManager.LoadScene(escenaDestino);
        }
    }

    private bool EstaDespuesDelMercader(string escenaActual)
    {
        foreach (string escena in escenasDespuesDelMercader)
        {
            if (escena == escenaActual)
            {
                return true;
            }
        }

        return false;
    }
}