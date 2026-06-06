using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using Dapasa.Audio;

public class MercaderInteractuar : MonoBehaviour
{
    [Header("Aparición de botones")]
    [SerializeField] private CanvasGroup grupoBotones;
    [SerializeField] private float fadeBotonesDialogo = 0.3f;

    [Header("Wiggle del retrato")]
    [SerializeField] private Transform retratoMercader;
    [SerializeField] private float wiggleAngulo = 4f;
    [SerializeField] private float wiggleVelocidad = 18f;
    private Coroutine wiggleRoutine;

    [Header("Aparición del bocadillo")]
    [SerializeField] private CanvasGroup bocadilloCanvasGroup;
    [SerializeField] private float fadeBocadillo = 0.25f;
    [SerializeField] private float escalaInicialBocadillo = 0.85f;
    private Coroutine aparicionRoutine;

    [Header("Typewriter")]
    [SerializeField] private float velocidadTexto = 0.03f;
    private Coroutine typewriterRoutine;
    private bool escribiendo;
    private string textoCompleto;

    [Header("UI")]
    [SerializeField] private GameObject textoInteraccion;
    [SerializeField] private GameObject bocadillo;
    [SerializeField] private TMP_Text textoBocadillo;
    [SerializeField] private string mensajeMercader = "¡Querido Alástor! ¿Qué deseas hacer?";

    [Header("Botones del bocadillo")]
    [SerializeField] private Button botonCurarse;
    [SerializeField] private Button botonComerciar;

    [Header("Curación")]
    [SerializeField] private int curacionMitadCorazones = 2;
    [SerializeField] private string tagJugador = "Player";

    [Header("Audio")]
    [SerializeField] private string idSonidoCurarse = "curarse_mercader";

    [Header("Tienda")]
    [SerializeField] private GameObject tienda;

    [Header("Input Manager antiguo")]
    [SerializeField] private string botonInteractuar = "Submit";
    [SerializeField] private string botonCerrar = "Cancel";
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

    private bool jugadorCerca;
    private bool bocadilloMostrado;
    private bool tiendaAbierta;

    private bool cursorVisibleAnterior;
    private CursorLockMode cursorLockAnterior;

    private Coroutine coroutineSeleccion;
    private readonly HashSet<string> botonesNoEncontrados = new HashSet<string>();

    private SistemaVida sistemaVidaJugador;

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

    private void Update()
    {
        if (tiendaAbierta)
        {
            if (BotonPulsado(botonCerrar)) CerrarTienda();
            return;
        }

        if (bocadilloMostrado)
        {
            if (escribiendo && (BotonPulsado(botonInteractuar) || BotonPulsado(botonClickRaton)))
            {
                CompletarTexto();
                return;
            }

            if (BotonPulsado(botonCerrar)) CerrarBocadillo();
            return;
        }

        if (!jugadorCerca) return;

        if (BotonPulsado(botonInteractuar) || ClickRatonSobreEsteMercader())
            Interactuar();
    }

    private bool BotonPulsado(string nombreBoton)
    {
        if (string.IsNullOrWhiteSpace(nombreBoton)) return false;
        if (botonesNoEncontrados.Contains(nombreBoton)) return false;

        try
        {
            return Input.GetButtonDown(nombreBoton);
        }
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

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(tagJugador)) return;

        jugadorCerca = true;

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

    private void Interactuar()
    {
        MostrarBocadillo();
    }

    private void MostrarBocadillo()
    {
        bocadilloMostrado = true;

        if (textoInteraccion != null) textoInteraccion.SetActive(false);
        if (bocadillo != null) bocadillo.SetActive(true);

        if (grupoBotones != null)
        {
            grupoBotones.alpha = 0f;
            grupoBotones.interactable = false;
            grupoBotones.blocksRaycasts = false;
        }

        if (bocadilloCanvasGroup != null)
        {
            if (aparicionRoutine != null) StopCoroutine(aparicionRoutine);
            aparicionRoutine = StartCoroutine(AparecerBocadillo());
        }

        if (textoBocadillo != null)
            IniciarTypewriter(mensajeMercader);
    }

    private void CerrarBocadillo(bool mostrarTextoInteraccion = true)
    {
        bocadilloMostrado = false;

        if (bocadillo != null)
            bocadillo.SetActive(false);

        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);

        if (mostrarTextoInteraccion && jugadorCerca && textoInteraccion != null)
            textoInteraccion.SetActive(true);
    }

    private void OnClickCurarse()
    {
        CerrarBocadillo(mostrarTextoInteraccion: false);

        if (sistemaVidaJugador != null)
        {
            sistemaVidaJugador.Curar(curacionMitadCorazones);
            ReproducirSonidoCurarse();
        }
        else
        {
            Debug.LogWarning("SistemaVida no cacheado. ¿El jugador entró en el trigger correctamente?", this);
        }

        if (jugadorCerca && textoInteraccion != null)
            textoInteraccion.SetActive(true);
    }

    private void ReproducirSonidoCurarse()
    {
        if (AudioManager.Instance == null)
        {
            Debug.LogWarning("No hay AudioManager en la escena.");
            return;
        }

        AudioManager.Instance.ReproducirSFX2D(idSonidoCurarse);
    }

    private void OnClickComerciar()
    {
        CerrarBocadillo(mostrarTextoInteraccion: false);
        AbrirTienda();
    }

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

    private void IniciarTypewriter(string texto)
    {
        textoCompleto = texto;

        if (typewriterRoutine != null)
            StopCoroutine(typewriterRoutine);

        typewriterRoutine = StartCoroutine(Typewriter());
    }

    private IEnumerator Typewriter()
    {
        escribiendo = true;
        textoBocadillo.text = "";

        if (retratoMercader != null)
        {
            if (wiggleRoutine != null) StopCoroutine(wiggleRoutine);
            wiggleRoutine = StartCoroutine(WiggleRetrato());
        }

        foreach (char c in textoCompleto)
        {
            textoBocadillo.text += c;
            yield return new WaitForSeconds(velocidadTexto);
        }

        escribiendo = false;
        typewriterRoutine = null;

        PararWiggle();
        MostrarBotones();
    }

    private void CompletarTexto()
    {
        if (typewriterRoutine != null)
            StopCoroutine(typewriterRoutine);

        textoBocadillo.text = textoCompleto;
        escribiendo = false;
        typewriterRoutine = null;

        PararWiggle();
        MostrarBotones();
    }

    private IEnumerator AparecerBocadillo()
    {
        Transform tr = bocadilloCanvasGroup.transform;
        Vector3 baseScale = Vector3.one;
        Vector3 desde = baseScale * escalaInicialBocadillo;

        bocadilloCanvasGroup.alpha = 0f;
        tr.localScale = desde;

        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / fadeBocadillo;
            float e = Mathf.Clamp01(t);

            bocadilloCanvasGroup.alpha = e;
            tr.localScale = Vector3.LerpUnclamped(desde, baseScale, 1f - Mathf.Pow(1f - e, 3f));

            yield return null;
        }

        bocadilloCanvasGroup.alpha = 1f;
        tr.localScale = baseScale;
    }

    private IEnumerator WiggleRetrato()
    {
        float t = 0f;

        while (true)
        {
            t += Time.deltaTime * wiggleVelocidad;
            float angulo = Mathf.Sin(t) * wiggleAngulo;

            retratoMercader.localRotation = Quaternion.Euler(0f, 0f, angulo);

            yield return null;
        }
    }

    private void PararWiggle()
    {
        if (wiggleRoutine != null)
            StopCoroutine(wiggleRoutine);

        wiggleRoutine = null;

        if (retratoMercader != null)
            retratoMercader.localRotation = Quaternion.identity;
    }

    private void MostrarBotones()
    {
        if (grupoBotones != null)
            StartCoroutine(FadeBotones());
    }

    private IEnumerator FadeBotones()
    {
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / fadeBotonesDialogo;
            grupoBotones.alpha = Mathf.Clamp01(t);
            yield return null;
        }

        grupoBotones.alpha = 1f;
        grupoBotones.interactable = true;
        grupoBotones.blocksRaycasts = true;
    }
}