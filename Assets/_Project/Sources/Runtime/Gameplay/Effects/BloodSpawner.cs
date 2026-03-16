using System.Collections.Generic;
using DG.Tweening;
using Sources.Runtime.Core.ServiceLocator;
using UnityEngine;

namespace Sources.Runtime.Gameplay.Effects
{
    public sealed class BloodSpawner : MonoBehaviour
    {
        public static BloodSpawner Instance { get; private set; }

        [SerializeField] private BloodDrop  bloodDropPrefab;
        [SerializeField] private BloodSplat bloodSplatPrefab;
        [SerializeField] private Sprite[]   dropSprites;
        [SerializeField] private Sprite[]   splatSprites;

        [SerializeField] private int   dropCount    = 10;
        [SerializeField] private float minSpeed     = 3f;
        [SerializeField] private float maxSpeed     = 7f;
        [SerializeField] private float spawnStagger = 0.03f;

        private readonly List<BloodSplat> _activeSplats = new();

        private void Awake()
        {
            if (Instance == null) Instance = this;
        }

        public void SpawnBlood(Vector2 hitPosition)
        {
            var spawnPos3D = new Vector3(hitPosition.x, hitPosition.y, transform.position.z);

            for (int i = 0; i < dropCount; i++)
            {
                int index = i;
                DOVirtual.DelayedCall(index * spawnStagger, () =>
                {
                    var drop = Instantiate(bloodDropPrefab, spawnPos3D, Quaternion.identity, transform);

                    float angle = Random.Range(55f, 125f) * Mathf.Deg2Rad;
                    float speed = Random.Range(minSpeed, maxSpeed);
                    Vector2 vel = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * speed;
                    vel.x += Random.Range(-1.5f, 1.5f);

                    if (dropSprites.Length > 0)
                        drop.GetComponent<SpriteRenderer>().sprite =
                            dropSprites[Random.Range(0, dropSprites.Length)];

                    drop.Initialize(spawnPos3D, vel, splatSprites, transform.position.z);
                    
                    ServiceLocator.Get<CameraShakeService>()?.ShakeMedium();
                });
            }
        }

        public void SpawnSplat(Vector3 position, Sprite[] splats)
        {
            if (splats == null || splats.Length == 0) return;

            var splat = Instantiate(bloodSplatPrefab, position, Quaternion.identity, transform);
            splat.Initialize(splats[Random.Range(0, splats.Length)]);
            _activeSplats.Add(splat);
        }

        public void ClearAllSplats(float fadeDuration = 0.6f)
        {
            foreach (var splat in _activeSplats)
                if (splat != null) splat.FadeOutFast(fadeDuration);

            _activeSplats.Clear();
        }
    }
}