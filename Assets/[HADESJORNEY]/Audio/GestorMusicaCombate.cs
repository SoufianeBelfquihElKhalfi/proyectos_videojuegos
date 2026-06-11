using System.Collections.Generic;
using UnityEngine;
using Dapasa.Audio;

public class GestorMusicaCombate : MonoBehaviour
{
    public static GestorMusicaCombate Instance { get; private set; }

    [Header("Música")]
    [SerializeField] private string idMusicaExploracion = "musica";
    [SerializeField] private string idMusicaCombate = "combate";
    [SerializeField] private float duracionFade = 1.5f;

    [Header("Ambiente de cueva (suena siempre como SFX loop)")]
    [SerializeField] private string idAmbienteCueva = "ambiente";

    private readonly HashSet<MonoBehaviour> enemigosEnCombate = new HashSet<MonoBehaviour>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    private void Start()
    {
        AudioManager.Instance.ReproducirSFXLoop2D(idAmbienteCueva);

        AudioManager.Instance.ReproducirMusicaConFade(idMusicaExploracion, duracionFade);
    }

    public void EntrarCombate(MonoBehaviour enemigo)
    {
        bool estabaVacio = enemigosEnCombate.Count == 0;
        enemigosEnCombate.Add(enemigo);

        if (estabaVacio)
            AudioManager.Instance.ReproducirMusicaConFade(idMusicaCombate, duracionFade);
    }

    public void SalirCombate(MonoBehaviour enemigo)
    {
        enemigosEnCombate.Remove(enemigo);

        if (enemigosEnCombate.Count == 0)
            AudioManager.Instance.ReproducirMusicaConFade(idMusicaExploracion, duracionFade);
    }
}