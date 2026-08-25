using UnityEngine;
using HappyShoot.View.Utils;

namespace HappyShoot.View.Projectiles
{
    /// <summary>
    /// Visual representation of the orbiting blades / spinning swords around the player.
    /// Dynamically updates blade count and radius when player levels up or acquires the orbital skill.
    /// </summary>
    public class OrbitingBladeView : MonoBehaviour
    {
        [SerializeField] private float _orbitRadius = 2.0f;
        [SerializeField] private float _rotationSpeed = 240f; // degrees per second
        [SerializeField] private int _bladeCount = 2;

        private Transform _playerTransform;
        private Transform[] _blades;
        private float _currentAngle;

        public void Initialize(Transform playerTransform, int bladeCount = 2, float orbitRadius = 2.0f)
        {
            _playerTransform = playerTransform;
            SetBlades(bladeCount, orbitRadius);
        }

        public void SetBlades(int bladeCount, float orbitRadius)
        {
            _bladeCount = Mathf.Max(1, bladeCount);
            _orbitRadius = orbitRadius;

            // Clear old blades if any
            if (_blades != null)
            {
                for (int i = 0; i < _blades.Length; i++)
                {
                    if (_blades[i] != null) Destroy(_blades[i].gameObject);
                }
            }

            _blades = new Transform[_bladeCount];
            for (int i = 0; i < _bladeCount; i++)
            {
                var bladeGo = new GameObject($"OrbitalBlade_{i + 1}");
                bladeGo.transform.SetParent(transform, false);
                var sr = bladeGo.AddComponent<SpriteRenderer>();
                sr.sprite = SpriteHelper.GetOrCreateSwordSprite();
                sr.sortingOrder = 22; // In front of monsters (10) and player (15)
                bladeGo.transform.localScale = Vector3.one * 1.3f;
                _blades[i] = bladeGo.transform;
            }
        }

        private void Update()
        {
            if (_playerTransform == null || _blades == null || _blades.Length == 0) return;

            transform.position = _playerTransform.position;
            _currentAngle += _rotationSpeed * Time.deltaTime;
            if (_currentAngle >= 360f) _currentAngle -= 360f;

            float step = 360f / _bladeCount;
            for (int i = 0; i < _blades.Length; i++)
            {
                if (_blades[i] == null) continue;

                float angle = (_currentAngle + i * step) * Mathf.Deg2Rad;
                float x = Mathf.Cos(angle) * _orbitRadius;
                float y = Mathf.Sin(angle) * _orbitRadius;

                _blades[i].localPosition = new Vector3(x, y, 0f);
                _blades[i].localRotation = Quaternion.Euler(0f, 0f, (_currentAngle + i * step) - 45f);
            }
        }
    }
}
