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
    [SerializeField] private string mensajeMercader = "¡Querido Alástor! ¿Qué deseas hacer?";

    [Header("Botones del bocadillo")]
    [Tooltip("Botón para curarse (dentro del bocadillo).")]
    [SerializeField] private Button botonCurarse;
    [Tooltip("Botón para abrir la tienda (dentro del bocadillo).")]
    [SerializeField] private Button botonComerciar;

    [Header("Curación")]
    [Tooltip("Cantidad de mitades de corazón que recupera al curarse.")]
    [SerializeField] private int curacionMitadCorazones = 2;
    [Tooltip("Tag del jugador para encontrar su SistemaVida.")]
    [SerializeField] private string tagJugador = "Player";

    [Header("Tienda")]
    [SerializeField] private GameObject tienda;

    [Header("Input Manager antiguo")]
    [Tooltip("Botón del Input Manager para hablar, aceptar o avanzar.")]
    [SerializeField] private string botonInteractuar = "Submit";
    [Tooltip("Botón del Input Manager para cerrar o cancelar.")]
    [SerializeField] private string botonCerrar = "Cancel";
    [Tooltip("Botón del Input Manager para clic/selección con ratón.")]
    [SerializeField] private string botonClickRaton = "Fire1";

    [Header("Ratón")]
    [SerializeField] private bool permitirClickSobreMercader = true;
    [SerializeField] private Camera camaraRaycast;

    [Header("Navegación UI")]
    [SerializeField] private Selectable primerElementoSeleccionado;
    [SerializeField] private bool seleccionarPrimerElementoAlAbrir = true;
    [SerializeField] private bool asegurarEventSystem = true;
    [SerializeField] private bool configurarStandaloneInputModule = true;
    [SerializeField] private bool controlarCursorEnTienda = true;
    [SerializeField] private string ejeUIHorizontal = "Horizontal";
    [SerializeField] private string ejeUIVertical = "Vertical";

    // ─────────────────────────────────────────
    //  Estado privado
    // ─────────────────────────────────────────

    private bool jugadorCerca;
    private bool bocadilloMostrado;
    private bool tiendaAbierta;

    private bool cursorVisibleAnterior;
    private CursorLockMode cursorLockAnterior;

    private Coroutine coroutineSeleccion;
    private readonly HashSet<string> botonesNoEncontrados = new HashSet<string>();

    // Referencia cacheada al SistemaVida del jugador
    private SistemaVida sistemaVidaJugador;

    // ─────────────────────────────────────────
    //  Inicialización
    // ─────────────────────────────────────────

    private void Start()
    {
        if (textoInteraccion != null) textoInteraccion.SetActive(false);
        if (bocadillo != null) bocadillo.SetActive(false);
        if (tienda != null) tienda.SetActive(false);

        if (camaraRaycast == null)
            camaraRaycast = Camera.main;

        if (botonCurarse != null) botonCurarse.onClick.AddListener(OnClickCurarse);
        if (botonComerciar != null) botonComerciar.onClick.AddListener(OnClickComerciar);
    }

    private void OnDestroy()
    {
        if (botonCurarse != null) botonCurarse.onClick.RemoveListener(OnClickCurarse);
        if (botonComerciar != null) botonComerciar.onClick.RemoveListener(OnClickComerciar);
    }

    // ─────────────────────────────────────────
    //  Update
    // ─────────────────────────────────────────

    private void Update()
    {
        if (tiendaAbierta)
        {
            if (BotonPulsado(botonCerrar)) CerrarTienda();
            return;
        }

        if (bocadilloMostrado)
        {
            if (BotonPulsado(botonCerrar)) CerrarBocadillo();
            return;
        }

        if (!jugadorCerca) return;

        if (BotonPulsado(botonInteractuar) || ClickRatonSobreEsteMercader())
            Interactuar();
    }

    // ─────────────────────────────────────────
    //  Input helpers
    // ─────────────────────────────────────────

    private bool BotonPulsado(string nombreBoton)
    {
        if (string.IsNullOrWhiteSpace(nombreBoton)) return false;
        if (botonesNoEncontrados.Contains(nombreBoton)) return false;

        try { return Input.GetButtonDown(nombreBoton); }
        catch (ArgumentException)
        {
            botonesNoEncontrados.Add(nombreBoton);
            Debug.LogWarning("El botón '" + nombreBoton + "' no existe en Project Settings > Input Manager.", this);
            return false;
        }
    }

    private bool ClickRatonSobreEsteMercader()
    {
        if (!permitirClickSobreMercader) return false;
        if (!BotonPulsado(botonClickRaton)) return false;
        if (camaraRaycast == null) return false;

        Ray ray = camaraRaycast.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            return hit.collider != null &&
                   (hit.collider.gameObject == gameObject ||
                    hit.collider.transform.IsChildOf(transform));
        }
        return false;
    }

    // ─────────────────────────────────────────
    //  Trigger
    // ─────────────────────────────────────────

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(tagJugador)) return;

        jugadorCerca = true;

        // Cachear SistemaVida subiendo desde el collider que entró
        // (funciona aunque el collider sea un hijo de Alastor)
        if (sistemaVidaJugador == null)
        {
            sistemaVidaJugador = other.GetComponentInParent<SistemaVida>();
            if (sistemaVidaJugador == null)
                sistemaVidaJugador = other.GetComponentInChildren<SistemaVida>();
        }

        if (!tiendaAbierta && !bocadilloMostrado && textoInteraccion != null)
            textoInteraccion.SetActive(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(tagJugador)) return;

        jugadorCerca = false;
        CerrarBocadillo(mostrarTextoInteraccion: false);

        if (textoInteraccion != null)
            textoInteraccion.SetActive(false);
    }

    // ─────────────────────────────────────────
    //  Flujo principal
    // ─────────────────────────────────────────

    private void Interactuar() => MostrarBocadillo();

    private void MostrarBocadillo()
    {
        bocadilloMostrado = true;

        if (textoInteraccion != null) textoInteraccion.SetActive(false);
        if (textoBocadillo != null) textoBocadillo.text = mensajeMercader;
        if (bocadillo != null) bocadillo.SetActive(true);
    }

    private void CerrarBocadillo(bool mostrarTextoInteraccion = true)
    {
        bocadilloMostrado = false;

        if (bocadillo != null) bocadillo.SetActive(false);

        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);

        if (mostrarTextoInteraccion && jugadorCerca && textoInteraccion != null)
            textoInteraccion.SetActive(true);
    }

    // ─────────────────────────────────────────
    //  Callbacks botones del bocadillo
    // ─────────────────────────────────────────

    private void OnClickCurarse()
    {
        CerrarBocadillo(mostrarTextoInteraccion: false);

        if (sistemaVidaJugador != null)
        {
            sistemaVidaJugador.Curar(curacionMitadCorazones);
        }
        else
        {
            Debug.LogWarning("SistemaVida no cacheado. ¿El jugador entró en el trigger correctamente?", this);
        }

        if (jugadorCerca && textoInteraccion != null)
            textoInteraccion.SetActive(true);
    }

    private void OnClickComerciar()
    {
        CerrarBocadillo(mostrarTextoInteraccion: false);
        AbrirTienda();
    }

    // ─────────────────────────────────────────
    //  Tienda
    // ─────────────────────────────────────────

    private void AbrirTienda()
    {
        if (tiendaAbierta || tienda == null) return;

        tiendaAbierta = true;

        GuardarYMostrarCursor();

        if (textoInteraccion != null) textoInteraccion.SetActive(false);
        if (bocadillo != null) bocadillo.SetActive(false);

        tienda.SetActive(true);
        PrepararNavegacionUI();
    }

    public void CerrarTienda()
    {
        tiendaAbierta = false;
        bocadilloMostrado = false;

        if (tienda != null) tienda.SetActive(false);
        if (bocadillo != null) bocadillo.SetActive(false);

        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);

        RestaurarCursor();

        if (jugadorCerca && textoInteraccion != null)
            textoInteraccion.SetActive(true);
    }

    // ─────────────────────────────────────────
    //  Cursor
    // ─────────────────────────────────────────

    private void GuardarYMostrarCursor()
    {
        if (!controlarCursorEnTienda) return;

        cursorVisibleAnterior = Cursor.visible;
        cursorLockAnterior = Cursor.lockState;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void RestaurarCursor()
    {
        if (!controlarCursorEnTienda) return;

        Cursor.visible = cursorVisibleAnterior;
        Cursor.lockState = cursorLockAnterior;
    }

    // ─────────────────────────────────────────
    //  Navegación UI
    // ─────────────────────────────────────────

    private void PrepararNavegacionUI()
    {
        AsegurarEventSystemYModuloInput();

        if (!seleccionarPrimerElementoAlAbrir) return;

        if (primerElementoSeleccionado == null && tienda != null)
            primerElementoSeleccionado = tienda.GetComponentInChildren<Selectable>(true);

        if (primerElementoSeleccionado == null) return;

        if (coroutineSeleccion != null) StopCoroutine(coroutineSeleccion);
        coroutineSeleccion = StartCoroutine(SeleccionarElementoCuandoEsteActivo(primerElementoSeleccionado));
    }

    private IEnumerator SeleccionarElementoCuandoEsteActivo(Selectable elemento)
    {
        yield return null;

        int intentos = 0;
        while (elemento != null && !elemento.gameObject.activeInHierarchy && intentos < 10)
        {
            yield return null;
            intentos++;
        }

        if (elemento != null && elemento.gameObject.activeInHierarchy && EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(elemento.gameObject);

        coroutineSeleccion = null;
    }

    private void AsegurarEventSystemYModuloInput()
    {
        if (!asegurarEventSystem) return;

        if (EventSystem.current == null)
        {
            var go = new GameObject("EventSystem");
            go.AddComponent<EventSystem>();
            go.AddComponent<StandaloneInputModule>();
            Debug.Log("MercaderInteractuar: EventSystem creado automáticamente.", this);
            return;
        }

        if (!configurarStandaloneInputModule) return;

        var modulo = EventSystem.current.GetComponent<StandaloneInputModule>();
        if (modulo == null)
        {
            EventSystem.current.gameObject.AddComponent<StandaloneInputModule>();
        }
        else
        {
            if (!string.IsNullOrWhiteSpace(ejeUIHorizontal)) modulo.horizontalAxis = ejeUIHorizontal;
            if (!string.IsNullOrWhiteSpace(ejeUIVertical)) modulo.verticalAxis = ejeUIVertical;
            if (!string.IsNullOrWhiteSpace(botonInteractuar)) modulo.submitButton = botonInteractuar;
            if (!string.IsNullOrWhiteSpace(botonCerrar)) modulo.cancelButton = botonCerrar;
        }
    }
}