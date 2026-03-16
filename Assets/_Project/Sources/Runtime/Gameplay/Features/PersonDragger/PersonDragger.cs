using System.Collections.Generic;
using Sources.Runtime.Core.ServiceLocator;
using Sources.Runtime.Gameplay.Audio;
using Sources.Runtime.Gameplay.Entities;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Sources.Runtime.Features.PersonDragger
{
    public sealed class PersonDragger : IPersonDragger
    {
        private readonly Canvas _canvas;
        private readonly RectTransform _personRect;
        private readonly CanvasGroup _canvasGroup;
        private readonly Person _person;
        private readonly Sprite _personSprite;

        private readonly IReadOnlyList<Slot> _leftSlots;
        private readonly IReadOnlyList<Slot> _rightSlots;

        private Slot    _currentSlot;
        private Slot    _originSlot;
        private Vector2 _positionBeforeDrag;
        private Slot    _lastHoveredSlot;

        private DragPreview _previewA;
        private DragPreview _previewB;

        public PersonDragger(Canvas canvas, RectTransform personRect, CanvasGroup canvasGroup, Person person,
            Sprite personSprite, IReadOnlyList<Slot> leftSlots, IReadOnlyList<Slot> rightSlots)
        {
            _canvas = canvas;
            _personRect = personRect;
            _canvasGroup = canvasGroup;
            _person = person;
            _personSprite = personSprite;
            _leftSlots = leftSlots;
            _rightSlots = rightSlots;
        }

        public void BindToSlot(Slot slot)
        {
            _currentSlot = slot;
            ApplyScale(_currentSlot);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            ServiceLocator.Get<AudioService>()?.PlayPersonPickUp();

            _positionBeforeDrag = _personRect.anchoredPosition;
            _originSlot = _currentSlot;

            _currentSlot?.TryRelease(out _);
            _currentSlot = null;

            _canvasGroup.blocksRaycasts = false;
            _canvasGroup.alpha = 0.75f;

            _person.SetOutline(true);
            _personRect.SetAsLastSibling();

            _previewA ??= new DragPreview(_canvas, _personSprite);
            _previewB ??= new DragPreview(_canvas, _personSprite);
        }

        public void OnDrag(PointerEventData eventData)
        {
            _personRect.anchoredPosition += eventData.delta / _canvas.scaleFactor;

            UpdateHoverHighlight(FindSlotUnderPointer(eventData));
            UpdatePreview(eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            _canvasGroup.blocksRaycasts = true;
            _canvasGroup.alpha = 1f;

            _person.SetOutline(false);
            UpdateHoverHighlight(null);
            _previewA?.Hide();
            _previewB?.Hide();

            // 1. Попали прямо на спрайт персонажа
            var targetPerson = FindPersonUnderPointer(eventData);
            if (targetPerson != null)
            {
                PerformSwap(targetPerson);
                _originSlot = null;
                return;
            }

            bool pointerOnLeft   = eventData.position.x < Screen.width * 0.5f;
            var  targetSideSlots = pointerOnLeft ? _leftSlots : _rightSlots;

            // 2. Есть свободный слот на целевой стороне
            var freeSlot = FindFreeSlot(targetSideSlots);
            if (freeSlot != null)
            {
                PlaceInSlot(freeSlot);
                _originSlot = null;
                return;
            }

            // 3. Попали на занятый слот — свап через слот
            var hoveredSlot = FindSlotUnderPointer(eventData);
            if (hoveredSlot != null && !hoveredSlot.IsFree)
            {
                PerformSwap(hoveredSlot.CurrentPerson);
                _originSlot = null;
                return;
            }

            ReturnToOrigin();
            _originSlot = null;
        }

        private void UpdatePreview(PointerEventData eventData)
        {
            Slot slotA = null;
            Slot slotB = null;

            var targetPerson = FindPersonUnderPointer(eventData);

            if (targetPerson != null)
            {
                // Свап с персонажем под курсором
                slotA = FindSlotOf(targetPerson);
                slotB = _originSlot;
            }
            else
            {
                bool pointerOnLeft = eventData.position.x < Screen.width * 0.5f;
                var  targetSlots   = pointerOnLeft ? _leftSlots : _rightSlots;

                var freeSlot = FindFreeSlot(targetSlots);
                if (freeSlot != null)
                {
                    // Перемещение в свободный слот — показываем только если меняем сторону
                    if (pointerOnLeft != IsOnLeftSide(_originSlot))
                        slotA = freeSlot;
                }
                else
                {
                    // Все слоты заняты — показываем свап через слот под курсором
                    var hoveredSlot = FindSlotUnderPointer(eventData);
                    if (hoveredSlot != null && !hoveredSlot.IsFree)
                    {
                        slotA = hoveredSlot;
                        slotB = _originSlot;
                    }
                }
            }

            if (slotA != null)
                _previewA?.ShowAt(slotA.transform.position, IsOnLeftSide(slotA) ? 1f : -1f);
            else
                _previewA?.Hide();

            if (slotB != null)
                _previewB?.ShowAt(slotB.transform.position, IsOnLeftSide(slotB) ? 1f : -1f);
            else
                _previewB?.Hide();
        }

        private Person FindPersonUnderPointer(PointerEventData eventData)
        {
            var results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);

            foreach (var r in results)
                if (r.gameObject.TryGetComponent<Person>(out var person) && person != _person)
                    return person;

            return null;
        }

        private void PerformSwap(Person other)
        {
            ServiceLocator.Get<AudioService>()?.PlayPersonSwap();

            var otherSlot = FindSlotOf(other);

            otherSlot?.TryRelease(out _);

            if (_originSlot != null)
            {
                _originSlot.TryOccupy(other);
                other.RelocateTo(_originSlot);
            }

            if (otherSlot != null)
            {
                PlaceInSlot(otherSlot);
            }
            else
            {
                ReturnToOrigin();
            }
        }

        private void PlaceInSlot(Slot slot)
        {
            ServiceLocator.Get<AudioService>()?.PlayPersonPlace();
            slot.TryOccupy(_person);
            SnapToSlot(slot);
            _currentSlot = slot;
            ApplyScale(_currentSlot);
        }

        private void ReturnToOrigin()
        {
            if (_originSlot != null && _originSlot.TryOccupy(_person) == true)
            {
                _personRect.anchoredPosition = _positionBeforeDrag;
                _currentSlot = _originSlot;
                ApplyScale(_originSlot);
            }
            else
            {
                _personRect.anchoredPosition = _positionBeforeDrag;
            }
        }

        private Slot FindFreeSlot(IReadOnlyList<Slot> slots)
        {
            foreach (var slot in slots)
                if (slot.IsFree == true)
                    return slot;

            return null;
        }

        private Slot FindSlotOf(Person person)
        {
            foreach (var slot in _leftSlots)
                if (slot.CurrentPerson == person)
                    return slot;

            foreach (var slot in _rightSlots)
                if (slot.CurrentPerson == person)
                    return slot;

            return null;
        }

        private bool IsOnLeftSide(Slot slot)
        {
            foreach (var s in _leftSlots)
                if (s == slot)
                    return true;

            return false;
        }

        private Slot FindSlotUnderPointer(PointerEventData eventData)
        {
            var results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);

            foreach (var r in results)
                if (r.gameObject.TryGetComponent<Slot>(out var slot))
                    return slot;

            return null;
        }

        private void UpdateHoverHighlight(Slot hoveredSlot)
        {
            if (_lastHoveredSlot == hoveredSlot)
                return;

            _lastHoveredSlot?.SetHovered(false);
            hoveredSlot?.SetHovered(true);
            _lastHoveredSlot = hoveredSlot;
        }

        private void SnapToSlot(Slot slot)
        {
            var slotScreenPos = RectTransformUtility.WorldToScreenPoint(_canvas.worldCamera, slot.transform.position);

            RectTransformUtility.ScreenPointToLocalPointInRectangle(_canvas.transform as RectTransform, slotScreenPos,
                _canvas.worldCamera, out var localPoint);

            _personRect.anchoredPosition = localPoint;
        }

        private void ApplyScale(Slot slot)
        {
            if (slot == null)
                return;

            float scaleX = IsOnLeftSide(slot) ? 1f : -1f;
            var s = _personRect.localScale;
            _personRect.localScale = new Vector3(scaleX, s.y, s.z);
        }
    }
}