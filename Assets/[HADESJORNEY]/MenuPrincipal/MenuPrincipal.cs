using UnityEngine;
using UnityEngine.SceneManagement;

// Controlador para el menú principal del juego
public class MenuPrincipal : MonoBehaviour
{
    public void CargarEscena()
    {
        SceneLoader.Load("Sala_Inicial", "Abriendo las puertas del Inframundo...");
    }

    public void AbrirOpciones()
    {
        //SceneManager.LoadScene("Opciones");
    }

    public void SalirDelJuego()
    {
        Application.Quit();

        // Solo sirve para comprobar que funciona dentro del editor de Unity
        Debug.Log("Saliendo del juego...");
    }
}