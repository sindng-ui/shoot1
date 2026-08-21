using UnityEngine;

namespace HappyShoot.View.Cameras
{
    /// <summary>
    /// Smooth 2D camera follow script that tracks the target transform.
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

        public void TriggerShake(float duration = 0.15f, float intensity = 0.2f)
        {
            if (!Domain.Settings.GameSettings.ScreenShake) return;
            _shakeTimer = duration;
            _shakeIntensity = intensity;
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
