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

    private static zonaTutorial tutorialActivo;

    private bool mostrado = false;
    private Coroutine rutinaActiva;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        MostrarMensajeManual();
    }

    public void MostrarMensajeManual()
    {
        if (mostrarUnaVez && mostrado) return;

        if (tutorialActivo != null && tutorialActivo != this)
            tutorialActivo.DetenerRutinaActiva();

        if (rutinaActiva != null)
            StopCoroutine(rutinaActiva);

        tutorialActivo = this;
        rutinaActiva = StartCoroutine(RutinaMostrarMensaje());
        mostrado = true;
    }

    private void DetenerRutinaActiva()
    {
        if (rutinaActiva != null)
            StopCoroutine(rutinaActiva);

        rutinaActiva = null;
    }

    private IEnumerator RutinaMostrarMensaje()
    {
        texto.text = mensaje;
        panelTexto.SetActive(true);

        CanvasGroup grupo = ObtenerCanvasGroup();
        grupo.alpha = 0f;

        yield return Fade(grupo, 0f, 1f);

        yield return new WaitForSeconds(duracionEnPantalla);

        yield return Fade(grupo, 1f, 0f);

        if (tutorialActivo == this)
        {
            panelTexto.SetActive(false);
            tutorialActivo = null;
        }

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