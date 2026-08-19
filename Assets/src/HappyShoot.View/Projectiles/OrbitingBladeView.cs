using UnityEngine;
using HappyShoot.View.Player;
using HappyShoot.View.Utils;

namespace HappyShoot.View.Projectiles
{
    /// <summary>
    /// Visual representation of the orbiting blades / spinning swords around the player.
    /// </summary>
    public class OrbitingBladeView : MonoBehaviour
    {
        [SerializeField] private float _orbitRadius = 2.0f;
        [SerializeField] private float _rotationSpeed = 180f; // degrees per second
        [SerializeField] private int _bladeCount = 2;

        private Transform _playerTransform;
        private Transform[] _blades;
        private float _currentAngle;

        public void Initialize(Transform playerTransform, int bladeCount = 2, float orbitRadius = 2.0f)
        {
            _playerTransform = playerTransform;
            _bladeCount = bladeCount;
            _orbitRadius = orbitRadius;

            _blades = new Transform[_bladeCount];
            for (int i = 0; i < _bladeCount; i++)
            {
                var bladeGo = new GameObject($"OrbitalBlade_{i + 1}");
                bladeGo.transform.SetParent(transform);
                var sr = bladeGo.AddComponent<SpriteRenderer>();
                sr.sprite = SpriteHelper.GetOrCreateSwordSprite();
                sr.sortingOrder = 6;
                bladeGo.transform.localScale = Vector3.one * 1.2f;
                _blades[i] = bladeGo.transform;
            }
        }

        private void Update()
        {
            if (_playerTransform == null || _blades == null) return;

            transform.position = _playerTransform.position;
            _currentAngle += _rotationSpeed * Time.deltaTime;
            if (_currentAngle >= 360f) _currentAngle -= 360f;

            float step = 360f / _bladeCount;
            for (int i = 0; i < _bladeCount; i++)
            {
                float angle = (_currentAngle + i * step) * Mathf.Deg2Rad;
                float x = Mathf.Cos(angle) * _orbitRadius;
                float y = Mathf.Sin(angle) * _orbitRadius;

                _blades[i].localPosition = new Vector3(x, y, 0f);
                _blades[i].localRotation = Quaternion.Euler(0f, 0f, (_currentAngle + i * step) + 90f);
            }
        }
    }
}
