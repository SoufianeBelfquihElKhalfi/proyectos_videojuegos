using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Controlador para la pantalla de carga, maneja la animación de la barra de progreso y el texto
public class LoadingScreenController : MonoBehaviour
{
    // Referencias a los elementos UI de la pantalla de carga
    [SerializeField] private Slider progressBar;
    [SerializeField] private TMP_Text loadingText;
    [SerializeField] private TMP_Text progressText;

    // Configuraciones para la animación de la barra de progreso y el tiempo mínimo en pantalla
    [SerializeField] private float visualFillSpeed = 1.2f;
    [SerializeField] private float minimumScreenTime = 0.6f;
    [SerializeField] private float fadeInDuration = 0.3f;

    // Nombre de la escena a cargar si no se especifica una escena destino
    private const string FallbackSceneName = "MAIN";

    // Referencias para la animación de respiración del arte en la pantalla de carga
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private CanvasGroup artworkCanvasGroup;

    // Configuraciones para la animación de respiración del arte
    [SerializeField] private float breathingMinAlpha = 0.9f;
    [SerializeField] private float breathingMaxAlpha = 1f;
    [SerializeField] private float breathingCycleDuration = 2.2f;

    [Header("Fundido entre pantalla de carga y escena destino")]
    [SerializeField] private float fadeOutAntesDeActivarEscena = 0.25f;
    [SerializeField] private float fadeInDespuesDeActivarEscena = 0.25f;

    // Coroutine principal que maneja la lógica de carga de la escena y las animaciones
    private IEnumerator Start()
    {
        string targetScene = SceneLoader.TargetSceneName;

        if (string.IsNullOrWhiteSpace(targetScene))
        {
            Debug.LogWarning("LoadingScreen: no hay escena destino. Volviendo a MAIN");
            SceneManager.LoadScene(FallbackSceneName);
            yield break;
        }

        if (loadingText != null)
        {
            loadingText.text = SceneLoader.LoadingMessage;
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            StartCoroutine(FadeInCanvas());
        }

        if (artworkCanvasGroup != null)
        {
            artworkCanvasGroup.alpha = breathingMaxAlpha;
            StartCoroutine(BreatheArtwork());
        }

        // Iniciar la carga asíncrona de la escena destino
        AsyncOperation operation = SceneManager.LoadSceneAsync(targetScene);
        operation.allowSceneActivation = false;

        float shownProgress = 0f;
        float elapsedTime = 0f;

        // Mientras la escena se esté cargando, actualizar la barra de progreso y el texto
        while (!operation.isDone)
        {
            float deltaTime = Time.unscaledDeltaTime;
            elapsedTime += deltaTime;

            float realProgress = Mathf.Clamp01(operation.progress / 0.9f);
            shownProgress = Mathf.MoveTowards(shownProgress, realProgress, visualFillSpeed * deltaTime);

            if (progressBar != null)
            {
                progressBar.value = shownProgress;
            }

            if (progressText != null)
            {
                progressText.text = Mathf.RoundToInt(shownProgress * 100f) + "%";
            }

            if (operation.progress >= 0.9f && shownProgress >= 1f && elapsedTime >= minimumScreenTime)
            {
                if (ScreenFade.Instance != null)
                {
                    yield return ScreenFade.Instance.FundidoANegro(fadeOutAntesDeActivarEscena);
                    ScreenFade.Instance.FundidoDesdeNegroTrasSiguienteEscena(fadeInDespuesDeActivarEscena);
                }

                operation.allowSceneActivation = true;
            }

            yield return null;
        }
    }

    // Coroutine para hacer un fade-in suave del canvas de la pantalla de carga
    private IEnumerator FadeInCanvas()
    {
        float elapsed = 0f;

        while (elapsed < fadeInDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Clamp01(elapsed / fadeInDuration);
            yield return null;
        }

        canvasGroup.alpha = 1f;
    }

    // Coroutine para animar la respiración del arte en la pantalla de carga, haciendo que su alpha oscile suavemente
    private IEnumerator BreatheArtwork()
    {
        float elapsed = 0f;

        while (true)
        {
            elapsed += Time.unscaledDeltaTime;

            float t = (Mathf.Sin((elapsed / breathingCycleDuration) * Mathf.PI * 2f) + 1f) * 0.5f;
            artworkCanvasGroup.alpha = Mathf.Lerp(breathingMinAlpha, breathingMaxAlpha, t);

            yield return null;
        }
    }
}