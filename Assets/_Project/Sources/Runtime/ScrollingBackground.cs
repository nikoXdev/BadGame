using UnityEngine;

namespace Sources.Runtime.Gameplay.Effects
{
    public sealed class ScrollingBackground : MonoBehaviour
    {
        [SerializeField] private float _scrollSpeedX = 2f;
        [SerializeField] private float _scrollSpeedY = 1f;

        [SerializeField] private float _tileWidth  = 12f;
        [SerializeField] private float _tileHeight = 6f;

        private float _startX;
        private float _startY;

        private void Awake()
        {
            _startX = transform.position.x;
            _startY = transform.position.y;
        }

        private void Update()
        {
            transform.position += new Vector3(
                -_scrollSpeedX * Time.deltaTime,
                -_scrollSpeedY * Time.deltaTime,
                0f);

            float x = transform.position.x;
            float y = transform.position.y;

            if (x <= _startX - _tileWidth)
                x = _startX;

            if (y <= _startY - _tileHeight)
                y = _startY;

            transform.position = new Vector3(x, y, transform.position.z);
        }
    }
}