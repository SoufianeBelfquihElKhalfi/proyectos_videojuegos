
using UnityEngine;
using UnityEngine.SceneManagement;
using Dapasa.Audio;

public class CreditosScroll : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private RectTransform textoCreditos;
    [SerializeField] private float velocidad = 50f;
    [SerializeField] private float posicionFinalY = 1200f;

    [Header("Música")]
    [SerializeField] private string idMusicaCreditos = "musica_creditos";
    [SerializeField] private bool reiniciarMusica = true;
    [SerializeField] private bool pararMusicaAlSalir = true;

    [Header("Escenas")]
    [SerializeField] private string escenaMenu = "MAIN";

    private bool terminado;

    private void Start()
    {
        // Evita que los créditos se queden pausados si se viene
        // desde una escena donde Time.timeScale estaba a 0.
        Time.timeScale = 1f;

        ReproducirMusicaCreditos();
    }

    private void Update()
    {
        if (!terminado && textoCreditos != null)
        {
            textoCreditos.anchoredPosition +=
                Vector2.up * velocidad * Time.deltaTime;

            if (textoCreditos.anchoredPosition.y >= posicionFinalY)
            {
                terminado = true;
            }
        }

        if (
            Input.GetKeyDown(KeyCode.Escape) ||
            Input.GetKeyDown(KeyCode.Return)
        )
        {
            VolverAlMenu();
        }
    }

    private void ReproducirMusicaCreditos()
    {
        if (AudioManager.Instance == null)
        {
            Debug.LogWarning(
                "CreditosScroll: no se ha encontrado AudioManager.",
                this
            );

            return;
        }

        if (string.IsNullOrWhiteSpace(idMusicaCreditos))
        {
            Debug.LogWarning(
                "CreditosScroll: el ID de la música está vacío.",
                this
            );

            return;
        }

        // Detiene inmediatamente la música de la escena anterior.
        AudioManager.Instance.PararMusica();

        // Reproduce la música de créditos en bucle.
        AudioManager.Instance.ReproducirMusica(
            idMusicaCreditos,
            reiniciarMusica
        );
    }

    public void VolverAlMenu()
    {
        Time.timeScale = 1f;

        if (
            pararMusicaAlSalir &&
            AudioManager.Instance != null
        )
        {
            AudioManager.Instance.PararMusica();
        }

        SceneManager.LoadScene(escenaMenu);
    }
}

