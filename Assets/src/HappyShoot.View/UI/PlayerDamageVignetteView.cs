using UnityEngine;
using UnityEngine.UI;
using HappyShoot.Domain.Events;

namespace HappyShoot.View.UI
{
    /// <summary>
    /// High-performance 0-GC screen edge damage vignette view.
    /// Pulses vibrant crimson edges on player hit and provides heartbeat low-HP warning.
    /// Pure procedural texture, zero blur shaders, 60fps mobile friendly.
    /// Strictly modular and under 500 lines.
    /// </summary>
    public class PlayerDamageVignetteView : MonoBehaviour
    {
        private Image _vignetteImage;
        private EventBus _eventBus;

        private float _flashAlpha;
        private const float FlashDuration = 0.25f;
        private const float MaxHitAlpha = 0.50f;

        private float _remainingHealthRatio = 1.0f;
        private static Sprite _cachedVignetteSprite;

        public void Initialize(EventBus eventBus, Transform canvasTransform)
        {
            _eventBus = eventBus;
            if (canvasTransform != null)
            {
                transform.SetParent(canvasTransform, false);
            }

            SetupVignetteImage();

            _eventBus?.Subscribe<PlayerDamagedEvent>(OnPlayerDamaged);
            _eventBus?.Subscribe<PlayerHealedEvent>(OnPlayerHealed);
            _eventBus?.Subscribe<PlayerDiedEvent>(OnPlayerDied);
        }

        private void SetupVignetteImage()
        {
            var rect = gameObject.GetComponent<RectTransform>() ?? gameObject.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            _vignetteImage = gameObject.GetComponent<Image>() ?? gameObject.AddComponent<Image>();
            _vignetteImage.sprite = GetOrCreateVignetteSprite();
            _vignetteImage.type = Image.Type.Simple;
            _vignetteImage.raycastTarget = false;
            _vignetteImage.color = new Color(0.95f, 0.05f, 0.1f, 0f);

            // Ensure vignette sits behind foreground popups but in front of game world
            transform.SetAsFirstSibling();
        }

        private void Update()
        {
            float dt = Time.unscaledDeltaTime;

            // Decay hit flash
            if (_flashAlpha > 0f)
            {
                _flashAlpha = Mathf.Max(0f, _flashAlpha - (dt / FlashDuration) * MaxHitAlpha);
            }

            // Low HP heartbeat warning pulse (under 30% HP)
            float lowHpBaseAlpha = 0f;
            if (_remainingHealthRatio > 0f && _remainingHealthRatio <= 0.30f)
            {
                float urgency = 1f - (_remainingHealthRatio / 0.30f); // 0 at 30%, 1 at 0%
                float pulse = 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * (6f + urgency * 4f));
                lowHpBaseAlpha = (0.12f + 0.15f * urgency) * pulse;
            }

            float finalAlpha = Mathf.Clamp01(Mathf.Max(_flashAlpha, lowHpBaseAlpha));
            if (_vignetteImage != null)
            {
                _vignetteImage.color = new Color(0.95f, 0.05f, 0.12f, finalAlpha);
            }
        }

        private void OnPlayerDamaged(PlayerDamagedEvent evt)
        {
            _flashAlpha = MaxHitAlpha;
            _remainingHealthRatio = evt.MaxHealth > 0f ? Mathf.Clamp01(evt.RemainingHealth / evt.MaxHealth) : 0f;
        }

        private void OnPlayerHealed(PlayerHealedEvent evt)
        {
            _remainingHealthRatio = evt.MaxHealth > 0f ? Mathf.Clamp01(evt.CurrentHealth / evt.MaxHealth) : 1f;
        }

        private void OnPlayerDied(PlayerDiedEvent evt)
        {
            _remainingHealthRatio = 0f;
            _flashAlpha = 0.65f;
        }

        private void OnDestroy()
        {
            _eventBus?.Unsubscribe<PlayerDamagedEvent>(OnPlayerDamaged);
            _eventBus?.Unsubscribe<PlayerHealedEvent>(OnPlayerHealed);
            _eventBus?.Unsubscribe<PlayerDiedEvent>(OnPlayerDied);
        }

        /// <summary>
        /// Generates a smooth, procedural radial vignette mask (outer edges red, center transparent).
        /// 0-GC cached statically, zero shader overhead.
        /// </summary>
        private static Sprite GetOrCreateVignetteSprite()
        {
            if (_cachedVignetteSprite != null) return _cachedVignetteSprite;

            const int size = 128;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;

            Color[] colors = new Color[size * size];
            float half = size * 0.5f;

            for (int y = 0; y < size; y++)
            {
                float ny = (y - half) / half; // -1 to 1
                for (int x = 0; x < size; x++)
                {
                    float nx = (x - half) / half; // -1 to 1
                    // Elliptical distance from center (scaled for wide 16:9 view)
                    float dist = Mathf.Sqrt(nx * nx + ny * ny);

                    // Inner radius 0.6 is fully transparent, outer reaches alpha 1.0 at edge
                    float t = Mathf.Clamp01((dist - 0.55f) / 0.55f);
                    float alpha = t * t * (3f - 2f * t); // Smoothstep curve

                    colors[y * size + x] = new Color(1f, 1f, 1f, alpha);
                }
            }

            tex.SetPixels(colors);
            tex.Apply();

            _cachedVignetteSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
            return _cachedVignetteSprite;
        }
    }
}
