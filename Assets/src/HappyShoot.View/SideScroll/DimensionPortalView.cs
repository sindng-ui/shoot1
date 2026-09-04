using System;
using UnityEngine;
using HappyShoot.View.Cameras;
using HappyShoot.View.Player;
using HappyShoot.View.Utils;

namespace HappyShoot.View.SideScroll
{
    /// <summary>
    /// Mystic dimension rift portal that spawns upon defeating Boss 3 on 3rd clear.
    /// Features glowing cosmic swirl visual, proximity detection, and player vortex suction animation.
    /// Strictly modular and under 500 lines.
    /// </summary>
    public class DimensionPortalView : MonoBehaviour
    {
        private PlayerView _playerView;
        private Action _onEnteredCallback;
        private SpriteRenderer _portalSr;
        private SpriteRenderer _glowSr;
        private SpriteRenderer _shadowSr;

        private float _swirlAngle;
        private float _pulseTimer;
        private bool _isEntering;
        private float _enterTimer;
        private const float EnterDuration = 1.1f;

        private static Sprite _portalSprite;
        private static Sprite _portalGlowSprite;

        public void Initialize(PlayerView playerView, Action onEnteredCallback)
        {
            _playerView = playerView;
            _onEnteredCallback = onEnteredCallback;

            BuildVisuals();
        }

        private void BuildVisuals()
        {
            // 1. 2.5D Blob Shadow
            var shadowGo = new GameObject("PortalShadow");
            shadowGo.transform.SetParent(transform, false);
            shadowGo.transform.localPosition = new Vector3(0f, -0.65f, 0f);
            shadowGo.transform.localScale = new Vector3(2.4f, 1.2f, 1f);
            _shadowSr = shadowGo.AddComponent<SpriteRenderer>();
            _shadowSr.sprite = SpriteHelper.GetOrCreateBlobShadowSprite();
            _shadowSr.sortingOrder = 8;
            _shadowSr.color = new Color(0.1f, 0f, 0.25f, 0.7f);

            // 2. Cosmic Portal Core (Swirl)
            var coreGo = new GameObject("PortalCore");
            coreGo.transform.SetParent(transform, false);
            coreGo.transform.localPosition = Vector3.zero;
            coreGo.transform.localScale = Vector3.one * 1.8f;
            _portalSr = coreGo.AddComponent<SpriteRenderer>();
            _portalSr.sprite = GetOrCreatePortalSprite();
            _portalSr.sortingOrder = 14; // Above floor/monsters, right under wizard body (16)

            // 3. Outer Glow Aura
            var glowGo = new GameObject("PortalGlow");
            glowGo.transform.SetParent(transform, false);
            glowGo.transform.localPosition = Vector3.zero;
            glowGo.transform.localScale = Vector3.one * 2.5f;
            _glowSr = glowGo.AddComponent<SpriteRenderer>();
            _glowSr.sprite = GetOrCreatePortalGlowSprite();
            _glowSr.sortingOrder = 13;
            _glowSr.color = new Color(0.6f, 0.2f, 1.0f, 0.45f);
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            _pulseTimer += dt * 3.5f;
            _swirlAngle += dt * 120f;

            if (_portalSr != null)
            {
                _portalSr.transform.localRotation = Quaternion.Euler(0f, 0f, _swirlAngle);
                float pulseScale = 1.8f + Mathf.Sin(_pulseTimer) * 0.15f;
                _portalSr.transform.localScale = new Vector3(pulseScale, pulseScale, 1f);
            }

            if (_glowSr != null)
            {
                _glowSr.transform.localRotation = Quaternion.Euler(0f, 0f, -_swirlAngle * 0.5f);
                float glowAlpha = 0.35f + Mathf.Sin(_pulseTimer * 1.5f) * 0.15f;
                _glowSr.color = new Color(0.7f, 0.25f, 1f, glowAlpha);
            }

            if (_isEntering)
            {
                UpdateEnteringSequence(dt);
                return;
            }

            // Proximity check with wizard player
            if (_playerView != null)
            {
                float dist = Vector2.Distance(transform.position, _playerView.transform.position);
                if (dist <= 1.65f)
                {
                    StartEnteringSequence();
                }
            }
        }

        private void StartEnteringSequence()
        {
            _isEntering = true;
            _enterTimer = EnterDuration;

            // Trigger portal entrance sound & camera rumble
            _playerView.EventBus?.Publish(new Domain.Events.PlaySoundEvent(Domain.Events.SoundEffectType.WeaponEvolve));
            CameraFollowView.Instance?.TriggerShake("meteor_strike", 0.6f, 0.35f);
        }

        private void UpdateEnteringSequence(float dt)
        {
            _enterTimer -= dt;
            float progress = Mathf.Clamp01(1f - (_enterTimer / EnterDuration));

            if (_playerView != null)
            {
                // Vortex suction: player spins and scales down into portal center
                _playerView.transform.position = Vector3.Lerp(_playerView.transform.position, transform.position, dt * 5.0f);
                float scale = Mathf.Lerp(1.0f, 0.05f, progress);
                _playerView.transform.localScale = Vector3.one * scale;
                _playerView.transform.Rotate(0f, 0f, dt * 720f);
            }

            if (_enterTimer <= 0f)
            {
                _isEntering = false;
                if (_playerView != null)
                {
                    _playerView.transform.localScale = Vector3.one * 0.9f;
                    _playerView.transform.rotation = Quaternion.identity;
                }

                _onEnteredCallback?.Invoke();
                Destroy(gameObject);
            }
        }

        public static Sprite GetOrCreatePortalSprite(int size = 64)
        {
            if (_portalSprite != null) return _portalSprite;

            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            var pixels = new Color[size * size];
            float center = size * 0.5f;
            float maxR = center - 1f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - center;
                    float dy = y - center;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    if (dist > maxR)
                    {
                        pixels[y * size + x] = Color.clear;
                        continue;
                    }

                    float angle = Mathf.Atan2(dy, dx);
                    float normR = dist / maxR;
                    float spiral = Mathf.Sin(angle * 3f + (1f - normR) * 6.28f);

                    // Dual cosmic gradient: Deep violet core to vibrant cyan edge
                    Color c = Color.Lerp(
                        new Color(0.9f, 0.4f, 1.0f, 0.95f),
                        new Color(0.1f, 0.85f, 1.0f, 0.9f),
                        normR);

                    if (spiral > 0.1f) c = Color.Lerp(c, Color.white, 0.5f);
                    c.a *= Mathf.Clamp01((1f - normR) * 2.5f);
                    pixels[y * size + x] = c;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            _portalSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
            return _portalSprite;
        }

        public static Sprite GetOrCreatePortalGlowSprite(int size = 64)
        {
            if (_portalGlowSprite != null) return _portalGlowSprite;

            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear };
            var pixels = new Color[size * size];
            float center = size * 0.5f;
            float maxR = center - 1f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                    float t = Mathf.Clamp01(dist / maxR);
                    float alpha = Mathf.Pow(1f - t, 2.2f);
                    pixels[y * size + x] = new Color(0.65f, 0.2f, 1.0f, alpha * 0.6f);
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            _portalGlowSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
            return _portalGlowSprite;
        }
    }
}
