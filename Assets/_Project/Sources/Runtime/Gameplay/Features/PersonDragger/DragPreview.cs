using UnityEngine;
using UnityEngine.UI;

namespace Sources.Runtime.Features.PersonDragger
{
    public sealed class DragPreview
    {
        private static readonly Color PreviewColor = new Color(0f, 1f, 0f, 0.45f);
 
        private readonly RectTransform _rect;
        private readonly Image _image;
 
        public DragPreview(Canvas canvas, Sprite sprite)
        {
            var go = new GameObject("DragPreview", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(canvas.transform, worldPositionStays: false);
 
            _rect  = go.GetComponent<RectTransform>();
            _image = go.GetComponent<Image>();
 
            _image.sprite = sprite;
            _image.color = PreviewColor;
            _image.raycastTarget = false;
 
            if (sprite != null)
                _rect.sizeDelta = sprite.rect.size / sprite.pixelsPerUnit * 100f;
 
            go.SetActive(false);
        }
 
        public void ShowAt(Vector3 worldPosition, float scaleX)
        {
            _rect.position = worldPosition;
            var s = _rect.localScale;
            _rect.localScale = new Vector3(scaleX, s.y, s.z);
            _rect.gameObject.SetActive(true);
            _rect.SetAsLastSibling();
        }
 
        public void Hide() => _rect.gameObject.SetActive(false);
 
        public void Destroy() => Object.Destroy(_rect.gameObject);
    }
}