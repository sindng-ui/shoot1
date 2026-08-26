using System.Collections.Generic;
using UnityEngine;
using HappyShoot.View.Player;
using HappyShoot.View.Utils;

namespace HappyShoot.View.Projectiles
{
    /// <summary>
    /// Ultra high-performance 0-allocation pooled manager for Enemy Projectiles.
    /// Supports Skeleton Bone Arrows and Dark Knight Void Slashes.
    /// Strictly modular and under 180 lines (500-line architecture rule).
    /// </summary>
    public class EnemyProjectileManagerView : MonoBehaviour
    {
        public enum ProjectileType
        {
            Bone = 0,
            DarkSlash = 1
        }

        private struct EnemyProjectile
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Damage;
            public float Lifetime;
            public float HitRadiusSqr;
            public ProjectileType Type;
            public bool IsActive;
        }

        private const int PoolCapacity = 96;
        private readonly EnemyProjectile[] _projectiles = new EnemyProjectile[PoolCapacity];
        private readonly GameObject[] _gameObjects = new GameObject[PoolCapacity];
        private readonly Transform[] _transforms = new Transform[PoolCapacity];
        private readonly SpriteRenderer[] _renderers = new SpriteRenderer[PoolCapacity];

        private Sprite _boneSprite;
        private Sprite _darkSlashSprite;
        private PlayerView _playerView;

        public void Initialize(PlayerView playerView)
        {
            _playerView = playerView;
            _boneSprite = SpriteHelper.GetOrCreateBoneSprite();
            _darkSlashSprite = SpriteHelper.GetOrCreateDarkSlashSprite();

            for (int i = 0; i < PoolCapacity; i++)
            {
                var go = new GameObject($"EnemyProj_{i + 1}");
                go.transform.SetParent(transform, false);
                go.transform.localScale = Vector3.one * 0.75f;
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = _boneSprite;
                sr.sortingOrder = 22; // Above ground and monsters
                go.SetActive(false);

                _gameObjects[i] = go;
                _transforms[i] = go.transform;
                _renderers[i] = sr;
                _projectiles[i] = default;
            }
        }

        public void SpawnBoneProjectile(Vector2 spawnPos, Vector2 direction, float speed = 2.75f, float damage = 10f)
        {
            SpawnGeneric(spawnPos, direction, speed, damage, 4.0f, 0.25f, ProjectileType.Bone, _boneSprite, Vector3.one * 0.75f);
        }

        public void SpawnDarkSlashProjectile(Vector2 spawnPos, Vector2 direction, float speed = 3.5f, float damage = 20f)
        {
            SpawnGeneric(spawnPos, direction, speed, damage, 4.5f, 0.40f, ProjectileType.DarkSlash, _darkSlashSprite, Vector3.one * 1.35f);
        }

        private void SpawnGeneric(Vector2 spawnPos, Vector2 direction, float speed, float damage, float lifetime, float hitRadiusSqr, ProjectileType type, Sprite sprite, Vector3 scale)
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
                    _projectiles[i].Lifetime = lifetime;
                    _projectiles[i].HitRadiusSqr = hitRadiusSqr;
                    _projectiles[i].Type = type;

                    var tf = _transforms[i];
                    tf.position = spawnPos;
                    tf.localScale = scale;

                    float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                    tf.rotation = Quaternion.Euler(0f, 0f, angle);

                    _renderers[i].sprite = sprite;
                    _renderers[i].color = Color.white;
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
                if (dx * dx + dy * dy <= _projectiles[i].HitRadiusSqr)
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
