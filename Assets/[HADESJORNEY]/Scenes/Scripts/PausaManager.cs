using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Dapasa.Audio;

public class PausaManager : MonoBehaviour
{
    [Header("Panel de pausa")]
    public GameObject pausa;
    public Selectable primerBotonPausa;

    [Header("Paneles internos")]
    [SerializeField] private GameObject panelMenuPausa;
    [SerializeField] private GameObject panelOpciones;
    [SerializeField] private Selectable primerControlOpciones;

    private bool opcionesActivo = false;

    [Header("Input Manager antiguo")]
    public string botonPausa = "Pausa";
    public string botonCancelar = "Cancel";
    public string botonAceptar = "Submit";
    public string ejeHorizontal = "Horizontal";
    public string ejeVertical = "Vertical";

    [Header("Escena")]
    public string escenaMenu = "MAIN";

    [Header("Audio UI")]
    [SerializeField] private string idSonidoSeleccion = "ui_select";
    [SerializeField] private string idSonidoVolver = "ui_back";

    private bool pausaActivo = false;

    private bool cursorVisibleAnterior;
    private CursorLockMode cursorLockAnterior;

    private readonly HashSet<string> botonesNoEncontrados = new HashSet<string>();

    void Start()
    {
        if (pausa != null)
        {
            pausa.SetActive(false);
        }

        panelMenuPausa.SetActive(true);
        panelOpciones.SetActive(false);

        AsegurarEventSystem();
    }

    void Update()
    {
        if (BotonPulsado(botonPausa))
        {
            if (pausaActivo && opcionesActivo)
            {
                VolverAPausa();
                return;
            }

            if (pausaActivo)
            {
                ReproducirSonidoUI(idSonidoVolver);
            }

            ToggleMenu();
            return;
        }

        if (pausaActivo && BotonPulsado(botonCancelar))
        {
            if (opcionesActivo)
            {
                VolverAPausa();
            }
            else
            {
                Reanudar();
            }

            return;
        }

        if (pausaActivo && EventSystem.current != null && EventSystem.current.currentSelectedGameObject == null)
        {
            if (opcionesActivo)
            {
                SeleccionarControlOpciones();
            }
            else
            {
                SeleccionarPrimerBoton();
            }
        }
    }

    private bool BotonPulsado(string nombreBoton)
    {
        if (string.IsNullOrEmpty(nombreBoton))
        {
            return false;
        }

        if (botonesNoEncontrados.Contains(nombreBoton))
        {
            return false;
        }

        try
        {
            return Input.GetButtonDown(nombreBoton);
        }
        catch (ArgumentException)
        {
            botonesNoEncontrados.Add(nombreBoton);

            Debug.LogWarning(
                "El botón '" + nombreBoton + "' no existe en Project Settings > Input Manager.",
                this
            );

            return false;
        }
    }

    public void ToggleMenu()
    {
        pausaActivo = !pausaActivo;

        if (pausa != null)
        {
            pausa.SetActive(pausaActivo);
        }

        Time.timeScale = pausaActivo ? 0f : 1f;

        if (pausaActivo)
        {
            AbrirPausa();
        }
        else
        {
            CerrarPausa();
        }
    }

    private void AbrirPausa()
    {
        cursorVisibleAnterior = Cursor.visible;
        cursorLockAnterior = Cursor.lockState;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        AsegurarEventSystem();
        MostrarPanelMenuPausa();
    }

    private void CerrarPausa()
    {
        Cursor.visible = cursorVisibleAnterior;
        Cursor.lockState = cursorLockAnterior;

        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }

    public void Reanudar()
    {
        ReproducirSonidoUI(idSonidoVolver);

        pausaActivo = false;
        opcionesActivo = false;

        if (pausa != null)
        {
            pausa.SetActive(false);
        }

        Time.timeScale = 1f;

        CerrarPausa();
    }

    public void Menu()
    {
        ReproducirSonidoUI(idSonidoSeleccion);

        Time.timeScale = 1f;

        if (string.IsNullOrWhiteSpace(escenaMenu))
        {
            throw new InvalidOperationException($"{name}: escenaMenu está vacío.");
        }

        if (!Application.CanStreamedLevelBeLoaded(escenaMenu))
        {
            throw new InvalidOperationException($"{name}: la escena '{escenaMenu}' no está en Build Settings o el nombre no coincide.");
        }

        SceneLoader.Load(
            escenaMenu,
            "Volviendo al menú principal..."
        );
    }

    public void SalirDelJuego()
    {
        Time.timeScale = 1f;
        Application.Quit();

        Debug.Log("Saliendo del juego...");
    }

    private void SeleccionarPrimerBoton()
    {
        if (primerBotonPausa == null && pausa != null)
        {
            primerBotonPausa = pausa.GetComponentInChildren<Selectable>(true);
        }

        if (primerBotonPausa == null || EventSystem.current == null)
        {
            return;
        }

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(primerBotonPausa.gameObject);
        primerBotonPausa.Select();
    }

    private void AsegurarEventSystem()
    {
        EventSystem eventSystem = EventSystem.current;

        if (eventSystem == null)
        {
            GameObject eventSystemGO = new GameObject("EventSystem");
            eventSystem = eventSystemGO.AddComponent<EventSystem>();
        }

        StandaloneInputModule inputModule = eventSystem.GetComponent<StandaloneInputModule>();

        if (inputModule == null)
        {
            inputModule = eventSystem.gameObject.AddComponent<StandaloneInputModule>();
        }

        inputModule.horizontalAxis = ejeHorizontal;
        inputModule.verticalAxis = ejeVertical;
        inputModule.submitButton = botonAceptar;
        inputModule.cancelButton = botonCancelar;
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

    public void AbrirOpciones()
    {
        ReproducirSonidoUI(idSonidoSeleccion);

        opcionesActivo = true;

        panelMenuPausa.SetActive(false);
        panelOpciones.SetActive(true);

        SeleccionarControlOpciones();
    }

    public void VolverAPausa()
    {
        ReproducirSonidoUI(idSonidoVolver);
        MostrarPanelMenuPausa();
    }

    private void MostrarPanelMenuPausa()
    {
        opcionesActivo = false;

        panelOpciones.SetActive(false);
        panelMenuPausa.SetActive(true);

        SeleccionarPrimerBoton();
    }

    private void SeleccionarControlOpciones()
    {
        Seleccionar(primerControlOpciones);
    }

    private void Seleccionar(Selectable seleccionable)
    {
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(seleccionable.gameObject);
        seleccionable.Select();
    }
}