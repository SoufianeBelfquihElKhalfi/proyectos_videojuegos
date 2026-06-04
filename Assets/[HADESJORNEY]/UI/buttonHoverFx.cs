using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class buttonHoverFx : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Hover")]
    [SerializeField] private float hoverScale = 1.06f;
    [SerializeField] private float duration = 0.15f;

    [Header("Shake al pulsar")]
    [SerializeField] private float shakeDuration = 0.25f;
    [SerializeField] private float shakeMagnitude = 12f;   // píxeles de temblor

    private Vector3 baseScale;
    private Vector3 basePos;
    private Coroutine hoverRoutine;
    private Coroutine shakeRoutine;

    private void Awake()
    {
        baseScale = transform.localScale;
        basePos = transform.localPosition;
    }

    // ---------- HOVER ----------
    public void OnPointerEnter(PointerEventData e) => scaleTo(baseScale * hoverScale);
    public void OnPointerExit(PointerEventData e) => scaleTo(baseScale);

    private void scaleTo(Vector3 target)
    {
        if (hoverRoutine != null) StopCoroutine(hoverRoutine);
        hoverRoutine = StartCoroutine(animateScale(target));
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

    // ---------- SHAKE AL PULSAR ----------
    public void OnPointerClick(PointerEventData e)
    {
        if (shakeRoutine != null) StopCoroutine(shakeRoutine);
        shakeRoutine = StartCoroutine(shake());
    }

    private IEnumerator shake()
    {
        float t = 0f;
        float frequency = 35f;          // vibraciones por segundo (alto = nervioso)
        float seed = Random.value * 100f;
        while (t < shakeDuration)
        {
            t += Time.deltaTime;
            float damper = 1f - (t / shakeDuration);
            // Mathf.PerlinNoise da un temblor coherente y rápido, no saltos aleatorios puros
            float offsetX = (Mathf.PerlinNoise(seed, t * frequency) - 0.5f) * 2f * shakeMagnitude * damper;
            float offsetY = (Mathf.PerlinNoise(seed + 50f, t * frequency) - 0.5f) * 2f * shakeMagnitude * 0.4f * damper; // Y reducido
            transform.localPosition = basePos + new Vector3(offsetX, offsetY, 0f);
            yield return null;
        }
        transform.localPosition = basePos;
    }

    private static float easeOutQuad(float t) => 1f - (1f - t) * (1f - t);
}