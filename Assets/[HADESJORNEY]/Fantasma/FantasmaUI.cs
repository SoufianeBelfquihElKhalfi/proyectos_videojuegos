using UnityEngine;
using UnityEngine.UI;
using TMPro;

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

    void Start()
    {
        // El fondo siempre se ve completo
        if (iconoFondo != null)
        {
            iconoFondo.fillAmount = 1f;
            iconoFondo.color = colorCooldown;
        }
    }

    void Update()
    {
        if (fantasmaCombate == null) return;

        ActualizarIcono();
        ActualizarTextoCooldown();
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
        }
        else
        {
            textoCooldown.gameObject.SetActive(false);
        }
    }
}