using UnityEngine;
using UnityEngine.Audio;

namespace Sources.Runtime.Gameplay.Audio
{
    [CreateAssetMenu(menuName = "Data/Audio", fileName = "AudioData")]
    public sealed class AudioData : ScriptableObject
    {
        [Header("Mixer")]
        public AudioMixer Mixer;

        [Header("UI")]
        public AudioClip ButtonClick;
        public AudioClip BookOpen;
        public AudioClip BookPageTurn;
        public AudioClip LeverPull;

        [Header("Dialogue")]
        public AudioClip DialogueOpen;
        public AudioClip DialogueClose;
        public AudioClip DialogueTextTick;

        [Header("Gameplay")]
        public AudioClip TrainApproach;
        public AudioClip TrainHit;
        public AudioClip TimerTick;
        public AudioClip TimerAlarm;
        public AudioClip PersonPickUp;
        public AudioClip PersonPlace;
        public AudioClip PersonSwap;

        [Header("Result")]
        public AudioClip BloodSplat;
        public AudioClip FloatingTextPop;
        public AudioClip MarmaladeCount;
        public AudioClip RoundEnd;
    }
}