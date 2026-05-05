using UnityEngine;

[CreateAssetMenu(fileName = "DatosJugador", menuName = "Juego/Datos Jugador")]
public class DatosJugador : ScriptableObject
{
    public int corazonesMitadActuales;
    public int corazonesMitadMaximos;
    public int corazonesMaximos;
}