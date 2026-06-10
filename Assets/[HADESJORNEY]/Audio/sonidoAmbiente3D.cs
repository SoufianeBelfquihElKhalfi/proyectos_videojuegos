using UnityEngine;
using UnityEngine.Audio;

namespace Dapasa.Audio
{
    [RequireComponent(typeof(AudioSource))]
    public class sonidoAmbiente3D : MonoBehaviour
    {
        [SerializeField] private AudioClip clip;

        [Range(0f, 1f)]
        [SerializeField] private float volumen = 1f;

        [SerializeField] private float distanciaMinima = 2f;
        [SerializeField] private float distanciaMaxima = 12f;

        [Tooltip("Grupo del mixer (arrastra el SFX)")]
        [SerializeField] private AudioMixerGroup grupoMixer;

        private AudioSource fuente;

        private void Awake()
        {
            fuente = GetComponent<AudioSource>();

            fuente.clip = clip;
            fuente.volume = volumen;
            fuente.loop = true;
            fuente.playOnAwake = true;
            fuente.spatialBlend = 1f;
            fuente.rolloffMode = AudioRolloffMode.Linear;
            fuente.minDistance = distanciaMinima;
            fuente.maxDistance = distanciaMaxima;

            if (grupoMixer != null)
                fuente.outputAudioMixerGroup = grupoMixer;

            fuente.Play();
        }
    }
}