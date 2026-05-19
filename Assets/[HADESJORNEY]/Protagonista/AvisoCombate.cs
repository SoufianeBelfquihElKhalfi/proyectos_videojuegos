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
}