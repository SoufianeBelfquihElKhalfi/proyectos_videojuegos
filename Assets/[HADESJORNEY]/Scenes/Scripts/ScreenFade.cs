using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(CanvasGroup))]
public class ScreenFade : MonoBehaviour
{
    public static ScreenFade Instance { get; private set; }

    [SerializeField] private float duracionPorDefecto = 0.25f;

    private CanvasGroup canvasGroup;
    private Coroutine fadeActivo;

    private float duracionFadeInPendiente;
    private float retrasoFadeInPendiente;
    private bool hayFadeInPendiente;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
    }

    public void CargarEscenaConFundido(
    string nombreEscena,
    float duracionSalida,
    float duracionEntrada,
    float retrasoEntrada = 0.08f)
    {
        StartCoroutine(CargarEscenaConFundidoCoroutine(
            nombreEscena,
            duracionSalida,
            duracionEntrada,
            retrasoEntrada
        ));
    }

    private IEnumerator CargarEscenaConFundidoCoroutine(
    string nombreEscena,
    float duracionSalida,
    float duracionEntrada,
    float retrasoEntrada)
    {
        yield return FundidoANegro(duracionSalida);

        FundidoDesdeNegroTrasSiguienteEscena(duracionEntrada, retrasoEntrada);

        SceneManager.LoadScene(nombreEscena);
    }

    public Coroutine FundidoANegro(float duracion)
    {
        return IniciarFundido(1f, duracion);
    }

    public Coroutine FundidoDesdeNegro(float duracion)
    {
        return IniciarFundido(0f, duracion);
    }

    public void FundidoDesdeNegroTrasSiguienteEscena(float duracion, float retraso = 0.08f)
    {
        duracionFadeInPendiente = duracion;
        retrasoFadeInPendiente = retraso;

        if (hayFadeInPendiente)
        {
            return;
        }

        hayFadeInPendiente = true;
        SceneManager.sceneLoaded += AlCargarEscena;
    }

    private void AlCargarEscena(Scene scene, LoadSceneMode mode)
    {
        hayFadeInPendiente = false;
        SceneManager.sceneLoaded -= AlCargarEscena;

        StartCoroutine(FundidoDesdeNegroTrasCargarCoroutine());
    }

    private IEnumerator FundidoDesdeNegroTrasCargarCoroutine()
    {
        yield return null;

        if (retrasoFadeInPendiente > 0f)
        {
            yield return new WaitForSecondsRealtime(retrasoFadeInPendiente);
        }

        FundidoDesdeNegro(duracionFadeInPendiente);
    }

    private Coroutine IniciarFundido(float alphaObjetivo, float duracion)
    {
        if (fadeActivo != null)
        {
            StopCoroutine(fadeActivo);
        }

        fadeActivo = StartCoroutine(FundidoCoroutine(alphaObjetivo, duracion));
        return fadeActivo;
    }

    private IEnumerator FundidoCoroutine(float alphaObjetivo, float duracion)
    {
        float alphaInicial = canvasGroup.alpha;
        float tiempo = 0f;

        if (duracion <= 0f)
        {
            duracion = duracionPorDefecto;
        }

        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;

        while (tiempo < duracion)
        {
            tiempo += Time.unscaledDeltaTime;

            float t = tiempo / duracion;
            canvasGroup.alpha = Mathf.Lerp(alphaInicial, alphaObjetivo, t);

            yield return null;
        }

        canvasGroup.alpha = alphaObjetivo;

        bool pantallaOscura = alphaObjetivo > 0.99f;
        canvasGroup.blocksRaycasts = pantallaOscura;
        canvasGroup.interactable = pantallaOscura;

        fadeActivo = null;
    }
}