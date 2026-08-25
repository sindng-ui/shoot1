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
        private SpriteRenderer _renderer;
        private Camera _mainCamera;
        private Vector2 _currentWorldPos;
        private float _clickPulseScale = 1.0f;

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
        }

        private void Update()
        {
            if (_mainCamera == null)
            {
                _mainCamera = Camera.main;
                if (_mainCamera == null) return;
            }

            Vector2 mouseScreenPos = Vector2.zero;
            bool isMouseActive = false;

            if (Mouse.current != null)
            {
                mouseScreenPos = Mouse.current.position.ReadValue();
                isMouseActive = true;

                // Click impulse pulse feedback
                if (Mouse.current.leftButton.wasPressedThisFrame)
                {
                    _clickPulseScale = 1.35f;
                }
            }

            if (!isMouseActive)
            {
                _renderer.enabled = false;
                return;
            }

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

            _renderer.enabled = true;
        }
    }
}
