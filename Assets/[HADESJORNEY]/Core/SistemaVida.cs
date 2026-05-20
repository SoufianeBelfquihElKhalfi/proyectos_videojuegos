using System.Collections;
using UnityEditor.Rendering.LookDev;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public class SistemaVida : MonoBehaviour
{
    [Header("Persistencia")]
    [Tooltip("Activar solo en el jugador. Los enemigos deben tenerlo desactivado.")]
    [SerializeField] private bool usarVidaGuardadaEntreEscenas = false;

    [Header("Efecto visual al recibir daño")]
    [SerializeField] private float duracionParpadeo = 0.3f;
    [SerializeField] private int cantidadParpadeos = 3;

    [Header("Vida en corazones")]
    [Min(1)] public int corazonesMaximos = 3;

    [Header("Vida inicial")]
    [Min(0)] public int corazonesMitadIniciales = -1;
    // -1 = vida completa

    [Header("Eventos")]
    public UnityEvent alMorir;
    public UnityEvent<int, int> alCambiarVida;

    private int corazonesMitadMaximos;
    private int corazonesMitadActuales;

    private Coroutine parpadeoActivo;
    private Renderer[] renderersGuardados;
    private Color[] coloresOriginales;

    [SerializeField] private bool esJugador = false;
    public int VidaActual => corazonesMitadActuales;
    public int VidaMaxima => corazonesMitadMaximos;
    public bool EstaMuerto => corazonesMitadActuales <= 0;
    public bool UsaVidaGuardadaEntreEscenas => usarVidaGuardadaEntreEscenas;

    private Animator animator;

    private void Awake()
    {
        InicializarVida();
        NotificarCambioVida();
        animator = GetComponentInChildren<Animator>();
    }

    private void InicializarVida()
    {
        corazonesMaximos = Mathf.Max(1, corazonesMaximos);
        corazonesMitadMaximos = corazonesMaximos * 2;

        if (usarVidaGuardadaEntreEscenas && DatosGlobales.hayDatosVida)
        {
            CargarVidaGuardada();
            return;
        }

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
        if (corazonesMitadIniciales < 0)
        {
            corazonesMitadActuales = corazonesMitadMaximos;
        }
        else
        {
            corazonesMitadActuales = Mathf.Clamp(
                corazonesMitadIniciales,
                0,
                corazonesMitadMaximos
            );
        }
    }

    public void RecibirDanio(int danioMitadCorazones)
    {
        if (EstaMuerto) return;
        if (danioMitadCorazones <= 0) return;

        // NUEVO: comprobar invencibilidad del dash
        var movimiento = GetComponent<MovimientoAlastor>();
        if (movimiento != null && movimiento.esInvulnerable) return;
        if (EstaMuerto) return;
        if (danioMitadCorazones <= 0) return;
        if (esJugador && flashGolpe.Instancia != null)
            StartCoroutine(flashGolpe.Instancia.MostrarFlash());
        corazonesMitadActuales -= danioMitadCorazones;
        corazonesMitadActuales = Mathf.Clamp(corazonesMitadActuales, 0, corazonesMitadMaximos);

        NotificarCambioVida();
        IniciarParpadeo();
        if (animator != null)
            animator.SetTrigger("Danio");

        if (corazonesMitadActuales <= 0)
        {
            Morir();
        }
        if (animator != null)
        {
            Debug.Log("Trigger Daño activado");
            animator.SetTrigger("Golpe");
        }
        else
        {
            Debug.Log("Animator null en SistemaVida");
        }
        var enemigoDistancia = GetComponent<EnemigoDistancia>();
        if (enemigoDistancia != null)
            enemigoDistancia.RecibirDanioAnimacion();
    }

    private void IniciarParpadeo()
    {
        if (parpadeoActivo != null)
        {
            StopCoroutine(parpadeoActivo);
            RestaurarColores();
        }

        renderersGuardados = GetComponentsInChildren<Renderer>();
        coloresOriginales = new Color[renderersGuardados.Length];

        for (int j = 0; j < renderersGuardados.Length; j++)
        {
            if (renderersGuardados[j] != null && renderersGuardados[j].material.HasProperty("_Color"))
            {
                coloresOriginales[j] = renderersGuardados[j].material.color;
            }
        }

        parpadeoActivo = StartCoroutine(ParpadeoGolpe());
    }

    private IEnumerator ParpadeoGolpe()
    {
        Color colorGolpe = Color.red;
        float tiempoPorParpadeo = duracionParpadeo / cantidadParpadeos;

        for (int i = 0; i < cantidadParpadeos; i++)
        {
            for (int j = 0; j < renderersGuardados.Length; j++)
            {
                if (renderersGuardados[j] != null && renderersGuardados[j].material.HasProperty("_Color"))
                {
                    renderersGuardados[j].material.color = colorGolpe;
                }
            }

            yield return new WaitForSecondsRealtime(tiempoPorParpadeo / 2f);

            RestaurarColores();

            yield return new WaitForSecondsRealtime(tiempoPorParpadeo / 2f);
        }

        parpadeoActivo = null;
    }

    private void RestaurarColores()
    {
        if (renderersGuardados == null || coloresOriginales == null) return;

        for (int j = 0; j < renderersGuardados.Length; j++)
        {
            if (renderersGuardados[j] != null && renderersGuardados[j].material.HasProperty("_Color"))
            {
                renderersGuardados[j].material.color = coloresOriginales[j];
            }
        }
    }

    public void Curar(int curacionMitadCorazones)
    {
        if (EstaMuerto) return;
        if (curacionMitadCorazones <= 0) return;

        corazonesMitadActuales += curacionMitadCorazones;
        corazonesMitadActuales = Mathf.Clamp(corazonesMitadActuales, 0, corazonesMitadMaximos);

        NotificarCambioVida();
    }

    public void CambiarCorazonesMaximos(int nuevosCorazones, bool rellenarVida = true)
    {
        corazonesMaximos = Mathf.Max(1, nuevosCorazones);
        corazonesMitadMaximos = corazonesMaximos * 2;

        if (rellenarVida)
        {
            corazonesMitadActuales = corazonesMitadMaximos;
        }
        else
        {
            corazonesMitadActuales = Mathf.Clamp(corazonesMitadActuales, 0, corazonesMitadMaximos);
        }

        NotificarCambioVida();
    }

    public void GuardarVida()
    {
        DatosGlobales.vidaActual = corazonesMitadActuales;
        DatosGlobales.vidaMaxima = corazonesMitadMaximos;
        DatosGlobales.hayDatosVida = true;
    }

    private void Morir()
    {
        if (animator != null)
            animator.SetTrigger("Muerte");

        var movimiento = GetComponent<MovimientoAlastor>();
        if (movimiento != null) movimiento.enabled = false;

        var combate = GetComponent<CombateJugador>();
        if (combate != null) combate.enabled = false;

        // Desactivar enemigo para que no siga atacando
        var enemigoDistancia = GetComponent<EnemigoDistancia>();
        if (enemigoDistancia != null) enemigoDistancia.enabled = false;

        var collider = GetComponent<Collider>();
        if (collider != null) collider.enabled = false;

        if (esJugador)
        {
            var patrulleros = FindObjectsByType<Patrullero>(FindObjectsSortMode.None);
            foreach (var p in patrulleros)
                p.enabled = false;

            var enemigosDistancia = FindObjectsByType<EnemigoDistancia>(FindObjectsSortMode.None);
            foreach (var e in enemigosDistancia)
                e.enabled = false;

            var agentes = FindObjectsByType<NavMeshAgent>(FindObjectsSortMode.None);
            foreach (var a in agentes)
                a.isStopped = true;
        }

        StartCoroutine(EsperarMuerte());
    }

    private IEnumerator EsperarMuerte()
    {
        // Espera a que termine la animación de muerte
        yield return new WaitForSeconds(2f);

        // Fade out solo para enemigos
        if (!esJugador)
        {
            Renderer[] renderers = GetComponentsInChildren<Renderer>();

            // Cambiar materiales a transparente
            foreach (Renderer rend in renderers)
            {
                foreach (Material mat in rend.materials)
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

                foreach (Renderer rend in renderers)
                {
                    foreach (Material mat in rend.materials)
                    {
                        if (mat.HasProperty("_Color"))
                        {
                            Color color = mat.color;
                            color.a = alpha;
                            mat.color = color;
                        }
                    }
                }
                yield return null;
            }

            Destroy(gameObject);
        }
        else
        {
            alMorir?.Invoke();
        }
    }
    private void NotificarCambioVida()
    {
        alCambiarVida?.Invoke(corazonesMitadActuales, corazonesMitadMaximos);
    }
}