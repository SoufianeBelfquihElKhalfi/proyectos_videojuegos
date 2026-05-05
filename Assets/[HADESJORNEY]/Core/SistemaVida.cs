using UnityEngine;
using UnityEngine.Events;
using System.Collections;

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

    public int VidaActual => corazonesMitadActuales;
    public int VidaMaxima => corazonesMitadMaximos;
    public bool EstaMuerto => corazonesMitadActuales <= 0;
    public bool UsaVidaGuardadaEntreEscenas => usarVidaGuardadaEntreEscenas;

    private void Awake()
    {
        InicializarVida();
        NotificarCambioVida();
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

        corazonesMitadActuales -= danioMitadCorazones;
        corazonesMitadActuales = Mathf.Clamp(corazonesMitadActuales, 0, corazonesMitadMaximos);

        NotificarCambioVida();
        IniciarParpadeo();

        if (corazonesMitadActuales <= 0)
        {
            Morir();
        }
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
        alMorir?.Invoke();
    }

    private void NotificarCambioVida()
    {
        alCambiarVida?.Invoke(corazonesMitadActuales, corazonesMitadMaximos);
    }
}