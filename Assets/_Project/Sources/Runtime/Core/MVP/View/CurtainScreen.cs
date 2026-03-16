using Cysharp.Threading.Tasks;
using UnityEngine;
using TransitionsPlus;

namespace Sources.Runtime.Core.MVP.View
{
    public sealed class CurtainScreen : MonoBehaviour
    {
        public TransitionAnimator TransitionAnimator => _transitionAnimator;

        
        [SerializeField] private TransitionAnimator _transitionAnimator;

        public async UniTask WaitShowAndHideAsync()
        {
            _transitionAnimator.profile.invert = false;
            _transitionAnimator.Play();
            await UniTask.WaitUntil(() => _transitionAnimator.progress >= 1f);

            _transitionAnimator.profile.invert = true;
            _transitionAnimator.Play();
        }

        public async UniTask ShowAsync()
        {
            _transitionAnimator.profile.invert = false; 
            _transitionAnimator.Play();
            
            await UniTask.WaitUntil(() => _transitionAnimator.progress >= 1f);
        }
        
        public async UniTask HideAsync()
        {
            _transitionAnimator.profile.invert = true; 
            _transitionAnimator.Play();
            
            await UniTask.WaitUntil(() => _transitionAnimator.progress >= 1f);
        }
    }
}