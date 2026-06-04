using UnityEngine;
using TMPro;

public class HUDAlmas : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textoAlmas;
    [SerializeField] private TextMeshProUGUI textoFragmentos;

    [Header("Animación (opcional)")]
    [SerializeField] private contadorAlmasFx fxAlmas;
    [SerializeField] private contadorAlmasFx fxFragmentos;

    void Start()
    {
        if (InventarioAlmas.Instancia == null) return;
        InventarioAlmas.Instancia.OnAlmasCambiaron.AddListener(ActualizarAlmas);
        InventarioAlmas.Instancia.OnFragmentosCambiaron.AddListener(ActualizarFragmentos);
        ActualizarAlmas(InventarioAlmas.Instancia.Almas);
        ActualizarFragmentos(InventarioAlmas.Instancia.Fragmentos);
    }

    void ActualizarAlmas(int valor)
    {
        if (fxAlmas != null)
            fxAlmas.MostrarValor(valor);
        else if (textoAlmas != null)
            textoAlmas.text = valor.ToString();
    }

    void ActualizarFragmentos(int valor)
    {
        if (fxFragmentos != null)
            fxFragmentos.MostrarValor(valor);
        else if (textoFragmentos != null)
            textoFragmentos.text = valor.ToString();
    }

    void OnDestroy()
    {
        if (InventarioAlmas.Instancia != null)
        {
            InventarioAlmas.Instancia.OnAlmasCambiaron.RemoveListener(ActualizarAlmas);
            InventarioAlmas.Instancia.OnFragmentosCambiaron.RemoveListener(ActualizarFragmentos);
        }
    }
}