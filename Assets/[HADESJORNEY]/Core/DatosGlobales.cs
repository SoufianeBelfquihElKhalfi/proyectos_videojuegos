public static class DatosGlobales
{
    public static int vidaActual;
    public static int vidaMaxima;
    public static bool hayDatosVida = false;

    public static void ReiniciarPartida()
    {
        vidaActual = 0;
        vidaMaxima = 0;
        hayDatosVida = false;
    }
}