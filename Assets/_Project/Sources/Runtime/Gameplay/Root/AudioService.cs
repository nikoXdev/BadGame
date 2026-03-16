    using System.Collections.Generic;
    using DG.Tweening;
    using Sources.Runtime.Core.ServiceLocator;
    using UnityEngine;
    using UnityEngine.Audio;
    
    namespace Sources.Runtime.Gameplay.Audio
    {
        public sealed class AudioService : MonoBehaviour, IService
        {
            public AudioData Data => _data;
            
            [SerializeField] private AudioData    _data;
            [SerializeField] private AudioSource  _musicSource;
            [SerializeField] private AudioSource  _sfxSourcePrefab;
            [SerializeField] private int          _poolSize = 10;
    
            private readonly Queue<AudioSource> _pool = new();
            private AudioSource _currentMusic;
    
            private void Awake()
            {
                for (int i = 0; i < _poolSize; i++)
                    _pool.Enqueue(CreatePooledSource());
            }
    
            public void PlayMusic(AudioClip clip, float fadeIn = 0.5f)
            {
                if (clip == null) return;
    
                _musicSource.clip   = clip;
                _musicSource.loop   = true;
                _musicSource.volume = 0f;
                _musicSource.Play();
    
                DOTween.To(() => _musicSource.volume,
                    v  => _musicSource.volume = v,
                    1f, fadeIn).SetEase(Ease.InQuad);
            }
    
            public void StopMusic(float fadeOut = 0.5f)
            {
                DOTween.To(() => _musicSource.volume,
                    v  => _musicSource.volume = v,
                    0f, fadeOut)
                    .SetEase(Ease.OutQuad)
                    .OnComplete(() => _musicSource.Stop());
            }
    
            public void PlaySFX(AudioClip clip, float volume = 1f, float pitch = 1f)
            {
                if (clip == null) return;
    
                var source = GetPooledSource();
                source.clip   = clip;
                source.volume = volume;
                source.pitch  = pitch;
                source.Play();
    
                StartCoroutine(ReturnAfterPlay(source, clip.length / pitch));
            }
    
            public void PlaySFXRandomPitch(AudioClip clip, float minPitch = 0.9f, float maxPitch = 1.1f, float volume = 1f)
                => PlaySFX(clip, volume, Random.Range(minPitch, maxPitch));
    
            public void SetVolume(string group, float normalizedVolume)
            {
                float db = normalizedVolume > 0.0001f
                    ? Mathf.Log10(normalizedVolume) * 20f
                    : -80f;
    
                _data.Mixer.SetFloat(group, db);
            }
    
            public float GetVolume(string group)
            {
                _data.Mixer.GetFloat(group, out float db);
                return Mathf.Pow(10f, db / 20f);
            }
    
            public void PlayButtonClick()    => PlaySFXRandomPitch(_data.ButtonClick);
            public void PlayBookOpen()       => PlaySFX(_data.BookOpen);
            public void PlayBookPageTurn()   => PlaySFXRandomPitch(_data.BookPageTurn);
            public void PlayLeverPull()      => PlaySFX(_data.LeverPull);
            public void PlayDialogueOpen()   => PlaySFX(_data.DialogueOpen);
            public void PlayDialogueTick()   => PlaySFXRandomPitch(_data.DialogueTextTick, 0.95f, 1.05f, 0.4f);
            public void PlayTrainHit()       => PlaySFX(_data.TrainHit);
            public void PlayBloodSplat()     => PlaySFXRandomPitch(_data.BloodSplat, 0.8f, 1.2f, 0.7f);
            public void PlayFloatingPop()    => PlaySFXRandomPitch(_data.FloatingTextPop, 0.9f, 1.2f);
            public void PlayMarmaladeCount() => PlaySFXRandomPitch(_data.MarmaladeCount, 0.95f, 1.05f, 0.5f);
            public void PlayRoundEnd()       => PlaySFX(_data.RoundEnd);
            public void PlayPersonPickUp()   => PlaySFXRandomPitch(_data.PersonPickUp);
            public void PlayPersonPlace()    => PlaySFXRandomPitch(_data.PersonPlace);
            public void PlayPersonSwap()     => PlaySFX(_data.PersonSwap);
            public void PlayTimerTick()      => PlaySFXRandomPitch(_data.TimerTick, 0.98f, 1.02f);
            public void PlayTimerAlarm()     => PlaySFX(_data.TimerAlarm);
            public void PlayDialogueClose()  => PlaySFX(_data.DialogueClose);
            public void PlayTrainApproach()  => PlaySFX(_data.TrainApproach);
            public void PlayBloodDropLand() => PlaySFXRandomPitch(_data.BloodSplat, 0.7f, 1.4f, 0.18f);
    
            private AudioSource GetPooledSource()
            {
                if (_pool.Count > 0)
                {
                    var s = _pool.Dequeue();
                    s.gameObject.SetActive(true);
                    return s;
                }
    
                return CreatePooledSource();
            }
    
            private AudioSource CreatePooledSource()
            {
                var source = Instantiate(_sfxSourcePrefab, transform);
                source.playOnAwake = false;
                source.gameObject.SetActive(false);
                return source;
            }
    
            private System.Collections.IEnumerator ReturnAfterPlay(AudioSource source, float delay)
            {
                yield return new WaitForSeconds(delay + 0.05f);
                source.Stop();
                source.gameObject.SetActive(false);
                _pool.Enqueue(source);
            }
        }
    }