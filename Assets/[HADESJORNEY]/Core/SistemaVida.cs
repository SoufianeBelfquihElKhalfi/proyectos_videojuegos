using UnityEngine;
using UnityEngine.Events;

public class SistemaVida : MonoBehaviour
{
    [Header("Vida en corazones")]
    [Min(1)] public int corazonesMaximos = 3;

    [Header("Vida inicial (opcional)")]
    [Min(0)] public int corazonesMitadIniciales = -1;
    // -1 = vida completa

    [Header("Eventos")]
    public UnityEvent alMorir;
    public UnityEvent<int, int> alCambiarVida;
    // vidaActual, vidaMaxima

    private int corazonesMitadMaximos;
    private int corazonesMitadActuales;

    public int VidaActual => corazonesMitadActuales;
    public int VidaMaxima => corazonesMitadMaximos;
    public bool EstaMuerto => corazonesMitadActuales <= 0;

    private void Awake()
    {
        corazonesMitadMaximos = corazonesMaximos * 2;

        if (corazonesMitadIniciales < 0)
            corazonesMitadActuales = corazonesMitadMaximos;
        else
            corazonesMitadActuales = Mathf.Clamp(corazonesMitadIniciales, 0, corazonesMitadMaximos);

        NotificarCambioVida();
    }

    public void RecibirDanio(int danioMitadCorazones)
    {
        if (EstaMuerto) return;
        if (danioMitadCorazones <= 0) return;

        corazonesMitadActuales -= danioMitadCorazones;
        corazonesMitadActuales = Mathf.Clamp(corazonesMitadActuales, 0, corazonesMitadMaximos);

        NotificarCambioVida();

        if (corazonesMitadActuales <= 0)
        {
            Morir();
        }
    }

    public void Curar(int curacionMitadCorazones)
    {
        if (EstaMuerto) return;
        if (curacionMitadCorazones <= 0) return;

        corazonesMitadActuales += curacionMitadCorazones;
        corazonesMitadActuales = Mathf.Clamp(corazonesMitadActuales, 0, corazonesMitadMaximos);

        NotificarCambioVida();
    }

    public void CambiarCorazonesMaximos(int nuevosCorazones, bool rellenarVida = true)
    {
        corazonesMaximos = Mathf.Max(1, nuevosCorazones);
        corazonesMitadMaximos = corazonesMaximos * 2;

        if (rellenarVida)
            corazonesMitadActuales = corazonesMitadMaximos;
        else
            corazonesMitadActuales = Mathf.Clamp(corazonesMitadActuales, 0, corazonesMitadMaximos);

        NotificarCambioVida();
    }

    private void Morir()
    {
        alMorir?.Invoke();
    }

    private void NotificarCambioVida()
    {
        alCambiarVida?.Invoke(corazonesMitadActuales, corazonesMitadMaximos);
    }
}