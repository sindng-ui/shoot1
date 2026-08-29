using System.Collections.Generic;
using UnityEngine;
using HappyShoot.Domain.Entities;
using HappyShoot.Domain.Events;
using HappyShoot.Domain.Spatial;
using HappyShoot.View.Utils;

namespace HappyShoot.View.Monsters
{
    /// <summary>
    /// Presentation manager for juicy death burst particle FX per monster archetype.
    /// Uses 0-allocation object pooling for high performance (60-120fps).
    /// Strictly modular and under 200 lines (500-line architecture rule).
    /// </summary>
    public class MonsterDeathFxManagerView : MonoBehaviour
    {
        private const int PoolSize = 64;
        private readonly List<DeathParticle> _pool = new List<DeathParticle>(PoolSize);
        private EventBus _eventBus;
        private Sprite _sparkSprite;

        public void Initialize(EventBus eventBus)
        {
            _eventBus = eventBus;
            _eventBus?.Subscribe<MonsterDiedEvent>(OnMonsterDied);

            _sparkSprite = GetOrCreateParticleSprite();

            for (int i = 0; i < PoolSize; i++)
            {
                var go = new GameObject($"DeathParticle_{i + 1}");
                go.transform.SetParent(transform, false);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = _sparkSprite;
                sr.sortingOrder = 22;
                go.SetActive(false);

                _pool.Add(new DeathParticle
                {
                    GameObject = go,
                    Transform = go.transform,
                    Renderer = sr,
                    IsActive = false
                });
            }
        }

        private static Sprite _cachedParticleSprite;
        private static Sprite GetOrCreateParticleSprite()
        {
            if (_cachedParticleSprite != null) return _cachedParticleSprite;
            var tex = new Texture2D(4, 4, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            Color[] pixels = new Color[16];
            for (int i = 0; i < 16; i++) pixels[i] = Color.clear;
            // 4x4 diamond spark shape
            pixels[1] = Color.white; pixels[2] = Color.white;
            pixels[4] = Color.white; pixels[5] = Color.white; pixels[6] = Color.white; pixels[7] = Color.white;
            pixels[8] = Color.white; pixels[9] = Color.white; pixels[10] = Color.white; pixels[11] = Color.white;
            pixels[13] = Color.white; pixels[14] = Color.white;
            tex.SetPixels(pixels);
            tex.Apply();
            _cachedParticleSprite = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4);
            return _cachedParticleSprite;
        }

        private void OnMonsterDied(MonsterDiedEvent evt)
        {
            // Spawn 3~5 juicy elemental particles around monster death position
            Vector2 origin = new Vector2(evt.Position.X, evt.Position.Y);
            Color baseColor = GetDeathColor(evt.MonsterType);
            int count = (evt.MonsterType == MonsterType.Boss || evt.MonsterType == MonsterType.Boss3 || evt.MonsterType == MonsterType.Golem || evt.MonsterType == MonsterType.Abomination) ? 6 : 4;

            for (int i = 0; i < count; i++)
            {
                float angle = (360f / count) * i + Random.Range(-25f, 25f);
                float rad = angle * Mathf.Deg2Rad;
                Vector2 velocity = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * Random.Range(1.8f, 3.8f);

                SpawnParticle(origin, velocity, baseColor, Random.Range(0.25f, 0.40f));
            }
        }

        private Color GetDeathColor(MonsterType type)
        {
            switch (type)
            {
                case MonsterType.FireImp:
                    return new Color(1.0f, 0.45f, 0.15f, 1.0f); // Fiery Orange
                case MonsterType.ToxicSpider:
                    return new Color(0.65f, 1.0f, 0.20f, 1.0f); // Fluorescent Toxic Green
                case MonsterType.DarkKnight:
                    return new Color(0.75f, 0.35f, 1.0f, 1.0f); // Dark Arcane Purple
                case MonsterType.Golem:
                    return new Color(1.0f, 0.80f, 0.30f, 1.0f); // Amber Rune Spark
                case MonsterType.Boss:
                    return new Color(1.0f, 0.20f, 0.35f, 1.0f); // Crimson Hellfire
                case MonsterType.Boss3:
                    return new Color(0.20f, 0.85f, 1.0f, 1.0f); // Arch-Lich Soul Azure
                case MonsterType.Wraith:
                    return new Color(0.35f, 0.85f, 0.95f, 1.0f); // Ghost Cyan
                case MonsterType.Necromancer:
                    return new Color(0.60f, 0.20f, 0.85f, 1.0f); // Cursed Purple
                case MonsterType.Abomination:
                    return new Color(0.40f, 0.55f, 0.30f, 1.0f); // Toxic Flesh
                case MonsterType.Reaper:
                    return new Color(0.85f, 0.10f, 0.20f, 1.0f); // Reaper Crimson Glint
                case MonsterType.Bat:
                    return new Color(0.85f, 0.30f, 0.95f, 1.0f); // Velvet Violet
                case MonsterType.Skeleton:
                    return new Color(0.95f, 0.92f, 0.85f, 1.0f); // Ivory Bone Shard
                case MonsterType.Slime:
                default:
                    return new Color(0.35f, 0.95f, 0.45f, 1.0f); // Emerald Jelly
            }
        }

        private void SpawnParticle(Vector2 origin, Vector2 velocity, Color color, float lifetime)
        {
            for (int i = 0; i < _pool.Count; i++)
            {
                var p = _pool[i];
                if (!p.IsActive)
                {
                    p.IsActive = true;
                    p.Position = origin;
                    p.Velocity = velocity;
                    p.InitialLifetime = lifetime;
                    p.CurrentLifetime = lifetime;
                    p.BaseColor = color;

                    p.Transform.position = new Vector3(origin.x, origin.y, 0f);
                    p.Transform.localScale = Vector3.one * 0.8f;
                    p.Renderer.color = color;
                    p.GameObject.SetActive(true);
                    return;
                }
            }
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            for (int i = 0; i < _pool.Count; i++)
            {
                var p = _pool[i];
                if (p.IsActive)
                {
                    p.CurrentLifetime -= dt;
                    if (p.CurrentLifetime <= 0f)
                    {
                        p.IsActive = false;
                        p.GameObject.SetActive(false);
                    }
                    else
                    {
                        float progress = 1.0f - (p.CurrentLifetime / p.InitialLifetime);
                        p.Position += p.Velocity * dt;
                        p.Velocity = Vector2.Lerp(p.Velocity, Vector2.zero, dt * 5f);

                        p.Transform.position = new Vector3(p.Position.x, p.Position.y, 0f);
                        float scale = Mathf.Lerp(0.85f, 0.1f, progress);
                        p.Transform.localScale = Vector3.one * scale;

                        Color c = p.BaseColor;
                        c.a = Mathf.Sin((1f - progress) * Mathf.PI * 0.5f);
                        p.Renderer.color = c;
                    }
                }
            }
        }

        private void OnDestroy()
        {
            _eventBus?.Unsubscribe<MonsterDiedEvent>(OnMonsterDied);
        }

        private class DeathParticle
        {
            public GameObject GameObject;
            public Transform Transform;
            public SpriteRenderer Renderer;
            public Vector2 Position;
            public Vector2 Velocity;
            public float InitialLifetime;
            public float CurrentLifetime;
            public Color BaseColor;
            public bool IsActive;
        }
    }
}
