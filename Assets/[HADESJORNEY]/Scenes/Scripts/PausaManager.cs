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

    [Header("Input Manager antiguo")]
    public string botonPausa = "Pausa";
    public string botonCancelar = "Cancel";
    public string botonAceptar = "Submit";
    public string ejeHorizontal = "Horizontal";
    public string ejeVertical = "Vertical";

    [Header("Tecla de pausa en WebGL")]
    [SerializeField] private KeyCode teclaPausaWebGL = KeyCode.P;

    [Header("Escena")]
    public string escenaMenu = "MAIN";

    [Header("Audio UI")]
    [SerializeField] private string idSonidoSeleccion = "ui_select";
    [SerializeField] private string idSonidoVolver = "ui_back";

    private bool pausaActivo;
    private bool opcionesActivo;

    private bool cursorVisibleAnterior;
    private CursorLockMode cursorLockAnterior;

    private readonly HashSet<string> botonesNoEncontrados =
        new HashSet<string>();

    private void Start()
    {
        pausaActivo = false;
        opcionesActivo = false;

        if (pausa != null)
        {
            pausa.SetActive(false);
        }

        if (panelMenuPausa != null)
        {
            panelMenuPausa.SetActive(true);
        }

        if (panelOpciones != null)
        {
            panelOpciones.SetActive(false);
        }

        Time.timeScale = 1f;

        AsegurarEventSystem();
    }

    private void Update()
    {
        /*
         * En WebGL usa la tecla P.
         * En el Editor y en otros builds usa el botón "Pausa"
         * configurado en el Input Manager.
         */
        if (BotonPausaPulsado())
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

        /*
         * En WebGL no se utiliza Escape para controlar la pausa.
         * En el Editor y otros builds sí se utiliza "Cancel".
         */
        if (pausaActivo && BotonCancelarPulsado())
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

        /*
         * Si se pierde la selección de la interfaz,
         * vuelve a seleccionar el elemento correspondiente.
         */
        if (
            pausaActivo &&
            EventSystem.current != null &&
            EventSystem.current.currentSelectedGameObject == null
        )
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

    private bool BotonPausaPulsado()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        return Input.GetKeyDown(teclaPausaWebGL);
#else
        return BotonPulsado(botonPausa);
#endif
    }

    private bool BotonCancelarPulsado()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        /*
         * En la versión WebGL Escape no controla el menú.
         * Para volver o cerrar se utiliza también la tecla P.
         */
        return false;
#else
        return BotonPulsado(botonCancelar);
#endif
    }

    private bool BotonPulsado(string nombreBoton)
    {
        if (string.IsNullOrWhiteSpace(nombreBoton))
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
                "El botón '" +
                nombreBoton +
                "' no existe en Project Settings > Input Manager.",
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
        opcionesActivo = false;

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

        pausaActivo = false;
        opcionesActivo = false;
        Time.timeScale = 1f;

        if (string.IsNullOrWhiteSpace(escenaMenu))
        {
            Debug.LogError(
                name + ": escenaMenu está vacío.",
                this
            );

            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(escenaMenu))
        {
            Debug.LogError(
                name +
                ": la escena '" +
                escenaMenu +
                "' no está en Build Settings o el nombre no coincide.",
                this
            );

            return;
        }

        SceneLoader.Load(
            escenaMenu,
            "Volviendo al menú principal..."
        );
    }

    public void SalirDelJuego()
    {
        pausaActivo = false;
        opcionesActivo = false;
        Time.timeScale = 1f;

        Application.Quit();

#if UNITY_EDITOR
        Debug.Log("Application.Quit no cierra el juego dentro del Editor.");
#endif
    }

    public void AbrirOpciones()
    {
        ReproducirSonidoUI(idSonidoSeleccion);

        opcionesActivo = true;

        if (panelMenuPausa != null)
        {
            panelMenuPausa.SetActive(false);
        }

        if (panelOpciones != null)
        {
            panelOpciones.SetActive(true);
        }

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

        if (panelOpciones != null)
        {
            panelOpciones.SetActive(false);
        }

        if (panelMenuPausa != null)
        {
            panelMenuPausa.SetActive(true);
        }

        SeleccionarPrimerBoton();
    }

    private void SeleccionarPrimerBoton()
    {
        if (primerBotonPausa == null && panelMenuPausa != null)
        {
            primerBotonPausa =
                panelMenuPausa.GetComponentInChildren<Selectable>(true);
        }

        if (primerBotonPausa == null || EventSystem.current == null)
        {
            return;
        }

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(
            primerBotonPausa.gameObject
        );

        primerBotonPausa.Select();
    }

    private void SeleccionarControlOpciones()
    {
        if (primerControlOpciones == null && panelOpciones != null)
        {
            primerControlOpciones =
                panelOpciones.GetComponentInChildren<Selectable>(true);
        }

        Seleccionar(primerControlOpciones);
    }

    private void Seleccionar(Selectable seleccionable)
    {
        if (seleccionable == null || EventSystem.current == null)
        {
            return;
        }

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(
            seleccionable.gameObject
        );

        seleccionable.Select();
    }

    private void AsegurarEventSystem()
    {
        EventSystem eventSystem = EventSystem.current;

        if (eventSystem == null)
        {
            GameObject eventSystemGO =
                new GameObject("EventSystem");

            eventSystem =
                eventSystemGO.AddComponent<EventSystem>();
        }

        StandaloneInputModule inputModule =
            eventSystem.GetComponent<StandaloneInputModule>();

        if (inputModule == null)
        {
            inputModule =
                eventSystem.gameObject
                    .AddComponent<StandaloneInputModule>();
        }

        inputModule.horizontalAxis = ejeHorizontal;
        inputModule.verticalAxis = ejeVertical;
        inputModule.submitButton = botonAceptar;
        inputModule.cancelButton = botonCancelar;
    }

    private void ReproducirSonidoUI(string idSonido)
    {
        if (string.IsNullOrWhiteSpace(idSonido))
        {
            return;
        }

        if (AudioManager.Instance == null)
        {
            Debug.LogWarning(
                name + ": no hay AudioManager en la escena.",
                this
            );

            return;
        }

        AudioManager.Instance.ReproducirSFX2D(idSonido);
    }

    private void OnDestroy()
    {
        /*
         * Evita que el juego se quede congelado si el objeto
         * se destruye mientras el menú está abierto.
         */
        if (pausaActivo)
        {
            Time.timeScale = 1f;
        }
    }
}