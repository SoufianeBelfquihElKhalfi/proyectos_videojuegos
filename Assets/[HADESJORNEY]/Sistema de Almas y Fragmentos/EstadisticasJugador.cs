using UnityEngine;

public class EstadisticasJugador : MonoBehaviour
{
    public static EstadisticasJugador Instancia;

    [Header("Granada de Perséfone")]
    public SistemaVida sistemaVidaJugador;
    public int maxMejorasVida = 3;
    public int corazonesExtraPorMejora = 1;

    [Header("Sal de Ares")]
    public float multiplicadorDanio = 1f;
    public float multiplicadorDanioMejorado = 2f;

    private int mejorasVidaCompradas = 0;
    private bool mejoraDanioComprada = false;

    // Guarda la mejora de daño entre escenas
    private static bool mejoraDanioGlobalComprada = false;
    private static float multiplicadorDanioGlobal = 1f;

    private void Awake()
    {
        if (Instancia != null && Instancia != this)
        {
            Destroy(gameObject);
            return;
        }

        Instancia = this;

        if (sistemaVidaJugador == null)
        {
            sistemaVidaJugador = GetComponent<SistemaVida>();
        }

        // Cargar daño guardado entre escenas
        mejoraDanioComprada = mejoraDanioGlobalComprada;
        multiplicadorDanio = multiplicadorDanioGlobal;

        if (sistemaVidaJugador == null)
        {
            Debug.LogError("EstadisticasJugador debe estar en el mismo objeto que SistemaVida, es decir, en Alastor.");
        }
    }

    public bool PuedeMejorarVida()
    {
        return mejorasVidaCompradas < maxMejorasVida;
    }

    public bool PuedeMejorarDanio()
    {
        return !mejoraDanioComprada;
    }

    public void MejorarVida()
    {
        if (sistemaVidaJugador == null)
        {
            sistemaVidaJugador = GetComponent<SistemaVida>();
        }

        if (sistemaVidaJugador == null)
        {
            Debug.LogError("No se puede mejorar la vida porque sistemaVidaJugador es null.");
            return;
        }

        if (!PuedeMejorarVida())
        {
            Debug.Log("Ya no se puede mejorar más la vida.");
            return;
        }

        int corazonesAntes = sistemaVidaJugador.corazonesMaximos;
        int corazonesDespues = corazonesAntes + corazonesExtraPorMejora;

        sistemaVidaJugador.CambiarCorazonesMaximos(corazonesDespues, true);
        sistemaVidaJugador.GuardarVida();

        mejorasVidaCompradas++;

        Debug.Log("Granada de Perséfone comprada.");
        Debug.Log("Corazones antes: " + corazonesAntes);
        Debug.Log("Corazones después: " + sistemaVidaJugador.corazonesMaximos);
    }

    public void MejorarDanio()
    {
        if (!PuedeMejorarDanio())
        {
            Debug.Log("Ya se ha comprado la Sal de Ares.");
            return;
        }

        mejoraDanioComprada = true;
        multiplicadorDanio = multiplicadorDanioMejorado;

        // Guardar entre escenas
        mejoraDanioGlobalComprada = true;
        multiplicadorDanioGlobal = multiplicadorDanio;

        Debug.Log("Sal de Ares comprada.");
        Debug.Log("Ahora el multiplicador de daño es: " + multiplicadorDanio);
        Debug.Log("Los golpes que hacían medio corazón ahora hacen un corazón entero.");
    }

    public void ResetearEstadisticas()
    {
        mejorasVidaCompradas = 0;

        mejoraDanioComprada = false;
        multiplicadorDanio = 1f;

        mejoraDanioGlobalComprada = false;
        multiplicadorDanioGlobal = 1f;

        Debug.Log("Estadísticas del jugador reseteadas.");
    }

    public static void ResetearEstadisticasGlobales()
    {
        mejoraDanioGlobalComprada = false;
        multiplicadorDanioGlobal = 1f;

        if (Instancia != null)
        {
            Instancia.ResetearEstadisticas();
        }

        Debug.Log("Estadísticas globales reseteadas.");
    }
}