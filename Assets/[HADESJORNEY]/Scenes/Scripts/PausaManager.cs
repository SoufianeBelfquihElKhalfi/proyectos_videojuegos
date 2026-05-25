using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PausaManager : MonoBehaviour
{
    [Header("Panel de pausa")]
    public GameObject pausa;
    public Selectable primerBotonPausa;

    [Header("Input Manager antiguo")]
    public string botonPausa = "Pausa";
    public string botonCancelar = "Cancel";
    public string botonAceptar = "Submit";
    public string ejeHorizontal = "Horizontal";
    public string ejeVertical = "Vertical";

    [Header("Escena")]
    public string escenaMenu = "MAIN";

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

        AsegurarEventSystem();
    }

    void Update()
    {
        if (BotonPulsado(botonPausa))
        {
            ToggleMenu();
            return;
        }

        if (pausaActivo && BotonPulsado(botonCancelar))
        {
            Reanudar();
            return;
        }

        if (pausaActivo && EventSystem.current != null && EventSystem.current.currentSelectedGameObject == null)
        {
            SeleccionarPrimerBoton();
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
        SeleccionarPrimerBoton();
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
        pausaActivo = false;

        if (pausa != null)
        {
            pausa.SetActive(false);
        }

        Time.timeScale = 1f;

        CerrarPausa();
    }

    public void Menu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(escenaMenu);
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
}