using UnityEngine;
using Dapasa.Audio;
public class MusicaMenu : MonoBehaviour
{
    [SerializeField] private string idMusica = "musica";

    private void Start()
    {
        AudioManager.Instance.ReproducirMusica(idMusica);
    }
}