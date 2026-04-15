using UnityEngine;
using TMPro;

public class MercaderInteractuar : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject textoInteraccion;
    [SerializeField] private GameObject bocadillo;
    [SerializeField] private TMP_Text textoBocadillo;
    [SerializeField] private string mensajeMercader = "¡Querido Alástor! ¿Qué deseas comprar?";

    [Header("Tienda")]
    [SerializeField] private GameObject tienda;

    private bool jugadorCerca = false;
    private bool bocadilloMostrado = false;
    private bool tiendaAbierta = false;

    private void Start()
    {
        if (textoInteraccion != null)
        {
            textoInteraccion.SetActive(false);
        }

        if (bocadillo != null)
        {
            bocadillo.SetActive(false);
        }

        if (tienda != null)
        {
            tienda.SetActive(false);
        }
    }

    private void Update()
    {
        if (!jugadorCerca || tiendaAbierta)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (!bocadilloMostrado)
            {
                MostrarBocadillo();
            }
            else
            {
                AbrirTienda();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        jugadorCerca = true;

        if (!tiendaAbierta && !bocadilloMostrado && textoInteraccion != null)
        {
            textoInteraccion.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        jugadorCerca = false;
        bocadilloMostrado = false;

        if (textoInteraccion != null)
        {
            textoInteraccion.SetActive(false);
        }

        if (bocadillo != null)
        {
            bocadillo.SetActive(false);
        }
    }

    private void MostrarBocadillo()
    {
        bocadilloMostrado = true;

        if (textoInteraccion != null)
        {
            textoInteraccion.SetActive(false);
        }

        if (textoBocadillo != null)
        {
            textoBocadillo.text = mensajeMercader;
        }

        if (bocadillo != null)
        {
            bocadillo.SetActive(true);
        }
    }

    private void AbrirTienda()
    {
        if (tiendaAbierta || tienda == null)
        {
            return;
        }

        tiendaAbierta = true;

        if (textoInteraccion != null)
        {
            textoInteraccion.SetActive(false);
        }

        if (bocadillo != null)
        {
            bocadillo.SetActive(false);
        }

        tienda.SetActive(true);
    }

    public void CerrarTienda()
    {
        tiendaAbierta = false;
        bocadilloMostrado = false;

        if (tienda != null)
        {
            tienda.SetActive(false);
        }

        if (bocadillo != null)
        {
            bocadillo.SetActive(false);
        }

        if (jugadorCerca && textoInteraccion != null)
        {
            textoInteraccion.SetActive(true);
        }
    }
}