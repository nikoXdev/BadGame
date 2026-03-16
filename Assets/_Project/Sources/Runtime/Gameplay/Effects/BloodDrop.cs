using DG.Tweening;
using Sources.Runtime.Core.ServiceLocator;
using Sources.Runtime.Gameplay.Audio;
using UnityEngine;

namespace Sources.Runtime.Gameplay.Effects
{
    public sealed class BloodDrop : MonoBehaviour
    {
        private Sprite[] _splatSprites;
        private float _spawnZ;
        private SpriteRenderer _sr;

        private void Awake() => _sr = GetComponent<SpriteRenderer>();

        public void Initialize(Vector3 startPos, Vector2 velocity, Sprite[] splats, float spawnZ)
        {
            _splatSprites = splats;
            _spawnZ = spawnZ;
            transform.position = startPos;

            float riseTime = Random.Range(0.25f, 0.45f);
            float fallTime = Random.Range(0.55f, 0.9f);

            Vector3 peak = new Vector3(
                startPos.x + velocity.x * riseTime,
                startPos.y + Mathf.Abs(velocity.y) * riseTime,
                spawnZ
            );

            Vector3 land = new Vector3(
                peak.x + velocity.x * fallTime * 0.5f,
                startPos.y,
                spawnZ
            );

            transform.localScale = Vector3.one;
            _sr.color = new Color(1f, 1f, 1f, Random.Range(0.75f, 1f));

            DOTween.Sequence()
                .Append(transform.DOMove(peak, riseTime)
                    .SetEase(Ease.OutQuad))
                .Join(transform.DOScale(new Vector3(0.7f, 1.4f, 1f), riseTime)
                    .SetEase(Ease.OutQuad))
                .Append(transform.DOMove(land, fallTime)
                    .SetEase(Ease.InQuad))
                .Join(transform.DOScale(new Vector3(1.3f, 0.8f, 1f), fallTime)
                    .SetEase(Ease.InQuad))
                .OnComplete(() =>
                {
                    ServiceLocator.Get<AudioService>()?.PlayBloodDropLand();
                    ServiceLocator.Get<CameraShakeService>()?.ShakeHeavy();
                    SpawnSplats(land);
                    Destroy(gameObject);
                });
        }

        private void SpawnSplats(Vector3 landPos)
        {
            int count = Random.Range(1, 4); // меньше пятен
            for (int i = 0; i < count; i++)
            {
                Vector3 spawnPos = new Vector3(
                    landPos.x + Random.Range(-0.35f, 0.35f),
                    landPos.y + Random.Range(-0.05f, 0.05f),
                    _spawnZ
                );
                BloodSpawner.Instance.SpawnSplat(spawnPos, _splatSprites);
            }
        }
    }
}