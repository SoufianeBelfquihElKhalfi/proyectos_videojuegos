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

    [Header("Aparición del bocadillo")]
    [SerializeField] private CanvasGroup bocadilloCanvasGroup;
    [SerializeField] private float fadeBocadillo = 0.25f;
    [SerializeField] private float escalaInicialBocadillo = 0.85f;
    [SerializeField] private float tiempoVisibleRespuesta = 2f;

    [Header("Typewriter")]
    [SerializeField] private float velocidadTexto = 0.03f;

    [Header("UI")]
    [SerializeField] private GameObject textoInteraccion;
    [SerializeField] private GameObject bocadillo;
    [SerializeField] private TMP_Text textoBocadillo;

    [Header("Diálogos iniciales")]
    [TextArea]
    [SerializeField]
    private string mensajeMercader =
        "¿Qué deseas, querido Alastor?";

    [TextArea]
    [SerializeField]
    private string mensajeMercaderSegundo =
        "¿Estás herido o prefieres comprar algo?";

    [Header("Diálogos de respuesta")]
    [TextArea]
    [SerializeField]
    private string mensajeCuracion =
        "Ten más cuidado la próxima.";

    [TextArea]
    [SerializeField]
    private string mensajeSinCompra =
        "¿Te marchas sin llevarte nada?";

    [Header("Botones del bocadillo")]
    [SerializeField] private Button botonCurarse;
    [SerializeField] private Button botonComerciar;

    [Header("Curación")]
    [SerializeField] private int curacionMitadCorazones = 2;
    [SerializeField] private string tagJugador = "Player";

    [Header("Audio")]
    [SerializeField]
    private string idSonidoHablarPrimero =
        "mercaderhablar1";

    [SerializeField]
    private string idSonidoHablarSegundo =
        "mercaderhablar2";

    [SerializeField]
    private string idSonidoCurarse =
        "curar";

    [SerializeField]
    private string idSonidoSalirSinComprar =
        "salir_sin_comprar";

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
    private bool mostrarBotonesAlFinal;
    private bool esperandoSegundoDialogoInicial;
    private bool compraRealizadaEnSesion;

    private bool cursorVisibleAnterior;
    private CursorLockMode cursorLockAnterior;

    private bool escribiendo;
    private string textoCompleto;

    private Coroutine wiggleRoutine;
    private Coroutine aparicionRoutine;
    private Coroutine typewriterRoutine;
    private Coroutine cierreRespuestaRoutine;
    private Coroutine coroutineSeleccion;
    private Coroutine fadeBotonesRoutine;

    private readonly HashSet<string> botonesNoEncontrados =
        new HashSet<string>();

    private SistemaVida sistemaVidaJugador;

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

        if (botonCurarse != null)
            botonCurarse.onClick.AddListener(OnClickCurarse);

        if (botonComerciar != null)
            botonComerciar.onClick.AddListener(OnClickComerciar);
    }

    private void OnDestroy()
    {
        if (botonCurarse != null)
            botonCurarse.onClick.RemoveListener(OnClickCurarse);

        if (botonComerciar != null)
            botonComerciar.onClick.RemoveListener(OnClickComerciar);
    }

    private void Update()
    {
        if (tiendaAbierta)
        {
            if (BotonPulsado(botonCerrar))
                CerrarTienda();

            return;
        }

        if (bocadilloMostrado)
        {
            bool avanzarDialogo =
                BotonPulsado(botonInteractuar) ||
                BotonPulsado(botonClickRaton);

            if (escribiendo && avanzarDialogo)
            {
                CompletarTexto();
                return;
            }

            if (
                !escribiendo &&
                esperandoSegundoDialogoInicial &&
                avanzarDialogo
            )
            {
                MostrarSegundoDialogoInicial();
                return;
            }

            if (BotonPulsado(botonCerrar))
                CerrarBocadillo();

            return;
        }

        if (!jugadorCerca)
            return;

        if (
            BotonPulsado(botonInteractuar) ||
            ClickRatonSobreEsteMercader()
        )
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
                "El botón '" + nombreBoton +
                "' no existe en Project Settings > Input Manager.",
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

        Ray ray =
            camaraRaycast.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            return hit.collider != null &&
                   (
                       hit.collider.gameObject == gameObject ||
                       hit.collider.transform.IsChildOf(transform)
                   );
        }

        return false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(tagJugador))
            return;

        jugadorCerca = true;

        if (sistemaVidaJugador == null)
        {
            sistemaVidaJugador =
                other.GetComponentInParent<SistemaVida>();

            if (sistemaVidaJugador == null)
            {
                sistemaVidaJugador =
                    other.GetComponentInChildren<SistemaVida>();
            }
        }

        if (
            !tiendaAbierta &&
            !bocadilloMostrado &&
            textoInteraccion != null
        )
        {
            textoInteraccion.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(tagJugador))
            return;

        jugadorCerca = false;

        CerrarBocadillo(
            mostrarTextoInteraccion: false
        );

        if (textoInteraccion != null)
            textoInteraccion.SetActive(false);
    }

    private void Interactuar()
    {
        MostrarDialogoPrincipal();
    }

    // --------------------------------------------------
    // DIÁLOGOS INICIALES
    // --------------------------------------------------

    private void MostrarDialogoPrincipal()
    {
        esperandoSegundoDialogoInicial = true;

        MostrarDialogo(
            mensajeMercader,
            idSonidoHablarPrimero,
            mostrarBotonesDespues: false
        );
    }

    private void MostrarSegundoDialogoInicial()
    {
        esperandoSegundoDialogoInicial = false;

        MostrarDialogo(
            mensajeMercaderSegundo,
            idSonidoHablarSegundo,
            mostrarBotonesDespues: true
        );
    }

    // --------------------------------------------------
    // DIÁLOGOS DE RESPUESTA
    // --------------------------------------------------

    private void MostrarDialogoRespuesta(
        string mensaje,
        string idSonido
    )
    {
        esperandoSegundoDialogoInicial = false;

        MostrarDialogo(
            mensaje,
            idSonido,
            mostrarBotonesDespues: false
        );
    }

    private void MostrarDialogo(
        string mensaje,
        string idSonido,
        bool mostrarBotonesDespues
    )
    {
        DetenerRutinasDialogo();

        bocadilloMostrado = true;
        mostrarBotonesAlFinal = mostrarBotonesDespues;

        if (textoInteraccion != null)
            textoInteraccion.SetActive(false);

        if (bocadillo != null)
            bocadillo.SetActive(true);

        OcultarBotones();
        ReproducirSonido(idSonido);

        if (bocadilloCanvasGroup != null)
        {
            aparicionRoutine =
                StartCoroutine(AparecerBocadillo());
        }

        if (textoBocadillo != null)
            IniciarTypewriter(mensaje);
    }

    private void OcultarBotones()
    {
        if (fadeBotonesRoutine != null)
        {
            StopCoroutine(fadeBotonesRoutine);
            fadeBotonesRoutine = null;
        }

        if (grupoBotones == null)
            return;

        grupoBotones.alpha = 0f;
        grupoBotones.interactable = false;
        grupoBotones.blocksRaycasts = false;
    }

    private void DetenerRutinasDialogo()
    {
        if (typewriterRoutine != null)
        {
            StopCoroutine(typewriterRoutine);
            typewriterRoutine = null;
        }

        if (aparicionRoutine != null)
        {
            StopCoroutine(aparicionRoutine);
            aparicionRoutine = null;
        }

        if (cierreRespuestaRoutine != null)
        {
            StopCoroutine(cierreRespuestaRoutine);
            cierreRespuestaRoutine = null;
        }

        PararWiggle();
    }

    private void CerrarBocadillo(
        bool mostrarTextoInteraccion = true
    )
    {
        DetenerRutinasDialogo();

        escribiendo = false;
        bocadilloMostrado = false;
        mostrarBotonesAlFinal = false;
        esperandoSegundoDialogoInicial = false;

        if (bocadillo != null)
            bocadillo.SetActive(false);

        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);

        if (
            mostrarTextoInteraccion &&
            jugadorCerca &&
            !tiendaAbierta &&
            textoInteraccion != null
        )
        {
            textoInteraccion.SetActive(true);
        }
    }

    // --------------------------------------------------
    // BOTONES DEL MERCADER
    // --------------------------------------------------

    private void OnClickCurarse()
    {
        CerrarBocadillo(
            mostrarTextoInteraccion: false
        );

        if (sistemaVidaJugador != null)
        {
            sistemaVidaJugador.Curar(
                curacionMitadCorazones
            );

            MostrarDialogoRespuesta(
                mensajeCuracion,
                idSonidoCurarse
            );
        }
        else
        {
            Debug.LogWarning(
                "SistemaVida no cacheado. " +
                "¿El jugador entró en el trigger correctamente?",
                this
            );
        }
    }

    private void OnClickComerciar()
    {
        CerrarBocadillo(
            mostrarTextoInteraccion: false
        );

        AbrirTienda();
    }

    // --------------------------------------------------
    // AVISO DESDE MEJORATIENDA
    // --------------------------------------------------

    public void NotificarCompraRealizada()
    {
        /*
         * MejoraTienda ya se encarga del audio y de la
         * lógica propia de la compra.
         *
         * Aquí solo se registra que el jugador compró
         * algo para no mostrar el diálogo de
         * "salir sin comprar" al cerrar la tienda.
         */
        compraRealizadaEnSesion = true;
    }

    // Se conserva por si ya estaba conectado
    // desde algún botón del Inspector.
    public void ReproducirSonidoNoCompra()
    {
        ReproducirSonido(
            idSonidoSalirSinComprar
        );
    }

    // --------------------------------------------------
    // AUDIO
    // --------------------------------------------------

    private void ReproducirSonido(string idSonido)
    {
        if (string.IsNullOrWhiteSpace(idSonido))
            return;

        if (AudioManager.Instance == null)
        {
            Debug.LogWarning(
                "No hay AudioManager en la escena para reproducir: " +
                idSonido,
                this
            );

            return;
        }

        AudioManager.Instance.ReproducirSFX2D(
            idSonido
        );
    }

    // --------------------------------------------------
    // TIENDA
    // --------------------------------------------------

    private void AbrirTienda()
    {
        if (tiendaAbierta || tienda == null)
            return;

        tiendaAbierta = true;
        compraRealizadaEnSesion = false;

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
        if (!tiendaAbierta)
            return;

        bool debeMostrarDialogoSinCompra =
            !compraRealizadaEnSesion;

        tiendaAbierta = false;
        bocadilloMostrado = false;

        if (tienda != null)
            tienda.SetActive(false);

        if (bocadillo != null)
            bocadillo.SetActive(false);

        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);

        RestaurarCursor();

        if (debeMostrarDialogoSinCompra)
        {
            MostrarDialogoRespuesta(
                mensajeSinCompra,
                idSonidoSalirSinComprar
            );
        }
        else if (
            jugadorCerca &&
            textoInteraccion != null
        )
        {
            textoInteraccion.SetActive(true);
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

    // --------------------------------------------------
    // NAVEGACIÓN UI
    // --------------------------------------------------

    private void PrepararNavegacionUI()
    {
        AsegurarEventSystemYModuloInput();

        if (!seleccionarPrimerElementoAlAbrir)
            return;

        if (
            primerElementoSeleccionado == null &&
            tienda != null
        )
        {
            primerElementoSeleccionado =
                tienda.GetComponentInChildren<Selectable>(true);
        }

        if (primerElementoSeleccionado == null)
            return;

        if (coroutineSeleccion != null)
            StopCoroutine(coroutineSeleccion);

        coroutineSeleccion = StartCoroutine(
            SeleccionarElementoCuandoEsteActivo(
                primerElementoSeleccionado
            )
        );
    }

    private IEnumerator SeleccionarElementoCuandoEsteActivo(
        Selectable elemento
    )
    {
        yield return null;

        int intentos = 0;

        while (
            elemento != null &&
            !elemento.gameObject.activeInHierarchy &&
            intentos < 10
        )
        {
            yield return null;
            intentos++;
        }

        if (
            elemento != null &&
            elemento.gameObject.activeInHierarchy &&
            EventSystem.current != null
        )
        {
            EventSystem.current.SetSelectedGameObject(
                elemento.gameObject
            );
        }

        coroutineSeleccion = null;
    }

    private void AsegurarEventSystemYModuloInput()
    {
        if (!asegurarEventSystem)
            return;

        if (EventSystem.current == null)
        {
            GameObject go =
                new GameObject("EventSystem");

            go.AddComponent<EventSystem>();
            go.AddComponent<StandaloneInputModule>();

            Debug.Log(
                "MercaderInteractuar: " +
                "EventSystem creado automáticamente.",
                this
            );

            return;
        }

        if (!configurarStandaloneInputModule)
            return;

        StandaloneInputModule modulo =
            EventSystem.current.GetComponent<StandaloneInputModule>();

        if (modulo == null)
        {
            EventSystem.current.gameObject
                .AddComponent<StandaloneInputModule>();
        }
        else
        {
            if (!string.IsNullOrWhiteSpace(ejeUIHorizontal))
                modulo.horizontalAxis = ejeUIHorizontal;

            if (!string.IsNullOrWhiteSpace(ejeUIVertical))
                modulo.verticalAxis = ejeUIVertical;

            if (!string.IsNullOrWhiteSpace(botonInteractuar))
                modulo.submitButton = botonInteractuar;

            if (!string.IsNullOrWhiteSpace(botonCerrar))
                modulo.cancelButton = botonCerrar;
        }
    }

    // --------------------------------------------------
    // TYPEWRITER
    // --------------------------------------------------

    private void IniciarTypewriter(string texto)
    {
        textoCompleto = texto ?? string.Empty;

        if (typewriterRoutine != null)
            StopCoroutine(typewriterRoutine);

        typewriterRoutine =
            StartCoroutine(Typewriter());
    }

    private IEnumerator Typewriter()
    {
        escribiendo = true;

        if (textoBocadillo != null)
            textoBocadillo.text = string.Empty;

        if (retratoMercader != null)
        {
            if (wiggleRoutine != null)
                StopCoroutine(wiggleRoutine);

            wiggleRoutine =
                StartCoroutine(WiggleRetrato());
        }

        foreach (char c in textoCompleto)
        {
            if (textoBocadillo != null)
                textoBocadillo.text += c;

            yield return new WaitForSeconds(
                velocidadTexto
            );
        }

        escribiendo = false;
        typewriterRoutine = null;

        PararWiggle();
        AlTerminarTexto();
    }

    private void CompletarTexto()
    {
        if (typewriterRoutine != null)
            StopCoroutine(typewriterRoutine);

        if (textoBocadillo != null)
            textoBocadillo.text = textoCompleto;

        escribiendo = false;
        typewriterRoutine = null;

        PararWiggle();
        AlTerminarTexto();
    }

    private void AlTerminarTexto()
    {
        if (esperandoSegundoDialogoInicial)
            return;

        if (mostrarBotonesAlFinal)
        {
            MostrarBotones();
            return;
        }

        if (cierreRespuestaRoutine != null)
            StopCoroutine(cierreRespuestaRoutine);

        cierreRespuestaRoutine =
            StartCoroutine(CerrarRespuestaTrasEspera());
    }

    private IEnumerator CerrarRespuestaTrasEspera()
    {
        yield return new WaitForSecondsRealtime(
            tiempoVisibleRespuesta
        );

        cierreRespuestaRoutine = null;

        CerrarBocadillo(
            mostrarTextoInteraccion: !tiendaAbierta
        );
    }

    // --------------------------------------------------
    // ANIMACIONES DEL BOCADILLO
    // --------------------------------------------------

    private IEnumerator AparecerBocadillo()
    {
        if (bocadilloCanvasGroup == null)
            yield break;

        Transform tr =
            bocadilloCanvasGroup.transform;

        Vector3 escalaBase = Vector3.one;

        Vector3 desde =
            escalaBase * escalaInicialBocadillo;

        bocadilloCanvasGroup.alpha = 0f;
        tr.localScale = desde;

        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / fadeBocadillo;

            float e = Mathf.Clamp01(t);

            bocadilloCanvasGroup.alpha = e;

            tr.localScale = Vector3.LerpUnclamped(
                desde,
                escalaBase,
                1f - Mathf.Pow(1f - e, 3f)
            );

            yield return null;
        }

        bocadilloCanvasGroup.alpha = 1f;
        tr.localScale = escalaBase;
        aparicionRoutine = null;
    }

    private IEnumerator WiggleRetrato()
    {
        float t = 0f;

        while (true)
        {
            t += Time.deltaTime * wiggleVelocidad;

            float angulo =
                Mathf.Sin(t) * wiggleAngulo;

            retratoMercader.localRotation =
                Quaternion.Euler(
                    0f,
                    0f,
                    angulo
                );

            yield return null;
        }
    }

    private void PararWiggle()
    {
        if (wiggleRoutine != null)
            StopCoroutine(wiggleRoutine);

        wiggleRoutine = null;

        if (retratoMercader != null)
        {
            retratoMercader.localRotation =
                Quaternion.identity;
        }
    }

    private void MostrarBotones()
    {
        if (grupoBotones == null)
            return;

        if (fadeBotonesRoutine != null)
            StopCoroutine(fadeBotonesRoutine);

        fadeBotonesRoutine =
            StartCoroutine(FadeBotones());
    }

    private IEnumerator FadeBotones()
    {
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime /
                 fadeBotonesDialogo;

            grupoBotones.alpha =
                Mathf.Clamp01(t);

            yield return null;
        }

        grupoBotones.alpha = 1f;
        grupoBotones.interactable = true;
        grupoBotones.blocksRaycasts = true;

        fadeBotonesRoutine = null;
    }
}