using System.Collections;
using UnityEngine;

public class defeatScreenFx : MonoBehaviour
{
    [Header("Tiempos")]
    [SerializeField] private float fadeOverlay = 0.6f;
    [SerializeField] private float fadeCalavera = 0.6f;
    [SerializeField] private float fadeTexto = 0.6f;
    [SerializeField] private float fadeBotones = 0.5f;
    [SerializeField] private float espera = 0.3f;
    [SerializeField] private float calaveraEscalaInicial = 1.5f;
    [SerializeField] private float textoEscalaInicial = 2f;

    [Header("Pulso de la calavera")]
    [SerializeField] private float calaveraPulsoScale = 1.06f;
    [SerializeField] private float calaveraPulsoDuracion = 2f;

    private CanvasGroup cgOverlay, cgVignette, cgCalavera, cgTexto, cgBotones;
    private Transform calaveraTr;
    private Transform textoTr;
    private CanvasGroup cgDerrota;

    private void Awake()
    {

        cgOverlay = PrepararCanvasGroup(BuscarHijo("DarkOverlay"));
        cgVignette = PrepararCanvasGroup(BuscarHijo("VignetteRoja"));

        calaveraTr = BuscarHijo("Image");
        cgCalavera = PrepararCanvasGroup(calaveraTr);

        textoTr = BuscarHijo("GameOverText");
        cgTexto = PrepararCanvasGroup(textoTr);

        Transform menuBtn = BuscarHijo("MenuButton");
        if (menuBtn != null && menuBtn.parent != null)
            cgBotones = PrepararCanvasGroup(menuBtn.parent);

        cgDerrota = PrepararCanvasGroup(BuscarHijo("derrota"));
    }

    private void OnEnable()
    {
        StartCoroutine(secuencia());
    }

    private IEnumerator secuencia()
    {
        // estado inicial
        if (cgOverlay != null) cgOverlay.alpha = 0f;
        if (cgVignette != null) cgVignette.alpha = 0f;
        if (cgDerrota != null) { cgDerrota.alpha = 0f; cgDerrota.interactable = false; cgDerrota.blocksRaycasts = false; }
        Transform derrotaTr = cgDerrota != null ? cgDerrota.transform : null;
        if (derrotaTr != null) derrotaTr.localScale = Vector3.one * 1.15f;

        // 1. overlay + viñeta roja
        if (cgVignette != null) StartCoroutine(Fade(cgVignette, 1f, fadeOverlay));
        if (cgOverlay != null) yield return Fade(cgOverlay, 1f, fadeOverlay);
        yield return Espera(espera);

        // 2. todo el contenido (derrota) entra junto con fade + escala
        if (cgDerrota != null)
        {
            float t = 0f;
            Vector3 desde = Vector3.one * 1.15f;
            while (t < 1f)
            {
                t += Time.unscaledDeltaTime / fadeTexto;
                float e = Mathf.Clamp01(t);
                cgDerrota.alpha = e;
                if (derrotaTr != null)
                    derrotaTr.localScale = Vector3.LerpUnclamped(desde, Vector3.one, 1f - Mathf.Pow(1f - e, 3f));
                yield return null;
            }
            cgDerrota.alpha = 1f;
            if (derrotaTr != null) derrotaTr.localScale = Vector3.one;
            cgDerrota.interactable = true;
            cgDerrota.blocksRaycasts = true;
        }

        // la calavera late (sigue funcionando porque calaveraTr está dentro)
        if (calaveraTr != null) StartCoroutine(pulsoCalavera());
    }

    // ---- helpers ----

    private Transform BuscarHijo(string nombre)
    {
        foreach (Transform t in GetComponentsInChildren<Transform>(true))
            if (t.name == nombre) return t;
        return null;
    }

    private CanvasGroup PrepararCanvasGroup(Transform t)
    {
        if (t == null) return null;
        var cg = t.GetComponent<CanvasGroup>();
        if (cg == null) cg = t.gameObject.AddComponent<CanvasGroup>();
        return cg;
    }

    private IEnumerator Fade(CanvasGroup cg, float objetivo, float dur)
    {
        float inicio = cg.alpha;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / dur;
            cg.alpha = Mathf.Lerp(inicio, objetivo, Mathf.Clamp01(t));
            yield return null;
        }
        cg.alpha = objetivo;
    }

    private IEnumerator FadeEscala(CanvasGroup cg, Transform tr, float dur, float escalaInicial)
    {
        Vector3 baseScale = Vector3.one;
        Vector3 desde = baseScale * escalaInicial;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / dur;
            float e = Mathf.Clamp01(t);
            cg.alpha = e;
            if (tr != null) tr.localScale = Vector3.LerpUnclamped(desde, baseScale, 1f - Mathf.Pow(1f - e, 3f));
            yield return null;
        }
        cg.alpha = 1f;
        if (tr != null) tr.localScale = baseScale;
    }

    private IEnumerator Espera(float segundos)
    {
        float t = 0f;
        while (t < segundos) { t += Time.unscaledDeltaTime; yield return null; }
    }

    private IEnumerator pulsoCalavera()
    {
        Vector3 baseScale = Vector3.one;
        Vector3 big = baseScale * calaveraPulsoScale;
        while (true)
        {
            float t = 0f;
            while (t < 1f)
            {
                t += Time.unscaledDeltaTime / (calaveraPulsoDuracion * 0.5f);
                calaveraTr.localScale = Vector3.LerpUnclamped(baseScale, big, easeInOutSine(Mathf.Clamp01(t)));
                yield return null;
            }
            t = 0f;
            while (t < 1f)
            {
                t += Time.unscaledDeltaTime / (calaveraPulsoDuracion * 0.5f);
                calaveraTr.localScale = Vector3.LerpUnclamped(big, baseScale, easeInOutSine(Mathf.Clamp01(t)));
                yield return null;
            }
        }
    }

    private static float easeInOutSine(float t) => -(Mathf.Cos(Mathf.PI * t) - 1f) * 0.5f;
}