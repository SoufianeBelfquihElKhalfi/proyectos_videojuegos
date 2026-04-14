using UnityEngine;

public class BotonCerrarTienda : MonoBehaviour
{
    [SerializeField] private MercaderInteractuar mercader;

    public void CerrarTienda()
    {
        if (mercader != null)
            mercader.CerrarTienda();
    }
}