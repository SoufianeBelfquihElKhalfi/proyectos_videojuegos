using UnityEngine;

// Controlador para el menú principal del juego, maneja la interacción del usuario para cargar la escena inicial
public class MenuPrincipal : MonoBehaviour
{
    public void CargarEscena()
    {
        SceneLoader.Load("Sala_Inicial", "Abriendo las puertas del Inframundo...");
    }
}