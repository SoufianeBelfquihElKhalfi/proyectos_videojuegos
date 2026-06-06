using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using Dapasa.Audio;

public class SistemaVida : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private bool esJugador = false;

    [Tooltip("Activar solo en enemigos comunes.")]
    [SerializeField] private bool usaMuerteEnemigoComun = false;

    [Tooltip("Activar solo en el jugador.")]
    [SerializeField] private bool usarVidaGuardadaEntreEscenas = false;

    [Header("Vida en corazones")]
    [Min(1)] public int corazonesMaximos = 3;
    [Min(0)] public int corazonesMitadIniciales = -1;

    [Header("Efecto visual al recibir daño")]
    [SerializeField] private Color colorParpadeo = Color.red;
    [SerializeField] private float duracionParpadeo = 0.3f;
    [SerializeField] private int cantidadParpadeos = 3;

    [Header("Eventos")]
    public UnityEvent alMorir;
    public UnityEvent<int, int> alCambiarVida;

    [Header("Audio vida baja")]
    [SerializeField] private string idSonidoVidaBaja = "vida_baja";

    // Estado
    private int corazonesMitadMaximos;
    private int corazonesMitadActuales;

    // Componentes cacheados
    private Animator animator;
    private MovimientoAlastor movimiento;
    private EnemigoDistancia enemigoDistancia;

    // Estado de efectos
    private Coroutine parpadeoActivo;
    private Renderer[] renderersGuardados;
    private Color[] coloresOriginales;

    private MuerteEnemigoComun muerteEnemigoComun;

    // Propiedades públicas
    public int VidaActual => corazonesMitadActuales;
    public int VidaMaxima => corazonesMitadMaximos;
    public bool EstaMuerto => corazonesMitadActuales <= 0;
    public bool UsaVidaGuardadaEntreEscenas => usarVidaGuardadaEntreEscenas;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        movimiento = GetComponent<MovimientoAlastor>();
        enemigoDistancia = GetComponent<EnemigoDistancia>();


        if (usaMuerteEnemigoComun)
        {
            muerteEnemigoComun = GetComponent<MuerteEnemigoComun>();

            if (muerteEnemigoComun == null)
            {
                throw new MissingComponentException($"{name} usa muerte de enemigo común, pero no tiene MuerteEnemigoComun.");
            }
        }

        InicializarVida();
        NotificarCambioVida();
        ComprobarSonidoVidaBaja();
    }

    private void InicializarVida()
    {
        corazonesMaximos = Mathf.Max(1, corazonesMaximos);
        corazonesMitadMaximos = corazonesMaximos * 2;

        if (usarVidaGuardadaEntreEscenas && DatosGlobales.hayDatosVida)
            CargarVidaGuardada();
        else
            InicializarVidaNormal();
    }

    private void CargarVidaGuardada()
    {
        corazonesMitadMaximos = Mathf.Max(2, DatosGlobales.vidaMaxima);
        corazonesMitadActuales = Mathf.Clamp(DatosGlobales.vidaActual, 0, corazonesMitadMaximos);
        corazonesMaximos = Mathf.Max(1, corazonesMitadMaximos / 2);
    }

    private void InicializarVidaNormal()
    {
        corazonesMitadActuales = corazonesMitadIniciales < 0
            ? corazonesMitadMaximos
            : Mathf.Clamp(corazonesMitadIniciales, 0, corazonesMitadMaximos);
    }

    public void RecibirDanio(int danioMitadCorazones)
    {
        if (EstaMuerto) return;
        if (danioMitadCorazones <= 0) return;
        if (esJugador && movimiento != null && movimiento.esInvulnerable) return;

        corazonesMitadActuales = Mathf.Clamp(
            corazonesMitadActuales - danioMitadCorazones,
            0,
            corazonesMitadMaximos
        );

        NotificarCambioVida();
        ComprobarSonidoVidaBaja();

        if (EstaMuerto)
        {
            Morir();
            return;
        }

        IniciarParpadeo();

        if (esJugador && flashGolpe.Instancia != null)
        {
            StartCoroutine(flashGolpe.Instancia.MostrarFlash());
        }

        if (animator != null)
            animator.SetTrigger("danio");

        if (enemigoDistancia != null)
        {
            enemigoDistancia.RecibirDanioAnimacion();
        }
    }

    public void Curar(int curacionMitadCorazones)
    {
        if (EstaMuerto) return;
        if (curacionMitadCorazones <= 0) return;

        corazonesMitadActuales = Mathf.Clamp(
            corazonesMitadActuales + curacionMitadCorazones,
            0,
            corazonesMitadMaximos
        );

        NotificarCambioVida();
        ComprobarSonidoVidaBaja();
    }

    public void CambiarCorazonesMaximos(int nuevosCorazones, bool rellenarVida = true)
    {
        corazonesMaximos = Mathf.Max(1, nuevosCorazones);
        corazonesMitadMaximos = corazonesMaximos * 2;

        corazonesMitadActuales = rellenarVida
            ? corazonesMitadMaximos
            : Mathf.Clamp(corazonesMitadActuales, 0, corazonesMitadMaximos);

        NotificarCambioVida();
        ComprobarSonidoVidaBaja();
    }

    public void GuardarVida()
    {
        DatosGlobales.vidaActual = corazonesMitadActuales;
        DatosGlobales.vidaMaxima = corazonesMitadMaximos;
        DatosGlobales.hayDatosVida = true;
    }

    // ---------- AUDIO VIDA BAJA ----------

    private void ComprobarSonidoVidaBaja()
    {
        if (!esJugador) return;

        bool vidaBaja = corazonesMitadActuales <= 1 && corazonesMitadActuales > 0;

        if (vidaBaja)
        {
            IniciarSonidoVidaBajaLoop();
        }
        else
        {
            DetenerSonidoVidaBajaLoop();
        }
    }
    private void IniciarSonidoVidaBajaLoop()
    {
        if (AudioManager.Instance == null)
            return;

        AudioManager.Instance.ReproducirSFXLoop2D(idSonidoVidaBaja);
    }

    private void DetenerSonidoVidaBajaLoop()
    {
        if (AudioManager.Instance == null)
            return;

        AudioManager.Instance.PararSFXLoop2D(idSonidoVidaBaja);
    }

    // ---------- PARPADEO ----------

    private void IniciarParpadeo()
    {
        if (parpadeoActivo != null)
        {
            StopCoroutine(parpadeoActivo);
            RestaurarColores();
        }

        renderersGuardados = GetComponentsInChildren<Renderer>();
        coloresOriginales = new Color[renderersGuardados.Length];

        for (int i = 0; i < renderersGuardados.Length; i++)
        {
            if (renderersGuardados[i] != null && renderersGuardados[i].material.HasProperty("_Color"))
                coloresOriginales[i] = renderersGuardados[i].material.color;
        }

        parpadeoActivo = StartCoroutine(ParpadeoGolpe());
    }

    private IEnumerator ParpadeoGolpe()
    {
        float tiempoPorParpadeo = duracionParpadeo / cantidadParpadeos;

        for (int i = 0; i < cantidadParpadeos; i++)
        {
            AplicarColor(colorParpadeo);
            yield return new WaitForSecondsRealtime(tiempoPorParpadeo / 2f);

            RestaurarColores();
            yield return new WaitForSecondsRealtime(tiempoPorParpadeo / 2f);
        }

        parpadeoActivo = null;
    }

    private void AplicarColor(Color color)
    {
        if (renderersGuardados == null) return;

        foreach (Renderer r in renderersGuardados)
        {
            if (r != null && r.material.HasProperty("_Color"))
                r.material.color = color;
        }
    }

    private void RestaurarColores()
    {
        if (renderersGuardados == null || coloresOriginales == null) return;

        for (int i = 0; i < renderersGuardados.Length; i++)
        {
            if (renderersGuardados[i] != null && renderersGuardados[i].material.HasProperty("_Color"))
                renderersGuardados[i].material.color = coloresOriginales[i];
        }
    }

    // ---------- MUERTE ----------

    private void Morir()
    {
        DetenerSonidoVidaBajaLoop();

        if (animator != null)
            animator.SetTrigger("Muerte");

        if (esJugador)
        {
            PrepararMuerteJugador();
        }
        else if (usaMuerteEnemigoComun)
        {
            muerteEnemigoComun.PrepararMuerte();
        }

        StartCoroutine(EsperarMuerte());
    }

    private void PrepararMuerteJugador()
    {
        if (movimiento != null)
            movimiento.enabled = false;

        CombateJugador combate = GetComponent<CombateJugador>();
        if (combate != null)
            combate.enabled = false;

        Collider colliderPrincipal = GetComponent<Collider>();
        if (colliderPrincipal != null)
            colliderPrincipal.enabled = false;

        DesactivarTodosLosEnemigos();
    }

    private void DesactivarTodosLosEnemigos()
    {
        foreach (var p in FindObjectsByType<Patrullero>(FindObjectsSortMode.None))
            p.enabled = false;

        foreach (var e in FindObjectsByType<EnemigoDistancia>(FindObjectsSortMode.None))
            e.enabled = false;

        foreach (var a in FindObjectsByType<NavMeshAgent>(FindObjectsSortMode.None))
            a.isStopped = true;
    }

    private IEnumerator EsperarMuerte()
    {
        yield return new WaitForSeconds(2f);

        alMorir?.Invoke();

        if (esJugador)
            yield break;

        yield return DesvanecerYDestruir();
    }

    private IEnumerator DesvanecerYDestruir()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();

        foreach (Renderer r in renderers)
        {
            foreach (Material mat in r.materials)
            {
                mat.SetFloat("_Mode", 3);
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0);
                mat.DisableKeyword("_ALPHATEST_ON");
                mat.EnableKeyword("_ALPHABLEND_ON");
                mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                mat.renderQueue = 3000;
            }
        }

        float duracionFade = 1.5f;
        float tiempo = 0f;

        while (tiempo < duracionFade)
        {
            tiempo += Time.deltaTime;
            float alpha = 1f - (tiempo / duracionFade);

            foreach (Renderer r in renderers)
            {
                foreach (Material mat in r.materials)
                {
                    if (mat.HasProperty("_Color"))
                    {
                        Color c = mat.color;
                        c.a = alpha;
                        mat.color = c;
                    }
                }
            }

            yield return null;
        }

        Destroy(gameObject);
    }

    private void NotificarCambioVida()
    {
        alCambiarVida?.Invoke(corazonesMitadActuales, corazonesMitadMaximos);
    }

    public void EstablecerVidaDesdeCheckpoint(int vidaActual, int vidaMaxima)
    {
        if (vidaMaxima <= 0)
        {
            throw new System.ArgumentOutOfRangeException(nameof(vidaMaxima), "La vida maxima del checkpoint debe ser mayor que 0.");
        }

        if (vidaActual < 0 || vidaActual > vidaMaxima)
        {
            throw new System.ArgumentOutOfRangeException(nameof(vidaActual), "La vida actual del checkpoint no es valida.");
        }

        corazonesMitadMaximos = vidaMaxima;
        corazonesMitadActuales = vidaActual;
        corazonesMaximos = corazonesMitadMaximos / 2;

        NotificarCambioVida();
        ComprobarSonidoVidaBaja();
    }
}