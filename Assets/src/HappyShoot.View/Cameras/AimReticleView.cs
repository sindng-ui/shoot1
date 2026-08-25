using UnityEngine;
using UnityEngine.InputSystem;
using HappyShoot.View.Utils;

namespace HappyShoot.View.Cameras
{
    /// <summary>
    /// Presentation manager for the Soulstone-style Neon Lime-Green Aim Reticle.
    /// Tracks the mouse cursor's 2D world position with smooth damping and pulse feedback.
    /// Strictly modular and under 150 lines (500-line architecture rule).
    /// </summary>
    public class AimReticleView : MonoBehaviour
    {
        public const float MouseIdleTimeout = 4.0f;
        public static bool IsMouseAimActive { get; private set; } = true;

        private SpriteRenderer _renderer;
        private Camera _mainCamera;
        private Vector2 _currentWorldPos;
        private float _clickPulseScale = 1.0f;

        private Vector2 _lastMouseScreenPos;
        private float _mouseIdleTimer;
        private float _currentAlpha = 1.0f;

        public void Initialize(Camera mainCam = null)
        {
            _mainCamera = mainCam != null ? mainCam : Camera.main;

            _renderer = gameObject.GetComponent<SpriteRenderer>();
            if (_renderer == null)
                _renderer = gameObject.AddComponent<SpriteRenderer>();

            _renderer.sprite = ReticleSpriteHelper.GetOrCreateAimReticleSprite(48);
            _renderer.sortingOrder = 45; // Above monsters & ground effects
            _renderer.color = Color.white;

            transform.localScale = Vector3.one * 1.1f;
            IsMouseAimActive = true;
            _mouseIdleTimer = 0f;
            _currentAlpha = 1.0f;
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
                if (_mainCamera == null) return;
            }

            // If game is paused (Time.timeScale == 0) for UI menus (LevelUp, Pause, Settings), show OS cursor and hide aim reticle
            if (Time.timeScale == 0f)
            {
                if (!Cursor.visible) Cursor.visible = true;
                if (_renderer != null && _renderer.enabled) _renderer.enabled = false;
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
                if (_renderer != null) _renderer.enabled = false;
                Cursor.visible = true;
                IsMouseAimActive = false;
                return;
            }

            // Reticle Alpha smooth fade in/out based on idle state
            float targetAlpha = IsMouseAimActive ? 1.0f : 0.0f;
            _currentAlpha = Mathf.MoveTowards(_currentAlpha, targetAlpha, Time.unscaledDeltaTime * 4.0f);

            if (_renderer != null)
            {
                Color c = _renderer.color;
                c.a = _currentAlpha;
                _renderer.color = c;
                _renderer.enabled = _currentAlpha > 0.01f;
            }

            if (_currentAlpha <= 0.01f)
                return;

            // Convert mouse screen coordinates to 2D world plane (Z = 0)
            Ray ray = _mainCamera.ScreenPointToRay(mouseScreenPos);
            float distanceToPlane = -ray.origin.z / (ray.direction.z != 0 ? ray.direction.z : 1f);
            Vector3 targetWorldPos = ray.origin + ray.direction * distanceToPlane;
            Vector2 targetPos2D = new Vector2(targetWorldPos.x, targetWorldPos.y);

            // Smooth position interpolation
            _currentWorldPos = Vector2.Lerp(_currentWorldPos, targetPos2D, Time.unscaledDeltaTime * 35f);
            transform.position = new Vector3(_currentWorldPos.x, _currentWorldPos.y, 0f);

            // Click impulse decay
            _clickPulseScale = Mathf.MoveTowards(_clickPulseScale, 1.0f, Time.unscaledDeltaTime * 4.5f);

            // Subtle breathing idle pulse (1.0 ~ 1.08x)
            float breath = 1.0f + 0.05f * Mathf.Sin(Time.unscaledTime * 4.2f);
            float finalScale = 1.15f * breath * _clickPulseScale;
            transform.localScale = new Vector3(finalScale, finalScale, 1f);
        }
    }
}
