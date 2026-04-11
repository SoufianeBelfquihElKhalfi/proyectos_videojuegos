using UnityEngine;
using UnityEngine.SceneManagement;

// Clase para centralizar los cambios de escena con una pantalla intermedia.
public static class SceneLoader
{
    // Nombre de la escena de carga. Debe tener el mismo nombre que la escena creada en Unity.
    public const string LoadingSceneName = "LoadingScene";

    // Propiedades para almacenar el nombre de la escena objetivo y el mensaje de carga.
    public static string TargetSceneName { get; private set; }
    public static string LoadingMessage { get; private set; } = "Descendiendo por el Inframundo...";

    // Método para iniciar la carga de una escena con un mensaje opcional.
    public static void Load(string targetSceneName, string loadingMessage = "Descendiendo por el Inframundo...")
    {
        if (string.IsNullOrWhiteSpace(targetSceneName))
        {
            Debug.LogError("SceneLoader: nombre de escena vacío.");
            return;
        }

        TargetSceneName = targetSceneName;
        LoadingMessage = loadingMessage;

        SceneManager.LoadScene(LoadingSceneName);
    }
}
