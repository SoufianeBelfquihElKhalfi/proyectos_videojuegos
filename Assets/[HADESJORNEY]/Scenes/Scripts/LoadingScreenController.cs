using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingScreenController : MonoBehaviour
{
    // UI
    [SerializeField] private Slider progressBar;
    [SerializeField] private TMP_Text loadingText;
    [SerializeField] private TMP_Text progressText;

    // Fade y carga
    [SerializeField] private float visualFillSpeed = 1.2f;
    [SerializeField] private float minimumScreenTime = 0.6f;
    [SerializeField] private float fadeInDuration = 0.3f;

    // Escena de respaldo
    private const string FallbackSceneName = "SplashScreen";

    // CanvasGroups
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private CanvasGroup artworkCanvasGroup;

    // Respiración de la imagen
    [SerializeField] private float breathingMinAlpha = 0.9f;
    [SerializeField] private float breathingMaxAlpha = 1f;
    [SerializeField] private float breathingCycleDuration = 2.2f;

    // Carga principal
    private IEnumerator Start()
    {
        string targetScene = SceneLoader.TargetSceneName;

        if (string.IsNullOrWhiteSpace(targetScene))
        {
            Debug.LogWarning("LoadingScreen: no hay escena destino. Volviendo a SplashScreen");
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

        AsyncOperation operation = SceneManager.LoadSceneAsync(targetScene);
        operation.allowSceneActivation = false;

        float shownProgress = 0f;
        float elapsedTime = 0f;

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
                operation.allowSceneActivation = true;
            }

            yield return null;
        }
    }

    // Fade de entrada
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

    // Respiración de la imagen
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