using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class MercaderInteractuar : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject textoInteraccion;
    [SerializeField] private GameObject bocadillo;
    [SerializeField] private TMP_Text textoBocadillo;
    [SerializeField] private string mensajeMercader = "¡Querido Alástor! ¿Qué deseas comprar?";

    [Header("Tienda")]
    [SerializeField] private GameObject tienda;

    [Header("Input Manager antiguo")]
    [Tooltip("Botón del Input Manager para hablar, aceptar o avanzar.")]
    [SerializeField] private string botonInteractuar = "Submit";

    [Tooltip("Botón del Input Manager para cerrar o cancelar.")]
    [SerializeField] private string botonCerrar = "Cancel";

    [Tooltip("Botón del Input Manager para clic/selección con ratón.")]
    [SerializeField] private string botonClickRaton = "Fire1";

    [Tooltip("Eje horizontal del Input Manager para navegar UI.")]
    [SerializeField] private string ejeUIHorizontal = "Horizontal";

    [Tooltip("Eje vertical del Input Manager para navegar UI.")]
    [SerializeField] private string ejeUIVertical = "Vertical";

    [Header("Ratón")]
    [SerializeField] private bool permitirClickSobreMercader = true;
    [SerializeField] private Camera camaraRaycast;

    [Header("Navegación UI")]
    [SerializeField] private Selectable primerElementoSeleccionado;
    [SerializeField] private bool seleccionarPrimerElementoAlAbrir = true;
    [SerializeField] private bool asegurarEventSystem = true;
    [SerializeField] private bool configurarStandaloneInputModule = true;
    [SerializeField] private bool controlarCursorEnTienda = true;

    private bool jugadorCerca;
    private bool bocadilloMostrado;
    private bool tiendaAbierta;

    private bool cursorVisibleAnterior;
    private CursorLockMode cursorLockAnterior;

    private Coroutine coroutineSeleccion;
    private readonly HashSet<string> botonesNoEncontrados = new HashSet<string>();

    private void Start()
    {
        if (textoInteraccion != null)
            textoInteraccion.SetActive(false);

        if (bocadillo != null)
            bocadillo.SetActive(false);

        if (tienda != null)
            tienda.SetActive(false);

        if (camaraRaycast == null)
            camaraRaycast = Camera.main;
    }

    private void Update()
    {
        if (tiendaAbierta)
        {
            if (BotonPulsado(botonCerrar))
            {
                CerrarTienda();
            }

            return;
        }

        if (!jugadorCerca)
            return;

        if (BotonPulsado(botonInteractuar) || ClickRatonSobreEsteMercader())
        {
            Interactuar();
        }
    }

    private bool BotonPulsado(string nombreBoton)
    {
        if (string.IsNullOrWhiteSpace(nombreBoton))
            return false;

        if (botonesNoEncontrados.Contains(nombreBoton))
            return false;

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

    private bool ClickRatonSobreEsteMercader()
    {
        if (!permitirClickSobreMercader)
            return false;

        if (!BotonPulsado(botonClickRaton))
            return false;

        if (camaraRaycast == null)
            return false;

        Ray ray = camaraRaycast.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            return hit.collider != null &&
                   (hit.collider.gameObject == gameObject ||
                    hit.collider.transform.IsChildOf(transform));
        }

        return false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        jugadorCerca = true;

        if (!tiendaAbierta && !bocadilloMostrado && textoInteraccion != null)
        {
            textoInteraccion.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        jugadorCerca = false;
        bocadilloMostrado = false;

        if (textoInteraccion != null)
            textoInteraccion.SetActive(false);

        if (bocadillo != null)
            bocadillo.SetActive(false);
    }

    private void Interactuar()
    {
        if (!bocadilloMostrado)
        {
            MostrarBocadillo();
        }
        else
        {
            AbrirTienda();
        }
    }

    private void MostrarBocadillo()
    {
        bocadilloMostrado = true;

        if (textoInteraccion != null)
            textoInteraccion.SetActive(false);

        if (textoBocadillo != null)
            textoBocadillo.text = mensajeMercader;

        if (bocadillo != null)
            bocadillo.SetActive(true);
    }

    private void AbrirTienda()
    {
        if (tiendaAbierta || tienda == null)
            return;

        tiendaAbierta = true;

        GuardarYMostrarCursor();

        if (textoInteraccion != null)
            textoInteraccion.SetActive(false);

        if (bocadillo != null)
            bocadillo.SetActive(false);

        tienda.SetActive(true);

        PrepararNavegacionUI();
    }

    public void CerrarTienda()
    {
        tiendaAbierta = false;
        bocadilloMostrado = false;

        if (tienda != null)
            tienda.SetActive(false);

        if (bocadillo != null)
            bocadillo.SetActive(false);

        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);

        RestaurarCursor();

        if (jugadorCerca && textoInteraccion != null)
            textoInteraccion.SetActive(true);
    }

    private void PrepararNavegacionUI()
    {
        AsegurarEventSystemYModuloInput();

        if (!seleccionarPrimerElementoAlAbrir)
            return;

        if (primerElementoSeleccionado == null && tienda != null)
        {
            primerElementoSeleccionado = tienda.GetComponentInChildren<Selectable>(true);
        }

        if (primerElementoSeleccionado == null)
            return;

        if (coroutineSeleccion != null)
            StopCoroutine(coroutineSeleccion);

        coroutineSeleccion = StartCoroutine(SeleccionarElementoCuandoEsteActivo(primerElementoSeleccionado));
    }

    private IEnumerator SeleccionarElementoCuandoEsteActivo(Selectable elemento)
    {
        yield return null;

        if (EventSystem.current == null)
            yield break;

        if (elemento == null)
            yield break;

        if (!elemento.gameObject.activeInHierarchy)
            yield break;

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(elemento.gameObject);
        elemento.Select();
    }

    private void AsegurarEventSystemYModuloInput()
    {
        if (!asegurarEventSystem && EventSystem.current == null)
            return;

        EventSystem eventSystem = EventSystem.current;

        if (eventSystem == null)
        {
            GameObject eventSystemGO = new GameObject("EventSystem");
            eventSystem = eventSystemGO.AddComponent<EventSystem>();
        }

        StandaloneInputModule modulo = eventSystem.GetComponent<StandaloneInputModule>();

        if (modulo == null)
            modulo = eventSystem.gameObject.AddComponent<StandaloneInputModule>();

        if (configurarStandaloneInputModule)
        {
            modulo.horizontalAxis = ejeUIHorizontal;
            modulo.verticalAxis = ejeUIVertical;
            modulo.submitButton = botonInteractuar;
            modulo.cancelButton = botonCerrar;
        }
    }

    private void GuardarYMostrarCursor()
    {
        if (!controlarCursorEnTienda)
            return;

        cursorVisibleAnterior = Cursor.visible;
        cursorLockAnterior = Cursor.lockState;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void RestaurarCursor()
    {
        if (!controlarCursorEnTienda)
            return;

        Cursor.visible = cursorVisibleAnterior;
        Cursor.lockState = cursorLockAnterior;
    }
}