using Dapasa.Audio;
using UnityEngine;

public class ambienteEscena : MonoBehaviour
{
    [SerializeField] private string idAmbiente;

    private void Start()
    {
        if (!string.IsNullOrWhiteSpace(idAmbiente))
            AudioManager.Instance.ReproducirMusica(idAmbiente);
    }
}