using System.Collections.Generic;
using UnityEngine;
using HappyShoot.Domain.Events;
using HappyShoot.View.Cameras;
using HappyShoot.View.Utils;

namespace HappyShoot.View.Projectiles
{
    /// <summary>
    /// Presentation manager for Storm Bow (Evolved Piercing Bow) hit explosions.
    /// Spawns satisfying, snappy cyan shockwave blast bursts at every arrow impact point!
    /// Strictly modular and under 500 lines.
    /// </summary>
    public class StormBowManagerView : MonoBehaviour
    {
        private class ActiveBlastBurst
        {
            public GameObject Root;
            public SpriteRenderer Renderer;
            public float Elapsed;
            public float Duration;
            public float MaxScale;
            public bool IsActive;
        }

        private const int MaxPoolCapacity = 64;
        private readonly List<ActiveBlastBurst> _blastPool = new List<ActiveBlastBurst>(MaxPoolCapacity);
        private readonly List<ActiveBlastBurst> _activeBlasts = new List<ActiveBlastBurst>(MaxPoolCapacity);

        private EventBus _eventBus;
        private Sprite _blastSprite;

        public void Initialize(EventBus eventBus, Monsters.MonsterSpawnerView spawnerView = null)
        {
            _eventBus = eventBus;
            _blastSprite = SkillSpriteHelper.GetOrCreateStormBlastSprite();

            PrewarmPool(MaxPoolCapacity);
            _eventBus?.Subscribe<StormArrowHitExplosionEvent>(OnStormArrowHitExplosion);
        }

        private void OnDestroy()
        {
            _eventBus?.Unsubscribe<StormArrowHitExplosionEvent>(OnStormArrowHitExplosion);
        }

        private void PrewarmPool(int count)
        {
            for (int i = 0; i < count; i++)
            {
                var go = new GameObject($"StormBlast_{i + 1}");
                go.transform.SetParent(transform, false);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = _blastSprite;
                sr.sortingOrder = 28; // On top of monsters and projectiles
                go.SetActive(false);

                _blastPool.Add(new ActiveBlastBurst
                {
                    Root = go,
                    Renderer = sr,
                    IsActive = false
                });
            }
        }

        private void OnStormArrowHitExplosion(StormArrowHitExplosionEvent evt)
        {
            var blast = GetOrCreateBlast();
            blast.IsActive = true;
            blast.Elapsed = 0f;
            blast.Duration = 0.16f; // Snappy fast pop
            blast.MaxScale = Mathf.Max(1.0f, evt.Radius * 2.2f);

            blast.Root.transform.position = new Vector3((float)evt.Position.X, (float)evt.Position.Y, -0.1f);
            blast.Root.transform.localScale = Vector3.one * 0.2f;
            blast.Root.transform.rotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));
            blast.Renderer.color = new Color(0.2f, 1.0f, 0.95f, 1.0f);
            blast.Root.SetActive(true);

            _activeBlasts.Add(blast);

            // Subtle punch shake
            CameraFollowView.Instance?.TriggerShake("storm_bow", duration: 0.08f, intensity: 0.12f);
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            if (dt <= 0f) return;

            for (int i = _activeBlasts.Count - 1; i >= 0; i--)
            {
                var blast = _activeBlasts[i];
                blast.Elapsed += dt;
                float progress = Mathf.Clamp01(blast.Elapsed / blast.Duration);

                // Snappy pop scale: quick expansion (ease-out cubic), then fade
                float scale = Mathf.Lerp(0.3f, blast.MaxScale, Mathf.Sin(progress * Mathf.PI * 0.5f));
                float alpha = progress > 0.4f ? 1.0f - ((progress - 0.4f) / 0.6f) : 1.0f;

                blast.Root.transform.localScale = Vector3.one * scale;
                blast.Renderer.color = new Color(0.25f, 1.0f, 0.95f, alpha * 0.95f);

                if (progress >= 1.0f)
                {
                    RecycleBlast(blast);
                    _activeBlasts.RemoveAt(i);
                }
            }
        }

        private ActiveBlastBurst GetOrCreateBlast()
        {
            if (_blastPool.Count > 0)
            {
                var item = _blastPool[_blastPool.Count - 1];
                _blastPool.RemoveAt(_blastPool.Count - 1);
                return item;
            }

            var go = new GameObject($"StormBlast_Dyn");
            go.transform.SetParent(transform, false);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = _blastSprite;
            sr.sortingOrder = 28;

            return new ActiveBlastBurst
            {
                Root = go,
                Renderer = sr,
                IsActive = false
            };
        }

        private void RecycleBlast(ActiveBlastBurst blast)
        {
            if (blast?.Root != null)
            {
                blast.IsActive = false;
                blast.Root.SetActive(false);
                _blastPool.Add(blast);
            }
        }
    }
}
