using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class FantasmaUI : MonoBehaviour
{
    [Header("Referencias")]
    public FantasmaCombate fantasmaCombate;

    [Header("UI Elements")]
    public Image iconoFondo;
    public Image iconoFill;
    public TextMeshProUGUI textoCooldown;

    [Header("Colores")]
    public Color colorDisponible = Color.white;
    public Color colorEnCombate = new Color(1f, 1f, 0f, 1f);
    public Color colorCooldown = new Color(0.3f, 0.3f, 0.3f, 1f);

    [Header("Destello al estar lista")]
    public Transform iconoRaiz;            // el icono entero, para el punch
    public float punchScale = 1.25f;
    public float punchDuration = 0.25f;

    private bool estabaEnCooldown = false;
    private Vector3 baseScale;
    private Coroutine flashRoutine;

    [Header("Pulso del texto cooldown")]
    public float textoPunchScale = 1.4f;
    public float textoPunchDuration = 0.2f;

    private int ultimoSegundo = -1;
    private Vector3 textoBaseScale;
    private Coroutine textoPunchRoutine;

    [Header("Latido cuando está lista")]
    public float latidoScale = 1.08f;
    public float latidoDuracion = 2f;

    private Coroutine latidoRoutine;
    private bool estaLatiendo = false;
    private bool combateHabilitadoEnEscena = true;
    void Start()
    {
        if (textoCooldown != null) textoBaseScale = textoCooldown.transform.localScale;
        if (iconoFondo != null)
        {
            iconoFondo.fillAmount = 1f;
            iconoFondo.color = colorCooldown;
        }
        if (iconoRaiz != null) baseScale = iconoRaiz.localScale;
        combateHabilitadoEnEscena = SceneManager.GetActiveScene().name != "EscenaMercader";
    }

    void Update()
    {
        if (fantasmaCombate == null) return;
        ActualizarIcono();
        ActualizarTextoCooldown();
        DetectarListo();
    }

    void ActualizarIcono()
    {
        if (iconoFill == null) return;

        if (fantasmaCombate.EnModoCombate)
        {
            iconoFill.fillAmount = fantasmaCombate.TiempoRestanteCombate / fantasmaCombate.DuracionCombate;
            iconoFill.color = colorEnCombate;
        }
        else if (fantasmaCombate.TiempoRestanteCooldown > 0f)
        {
            iconoFill.fillAmount = 1f - (fantasmaCombate.TiempoRestanteCooldown / fantasmaCombate.Cooldown);
            iconoFill.color = colorCooldown;
        }
        else
        {
            iconoFill.fillAmount = 1f;
            iconoFill.color = colorDisponible;
        }
    }

    void ActualizarTextoCooldown()
    {
        if (textoCooldown == null) return;

        if (fantasmaCombate.TiempoRestanteCooldown > 0f && !fantasmaCombate.EnModoCombate)
        {
            int segundos = Mathf.CeilToInt(fantasmaCombate.TiempoRestanteCooldown);
            textoCooldown.text = segundos.ToString();
            textoCooldown.gameObject.SetActive(true);

            // si cambió el segundo respecto al frame anterior -> pulso
            if (segundos != ultimoSegundo)
            {
                ultimoSegundo = segundos;
                DispararPulsoTexto();
            }
        }
        else
        {
            textoCooldown.gameObject.SetActive(false);
            ultimoSegundo = -1;   // reset para la próxima vez
        }
    }

    void DetectarListo()
    {
        // en la sala del mercader (combate desactivado), nada de latido ni flash
        if (!combateHabilitadoEnEscena)
        {
            if (estaLatiendo) PararLatido();
            estabaEnCooldown = false;
            return;
        }
        bool enCooldownAhora = fantasmaCombate.TiempoRestanteCooldown > 0f && !fantasmaCombate.EnModoCombate;
        bool listaAhora = !enCooldownAhora && !fantasmaCombate.EnModoCombate;

        // acaba de quedar lista TRAS un cooldown -> flash (solo en la transición)
        if (estabaEnCooldown && listaAhora)
            DispararFlash();

        // está lista y aún no late -> arrancar latido (cubre inicio y post-recarga)
        if (listaAhora && !estaLatiendo)
            IniciarLatido();

        // ya no está lista -> parar latido
        if (!listaAhora && estaLatiendo)
            PararLatido();

        estabaEnCooldown = enCooldownAhora;
    }

    void DispararFlash()
    {
        if (iconoRaiz == null) return;
        if (flashRoutine != null) StopCoroutine(flashRoutine);
        flashRoutine = StartCoroutine(flashListo());
    }

    IEnumerator flashListo()
    {
        Vector3 big = baseScale * punchScale;
        iconoRaiz.localScale = big;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / punchDuration;
            iconoRaiz.localScale = Vector3.LerpUnclamped(big, baseScale, 1f - (1f - t) * (1f - t));
            yield return null;
        }
        iconoRaiz.localScale = baseScale;
    }
    void DispararPulsoTexto()
    {
        if (textoPunchRoutine != null) StopCoroutine(textoPunchRoutine);
        textoPunchRoutine = StartCoroutine(pulsoTexto());
    }

    IEnumerator pulsoTexto()
    {
        Vector3 big = textoBaseScale * textoPunchScale;
        textoCooldown.transform.localScale = big;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / textoPunchDuration;
            textoCooldown.transform.localScale = Vector3.LerpUnclamped(big, textoBaseScale, 1f - (1f - t) * (1f - t));
            yield return null;
        }
        textoCooldown.transform.localScale = textoBaseScale;
    }
    void IniciarLatido()
    {
        if (iconoRaiz == null) return;
        if (latidoRoutine != null) StopCoroutine(latidoRoutine);
        latidoRoutine = StartCoroutine(latido());
        estaLatiendo = true;
    }

    void PararLatido()
    {
        if (latidoRoutine != null) StopCoroutine(latidoRoutine);
        estaLatiendo = false;
        if (iconoRaiz != null) iconoRaiz.localScale = baseScale;
    }

    IEnumerator latido()
    {
        Vector3 big = baseScale * latidoScale;
        while (true)
        {
            // sube
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / (latidoDuracion * 0.5f);
                iconoRaiz.localScale = Vector3.LerpUnclamped(baseScale, big, easeInOutSine(Mathf.Clamp01(t)));
                yield return null;
            }
            // baja
            t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / (latidoDuracion * 0.5f);
                iconoRaiz.localScale = Vector3.LerpUnclamped(big, baseScale, easeInOutSine(Mathf.Clamp01(t)));
                yield return null;
            }
        }
    }

    static float easeInOutSine(float t) => -(Mathf.Cos(Mathf.PI * t) - 1f) * 0.5f;
}