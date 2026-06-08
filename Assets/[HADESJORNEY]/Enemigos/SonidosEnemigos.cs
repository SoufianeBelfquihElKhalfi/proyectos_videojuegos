using UnityEngine;
using Dapasa.Audio;

public class SonidosEnemigos : MonoBehaviour
{
    [Header("IDs AudioManager")]
    [SerializeField] private string idAlerta = "enemigo_alerta";
    [SerializeField] private string idAtaque = "enemigo_ataque";
    [SerializeField] private string idRecibirGolpe = "recibir_golpe_enemigo";
    [SerializeField] private string idMuerte = "muerte_enemigo";

    public void ReproducirAlerta()
    {
        Reproducir3D(idAlerta);
    }

    public void ReproducirAtaque()
    {
        Reproducir3D(idAtaque);
    }

    public void ReproducirRecibirGolpe()
    {
        Reproducir3D(idRecibirGolpe);
    }

    public void ReproducirMuerte()
    {
        Reproducir3D(idMuerte);
    }

    private void Reproducir3D(string idSonido)
    {
        if (string.IsNullOrWhiteSpace(idSonido))
            return;

        if (AudioManager.Instance == null)
        {
            throw new MissingReferenceException($"{name}: falta AudioManager en la escena.");
        }

        AudioManager.Instance.ReproducirSFX3D(idSonido, transform.position);
    }
}
