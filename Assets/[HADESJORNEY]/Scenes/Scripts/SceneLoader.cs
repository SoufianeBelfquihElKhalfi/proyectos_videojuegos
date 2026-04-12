using UnityEngine;
using UnityEngine.SceneManagement;

// Clase estática para manejar la carga de escenas con una pantalla de carga personalizada
public static class SceneLoader
{
    public const string LoadingSceneName = "LoadingScene";

    public static string TargetSceneName { get; private set; }
    public static string LoadingMessage { get; private set; } = "Cargando...";

    // Método para iniciar la carga de una escena con un mensaje opcional
    public static void Load(string targetSceneName, string loadingMessage = "Cargando...")
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