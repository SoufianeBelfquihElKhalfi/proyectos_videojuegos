using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuPrincipal : MonoBehaviour
{
    public void CargarEscena()
    {
        SceneManager.LoadScene("Sala_Inicial");
    }
}
