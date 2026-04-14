using UnityEngine;

public class MercaderInteractuar : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject textoInteraccion;

    [Header("Tienda")]
    [SerializeField] private GameObject tienda;

    private bool jugadorCerca = false;
    private bool tiendaAbierta = false;

    void Start()
    {
        if (textoInteraccion != null)
            textoInteraccion.SetActive(false);

        if (tienda != null)
            tienda.SetActive(false);
    }

    void Update()
    {
        if (jugadorCerca && Input.GetKeyDown(KeyCode.E) && !tiendaAbierta)
        {
            AbrirTienda();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            jugadorCerca = true;

            if (textoInteraccion != null && !tiendaAbierta)
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
        tiendaAbierta = true;

        if (textoInteraccion != null)
            textoInteraccion.SetActive(false);

        if (tienda != null)
        {
            Debug.Log("Abriendo HUD de tienda");
            tienda.SetActive(true);
        }
    }

    public void CerrarTienda()
    {
        tiendaAbierta = false;

        if (tienda != null)
            tienda.SetActive(false);

        if (jugadorCerca && textoInteraccion != null)
            textoInteraccion.SetActive(true);
    }
}