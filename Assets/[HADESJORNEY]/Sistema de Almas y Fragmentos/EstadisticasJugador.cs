using UnityEngine;

public class EstadisticasJugador : MonoBehaviour
{
    public static EstadisticasJugador Instancia;

    [Header("Vida")]
    public int limiteCorazones = 3;

    [Header("Daño")]
    public float multiplicadorDanio = 1f;

    private SistemaVida sistemaVida;

    void Awake()
    {
        Instancia = this;
        sistemaVida = GetComponent<SistemaVida>();
    }

    public bool PuedeMejorarVida()
    {
        if (sistemaVida == null) return false;
        return sistemaVida.corazonesMaximos < limiteCorazones;
    }

    public void MejorarVida()
    {
        if (!PuedeMejorarVida()) return;

        sistemaVida.CambiarCorazonesMaximos(sistemaVida.corazonesMaximos + 1, true);
        Debug.Log("Vida máxima mejorada.");
    }

    public void MejorarDanio()
    {
        multiplicadorDanio += 0.10f;
        Debug.Log("Daño mejorado. Multiplicador: " + multiplicadorDanio);
    }
}