using UnityEngine;
using UnityEngine.SceneManagement;
public class cambioDeEscena : MonoBehaviour
{
    public string nombreEscena;

    void OnCollisionEnter(Collision colision)
    {
        if (colision.gameObject.CompareTag("Player"))
        {
            SceneManager.LoadScene(nombreEscena);
        }
    }
}
