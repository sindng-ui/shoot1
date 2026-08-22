using UnityEngine;
using HappyShoot.View.Config;

namespace HappyShoot.View.Cameras
{
    /// <summary>
    /// Smooth 2D camera follow script that tracks the target transform
    /// and supports per-skill camera shake on/off configuration.
    /// Strictly modular and under 500 lines.
    /// </summary>
    public class CameraFollowView : MonoBehaviour
    {
        [SerializeField] private Transform _target;
        [SerializeField] private float _smoothSpeed = 5.0f;
        [SerializeField] private Vector3 _offset = new Vector3(0f, 0f, -10f);

        private float _shakeTimer;
        private float _shakeIntensity;

        public static CameraFollowView Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
        }

        public void SetTarget(Transform target)
        {
            _target = target;
        }

        public void TriggerShake(string skillId = null, float duration = 0.15f, float intensity = 0.2f)
        {
            if (!Domain.Settings.GameSettings.ScreenShake) return;
            if (!string.IsNullOrEmpty(skillId) && !IsSkillShakeEnabled(skillId)) return;

            _shakeTimer = duration;
            _shakeIntensity = intensity;
        }

        public static bool IsSkillShakeEnabled(string skillId)
        {
            var cfg = SkillConfigRepository.Instance.GetConfig();
            if (cfg == null) return false;

            switch (skillId)
            {
                case "slash": return cfg.Slash.EnableCameraShake;
                case "ground_stomp": return cfg.GroundStomp.EnableCameraShake;
                case "whirlwind": return cfg.Whirlwind.EnableCameraShake;
                case "bow": return cfg.Bow.EnableCameraShake;
                case "glaive": return cfg.Glaive.EnableCameraShake;
                case "arrow_rain": return cfg.ArrowRain.EnableCameraShake;
                case "fireball": return cfg.Fireball.EnableCameraShake;
                case "frost_nova": return cfg.FrostNova.EnableCameraShake;
                case "chain_lightning": return cfg.ChainLightning.EnableCameraShake;
                case "orbital": return cfg.Orbital.EnableCameraShake;
                default: return true;
            }
        }

        private void LateUpdate()
        {
            if (_target == null) return;

            Vector3 desiredPosition = _target.position + _offset;

            if (_shakeTimer > 0f)
            {
                _shakeTimer -= Time.deltaTime;
                Vector3 shakeOffset = new Vector3(
                    Random.Range(-_shakeIntensity, _shakeIntensity),
                    Random.Range(-_shakeIntensity, _shakeIntensity),
                    0f
                );
                desiredPosition += shakeOffset;
            }

            transform.position = Vector3.Lerp(transform.position, desiredPosition, _smoothSpeed * Time.deltaTime);
        }
    }
}
