using UnityEngine;
using UnityEngine.SceneManagement;

public class CreditosScroll : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private RectTransform textoCreditos;
    [SerializeField] private float velocidad = 50f;
    [SerializeField] private float posicionFinalY = 1200f;

    [Header("Escenas")]
    [SerializeField] private string escenaMenu = "MAIN";

    private bool terminado = false;

    void Update()
    {
        if (terminado) return;

        textoCreditos.anchoredPosition += Vector2.up * velocidad * Time.deltaTime;

        if (textoCreditos.anchoredPosition.y >= posicionFinalY)
        {
            terminado = true;
        }

        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Return))
        {
            VolverAlMenu();
        }
    }

    public void VolverAlMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(escenaMenu);
    }
}