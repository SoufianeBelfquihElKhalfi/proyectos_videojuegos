using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class buttonHoverFx : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Hover - escala")]
    [SerializeField] private float hoverScale = 1.06f;
    [SerializeField] private float duration = 0.15f;

    [Header("Hover - glow (opcional)")]
    [SerializeField] private Image hoverGlow;          // Image de resplandor detrás del botón
    [SerializeField] private float glowMaxAlpha = 0.6f;

    [Header("Click - destello")]
    [SerializeField] private Image targetImage;        // la Image del propio botón
    [SerializeField] private Color flashColor = new Color(1f, 0.9f, 0.5f);
    [SerializeField] private float flashDuration = 0.2f;

    private Vector3 baseScale;
    private Coroutine scaleRoutine;
    private Coroutine glowRoutine;
    private Coroutine flashRoutine;
    private Color originalColor;
    private void Awake()
    {
        baseScale = transform.localScale;
        if (hoverGlow != null) setGlowAlpha(0f);   // empieza invisible
        if (targetImage != null) originalColor = targetImage.color;
    }

    // ---------- HOVER ----------
    public void OnPointerEnter(PointerEventData e)
    {
        scaleTo(baseScale * hoverScale);
        glowTo(glowMaxAlpha);
    }

    public void OnPointerExit(PointerEventData e)
    {
        scaleTo(baseScale);
        glowTo(0f);
    }

    private void scaleTo(Vector3 target)
    {
        if (scaleRoutine != null) StopCoroutine(scaleRoutine);
        scaleRoutine = StartCoroutine(animateScale(target));
    }

    private IEnumerator animateScale(Vector3 target)
    {
        Vector3 start = transform.localScale;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            transform.localScale = Vector3.LerpUnclamped(start, target, easeOutQuad(Mathf.Clamp01(t)));
            yield return null;
        }
        transform.localScale = target;
    }

    private void glowTo(float targetAlpha)
    {
        if (hoverGlow == null) return;
        if (glowRoutine != null) StopCoroutine(glowRoutine);
        glowRoutine = StartCoroutine(animateGlow(targetAlpha));
    }

    private IEnumerator animateGlow(float targetAlpha)
    {
        float start = hoverGlow.color.a;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            setGlowAlpha(Mathf.Lerp(start, targetAlpha, Mathf.Clamp01(t)));
            yield return null;
        }
        setGlowAlpha(targetAlpha);
    }

    private void setGlowAlpha(float a)
    {
        var c = hoverGlow.color;
        c.a = a;
        hoverGlow.color = c;
    }

    // ---------- CLICK - destello ----------
    public void OnPointerClick(PointerEventData e)
    {
        // si el objeto ya no está activo, no intentes arrancar la corrutina
        if (!isActiveAndEnabled) return;
        if (targetImage == null) return;

        if (flashRoutine != null) StopCoroutine(flashRoutine);
        flashRoutine = StartCoroutine(colorPulse());
    }

    private IEnumerator colorPulse()
    {
        if (targetImage == null) yield break;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / (flashDuration * 0.3f);
            targetImage.color = Color.Lerp(originalColor, flashColor, Mathf.Clamp01(t));
            yield return null;
        }
        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / (flashDuration * 0.7f);
            targetImage.color = Color.Lerp(flashColor, originalColor, Mathf.Clamp01(t));
            yield return null;
        }
        targetImage.color = originalColor;
    }
    private void OnDisable()
    {
        // detener cualquier animación y dejar el botón en estado neutro
        StopAllCoroutines();
        transform.localScale = baseScale;
        if (targetImage != null && originalColor.a > 0f)
            targetImage.color = originalColor;
        if (hoverGlow != null) setGlowAlpha(0f);
    }
    private static float easeOutQuad(float t) => 1f - (1f - t) * (1f - t);
}