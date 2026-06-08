using UnityEngine;
using Dapasa.Audio;

public class sonidoProyectilEnemigo : MonoBehaviour
{
    [SerializeField] private string idSonido = "disparo_enemigo";

    private void Start()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.ReproducirSFX3D(idSonido, transform.position);
    }
}