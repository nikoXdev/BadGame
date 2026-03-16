using Sources.Runtime.Core.ServiceLocator;
using Sources.Runtime.Features.PersonDragger;
using Sources.Runtime.Gameplay.Effects;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Sources.Runtime.Gameplay.Entities
{
    [RequireComponent(typeof(CanvasGroup))]
    public sealed class Person : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public PersonData Data => _data;
        
        [SerializeField] private Image _image;
        
        private IPersonDragger _dragger;
        private RectTransform _rectTransform;
        private PersonData _data;
        
        private bool _interactable = true;

        public void Construct(IPersonDragger dragger, PersonData data)
        {
            _dragger = dragger;
            _data = data;
            _rectTransform = GetComponent<RectTransform>();

            _image.sprite = _data.NormalSprite;
            
            SetInteractable(true);
        }

        public void RelocateTo(Slot slot)
        {
            _dragger.BindToSlot(slot);
            _rectTransform.position = slot.transform.position;
        }

        public void SetOutline(bool outlined)
        {
            if (_data == null || _image == null) 
                return;
            
            _image.sprite = outlined ? _data.OutlineSprite : _data.NormalSprite;
        }
        
        public void SetInteractable(bool interactable)
        {
            _interactable = interactable;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (_interactable == false) 
                return;
            
            ServiceLocator.Get<CameraShakeService>()?.ShakeLight();
            
            _dragger.OnBeginDrag(eventData);
        }
 
        public void OnDrag(PointerEventData eventData)
        {
            if (_interactable == false) 
                return;
            
            _dragger.OnDrag(eventData);
        }
 
        public void OnEndDrag(PointerEventData eventData)
        {
            if (_interactable == false) 
                return;
            
            _dragger.OnEndDrag(eventData);
        }
    }
}