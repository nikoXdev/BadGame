using System;
using UnityEngine;
using UnityEngine.UI;

namespace Sources.Runtime.Gameplay.Entities
{
    public sealed class Slot : MonoBehaviour
    {
        public event Action OnOccupancyChanged;
        
        [SerializeField] private Image _highlightImage;
        [SerializeField] private Color _freeColor = new Color(1f, 1f, 1f, 0.3f);
        [SerializeField] private Color _occupiedColor = new Color(0.5f, 0.5f, 0.5f, 0.3f);
        [SerializeField] private Color _hoverColor = new Color(0f, 1f, 0f, 0.5f);
 
        public bool IsFree => _currentPerson == null;
        public Person CurrentPerson => _currentPerson;
 
        private Person _currentPerson;
 
        private void Awake() => UpdateVisual();

        public bool TryOccupy(Person person)
        {
            if (IsFree == false)
                return false;
 
            _currentPerson = person;
            UpdateVisual();
            OnOccupancyChanged?.Invoke();
 
            return true;
        }
        
        public bool TryRelease(out Person person)
        {
            person = null;
 
            if (_currentPerson == null)
                return false;
 
            person = _currentPerson;
            _currentPerson = null;
            UpdateVisual();
            OnOccupancyChanged?.Invoke();
 
            return true;
        }
        
        public void SetHovered(bool hovered)
        {
            if (_highlightImage == null) 
                return;
            
            _highlightImage.color = hovered == true ? _hoverColor : (IsFree == true ? _freeColor : _occupiedColor);
        }
        
        private void UpdateVisual()
        {
            if (_highlightImage == null) 
                return;
            
            _highlightImage.color = IsFree == true ? _freeColor : _occupiedColor;
        }
    }
}