using System.Collections.Generic;
using UnityEngine;
using HappyShoot.View.Player;
using HappyShoot.View.Utils;

namespace HappyShoot.View.Projectiles
{
    /// <summary>
    /// Ultra high-performance 0-allocation pooled manager for Enemy Projectiles.
    /// Utilizes flat struct arrays and cached positions to completely eliminate CPU spikes during mass skeleton spawns.
    /// </summary>
    public class EnemyProjectileManagerView : MonoBehaviour
    {
        private struct BoneProjectile
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Damage;
            public float Lifetime;
            public bool IsActive;
        }

        private const int PoolCapacity = 64;
        private readonly BoneProjectile[] _projectiles = new BoneProjectile[PoolCapacity];
        private readonly GameObject[] _gameObjects = new GameObject[PoolCapacity];
        private readonly Transform[] _transforms = new Transform[PoolCapacity];
        private PlayerView _playerView;

        public void Initialize(PlayerView playerView)
        {
            _playerView = playerView;
            var boneSprite = SpriteHelper.GetOrCreateBoneSprite();

            for (int i = 0; i < PoolCapacity; i++)
            {
                var go = new GameObject($"EnemyBone_{i + 1}");
                go.transform.SetParent(transform, false);
                go.transform.localScale = Vector3.one * 0.75f;
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = boneSprite;
                sr.sortingOrder = 5;
                go.SetActive(false);

                _gameObjects[i] = go;
                _transforms[i] = go.transform;
                _projectiles[i] = default;
            }
        }

        public void SpawnBoneProjectile(Vector2 spawnPos, Vector2 direction, float speed = 2.75f, float damage = 10f)
        {
            if (direction.sqrMagnitude < 0.001f) direction = Vector2.left;
            else direction.Normalize();

            for (int i = 0; i < PoolCapacity; i++)
            {
                if (!_projectiles[i].IsActive)
                {
                    _projectiles[i].IsActive = true;
                    _projectiles[i].Position = spawnPos;
                    _projectiles[i].Velocity = direction * speed;
                    _projectiles[i].Damage = damage;
                    _projectiles[i].Lifetime = 4.0f;

                    var tf = _transforms[i];
                    tf.position = spawnPos;

                    float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                    tf.rotation = Quaternion.Euler(0f, 0f, angle);

                    _gameObjects[i].SetActive(true);
                    return;
                }
            }
        }

        private void Update()
        {
            if (_playerView == null || _playerView.Entity == null || _playerView.Entity.IsDead) return;

            var playerDomainPos = _playerView.Entity.Position;
            Vector2 playerPos = new Vector2(playerDomainPos.X, playerDomainPos.Y);
            float hitRadiusSqr = 0.25f; // 0.5m radius squared
            float dt = Time.deltaTime;

            for (int i = 0; i < PoolCapacity; i++)
            {
                if (!_projectiles[i].IsActive) continue;

                _projectiles[i].Lifetime -= dt;
                if (_projectiles[i].Lifetime <= 0f)
                {
                    Despawn(i);
                    continue;
                }

                // Fast vector math
                Vector2 nextPos = _projectiles[i].Position + _projectiles[i].Velocity * dt;
                _projectiles[i].Position = nextPos;
                _transforms[i].position = nextPos;

                // Collision check against player
                float dx = nextPos.x - playerPos.x;
                float dy = nextPos.y - playerPos.y;
                if (dx * dx + dy * dy <= hitRadiusSqr)
                {
                    _playerView.Entity.TakeDamage(_projectiles[i].Damage);
                    Despawn(i);
                }
            }
        }

        private void Despawn(int index)
        {
            _projectiles[index].IsActive = false;
            _gameObjects[index].SetActive(false);
        }
    }
}
