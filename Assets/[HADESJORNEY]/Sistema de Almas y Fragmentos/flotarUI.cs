using System.Collections;
using UnityEngine;

public class flotarUI : MonoBehaviour
{
    [SerializeField] private float amplitude = 5f;     // píxeles arriba/abajo
    [SerializeField] private float duration = 2.5f;    // lento

    private void Start() => StartCoroutine(flotar());

    private IEnumerator flotar()
    {
        Vector3 basePos = transform.localPosition;
        while (true)
        {
            float t = 0f;
            float half = duration * 0.5f;
            while (t < half)   // sube
            {
                t += Time.deltaTime;
                transform.localPosition = basePos + new Vector3(0f, Mathf.Lerp(0f, amplitude, easeInOutSine(t / half)), 0f);
                yield return null;
            }
            t = 0f;
            while (t < half)   // baja
            {
                t += Time.deltaTime;
                transform.localPosition = basePos + new Vector3(0f, Mathf.Lerp(amplitude, 0f, easeInOutSine(t / half)), 0f);
                yield return null;
            }
        }
    }

    private static float easeInOutSine(float t) => -(Mathf.Cos(Mathf.PI * t) - 1f) * 0.5f;
}