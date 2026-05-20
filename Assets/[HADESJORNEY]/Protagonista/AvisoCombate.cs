using UnityEngine;

public class AvisoCombate : MonoBehaviour
{
    private CombateJugador combate;

    void Awake()
    {
        combate = GetComponentInParent<CombateJugador>();
    }

    public void AplicarDanioGolpe()
    {
        combate?.AplicarDanioGolpe();
    }
    public void AbrirVentanaCombo()
    {
        combate?.AbrirVentanaCombo();
    }
    public void CerrarVentanaCombo()
    {
        combate?.CerrarVentanaCombo();
    }
}