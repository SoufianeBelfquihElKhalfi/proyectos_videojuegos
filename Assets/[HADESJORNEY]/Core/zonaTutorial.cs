using UnityEngine;
using TMPro;
using System.Collections;

public class zonaTutorial : MonoBehaviour
{
    [Header("Referencias UI")]
    [SerializeField] private GameObject panelTexto;
    [SerializeField] private TextMeshProUGUI texto;

    [Header("Mensaje")]
    [TextArea(2, 4)]
    [SerializeField] private string mensaje;

    [Header("Comportamiento")]
    [SerializeField] private bool mostrarUnaVez = true;
    [SerializeField] private float duracionEnPantalla = 4f;
    [SerializeField] private float duracionFade = 0.3f;

    private bool mostrado = false;
    private Coroutine rutinaActiva;

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (mostrarUnaVez && mostrado) return;

        if (rutinaActiva != null) StopCoroutine(rutinaActiva);
        rutinaActiva = StartCoroutine(MostrarMensaje());
        mostrado = true;
    }

    private IEnumerator MostrarMensaje()
    {
        texto.text = mensaje;
        panelTexto.SetActive(true);

        CanvasGroup grupo = ObtenerCanvasGroup();
        grupo.alpha = 0f;

        // Fade in
        yield return Fade(grupo, 0f, 1f);

        // Mantener en pantalla
        yield return new WaitForSeconds(duracionEnPantalla);

        // Fade out
        yield return Fade(grupo, 1f, 0f);

        panelTexto.SetActive(false);
        rutinaActiva = null;
    }

    private IEnumerator Fade(CanvasGroup grupo, float desde, float hasta)
    {
        float tiempo = 0f;
        while (tiempo < duracionFade)
        {
            tiempo += Time.deltaTime;
            grupo.alpha = Mathf.Lerp(desde, hasta, tiempo / duracionFade);
            yield return null;
        }
        grupo.alpha = hasta;
    }

    private CanvasGroup ObtenerCanvasGroup()
    {
        CanvasGroup grupo = panelTexto.GetComponent<CanvasGroup>();
        if (grupo == null)
            grupo = panelTexto.AddComponent<CanvasGroup>();
        return grupo;
    }
}