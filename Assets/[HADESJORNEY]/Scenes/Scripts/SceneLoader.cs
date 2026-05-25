using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneLoader
{
    public const string LoadingSceneName = "LoadingScene";

    public static string TargetSceneName { get; private set; }
    public static string LoadingMessage { get; private set; } = "Cargando...";

    private const float DuracionSalidaHaciaCarga = 0.25f;
    private const float DuracionEntradaPantallaCarga = 0.35f;
    private const float RetrasoAntesDeMostrarPantallaCarga = 0.08f;

    public static void Load(string targetSceneName, string loadingMessage = "Cargando...")
    {
        if (string.IsNullOrWhiteSpace(targetSceneName))
        {
            Debug.LogError("SceneLoader: nombre de escena vacío.");
            return;
        }

        TargetSceneName = targetSceneName;
        LoadingMessage = loadingMessage;

        if (ScreenFade.Instance == null)
        {
            Debug.LogWarning("SceneLoader: no hay ScreenFade en la escena. Se cargará sin fundido.");
            SceneManager.LoadScene(LoadingSceneName);
            return;
        }

        ScreenFade.Instance.CargarEscenaConFundido(
            LoadingSceneName,
            DuracionSalidaHaciaCarga,
            DuracionEntradaPantallaCarga,
            RetrasoAntesDeMostrarPantallaCarga
        );
    }
}