using UnityEngine;

public class MercaderInteractuar : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject textoInteraccion;

    [Header("Tienda")]
    [SerializeField] private GameObject shopUI;

    private bool jugadorCerca = false;

    void Update()
    {
        if (jugadorCerca && Input.GetKeyDown(KeyCode.E))
        {
            AbrirTienda();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            jugadorCerca = true;

            if (textoInteraccion != null)
                textoInteraccion.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            jugadorCerca = false;

            if (textoInteraccion != null)
                textoInteraccion.SetActive(false);
        }
    }

    void AbrirTienda()
    {
        Debug.Log("Tienda abierta");

        if (shopUI != null)
            shopUI.SetActive(true);
    }
}