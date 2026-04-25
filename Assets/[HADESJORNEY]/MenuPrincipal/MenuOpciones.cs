using UnityEngine;
using UnityEngine.UI;

public class OpcionesManager : MonoBehaviour
{
    public Slider sliderVolumen;
    public Toggle togglePantallaCompleta;

    void Start()
    {
        sliderVolumen.value = 0.5f;
        togglePantallaCompleta.isOn = Screen.fullScreen;

        sliderVolumen.onValueChanged.AddListener(CambiarVolumen);
        togglePantallaCompleta.onValueChanged.AddListener(CambiarPantallaCompleta);
    }

    public void CambiarVolumen(float valor)
    {
        Debug.Log("Volumen simulado: " + valor);
    }

    public void CambiarPantallaCompleta(bool pantallaCompleta)
    {
        Screen.fullScreen = pantallaCompleta;
        Debug.Log("Pantalla completa: " + pantallaCompleta);
    }
}