
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
    [SerializeField] private TipoMejora tipoMejora;
    [SerializeField] private int coste = 60;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI textoPrecio;
    [SerializeField] private Button botonComprar;

    [Header("Audio")]
    [SerializeField] private string idSonidoCompraExitosa = "CompraExitosa";
    [SerializeField] private string idSonidoNoCompra = "nocompra";

    private readonly Color colorDisponible = Color.white;
    private readonly Color colorNoDisponible = Color.red;

    private void Start()
    {
        if (botonComprar != null)
        {
            botonComprar.onClick.AddListener(Comprar);
        }
        else
        {
            Debug.LogWarning(
                "MejoraTienda: no se ha asignado el botón de compra.",
                this
            );
        }

        if (InventarioAlmas.Instancia != null)
        {
            InventarioAlmas.Instancia.OnAlmasCambiaron.AddListener(
                AlCambiarAlmas
            );
        }

        ActualizarEstado();
    }

    private void OnDestroy()
    {
        if (botonComprar != null)
        {
            botonComprar.onClick.RemoveListener(Comprar);
        }

        if (InventarioAlmas.Instancia != null)
        {
            InventarioAlmas.Instancia.OnAlmasCambiaron.RemoveListener(
                AlCambiarAlmas
            );
        }
    }

    private void AlCambiarAlmas(int almasActuales)
    {
        ActualizarEstado();
    }

    private void ActualizarEstado()
    {
        if (textoPrecio != null)
        {
            textoPrecio.text = coste.ToString();

            textoPrecio.color = PuedeComprar()
                ? colorDisponible
                : colorNoDisponible;
        }

        /*
         * El botón debe seguir activo aunque falten almas.
         * De esta forma se puede pulsar y reproducir "nocompra".
         */
        if (botonComprar != null)
        {
            botonComprar.interactable = true;
        }
    }

    private bool TieneAlmasSuficientes()
    {
        return InventarioAlmas.Instancia != null &&
               InventarioAlmas.Instancia.Almas >= coste;
    }

    private bool MejoraDisponible()
    {
        if (EstadisticasJugador.Instancia == null)
        {
            return false;
        }

        switch (tipoMejora)
        {
            case TipoMejora.GranadaPersefone:
                return EstadisticasJugador.Instancia.PuedeMejorarVida();

            case TipoMejora.SalAres:
                return EstadisticasJugador.Instancia.PuedeMejorarDanio();

            default:
                return false;
        }
    }

    private bool PuedeComprar()
    {
        return TieneAlmasSuficientes() && MejoraDisponible();
    }

    public void Comprar()
    {
        if (InventarioAlmas.Instancia == null)
        {
            Debug.LogWarning(
                "MejoraTienda: no se ha encontrado InventarioAlmas.",
                this
            );

            return;
        }

        if (EstadisticasJugador.Instancia == null)
        {
            Debug.LogWarning(
                "MejoraTienda: no se ha encontrado EstadisticasJugador.",
                this
            );

            return;
        }

        /*
         * Si faltan almas o la mejora ya está al máximo,
         * no realiza la compra y reproduce "nocompra".
         */
        if (!PuedeComprar())
        {
            ReproducirSonido(idSonidoNoCompra);
            ActualizarEstado();
            return;
        }

        bool compraPagada =
            InventarioAlmas.Instancia.GastarAlmas(coste);

        /*
         * Comprobación adicional por si el pago no puede realizarse.
         */
        if (!compraPagada)
        {
            ReproducirSonido(idSonidoNoCompra);
            ActualizarEstado();
            return;
        }

        AplicarMejora();

        /*
         * La compra se ha pagado y la mejora se ha aplicado.
         */
        ReproducirSonido(idSonidoCompraExitosa);

        ActualizarEstado();
    }

    private void AplicarMejora()
    {
        switch (tipoMejora)
        {
            case TipoMejora.GranadaPersefone:

                EstadisticasJugador.Instancia.MejorarVida();
                break;

            case TipoMejora.SalAres:

                EstadisticasJugador.Instancia.MejorarDanio();
                break;
        }
    }

    private void ReproducirSonido(string idSonido)
    {
        if (string.IsNullOrWhiteSpace(idSonido))
        {
            return;
        }

        if (AudioManager.Instance == null)
        {
            Debug.LogWarning(
                "MejoraTienda: no se ha encontrado AudioManager.",
                this
            );

            return;
        }

        AudioManager.Instance.ReproducirSFX2D(idSonido);
    }
}

