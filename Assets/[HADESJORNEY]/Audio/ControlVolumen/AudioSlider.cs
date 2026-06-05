using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Dapasa.Audio
{
    [RequireComponent(typeof(Slider))]
    public class AudioSlider : MonoBehaviour
    {
        [SerializeField] private AudioGroups group;

        [Header("Test")]
        [SerializeField] private AudioClip _testClip;
        [SerializeField] private AudioSource _testSource;

        private Slider _slider;

        private IEnumerator Start()
        {
            _slider = GetComponent<Slider>();

            yield return null;

            float value;

            switch (group)
            {
                case AudioGroups.Master:
                    value = DbToLineal(AudioVolumeManager.Instance.MasterVolume);
                    break;

                case AudioGroups.Musica:
                    value = DbToLineal(AudioVolumeManager.Instance.MusicVolume);
                    break;

                case AudioGroups.Sfx:
                    value = DbToLineal(AudioVolumeManager.Instance.SfxVolume);
                    break;

                default:
                    value = 1f;
                    break;
            }

            _slider.value = value;
            _slider.onValueChanged.AddListener(ChangeValue);
        }
        private void ChangeValue(float value)
        {
            float vol = value == 0 ? -80f : LinealToDb(value);

            switch (group)
            {
                case AudioGroups.Master:
                    AudioVolumeManager.Instance.MasterVolume = vol;
                    break;

                case AudioGroups.Musica:
                    AudioVolumeManager.Instance.MusicVolume = vol;
                    break;

                case AudioGroups.Sfx:
                    AudioVolumeManager.Instance.SfxVolume = vol;
                    break;
            }
        }
        public void TestVolume()
        {
            if (_testClip != null && _testSource != null)
            {
                _testSource.PlayOneShot(_testClip);
            }
        }

        private float DbToLineal(float dB)
        {
            return Mathf.Pow(10f, dB / 20f);
        }

        private float LinealToDb(float lineal)
        {
            return Mathf.Log10(lineal) * 20f;
        }
    }
}