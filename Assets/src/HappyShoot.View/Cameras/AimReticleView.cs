using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using HappyShoot.View.Utils;

namespace HappyShoot.View.Cameras
{
    /// <summary>
    /// Presentation manager for the Soulstone-style Neon Lime-Green Aim Reticle.
    /// Renders on a dedicated highest-layer ScreenSpaceOverlay Canvas (sortingOrder: 32760)
    /// ensuring it always renders cleanly above Sandbox menus, HUDs, and world entities.
    /// Strictly modular and under 180 lines (500-line architecture rule).
    /// </summary>
    public class AimReticleView : MonoBehaviour
    {
        public const float MouseIdleTimeout = 4.0f;
        public static bool IsMouseAimActive { get; private set; } = true;

        private Canvas _canvas;
        private Image _reticleImage;
        private RectTransform _reticleRt;
        private Camera _mainCamera;
        private Vector2 _currentScreenPos;
        private float _clickPulseScale = 1.0f;

        private Vector2 _lastMouseScreenPos;
        private float _mouseIdleTimer;
        private float _currentAlpha = 1.0f;

        public void Initialize(Camera mainCam = null)
        {
            _mainCamera = mainCam != null ? mainCam : Camera.main;

            // 1. Setup dedicated topmost ScreenSpaceOverlay Canvas
            _canvas = gameObject.GetComponent<Canvas>();
            if (_canvas == null) _canvas = gameObject.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.overrideSorting = true;
            _canvas.sortingOrder = 32760; // Topmost above Sandbox menu & HUD

            var scaler = gameObject.GetComponent<CanvasScaler>();
            if (scaler == null) scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            // 2. Setup Reticle UI Image
            var iconGo = new GameObject("ReticleImage");
            iconGo.transform.SetParent(transform, false);
            _reticleRt = iconGo.AddComponent<RectTransform>();
            _reticleRt.anchorMin = _reticleRt.anchorMax = _reticleRt.pivot = new Vector2(0.5f, 0.5f);
            _reticleRt.sizeDelta = new Vector2(48f, 48f);

            _reticleImage = iconGo.AddComponent<Image>();
            _reticleImage.sprite = ReticleSpriteHelper.GetOrCreateAimReticleSprite(48);
            _reticleImage.color = Color.white;
            _reticleImage.raycastTarget = false; // Does not block UI slider/button interactions

            IsMouseAimActive = true;
            _mouseIdleTimer = 0f;
            _currentAlpha = 1.0f;
            _currentScreenPos = Mouse.current != null ? Mouse.current.position.ReadValue() : (Vector2)Input.mousePosition;
        }

        private void OnEnable()
        {
            if (Time.timeScale > 0f)
            {
                Cursor.visible = false;
            }
            IsMouseAimActive = true;
            _mouseIdleTimer = 0f;
        }

        private void OnDisable()
        {
            Cursor.visible = true;
            IsMouseAimActive = true;
        }

        private void OnDestroy()
        {
            Cursor.visible = true;
            IsMouseAimActive = true;
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus)
            {
                Cursor.visible = true;
            }
            else if (Time.timeScale > 0f && gameObject.activeInHierarchy)
            {
                Cursor.visible = false;
            }
        }

        private void Update()
        {
            if (_mainCamera == null)
            {
                _mainCamera = Camera.main;
            }

            // If game is paused (Time.timeScale == 0) for UI menus (LevelUp, Pause, Settings), show OS cursor and hide aim reticle
            if (Time.timeScale == 0f)
            {
                if (!Cursor.visible) Cursor.visible = true;
                if (_reticleImage != null && _reticleImage.enabled) _reticleImage.enabled = false;
                return;
            }
            else
            {
                if (Cursor.visible) Cursor.visible = false;
            }

            Vector2 mouseScreenPos = Vector2.zero;
            bool isMouseActive = false;

            if (Mouse.current != null)
            {
                mouseScreenPos = Mouse.current.position.ReadValue();
                isMouseActive = true;

                // Mouse movement & click idle timer detection
                float deltaDistSqr = (mouseScreenPos - _lastMouseScreenPos).sqrMagnitude;
                bool isClicked = Mouse.current.leftButton.wasPressedThisFrame;

                if (deltaDistSqr > 3.0f || isClicked)
                {
                    _mouseIdleTimer = 0f;
                    IsMouseAimActive = true;
                    _lastMouseScreenPos = mouseScreenPos;
                }
                else
                {
                    _mouseIdleTimer += Time.unscaledDeltaTime;
                    if (_mouseIdleTimer >= MouseIdleTimeout)
                    {
                        IsMouseAimActive = false;
                    }
                }

                // Click impulse pulse feedback
                if (isClicked)
                {
                    _clickPulseScale = 1.35f;
                }
            }

            if (!isMouseActive)
            {
                if (_reticleImage != null) _reticleImage.enabled = false;
                Cursor.visible = true;
                IsMouseAimActive = false;
                return;
            }

            // Reticle Alpha smooth fade in/out based on idle state
            float targetAlpha = IsMouseAimActive ? 1.0f : 0.0f;
            _currentAlpha = Mathf.MoveTowards(_currentAlpha, targetAlpha, Time.unscaledDeltaTime * 4.0f);

            if (_reticleImage != null)
            {
                Color c = _reticleImage.color;
                c.a = _currentAlpha;
                _reticleImage.color = c;
                _reticleImage.enabled = _currentAlpha > 0.01f;
            }

            if (_currentAlpha <= 0.01f || _reticleRt == null)
                return;

            // Direct smooth screen position interpolation
            _currentScreenPos = Vector2.Lerp(_currentScreenPos, mouseScreenPos, Time.unscaledDeltaTime * 45f);
            _reticleRt.position = new Vector3(_currentScreenPos.x, _currentScreenPos.y, 0f);

            // Click impulse decay
            _clickPulseScale = Mathf.MoveTowards(_clickPulseScale, 1.0f, Time.unscaledDeltaTime * 4.5f);

            // Subtle breathing idle pulse (1.0 ~ 1.08x)
            float breath = 1.0f + 0.05f * Mathf.Sin(Time.unscaledTime * 4.2f);
            float finalScale = 1.15f * breath * _clickPulseScale;
            _reticleRt.localScale = new Vector3(finalScale, finalScale, 1f);
        }
    }
}
