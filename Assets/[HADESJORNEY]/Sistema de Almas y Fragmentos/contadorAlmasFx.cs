using System.Collections;
using UnityEngine;
using TMPro;

public class contadorAlmasFx : MonoBehaviour
{
    [SerializeField] private TMP_Text texto;
    [SerializeField] private float countDuration = 0.4f;   // cuánto tarda en contar
    [SerializeField] private float punchScale = 1.3f;      // cuánto crece al cambiar
    [SerializeField] private float punchDuration = 0.2f;

    [Header("Icono (opcional)")]
    [SerializeField] private Transform icono;
    [SerializeField] private float iconPunchScale = 1.25f;
    [SerializeField] private float iconPunchDuration = 0.25f;

    private Vector3 iconoBaseScale;
    private Coroutine iconoRoutine;
    private int valorActual;
    private Coroutine countRoutine;
    private Coroutine punchRoutine;
    private Vector3 baseScale;
    private bool primeraVez = true;

    private void Awake()
    {
        if (texto == null) texto = GetComponent<TMP_Text>();
        baseScale = texto.transform.localScale;
        if (icono != null) iconoBaseScale = icono.localScale;
    }

    public void MostrarValor(int nuevo)
    {
        // la primera vez (al cargar) no animamos, solo ponemos el número
        if (primeraVez)
        {
            primeraVez = false;
            valorActual = nuevo;
            texto.text = nuevo.ToString();
            return;
        }

        if (countRoutine != null) StopCoroutine(countRoutine);
        countRoutine = StartCoroutine(contarHacia(nuevo));

        if (icono != null)
        {
            if (iconoRoutine != null) StopCoroutine(iconoRoutine);
            iconoRoutine = StartCoroutine(punchIcono());
        }
    }

    private IEnumerator contarHacia(int objetivo)
    {
        int inicio = valorActual;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / countDuration;
            int v = Mathf.RoundToInt(Mathf.Lerp(inicio, objetivo, Mathf.Clamp01(t)));
            texto.text = v.ToString();
            yield return null;
        }
        texto.text = objetivo.ToString();
        valorActual = objetivo;
    }

    private IEnumerator punch()
    {
        var tr = texto.transform;
        tr.localScale = baseScale * punchScale;   // crece de golpe
        float t = 0f;
        while (t < 1f)                              // y vuelve suave
        {
            t += Time.deltaTime / punchDuration;
            tr.localScale = Vector3.LerpUnclamped(baseScale * punchScale, baseScale, easeOutQuad(Mathf.Clamp01(t)));
            yield return null;
        }
        tr.localScale = baseScale;
    }
    private IEnumerator punchIcono()
    {
        icono.localScale = iconoBaseScale * iconPunchScale;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / iconPunchDuration;
            icono.localScale = Vector3.LerpUnclamped(iconoBaseScale * iconPunchScale, iconoBaseScale, easeOutQuad(Mathf.Clamp01(t)));
            yield return null;
        }
        icono.localScale = iconoBaseScale;
    }
    private static float easeOutQuad(float t) => 1f - (1f - t) * (1f - t);
}