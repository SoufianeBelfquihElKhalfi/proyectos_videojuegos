using UnityEngine;
using System.Collections;

public class shakeCamara : MonoBehaviour
{
    public static shakeCamara Instancia;

    private Coroutine shakeActivo;

    void Awake() => Instancia = this;

    public void Shake(float duracion, float fuerza)
    {
        Debug.Log("SHAKE llamado en tiempo: " + Time.time + "\nStackTrace: " + System.Environment.StackTrace);
        if (shakeActivo != null) StopCoroutine(shakeActivo);
        shakeActivo = StartCoroutine(Rutina(duracion, fuerza));
    }

    IEnumerator Rutina(float duracion, float fuerza)
    {
        Camera cam = Camera.main;
        if (cam == null) yield break;

        Transform t = cam.transform;
        Vector3 posOriginal = t.localPosition;
        float tiempo = 0f;

        while (tiempo < duracion)
        {
            Vector3 offset = Random.insideUnitSphere * fuerza;
            t.localPosition = posOriginal + offset;
            tiempo += Time.unscaledDeltaTime;
            yield return null;
        }

        t.localPosition = posOriginal;
        shakeActivo = null;
    }
}