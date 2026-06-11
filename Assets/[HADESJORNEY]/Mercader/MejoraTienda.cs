using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Dapasa.Audio;

public class MejoraTienda : MonoBehaviour
{
    public enum TipoMejora
    {
        GranadaPersefone,
        SalAres
    }

    [Header("Mejora")]
    public TipoMejora tipoMejora;
    public int coste = 0;

    [Header("UI")]
    public TextMeshProUGUI textoPrecio;
    public Button botonComprar;

    [Header("Audio")]
    [SerializeField] private string idSonidoCompraExitosa = "CompraExitosa";
    [SerializeField] private string idSonidoNoCompra = "nocompra";

    private Color colorNormal = Color.white;
    private Color colorBloqueado = Color.red;

    private void Start()
    {
        ActualizarEstado();

        if (InventarioAlmas.Instancia != null)
        {
            InventarioAlmas.Instancia.OnAlmasCambiaron.AddListener(
                delegate { ActualizarEstado(); }
            );
        }

        if (botonComprar != null)
        {
            botonComprar.onClick.AddListener(Comprar);
        }
    }

    private void OnDestroy()
    {
        if (botonComprar != null)
        {
            botonComprar.onClick.RemoveListener(Comprar);
        }
    }

    private void ActualizarEstado()
    {
        if (textoPrecio != null)
        {
            textoPrecio.text = coste.ToString();
        }

        bool puedeComprar = PuedeComprar();

        if (textoPrecio != null)
        {
            textoPrecio.color = puedeComprar
                ? colorNormal
                : colorBloqueado;
        }

        /*
         * Se mantiene activo incluso cuando no se puede comprar.
         * Así el jugador puede pulsarlo y escuchar "nocompra".
         */
        if (botonComprar != null)
        {
            botonComprar.interactable = true;
        }
    }

    private bool PuedeComprar()
    {
        if (InventarioAlmas.Instancia == null)
        {
            return false;
        }

        if (InventarioAlmas.Instancia.Almas < coste)
        {
            return false;
        }

        if (EstadisticasJugador.Instancia == null)
        {
            return false;
        }

        if (tipoMejora == TipoMejora.GranadaPersefone)
        {
            return EstadisticasJugador.Instancia.PuedeMejorarVida();
        }

        if (tipoMejora == TipoMejora.SalAres)
        {
            return EstadisticasJugador.Instancia.PuedeMejorarDanio();
        }

        return false;
    }

    public void Comprar()
    {
        // No tiene suficientes almas o la mejora está al máximo.
        if (!PuedeComprar())
        {
            ReproducirSonido(idSonidoNoCompra);
            ActualizarEstado();
            return;
        }

        bool pagado = InventarioAlmas.Instancia.GastarAlmas(coste);

        // Seguridad adicional por si no se consigue realizar el pago.
        if (!pagado)
        {
            ReproducirSonido(idSonidoNoCompra);
            ActualizarEstado();
            return;
        }

        AplicarMejora();

        // La compra se ha pagado y la mejora se ha aplicado.
        ReproducirSonido(idSonidoCompraExitosa);

        ActualizarEstado();
    }

    private void AplicarMejora()
    {
        if (EstadisticasJugador.Instancia == null)
        {
            return;
        }

        if (tipoMejora == TipoMejora.GranadaPersefone)
        {
            EstadisticasJugador.Instancia.MejorarVida();
        }
        else if (tipoMejora == TipoMejora.SalAres)
        {
            EstadisticasJugador.Instancia.MejorarDanio();
        }
    }

    private void ReproducirSonido(string idSonido)
    {
        if (AudioManager.Instance == null)
        {
            Debug.LogWarning(
                "MejoraTienda: no se ha encontrado el AudioManager.",
                this
            );

            return;
        }

        AudioManager.Instance.ReproducirSFX2D(idSonido);
    }
}