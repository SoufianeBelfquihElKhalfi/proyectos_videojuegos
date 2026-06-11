using Dapasa.Audio;
using UnityEngine;

public class ControladorSala : MonoBehaviour
{
    [Header("Puerta")]
    [SerializeField] private Animator animatorPuerta;
    [SerializeField] private Transform puntoSonidoPuerta;

    [Header("Enemigos")]
    [SerializeField] private SistemaVida[] enemigos;

    [Header("Audio")]
    [SerializeField] private string idSonidoAbrirPuerta = "abrir_puerta";
    [SerializeField] private string idMusicaPatrulla = "musica";

    private int enemigosVivos;
    private bool puertaAbierta;

    private void Awake()
    {
        ValidarConfiguracion();
    }

    private void Start()
    {
        enemigosVivos = 0;

        foreach (SistemaVida enemigo in enemigos)
        {
            if (enemigo == null)
            {
                throw new MissingReferenceException($"{name}: hay un enemigo sin asignar en la lista.");
            }

            if (enemigo.EstaMuerto)
            {
                continue;
            }

            enemigo.alMorir.AddListener(EnemigoMuerto);
            enemigosVivos++;
        }

        if (enemigosVivos == 0)
        {
            AbrirPuerta();
        }
    }

    public void EnemigoMuerto()
    {
        if (puertaAbierta)
        {
            return;
        }

        enemigosVivos--;

        if (enemigosVivos <= 0)
        {
            AbrirPuerta();
            AudioManager.Instance.ReproducirMusicaConFade(idMusicaPatrulla);
        }
    }

    private void AbrirPuerta()
    {
        if (puertaAbierta)
        {
            return;
        }

        puertaAbierta = true;

        ReproducirSonidoAbrirPuerta();

        animatorPuerta.SetTrigger("Abrir");
    }

    private void ReproducirSonidoAbrirPuerta()
    {
        if (AudioManager.Instance == null)
        {
            throw new MissingReferenceException($"{name}: falta AudioManager en la escena.");
        }

        Vector3 posicionSonido = puntoSonidoPuerta != null
            ? puntoSonidoPuerta.position
            : animatorPuerta.transform.position;

        AudioManager.Instance.ReproducirSFX3D(idSonidoAbrirPuerta, posicionSonido);
    }

    private void ValidarConfiguracion()
    {
        if (animatorPuerta == null)
        {
            throw new MissingReferenceException($"{name}: falta asignar animatorPuerta.");
        }

        if (enemigos == null || enemigos.Length == 0)
        {
            throw new System.InvalidOperationException($"{name}: la sala no tiene enemigos asignados.");
        }

        if (string.IsNullOrWhiteSpace(idSonidoAbrirPuerta))
        {
            throw new System.InvalidOperationException($"{name}: idSonidoAbrirPuerta está vacío.");
        }
    }
}