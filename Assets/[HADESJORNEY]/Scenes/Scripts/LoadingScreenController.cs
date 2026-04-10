using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingScreenController : MonoBehaviour
{
    // Referencias a los elementos UI de la pantalla de carga.
    [SerializeField] private Slider progressBar;
    [SerializeField] private TMP_Text loadingText;
    [SerializeField] private TMP_Text progressText;

    // Configuraciones para la velocidad de llenado visual y el tiempo mínimo que la pantalla debe mostrarse.
    [SerializeField] private float visualFillSpeed = 1.2f;
    [SerializeField] private float minimumScreenTime = 0.6f;

    // Nombre de la escena de respaldo a cargar si no se especifica una escena destino válida.
    private const string FallbackSceneName = "SplashScreen";

    // Corutina principal que maneja la lógica de carga de la escena.
    private IEnumerator Start()
    {
        // Obtener el nombre de la escena destino desde el SceneLoader.
        string targetScene = SceneLoader.TargetSceneName;

        // Si no se ha especificado una escena destino, mostrar una advertencia y volver a la pantalla de inicio.
        if (string.IsNullOrWhiteSpace(targetScene))
        {
            Debug.LogWarning("LoadingScreen: no hay escena destino. Volviendo a SplashScreen");
            SceneManager.LoadScene(FallbackSceneName);
            yield break;
        }

        // Mostrar el mensaje de carga si se ha proporcionado.
        if (loadingText != null)
        {
            loadingText.text = SceneLoader.LoadingMessage;
        }

        // Iniciar la carga asíncrona de la escena destino. Empieza la carga en segundo plano, pero no permite la activación automática.
        AsyncOperation operation = SceneManager.LoadSceneAsync(targetScene);
        operation.allowSceneActivation = false;

        // Variables para controlar el progreso mostrado y el tiempo transcurrido.
        float shownProgress = 0f;
        float elapsedTime = 0f;

        // Bucle principal que se ejecuta mientras la escena se está cargando.
        while (!operation.isDone)
        {

            // Incrementar el tiempo transcurrido.
            float deltaTime = Time.unscaledDeltaTime;
            elapsedTime += deltaTime;

            // Calcular el progreso real de la carga, ajustando para que alcance 1.0 cuando la operación esté al 90% (ya que el último 10% es la activación de la escena).
            float realProgress = Mathf.Clamp01(operation.progress / 0.9f);
            // Suavizar el progreso mostrado para que no salte abruptamente, usando MoveTowards para acercarse al progreso real a una velocidad controlada.
            shownProgress = Mathf.MoveTowards(shownProgress, realProgress, visualFillSpeed * deltaTime);

            // Actualizar la barra de progreso y el texto de porcentaje en la UI.
            if (progressBar != null)
            {
                progressBar.value = shownProgress;
            }

            // Mostrar el porcentaje de progreso redondeado al entero más cercano.
            if (progressText != null)
            {
                progressText.text = Mathf.RoundToInt(shownProgress * 100f) + "%";
            }

            // Permitir la activación de la escena solo si el progreso real ha alcanzado el 90%, el progreso mostrado ha alcanzado el 100% y se ha cumplido el tiempo mínimo de pantalla.
            if (operation.progress >= 0.9f && shownProgress >= 1f && elapsedTime >= minimumScreenTime)
            {
                operation.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}
