using System.Collections;
using UnityEngine;

public class mainMenuFx : MonoBehaviour
{
    [Header("Glow del logo")]
    [SerializeField] private UnityEngine.UI.Image logoGlow;
    [SerializeField] private float glowMinAlpha = 0.25f;
    [SerializeField] private float glowMaxAlpha = 0.7f;
    [SerializeField] private float glowDuration = 1.8f;
    [Header("Logo")]
    [SerializeField] private RectTransform logo;
    [SerializeField] private float breathScale = 1.03f;
    [SerializeField] private float breathDuration = 2f;
    [Header("Botones (en orden de aparición)")]
    [SerializeField] private System.Collections.Generic.List<CanvasGroup> buttons = new System.Collections.Generic.List<CanvasGroup>();
    [SerializeField] private float slideFrom = -600f;
    [SerializeField] private float entranceDuration = 0.5f;
    [SerializeField] private float stagger = 0.12f;
    private void OnEnable()
    {
        if (logo != null)
            StartCoroutine(breathe());

        if (logoGlow != null)
            StartCoroutine(pulseGlow());

        StartCoroutine(buttonsEntrance());
    }
    private void OnDisable()
    {
        StopAllCoroutines();
    }
    private IEnumerator breathe()
    {
        Vector3 baseScale = logo.localScale;
        Vector3 maxScale = baseScale * breathScale;
        while (true)
        {
            yield return pingPong(breathDuration, t =>
                logo.localScale = Vector3.LerpUnclamped(baseScale, maxScale, easeInOutSine(t)));
        }
    }

    private IEnumerator pingPong(float duration, System.Action<float> apply)
    {
        float t = 0f;
        float half = duration * 0.5f;
        while (t < half) { t += Time.deltaTime; apply(t / half); yield return null; }
        t = 0f;
        while (t < half) { t += Time.deltaTime; apply(1f - t / half); yield return null; }
    }
    private IEnumerator buttonsEntrance()
    {
        var targets = new Vector2[buttons.Count];
        for (int i = 0; i < buttons.Count; i++)
        {
            if (buttons[i] == null) continue;
            var rect = (RectTransform)buttons[i].transform;
            targets[i] = rect.anchoredPosition;
            rect.anchoredPosition = targets[i] + new Vector2(slideFrom, 0f);
            buttons[i].alpha = 0f;
        }

        for (int i = 0; i < buttons.Count; i++)
        {
            if (buttons[i] != null)
                StartCoroutine(slideIn(buttons[i], targets[i]));
            yield return new WaitForSeconds(stagger);
        }
    }

    private IEnumerator slideIn(CanvasGroup cg, Vector2 target)
    {
        var rect = (RectTransform)cg.transform;
        Vector2 start = rect.anchoredPosition;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / entranceDuration;
            float e = easeOutCubic(Mathf.Clamp01(t));
            rect.anchoredPosition = Vector2.LerpUnclamped(start, target, e);
            cg.alpha = e;
            yield return null;
        }
        rect.anchoredPosition = target;
        cg.alpha = 1f;
    }
    private IEnumerator pulseGlow()
    {
        while (true)
        {
            yield return pingPong(glowDuration, t =>
            {
                float a = Mathf.Lerp(glowMinAlpha, glowMaxAlpha, easeInOutSine(t));
                var c = logoGlow.color;
                c.a = a;
                logoGlow.color = c;
            });
        }
    }
    private static float easeOutCubic(float t) => 1f - Mathf.Pow(1f - t, 3f);
    private static float easeInOutSine(float t) => -(Mathf.Cos(Mathf.PI * t) - 1f) * 0.5f;
}