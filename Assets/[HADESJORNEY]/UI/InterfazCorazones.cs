using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InterfazCorazones : MonoBehaviour
{
    [Header("Referencia a la vida")]
    public SistemaVida sistemaVida;

    [Header("UI")]
    public GameObject prefabCorazon;
    public Transform contenedorCorazones;

    [Header("Sprites")]
    public Sprite corazonLleno;
    public Sprite medioCorazon;
    public Sprite corazonVacio;

    [Header("Shake al recibir daño")]
    public float shakeMagnitude = 12f;
    public float shakeDuration = 0.3f;

    [Header("Punch del corazón perdido")]
    public float punchScale = 1.4f;
    public float punchDuration = 0.25f;

    [Header("Latido cuando la vida está baja")]
    [Range(0f, 1f)] public float umbralVidaBaja = 0.3f;   // 30% de vida
    public float latidoScale = 1.12f;
    public float latidoDuration = 0.7f;

    private List<Image> imagenesCorazones = new List<Image>();
    private int vidaAnterior = -1;
    private Vector3 contenedorBasePos;
    private Coroutine shakeRoutine;
    private Coroutine latidoRoutine;
    private bool estaLatiendo = false;

    private void Start()
    {
        if (sistemaVida == null) return;
        contenedorBasePos = contenedorCorazones.localPosition;
        CrearCorazones();
        vidaAnterior = sistemaVida.VidaActual;
        ActualizarCorazones(sistemaVida.VidaActual, sistemaVida.VidaMaxima);
        sistemaVida.alCambiarVida.AddListener(ActualizarCorazones);
    }

    private void OnDestroy()
    {
        if (sistemaVida != null)
            sistemaVida.alCambiarVida.RemoveListener(ActualizarCorazones);
    }

    private void CrearCorazones()
    {
        foreach (Transform hijo in contenedorCorazones)
            Destroy(hijo.gameObject);

        imagenesCorazones.Clear();
        int totalCorazones = sistemaVida.VidaMaxima / 2;
        for (int i = 0; i < totalCorazones; i++)
        {
            GameObject obj = Instantiate(prefabCorazon, contenedorCorazones);
            Image img = obj.GetComponent<Image>();
            imagenesCorazones.Add(img);
        }
    }

    public void ActualizarCorazones(int vidaActual, int vidaMaxima)
    {
        int totalCorazones = vidaMaxima / 2;
        if (imagenesCorazones.Count != totalCorazones)
            CrearCorazones();

        for (int i = 0; i < totalCorazones; i++)
        {
            int vidaRestante = vidaActual - (i * 2);
            if (vidaRestante >= 2)
                imagenesCorazones[i].sprite = corazonLleno;
            else if (vidaRestante == 1)
                imagenesCorazones[i].sprite = medioCorazon;
            else
                imagenesCorazones[i].sprite = corazonVacio;
        }

        // --- detectar qué pasó comparando con la vida anterior ---
        if (vidaAnterior >= 0 && vidaActual < vidaAnterior)
        {
            // recibió daño
            DispararShake();
            PunchUltimoCorazonConVida(vidaActual);
        }

        vidaAnterior = vidaActual;

        // --- latido si la vida está baja ---
        bool vidaBaja = vidaMaxima > 0 && (float)vidaActual / vidaMaxima <= umbralVidaBaja && vidaActual > 0;
        if (vidaBaja && !estaLatiendo) IniciarLatido();
        else if (!vidaBaja && estaLatiendo) PararLatido();
    }

    // ---------- SHAKE ----------
    private void DispararShake()
    {
        if (shakeRoutine != null) StopCoroutine(shakeRoutine);
        shakeRoutine = StartCoroutine(shake());
    }

    private IEnumerator shake()
    {
        float t = 0f;
        float seed = Random.value * 100f;
        while (t < shakeDuration)
        {
            t += Time.deltaTime;
            float damper = 1f - (t / shakeDuration);
            float x = (Mathf.PerlinNoise(seed, t * 40f) - 0.5f) * 2f * shakeMagnitude * damper;
            float y = (Mathf.PerlinNoise(seed + 50f, t * 40f) - 0.5f) * 2f * shakeMagnitude * 0.5f * damper;
            contenedorCorazones.localPosition = contenedorBasePos + new Vector3(x, y, 0f);
            yield return null;
        }
        contenedorCorazones.localPosition = contenedorBasePos;
    }

    // ---------- PUNCH DEL CORAZÓN ----------
    private void PunchUltimoCorazonConVida(int vidaActual)
    {
        // el corazón más a la derecha que aún tiene algo de vida (o el que acaba de vaciarse)
        int indice = Mathf.Clamp((vidaActual + 1) / 2 - 1, 0, imagenesCorazones.Count - 1);
        if (indice >= 0 && indice < imagenesCorazones.Count)
            StartCoroutine(punchCorazon(imagenesCorazones[indice].transform));
    }

    private IEnumerator punchCorazon(Transform corazon)
    {
        Vector3 baseScale = Vector3.one;
        Vector3 big = baseScale * punchScale;
        corazon.localScale = big;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / punchDuration;
            corazon.localScale = Vector3.LerpUnclamped(big, baseScale, 1f - (1f - t) * (1f - t));
            yield return null;
        }
        corazon.localScale = baseScale;
    }

    // ---------- LATIDO VIDA BAJA ----------
    private void IniciarLatido()
    {
        if (latidoRoutine != null) StopCoroutine(latidoRoutine);
        latidoRoutine = StartCoroutine(latido());
        estaLatiendo = true;
    }

    private void PararLatido()
    {
        if (latidoRoutine != null) StopCoroutine(latidoRoutine);
        estaLatiendo = false;
        contenedorCorazones.localScale = Vector3.one;
    }

    private IEnumerator latido()
    {
        Vector3 baseScale = Vector3.one;
        Vector3 big = baseScale * latidoScale;
        while (true)
        {
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / (latidoDuration * 0.5f);
                contenedorCorazones.localScale = Vector3.LerpUnclamped(baseScale, big, easeInOutSine(Mathf.Clamp01(t)));
                yield return null;
            }
            t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / (latidoDuration * 0.5f);
                contenedorCorazones.localScale = Vector3.LerpUnclamped(big, baseScale, easeInOutSine(Mathf.Clamp01(t)));
                yield return null;
            }
        }
    }

    private static float easeInOutSine(float t) => -(Mathf.Cos(Mathf.PI * t) - 1f) * 0.5f;
}