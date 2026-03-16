using Sources.Runtime.Core.ServiceLocator;
using UnityEngine;

namespace Sources.Runtime.Gameplay.Audio
{
    public sealed class AudioSettings : MonoBehaviour, IService
    {
        private const string KeyMaster = "audio_master";
        private const string KeyMusic  = "audio_music";
        private const string KeySFX    = "audio_sfx";

        public float MasterVolume => PlayerPrefs.GetFloat(KeyMaster, 1f);
        public float MusicVolume  => PlayerPrefs.GetFloat(KeyMusic,  1f);
        public float SFXVolume    => PlayerPrefs.GetFloat(KeySFX,    1f);

        private AudioService _audio;

        private void Start()
        {
            _audio = ServiceLocator.Get<AudioService>();
            Apply();
        }

        public void SetMaster(float value)
        {
            PlayerPrefs.SetFloat(KeyMaster, Mathf.Clamp01(value));
            _audio.SetVolume(AudioMixerGroups.Master, value);
        }

        public void SetMusic(float value)
        {
            PlayerPrefs.SetFloat(KeyMusic, Mathf.Clamp01(value));
            _audio.SetVolume(AudioMixerGroups.Music, value);
        }

        public void SetSFX(float value)
        {
            PlayerPrefs.SetFloat(KeySFX, Mathf.Clamp01(value));
            _audio.SetVolume(AudioMixerGroups.SFX, value);
        }

        public void Apply()
        {
            if (_audio == null) return;
            _audio.SetVolume(AudioMixerGroups.Master, MasterVolume);
            _audio.SetVolume(AudioMixerGroups.Music,  MusicVolume);
            _audio.SetVolume(AudioMixerGroups.SFX,    SFXVolume);
        }
    }
}