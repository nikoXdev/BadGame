using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Sources.Runtime.Gameplay.UI.Book
{
    public sealed class BookSlot : MonoBehaviour
    {
        [SerializeField] private Image _icon;
        [SerializeField] private TextMeshProUGUI _text;

        public void Initialize(Sprite icon, string value)
        {
            _icon.sprite = icon;
            _text.text = value;
        }
    }
}