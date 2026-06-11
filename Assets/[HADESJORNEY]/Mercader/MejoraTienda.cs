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

    [Header("Mercader")]
    [SerializeField] private MercaderInteractuar mercaderInteractuar;

    [Header("Audio")]
    [SerializeField]
    private string idSonidoCompraExitosa =
        "CompraExitosa";

    [SerializeField]
    private string idSonidoErrorCompra =
        "error_compra";

    private readonly Color colorDisponible =
        Color.white;

    private readonly Color colorNoDisponible =
        Color.red;

    private void Start()
    {
        if (botonComprar != null)
        {
            botonComprar.onClick.AddListener(
                Comprar
            );
        }
        else
        {
            Debug.LogWarning(
                "MejoraTienda: no se ha asignado " +
                "el botón de compra.",
                this
            );
        }

        if (mercaderInteractuar == null)
        {
            mercaderInteractuar =
                FindFirstObjectByType<MercaderInteractuar>();
        }

        if (InventarioAlmas.Instancia != null)
        {
            InventarioAlmas.Instancia
                .OnAlmasCambiaron
                .AddListener(AlCambiarAlmas);
        }

        ActualizarEstado();
    }

    private void OnDestroy()
    {
        if (botonComprar != null)
        {
            botonComprar.onClick.RemoveListener(
                Comprar
            );
        }

        if (InventarioAlmas.Instancia != null)
        {
            InventarioAlmas.Instancia
                .OnAlmasCambiaron
                .RemoveListener(AlCambiarAlmas);
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

        if (botonComprar != null)
            botonComprar.interactable = true;
    }

    private bool TieneAlmasSuficientes()
    {
        return InventarioAlmas.Instancia != null &&
               InventarioAlmas.Instancia.Almas >= coste;
    }

    private bool MejoraDisponible()
    {
        if (EstadisticasJugador.Instancia == null)
            return false;

        switch (tipoMejora)
        {
            case TipoMejora.GranadaPersefone:
                return EstadisticasJugador
                    .Instancia
                    .PuedeMejorarVida();

            case TipoMejora.SalAres:
                return EstadisticasJugador
                    .Instancia
                    .PuedeMejorarDanio();

            default:
                return false;
        }
    }

    private bool PuedeComprar()
    {
        return TieneAlmasSuficientes() &&
               MejoraDisponible();
    }

    public void Comprar()
    {
        if (InventarioAlmas.Instancia == null)
        {
            Debug.LogWarning(
                "MejoraTienda: no se ha encontrado " +
                "InventarioAlmas.",
                this
            );

            return;
        }

        if (EstadisticasJugador.Instancia == null)
        {
            Debug.LogWarning(
                "MejoraTienda: no se ha encontrado " +
                "EstadisticasJugador.",
                this
            );

            return;
        }

        if (!PuedeComprar())
        {
            ReproducirSonido(
                idSonidoErrorCompra
            );

            ActualizarEstado();
            return;
        }

        bool compraPagada =
            InventarioAlmas.Instancia
                .GastarAlmas(coste);

        if (!compraPagada)
        {
            ReproducirSonido(
                idSonidoErrorCompra
            );

            ActualizarEstado();
            return;
        }

        AplicarMejora();

        ReproducirSonido(
            idSonidoCompraExitosa
        );

        if (mercaderInteractuar != null)
        {
            mercaderInteractuar
                .NotificarCompraRealizada();
        }
        else
        {
            Debug.LogWarning(
                "MejoraTienda: no se encontró " +
                "MercaderInteractuar.",
                this
            );
        }

        ActualizarEstado();
    }

    private void AplicarMejora()
    {
        switch (tipoMejora)
        {
            case TipoMejora.GranadaPersefone:

                EstadisticasJugador
                    .Instancia
                    .MejorarVida();

                break;

            case TipoMejora.SalAres:

                EstadisticasJugador
                    .Instancia
                    .MejorarDanio();

                break;
        }
    }

    private void ReproducirSonido(
        string idSonido
    )
    {
        if (string.IsNullOrWhiteSpace(idSonido))
            return;

        if (AudioManager.Instance == null)
        {
            Debug.LogWarning(
                "MejoraTienda: no se ha encontrado " +
                "AudioManager.",
                this
            );

            return;
        }

        AudioManager.Instance.ReproducirSFX2D(
            idSonido
        );
    }
}