using UnityEngine;
using UnityEngine.SceneManagement;

public class PausaManager : MonoBehaviour
{
    public GameObject pausa;
    private bool pausaActivo = false;

    void Start()
    {
        pausa.SetActive(false); 
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleMenu();
        }
    }

    void ToggleMenu()
    {
        pausaActivo = !pausaActivo;
        pausa.SetActive(pausaActivo);

        Time.timeScale = pausaActivo ? 0f : 1f;
    }

    public void Reanudar()
    {
        pausaActivo = false;
        pausa.SetActive(false);
        Time.timeScale = 1f;
    }

    public void Menu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("SplashScreen"); 
    }
}