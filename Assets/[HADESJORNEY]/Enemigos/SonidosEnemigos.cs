using UnityEngine;
using Dapasa.Audio;

public class SonidosEnemigos : MonoBehaviour
{
    [Header("IDs AudioManager")]
    [SerializeField] private string idAlerta = "enemigo_alerta";
    [SerializeField] private string idAtaque = "enemigo_ataque";
    [SerializeField] private string idRecibirGolpe = "recibir_golpe_enemigo";
    [SerializeField] private string idMuerte = "muerte_enemigo";
    [SerializeField] private string idPisada = "pisada_enemigo";

    [Header("Variación pisadas")]
    [SerializeField] private float pitchMinPisada = 0.92f;
    [SerializeField] private float pitchMaxPisada = 1.08f;
    [SerializeField] private float volumenMinPisada = 0.75f;
    [SerializeField] private float volumenMaxPisada = 1f;
    [SerializeField] private float tiempoMinimoEntrePisadas = 0.12f;

    private float tiempoUltimaPisada = -999f;

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

    public void ReproducirPisada()
    {
        if (Time.time - tiempoUltimaPisada < tiempoMinimoEntrePisadas)
            return;

        tiempoUltimaPisada = Time.time;

        Reproducir3DConVariacion(
            idPisada,
            pitchMinPisada,
            pitchMaxPisada,
            volumenMinPisada,
            volumenMaxPisada
        );
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

    private void Reproducir3DConVariacion(
        string idSonido,
        float pitchMin,
        float pitchMax,
        float volumenMin,
        float volumenMax
    )
    {
        if (string.IsNullOrWhiteSpace(idSonido))
            return;

        if (AudioManager.Instance == null)
        {
            throw new MissingReferenceException($"{name}: falta AudioManager en la escena.");
        }

        AudioManager.Instance.ReproducirSFX3DConVariacion(
            idSonido,
            transform.position,
            pitchMin,
            pitchMax,
            volumenMin,
            volumenMax
        );
    }
}