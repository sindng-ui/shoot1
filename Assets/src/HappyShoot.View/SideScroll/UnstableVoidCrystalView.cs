using System;
using UnityEngine;
using HappyShoot.Domain.Entities;
using HappyShoot.Domain.Events;
using HappyShoot.Domain.Spatial;
using HappyShoot.View.Cameras;
using HappyShoot.View.Monsters;
using HappyShoot.View.Player;

namespace HappyShoot.View.SideScroll
{
    /// <summary>
    /// Interactive explosive hazard crystal in side-scrolling corridor.
    /// When struck by player/companion projectiles or close combat,
    /// detonates in a massive chain reaction, wiping out nearby monsters in a 6m radius.
    /// Strictly modular and under 500 lines.
    /// </summary>
    public class UnstableVoidCrystalView : MonoBehaviour
    {
        private PlayerView _playerView;
        private MonsterSpawnerView _mainSpawner;
        private bool _detonated;
        private float _pulseTimer;
        private static Sprite _crystalSprite;

        public void Initialize(PlayerView playerView, MonsterSpawnerView mainSpawner)
        {
            _playerView = playerView;
            _mainSpawner = mainSpawner;

            var sr = gameObject.AddComponent<SpriteRenderer>();
            sr.sprite = GetOrCreateCrystalSprite();
            sr.sortingOrder = 14;
            transform.localScale = Vector3.one * 1.6f;
        }

        private void Update()
        {
            if (_detonated) return;

            // Pulse glowing red animation
            _pulseTimer += Time.deltaTime * 6f;
            float scaleMod = 1.6f + Mathf.Sin(_pulseTimer) * 0.18f;
            transform.localScale = new Vector3(scaleMod, scaleMod, 1f);

            // Proximity check: If player is very close or attacks hit
            if (_playerView != null)
            {
                float dist = Vector2.Distance(transform.position, _playerView.transform.position);
                if (dist <= 1.4f)
                {
                    Detonate();
                }
            }
        }

        public void Detonate()
        {
            if (_detonated) return;
            _detonated = true;

            Debug.Log("[UnstableVoidCrystal] 💥 BOOM! Massive chain explosion triggered!");

            // 1. Audio & Camera Juice
            _playerView?.EventBus?.Publish(new Domain.Events.PlaySoundEvent(Domain.Events.SoundEffectType.MagicExplosion, 1.2f));
            CameraFollowView.Instance?.TriggerShake(null, 0.6f, 0.45f);

            // 2. Damage and wipe out all monsters in 6m blast radius
            if (_mainSpawner?.DomainSpawner != null)
            {
                var activeMonsters = _mainSpawner.DomainSpawner.ActiveMonsters;
                Vector2 crystalPos = transform.position;

                for (int i = activeMonsters.Count - 1; i >= 0; i--)
                {
                    var m = activeMonsters[i];
                    if (m.IsActive && !m.IsDead)
                    {
                        float dist = Vector2.Distance(crystalPos, new Vector2(m.Position.X, m.Position.Y));
                        if (dist <= 6.0f)
                        {
                            m.TakeDamage(180f, isCritical: true, DamageType.Fireball);
                        }
                    }
                }
            }

            // 3. Temporary explosion blast flash
            var blastGo = new GameObject("CrystalBlastFx");
            blastGo.transform.position = transform.position;
            blastGo.transform.localScale = Vector3.one * 5.5f;
            var bsr = blastGo.AddComponent<SpriteRenderer>();
            bsr.sprite = Utils.SpriteHelper.GetOrCreateWhiteSprite();
            bsr.color = new Color(1f, 0.25f, 0.1f, 0.9f);
            bsr.sortingOrder = 25;
            Destroy(blastGo, 0.22f);

            Destroy(gameObject);
        }

        public static Sprite GetOrCreateCrystalSprite(int size = 32)
        {
            if (_crystalSprite != null) return _crystalSprite;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            var pixels = new Color[size * size];
            float c = size * 0.5f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    // Diamond crystal shape
                    float dx = Mathf.Abs(x - c);
                    float dy = Mathf.Abs(y - c);
                    if (dx + dy <= 13f)
                    {
                        if (dx + dy <= 6f)
                            pixels[y * size + x] = new Color(1f, 0.9f, 0.3f, 1f); // Hot yellow core
                        else
                            pixels[y * size + x] = new Color(1f, 0.15f, 0.25f, 0.95f); // Fiery red rim
                    }
                    else
                    {
                        pixels[y * size + x] = Color.clear;
                    }
                }
            }
            tex.SetPixels(pixels);
            tex.Apply();
            _crystalSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
            return _crystalSprite;
        }
    }
}
