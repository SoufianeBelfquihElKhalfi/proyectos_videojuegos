using UnityEngine;
using UnityEngine.UI;
using Dapasa.Audio;

public class MenuOpciones : MonoBehaviour
{
    [Header("Sliders de volumen (0 a 1)")]
    [SerializeField] private Slider sliderMaster;
    [SerializeField] private Slider sliderMusica;
    [SerializeField] private Slider sliderSFX;

    [Header("Pantalla")]
    [SerializeField] private Toggle togglePantallaCompleta;

    private void Start()
    {
        // Pantalla completa
        if (togglePantallaCompleta != null)
        {
            togglePantallaCompleta.isOn = Screen.fullScreen;
            togglePantallaCompleta.onValueChanged.AddListener(CambiarPantallaCompleta);
        }

        // Sliders -> inicializa con el valor actual del mixer y engancha
        InicializarSlider(sliderMaster, CambiarMaster);
        InicializarSlider(sliderMusica, CambiarMusica);
        InicializarSlider(sliderSFX, CambiarSFX);
    }

    private void InicializarSlider(Slider slider, UnityEngine.Events.UnityAction<float> callback)
    {
        if (slider == null) return;
        slider.minValue = 0.0001f;   // evita el 0 absoluto (log de 0 es -infinito)
        slider.maxValue = 1f;
        slider.onValueChanged.AddListener(callback);
    }

    // Convierte el valor del slider (0-1) a decibelios para el mixer
    private float ABarrios(float valor01)
    {
        return Mathf.Log10(Mathf.Clamp(valor01, 0.0001f, 1f)) * 20f;
    }

    private void CambiarMaster(float v)
    {
        if (AudioVolumeManager.Instance != null)
            AudioVolumeManager.Instance.MasterVolume = ABarrios(v);
    }

    private void CambiarMusica(float v)
    {
        if (AudioVolumeManager.Instance != null)
            AudioVolumeManager.Instance.MusicVolume = ABarrios(v);
    }

    private void CambiarSFX(float v)
    {
        if (AudioVolumeManager.Instance != null)
            AudioVolumeManager.Instance.SfxVolume = ABarrios(v);
    }

    private void CambiarPantallaCompleta(bool pantallaCompleta)
    {
        Screen.fullScreen = pantallaCompleta;
    }
}