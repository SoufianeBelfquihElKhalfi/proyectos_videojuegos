using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MejoraTienda : MonoBehaviour
{
    public enum TipoMejora
    {
        GranadaPersefone,
        SalAres
    }

    [Header("Mejora")]
    public TipoMejora tipoMejora;
    public int coste = 60;

    [Header("UI")]
    public TextMeshProUGUI textoPrecio;
    public Button botonComprar;

    [Header("Sonido")]
    public AudioSource audioSource;
    public AudioClip sonidoError;

    private Color colorNormal = Color.white;
    private Color colorBloqueado = Color.red;

    void Start()
    {
        ActualizarEstado();

        if (InventarioAlmas.Instancia != null)
        {
            InventarioAlmas.Instancia.OnAlmasCambiaron.AddListener(delegate { ActualizarEstado(); });
        }

        if (botonComprar != null)
        {
            botonComprar.onClick.AddListener(Comprar);
        }
    }

    void ActualizarEstado()
    {
        if (textoPrecio != null)
        {
            textoPrecio.text = coste.ToString();
        }

        bool puedeComprar = PuedeComprar();

        if (textoPrecio != null)
        {
            textoPrecio.color = puedeComprar ? colorNormal : colorBloqueado;
        }
    }

    bool PuedeComprar()
    {
        if (InventarioAlmas.Instancia == null) return false;
        if (InventarioAlmas.Instancia.Almas < coste) return false;

        if (tipoMejora == TipoMejora.GranadaPersefone)
        {
            if (EstadisticasJugador.Instancia == null) return false;
            return EstadisticasJugador.Instancia.PuedeMejorarVida();
        }

        return true;
    }

    public void Comprar()
    {
        if (!PuedeComprar())
        {
            ReproducirError();
            ActualizarEstado();
            return;
        }

        bool pagado = InventarioAlmas.Instancia.GastarAlmas(coste);

        if (!pagado)
        {
            ReproducirError();
            ActualizarEstado();
            return;
        }

        AplicarMejora();
        ActualizarEstado();
    }

    void AplicarMejora()
    {
        if (EstadisticasJugador.Instancia == null) return;

        if (tipoMejora == TipoMejora.GranadaPersefone)
        {
            EstadisticasJugador.Instancia.MejorarVida();
        }
        else if (tipoMejora == TipoMejora.SalAres)
        {
            EstadisticasJugador.Instancia.MejorarDanio();
        }
    }

    void ReproducirError()
    {
        if (audioSource != null && sonidoError != null)
        {
            audioSource.PlayOneShot(sonidoError);
        }
    }
}