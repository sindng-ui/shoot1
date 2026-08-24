using UnityEngine;

namespace HappyShoot.View.Utils
{
    /// <summary>
    /// Manages Juice Hit-Stop (Micro-freeze / Frame Pause) upon impactful hits, crits, and explosions.
    /// Uses a pure zero-allocation Update loop timer (eliminates Coroutine and WaitForSecondsRealtime GC pressure).
    /// Strictly modular and under 500 lines.
    /// </summary>
    public class HitStopManager : MonoBehaviour
    {
        public static HitStopManager Instance { get; private set; }

        private float _remainingHitStopTime = 0f;
        private float _originalTimeScale = 1.0f;
        private bool _isHitStopping = false;

        private float _lastHitStopTime = -1f;
        private const float HitStopDebounceCooldown = 0.20f;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void OnDestroy()
        {
            if (_isHitStopping && Time.timeScale > 0.001f)
            {
                Time.timeScale = _originalTimeScale;
            }
            if (Instance == this)
            {
                Instance = null;
            }
        }

        /// <summary>
        /// Triggers a micro freeze-frame hit-stop without any garbage collection.
        /// </summary>
        /// <param name="duration">Real-time seconds to freeze/slowdown (typical: 0.02f - 0.05f)</param>
        /// <param name="slowScale">Time scale during freeze (typical: 0.15f - 0.25f for smooth cinematic punch)</param>
        public void TriggerHitStop(float duration = 0.035f, float slowScale = 0.20f)
        {
            if (!gameObject.activeInHierarchy || duration <= 0f) return;
            // Never trigger hit stop if game is paused (LevelUp / PauseMenu)
            if (Time.timeScale <= 0.001f) return;

            // Debounce to prevent lag from overlapping hit-stops
            if (Time.unscaledTime - _lastHitStopTime < HitStopDebounceCooldown) return;
            _lastHitStopTime = Time.unscaledTime;

            if (!_isHitStopping)
            {
                _originalTimeScale = Time.timeScale > 0.001f ? Time.timeScale : 1.0f;
            }

            _remainingHitStopTime = duration;
            _isHitStopping = true;
            Time.timeScale = slowScale;
        }

        /// <summary>
        /// Immediately cancels any active hit-stop and leaves Time.timeScale untouched.
        /// Used by LevelUpUiView and Pause menus.
        /// </summary>
        public void CancelHitStop()
        {
            _remainingHitStopTime = 0f;
            _isHitStopping = false;
        }

        private void Update()
        {
            if (!_isHitStopping) return;

            // If game got paused externally (LevelUp or Pause Menu), abort hitstop immediately
            if (Time.timeScale <= 0.001f)
            {
                _remainingHitStopTime = 0f;
                _isHitStopping = false;
                return;
            }

            _remainingHitStopTime -= Time.unscaledDeltaTime;
            if (_remainingHitStopTime <= 0f)
            {
                _remainingHitStopTime = 0f;
                _isHitStopping = false;
                Time.timeScale = _originalTimeScale;
            }
        }
    }
}
