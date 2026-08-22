using System.Collections;
using UnityEngine;

namespace HappyShoot.View.Utils
{
    /// <summary>
    /// Manages Juice Hit-Stop (Micro-freeze / Frame Pause) upon impactful hits, crits, and explosions.
    /// Greatly enhances the physical feedback and punchy feel of combat.
    /// Strictly modular and under 500 lines.
    /// </summary>
    public class HitStopManager : MonoBehaviour
    {
        public static HitStopManager Instance { get; private set; }

        private Coroutine _hitStopCoroutine;
        private float _originalTimeScale = 1.0f;
        private bool _isHitStopping = false;

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
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private float _lastHitStopTime = -1f;
        private const float HitStopDebounceCooldown = 0.20f;

        /// <summary>
        /// Triggers a micro freeze-frame hit-stop.
        /// </summary>
        /// <param name="duration">Real-time seconds to freeze/slowdown (typical: 0.03f - 0.06f)</param>
        /// <param name="slowScale">Time scale during freeze (default: 0.05f near-freeze)</param>
        public void TriggerHitStop(float duration = 0.04f, float slowScale = 0.05f)
        {
            if (!gameObject.activeInHierarchy || duration <= 0f) return;
            // Never trigger hit stop if game is paused (LevelUp / PauseMenu)
            if (Time.timeScale <= 0.001f) return;

            // Debounce to prevent lag from overlapping hit-stops
            if (Time.unscaledTime - _lastHitStopTime < HitStopDebounceCooldown) return;
            _lastHitStopTime = Time.unscaledTime;

            if (_hitStopCoroutine != null)
            {
                StopCoroutine(_hitStopCoroutine);
                if (!_isHitStopping)
                {
                    _originalTimeScale = Time.timeScale;
                }
            }
            else
            {
                _originalTimeScale = Time.timeScale;
            }

            _hitStopCoroutine = StartCoroutine(DoHitStop(duration, slowScale));
        }

        /// <summary>
        /// Immediately cancels any active hit-stop and leaves Time.timeScale untouched.
        /// Used by LevelUpUiView and Pause menus.
        /// </summary>
        public void CancelHitStop()
        {
            if (_hitStopCoroutine != null)
            {
                StopCoroutine(_hitStopCoroutine);
                _hitStopCoroutine = null;
            }
            _isHitStopping = false;
        }

        private IEnumerator DoHitStop(float duration, float slowScale)
        {
            _isHitStopping = true;
            float prevScale = _originalTimeScale;

            if (prevScale > 0.01f && Time.timeScale > 0.01f)
            {
                Time.timeScale = slowScale;
            }
            else
            {
                _isHitStopping = false;
                _hitStopCoroutine = null;
                yield break;
            }

            yield return new WaitForSecondsRealtime(duration);

            // Crucial: Only restore time scale if the game was NOT paused (LevelUp / PauseMenu) during the wait!
            if (_isHitStopping && Time.timeScale > 0.001f)
            {
                Time.timeScale = prevScale;
            }

            _isHitStopping = false;
            _hitStopCoroutine = null;
        }
    }
}
