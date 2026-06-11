using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace Dapasa.Audio
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [System.Serializable]
        public class Sonido
        {
            public string id;
            public AudioClip clip;

            [Range(0f, 1f)]
            public float volumen = 1f;

            [Range(0.1f, 3f)]
            public float pitch = 1f;
        }

        [Header("Mixer Groups")]
        [SerializeField] private AudioMixerGroup grupoMusica;
        [SerializeField] private AudioMixerGroup grupoSFX;

        [Header("Audio Sources 2D")]
        [SerializeField] private AudioSource fuenteMusica;
        [SerializeField] private AudioSource fuenteSFX2D;
        [SerializeField] private AudioSource fuenteSFXLoop2D;

        [Header("Lista general de sonidos")]
        [SerializeField] private List<Sonido> sonidos = new List<Sonido>();

        [Header("Configuración SFX 3D")]
        [SerializeField] private float distanciaMinima3D = 1f;
        [SerializeField] private float distanciaMaxima3D = 15f;

        private Dictionary<string, Sonido> diccionarioSonidos;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                Inicializar();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        // Start() eliminado: la música la arranca cada escena con su propio componente
        // (MusicaMenu en el menú, GestorMusicaCombate en la cueva)

        private void Inicializar()
        {
            PrepararFuentes();
            CrearDiccionario();
        }

        private void PrepararFuentes()
        {
            if (fuenteMusica == null)
            {
                GameObject musicaGO = new GameObject("Fuente_Musica");
                musicaGO.transform.SetParent(transform);
                fuenteMusica = musicaGO.AddComponent<AudioSource>();
            }

            if (fuenteSFX2D == null)
            {
                GameObject sfxGO = new GameObject("Fuente_SFX_2D");
                sfxGO.transform.SetParent(transform);
                fuenteSFX2D = sfxGO.AddComponent<AudioSource>();
            }

            if (fuenteSFXLoop2D == null)
            {
                GameObject sfxLoopGO = new GameObject("Fuente_SFX_Loop_2D");
                sfxLoopGO.transform.SetParent(transform);
                fuenteSFXLoop2D = sfxLoopGO.AddComponent<AudioSource>();
            }

            fuenteMusica.playOnAwake = false;
            fuenteMusica.loop = true;
            fuenteMusica.spatialBlend = 0f;

            fuenteSFX2D.playOnAwake = false;
            fuenteSFX2D.loop = false;
            fuenteSFX2D.spatialBlend = 0f;

            fuenteSFXLoop2D.playOnAwake = false;
            fuenteSFXLoop2D.loop = true;
            fuenteSFXLoop2D.spatialBlend = 0f;

            if (grupoMusica != null)
            {
                fuenteMusica.outputAudioMixerGroup = grupoMusica;
            }

            if (grupoSFX != null)
            {
                fuenteSFX2D.outputAudioMixerGroup = grupoSFX;
            }

            if (grupoSFX != null)
            {
                fuenteSFXLoop2D.outputAudioMixerGroup = grupoSFX;
            }
        }

        private void CrearDiccionario()
        {
            diccionarioSonidos = new Dictionary<string, Sonido>();

            foreach (Sonido sonido in sonidos)
            {
                if (sonido == null)
                    continue;

                if (string.IsNullOrEmpty(sonido.id))
                    continue;

                if (sonido.clip == null)
                    continue;

                if (!diccionarioSonidos.ContainsKey(sonido.id))
                {
                    diccionarioSonidos.Add(sonido.id, sonido);
                }
                else
                {
                    Debug.LogWarning("AudioManager: sonido duplicado con id: " + sonido.id);
                }
            }
        }

        // -----------------------------
        // MÚSICA
        // -----------------------------

        public void ReproducirMusica(string id, bool reiniciarSiYaSuena = false)
        {
            Sonido sonido = ObtenerSonido(id);

            if (sonido == null)
                return;

            if (!reiniciarSiYaSuena && fuenteMusica.clip == sonido.clip && fuenteMusica.isPlaying)
                return;

            fuenteMusica.clip = sonido.clip;
            fuenteMusica.volume = sonido.volumen;
            fuenteMusica.pitch = sonido.pitch;
            fuenteMusica.loop = true;
            fuenteMusica.Play();
        }

        public void PararMusica()
        {
            fuenteMusica.Stop();
        }

        public void PausarMusica()
        {
            fuenteMusica.Pause();
        }

        public void ReanudarMusica()
        {
            fuenteMusica.UnPause();
        }

        // -----------------------------
        // SFX 2D
        // UI, botones, menús, sonidos globales
        // -----------------------------

        public void ReproducirSFX2D(string id)
        {
            Sonido sonido = ObtenerSonido(id);

            if (sonido == null)
                return;

            fuenteSFX2D.pitch = sonido.pitch;
            fuenteSFX2D.PlayOneShot(sonido.clip, sonido.volumen);
        }

        public void ReproducirSFX2D(AudioClip clip, float volumen = 1f)
        {
            if (clip == null)
                return;

            fuenteSFX2D.pitch = 1f;
            fuenteSFX2D.PlayOneShot(clip, volumen);
        }
        public void ReproducirSFX2DConVariacion(
        string id,
        float pitchMin = 0.9f,
        float pitchMax = 1.1f,
        float volumenMin = 0.9f,
        float volumenMax = 1f
        )
        {
            Sonido sonido = ObtenerSonido(id);

            if (sonido == null)
                return;

            float pitchAleatorio = Random.Range(pitchMin, pitchMax);
            float volumenAleatorio = Random.Range(volumenMin, volumenMax);

            fuenteSFX2D.pitch = sonido.pitch * pitchAleatorio;
            fuenteSFX2D.PlayOneShot(sonido.clip, sonido.volumen * volumenAleatorio);
        }


        //para los loop
        public void ReproducirSFXLoop2D(string id)
        {
            Sonido sonido = ObtenerSonido(id);

            if (sonido == null)
                return;

            if (fuenteSFXLoop2D.clip == sonido.clip && fuenteSFXLoop2D.isPlaying)
                return;

            fuenteSFXLoop2D.clip = sonido.clip;
            fuenteSFXLoop2D.volume = sonido.volumen;
            fuenteSFXLoop2D.pitch = sonido.pitch;
            fuenteSFXLoop2D.loop = true;
            fuenteSFXLoop2D.Play();
        }

        public void PararSFXLoop2D(string id)
        {
            Sonido sonido = ObtenerSonido(id);

            if (sonido == null)
                return;

            if (fuenteSFXLoop2D.clip == sonido.clip && fuenteSFXLoop2D.isPlaying)
            {
                fuenteSFXLoop2D.Stop();
                fuenteSFXLoop2D.clip = null;
            }
        }

        public void PararSFXLoop2D()
        {
            if (fuenteSFXLoop2D != null && fuenteSFXLoop2D.isPlaying)
            {
                fuenteSFXLoop2D.Stop();
                fuenteSFXLoop2D.clip = null;
            }
        }

        // -----------------------------
        // SFX 3D
        // Enemigos, impactos, proyectiles, mundo
        // -----------------------------

        public void ReproducirSFX3D(string id, Vector3 posicion)
        {
            Sonido sonido = ObtenerSonido(id);

            if (sonido == null)
                return;

            CrearAudio3D(sonido.clip, posicion, sonido.volumen, sonido.pitch);
        }

        public void ReproducirSFX3D(AudioClip clip, Vector3 posicion, float volumen = 1f)
        {
            if (clip == null)
                return;

            CrearAudio3D(clip, posicion, volumen, 1f);
        }

        private void CrearAudio3D(AudioClip clip, Vector3 posicion, float volumen, float pitch)
        {
            GameObject audioGO = new GameObject("SFX_3D_" + clip.name);
            audioGO.transform.position = posicion;

            AudioSource audioSource = audioGO.AddComponent<AudioSource>();

            audioSource.clip = clip;
            audioSource.volume = volumen;
            audioSource.pitch = pitch;
            audioSource.spatialBlend = 1f;
            audioSource.playOnAwake = false;
            audioSource.loop = false;

            audioSource.minDistance = distanciaMinima3D;
            audioSource.maxDistance = distanciaMaxima3D;
            audioSource.rolloffMode = AudioRolloffMode.Linear;

            if (grupoSFX != null)
            {
                audioSource.outputAudioMixerGroup = grupoSFX;
            }

            audioSource.Play();

            float duracion = clip.length / Mathf.Abs(pitch);
            Destroy(audioGO, duracion + 0.2f);
        }

        public void ReproducirSFX3DConVariacion(
            string id,
            Vector3 posicion,
            float pitchMin = 0.9f,
            float pitchMax = 1.1f,
            float volumenMin = 0.9f,
            float volumenMax = 1f
        )
        {
            Sonido sonido = ObtenerSonido(id);

            if (sonido == null)
                return;

            float pitchAleatorio = Random.Range(pitchMin, pitchMax);
            float volumenAleatorio = Random.Range(volumenMin, volumenMax);

            CrearAudio3D(
                sonido.clip,
                posicion,
                sonido.volumen * volumenAleatorio,
                sonido.pitch * pitchAleatorio
            );
        }

        // -----------------------------
        // UTILIDAD
        // -----------------------------

        private Sonido ObtenerSonido(string id)
        {
            if (diccionarioSonidos == null)
            {
                CrearDiccionario();
            }

            if (diccionarioSonidos.TryGetValue(id, out Sonido sonido))
            {
                return sonido;
            }

            Debug.LogWarning("AudioManager: no existe ningún sonido con id: " + id);
            return null;
        }
        private Coroutine corrutinaFadeMusica;

        public void ReproducirMusicaConFade(string id, float duracionFade = 1.5f)
        {
            Sonido sonido = ObtenerSonido(id);
            if (sonido == null || (fuenteMusica.clip == sonido.clip && fuenteMusica.isPlaying))
                return;

            if (corrutinaFadeMusica != null)
                StopCoroutine(corrutinaFadeMusica);

            corrutinaFadeMusica = StartCoroutine(TransicionMusica(sonido, duracionFade));
        }

        private IEnumerator TransicionMusica(Sonido nuevoSonido, float duracionTotal)
        {
            float mitadTiempo = duracionTotal / 2f;

            if (fuenteMusica.isPlaying)
            {
                float volInicial = fuenteMusica.volume;
                for (float t = 0; t < mitadTiempo; t += Time.deltaTime)
                {
                    fuenteMusica.volume = Mathf.Lerp(volInicial, 0f, t / mitadTiempo);
                    yield return null;
                }
            }

            fuenteMusica.Stop();
            fuenteMusica.clip = nuevoSonido.clip;
            fuenteMusica.volume = 0f;
            fuenteMusica.pitch = nuevoSonido.pitch;
            fuenteMusica.Play();

            for (float t = 0; t < mitadTiempo; t += Time.deltaTime)
            {
                fuenteMusica.volume = Mathf.Lerp(0f, nuevoSonido.volumen, t / mitadTiempo);
                yield return null;
            }

            fuenteMusica.volume = nuevoSonido.volumen;
        }
    }

}