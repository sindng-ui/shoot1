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

        public bool LockYAxis { get; set; }
        public float LockedY { get; set; }
        public float TargetOrthoSize { get; set; }
        public float OffsetX
        {
            get => _offset.x;
            set => _offset.x = value;
        }
        private UnityEngine.Camera _cam;

        private void Awake()
        {
            Instance = this;
            _cam = GetComponent<UnityEngine.Camera>();
            if (_cam != null) TargetOrthoSize = _cam.orthographicSize;
        }

        public void SetTarget(Transform target)
        {
            _target = target;
        }

        public void TriggerShake(string skillId = null, float duration = 0.15f, float intensity = 0.2f)
        {
            if (!Domain.Settings.GameSettings.ScreenShake) return;

            float skillScale = GetSkillShakeScale(skillId);
            float masterScale = GetMasterShakeScale();
            float finalIntensity = intensity * (skillScale / 100f) * (masterScale / 100f);

            if (finalIntensity <= 0.001f) return;

            // 🎯 When not currently shaking, immediately set new intensity.
            // When already shaking, apply Max Clamping & Hard Ceiling.
            if (_shakeTimer <= 0f)
            {
                _shakeIntensity = Mathf.Min(0.38f, finalIntensity);
                _shakeTimer = duration;
            }
            else
            {
                _shakeIntensity = Mathf.Min(0.38f, Mathf.Max(_shakeIntensity, finalIntensity));
                _shakeTimer = Mathf.Max(_shakeTimer, duration);
            }
        }

        public static float GetMasterShakeScale()
        {
            var cfg = SkillConfigRepository.Instance.GetConfig();
            return cfg?.Exp?.MasterCameraShakeScale ?? 100f;
        }

        public static float GetSkillShakeScale(string skillId)
        {
            if (string.IsNullOrEmpty(skillId)) return 100f;

            var cfg = SkillConfigRepository.Instance.GetConfig();
            if (cfg == null) return 100f;

            switch (skillId)
            {
                // 1. Warrior
                case "slash": return cfg.Slash?.CameraShakeScale ?? 0f;
                case "ground_stomp": return cfg.GroundStomp?.CameraShakeScale ?? 0f;
                case "whirlwind": return cfg.Whirlwind?.CameraShakeScale ?? 0f;

                // 2. Ranger
                case "bow": return cfg.Bow?.CameraShakeScale ?? 0f;
                case "glaive": return cfg.Glaive?.CameraShakeScale ?? 0f;
                case "arrow_rain": return cfg.ArrowRain?.CameraShakeScale ?? 0f;

                // 3. Wizard
                case "fireball": return cfg.Fireball?.CameraShakeScale ?? 0f;
                case "frost_nova": return cfg.FrostNova?.CameraShakeScale ?? 0f;
                case "chain_lightning": return cfg.ChainLightning?.CameraShakeScale ?? 0f;

                // 4. Shared
                case "orbital": return cfg.Orbital?.CameraShakeScale ?? 0f;

                // 5. Ultimate Evolutions (9 Total)
                case "blood_eater": return cfg.BloodEater?.CameraShakeScale ?? 0f;
                case "tempest_whirlwind": return cfg.TempestWhirlwind?.CameraShakeScale ?? 0f;
                case "earthshaker": return cfg.Earthshaker?.CameraShakeScale ?? 0f;
                case "storm_bow": return cfg.StormBow?.CameraShakeScale ?? 0f;
                case "phantom_glaive": return cfg.PhantomGlaive?.CameraShakeScale ?? 0f;
                case "stellar_rain": return cfg.StellarRain?.CameraShakeScale ?? 0f;
                case "meteor_strike": return cfg.MeteorStrike?.CameraShakeScale ?? 0f;
                case "gigastorm_lightning": return cfg.GigastormLightning?.CameraShakeScale ?? 0f;
                case "blizzard_nova": return cfg.BlizzardNova?.CameraShakeScale ?? 0f;

                default: return 100f;
            }
        }

        public static bool IsSkillShakeEnabled(string skillId)
        {
            return GetSkillShakeScale(skillId) > 0.001f;
        }

        private void LateUpdate()
        {
            if (_target == null) return;

            Vector3 desiredPosition = _target.position + _offset;
            if (LockYAxis) desiredPosition.y = LockedY + _offset.y;

            transform.position = Vector3.Lerp(transform.position, desiredPosition, _smoothSpeed * Time.deltaTime);

            if (_cam != null && TargetOrthoSize > 0f && Mathf.Abs(_cam.orthographicSize - TargetOrthoSize) > 0.01f)
            {
                _cam.orthographicSize = Mathf.Lerp(_cam.orthographicSize, TargetOrthoSize, 3.5f * Time.deltaTime);
            }

            if (_shakeTimer > 0f)
            {
                _shakeTimer -= Time.deltaTime;
                Vector3 shakeOffset = new Vector3(
                    Random.Range(-_shakeIntensity, _shakeIntensity),
                    Random.Range(-_shakeIntensity, _shakeIntensity),
                    0f
                );
                transform.position += shakeOffset;

                if (_shakeTimer <= 0f)
                {
                    _shakeTimer = 0f;
                    _shakeIntensity = 0f;
                }
            }
        }
    }
}
