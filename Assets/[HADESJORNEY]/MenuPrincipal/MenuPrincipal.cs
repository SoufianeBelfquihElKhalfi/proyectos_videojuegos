using UnityEngine;

public class MenuPrincipal : MonoBehaviour
{
    public void CargarEscena()
    {
        SceneLoader.Load("Sala_Inicial", "Abriendo las puertas del Inframundo...");
    }
}