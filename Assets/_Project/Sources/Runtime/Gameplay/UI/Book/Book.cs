using Sources.Runtime.Core.ServiceLocator;
using Sources.Runtime.Gameplay.Audio;
using Sources.Runtime.Gameplay.Entities;
using UnityEngine;
using UnityEngine.UI;

namespace Sources.Runtime.Gameplay.UI.Book
{
    public sealed class Book : MonoBehaviour
    {
        [SerializeField] private BookSlot[]   _bookSlots;
        [SerializeField] private Button       _leftArrow;
        [SerializeField] private Button       _rightArrow;
        [SerializeField] private AllPeopleData _allPeopleData;

        private int _page;
        private int _pageCount;

        private void Awake()
        {
            var people = _allPeopleData.Get();
            int slotsPerPage = _bookSlots.Length;

            _pageCount = Mathf.CeilToInt((float)people.Count / slotsPerPage);
            _page      = 0;

            _leftArrow.onClick.AddListener(PrevPage);
            _rightArrow.onClick.AddListener(NextPage);

            Refresh();
        }

        private void OnDestroy()
        {
            _leftArrow.onClick.RemoveListener(PrevPage);
            _rightArrow.onClick.RemoveListener(NextPage);
        }

        private void PrevPage()
        {
            if (_page <= 0) return;
            _page--;
            ServiceLocator.Get<AudioService>()?.PlayBookPageTurn();
            Refresh();
        }

        private void NextPage()
        {
            if (_page >= _pageCount - 1) return;
            _page++;
            ServiceLocator.Get<AudioService>()?.PlayBookPageTurn();
            Refresh();
        }

        private void Refresh()
        {
            var people       = _allPeopleData.Get();
            int slotsPerPage = _bookSlots.Length;
            int startIndex   = _page * slotsPerPage;

            for (int i = 0; i < _bookSlots.Length; i++)
            {
                int dataIndex = startIndex + i;

                if (dataIndex < people.Count)
                {
                    var data  = people[dataIndex];
                    string label = BuildLabel(data);
                    _bookSlots[i].Initialize(data.NormalSprite, label);
                    _bookSlots[i].gameObject.SetActive(true);
                }
                else
                {
                    _bookSlots[i].gameObject.SetActive(false);
                }
            }

            _leftArrow.interactable  = _page > 0;
            _rightArrow.interactable = _page < _pageCount - 1;
        }

        private static string BuildLabel(PersonData data)
        {
            string action = data.Action switch
            {
                PersonAction.Add      => $"<color=#88FF88><b>+{data.Number}</b></color>",
                PersonAction.Subtract => $"<color=#FF4444><b>-{data.Number}</b></color>",
                PersonAction.Multiply => $"<color=#FFD700><b>×{data.Number}</b></color>",
                PersonAction.Divide   => $"<color=#FF9966><b>÷{data.Number}</b></color>",
                _                     => data.Number.ToString()
            };

            return $"{action}";
        }
    }
}