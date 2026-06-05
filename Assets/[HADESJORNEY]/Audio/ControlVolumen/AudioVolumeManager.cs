using UnityEngine;
using UnityEngine.Audio;

namespace Dapasa.Audio
{
    public class AudioVolumeManager : MonoBehaviour
    {
        [Tooltip("Seleccionar para que los valores se mantengan entre sesiones de juego")]
        [SerializeField] private bool _saveToPrefs = false;

        [Header("Mixer")]
        [SerializeField] private AudioMixer _audioMixer;

        [Header("Exposed Params")]
        [SerializeField] private string _masterParam = "Vol_Master";
        [SerializeField] private string _musicParam = "Vol_Musica";
        [SerializeField] private string _sfxParam = "Vol_Sfx";

        private static AudioVolumeManager _instance;

        public static AudioVolumeManager Instance
        {
            get { return _instance; }
        }

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            if (_saveToPrefs)
            {
                MasterVolume = PlayerPrefs.GetFloat(_masterParam, 0f);
                MusicVolume = PlayerPrefs.GetFloat(_musicParam, 0f);
                SfxVolume = PlayerPrefs.GetFloat(_sfxParam, 0f);
            }
        }

        public float MasterVolume
        {
            get
            {
                if (_audioMixer.GetFloat(_masterParam, out float value))
                {
                    return value;
                }

                return 0f;
            }
            set
            {
                _audioMixer.SetFloat(_masterParam, value);

                if (_saveToPrefs)
                {
                    PlayerPrefs.SetFloat(_masterParam, value);
                }
            }
        }

        public float MusicVolume
        {
            get
            {
                if (_audioMixer.GetFloat(_musicParam, out float value))
                {
                    return value;
                }

                return 0f;
            }
            set
            {
                _audioMixer.SetFloat(_musicParam, value);

                if (_saveToPrefs)
                {
                    PlayerPrefs.SetFloat(_musicParam, value);
                }
            }
        }

        public float SfxVolume
        {
            get
            {
                if (_audioMixer.GetFloat(_sfxParam, out float value))
                {
                    return value;
                }

                return 0f;
            }
            set
            {
                _audioMixer.SetFloat(_sfxParam, value);

                if (_saveToPrefs)
                {
                    PlayerPrefs.SetFloat(_sfxParam, value);
                }
            }
        }
    }

    public enum AudioGroups
    {
        Master,
        Musica,
        Sfx
    }
}