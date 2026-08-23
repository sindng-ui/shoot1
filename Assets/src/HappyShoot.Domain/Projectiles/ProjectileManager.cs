using System;
using System.Collections.Generic;
using HappyShoot.Domain.Entities;
using HappyShoot.Domain.Events;
using HappyShoot.Domain.Pool;
using HappyShoot.Domain.Spatial;

namespace HappyShoot.Domain.Projectiles
{
    /// <summary>
    /// Manages spawning, pooling, and tick updates for all active projectiles.
    /// Strictly modular and under 500 lines.
    /// </summary>
    public class ProjectileManager
    {
        private readonly ObjectPool<ProjectileEntity> _pool;
        private readonly List<ProjectileEntity> _activeProjectiles = new List<ProjectileEntity>(256);
        private readonly List<MonsterEntity> _collisionQueryBuffer = new List<MonsterEntity>(16);
        private int _idCounter = 5000;
        private EventBus _eventBus;

        public IReadOnlyList<ProjectileEntity> ActiveProjectiles => _activeProjectiles;
        public int ActiveCount => _activeProjectiles.Count;
        public event Action<ProjectileEntity> OnProjectileSpawned;

        public ProjectileManager(int initialCapacity = 128, EventBus eventBus = null)
        {
            _eventBus = eventBus;
            _pool = new ObjectPool<ProjectileEntity>(() => new ProjectileEntity(), initialCapacity: initialCapacity);
        }

        public void SetEventBus(EventBus eventBus)
        {
            _eventBus = eventBus;
        }

        /// <summary>
        /// Spawns a projectile from the pool with optional per-hit AoE explosion.
        /// </summary>
        public ProjectileEntity LaunchProjectile(
            Vector2D origin,
            Vector2D direction,
            float speed = 12f,
            float damage = 20f,
            int pierceCount = 1,
            float lifetime = 3f,
            float explosionRadius = 0f,
            float explosionDamage = 0f,
            float critChance = 0f,
            float critDamageMultiplier = 1.5f)
        {
            var projectile = _pool.Spawn();
            projectile.Initialize(
                id: ++_idCounter,
                startPosition: origin,
                direction: direction,
                speed: speed,
                damage: damage,
                pierceCount: pierceCount,
                lifetime: lifetime,
                explosionRadius: explosionRadius,
                explosionDamage: explosionDamage,
                critChance: critChance,
                critDamageMultiplier: critDamageMultiplier,
                eventBus: _eventBus
            );

            _activeProjectiles.Add(projectile);
            OnProjectileSpawned?.Invoke(projectile);
            return projectile;
        }

        /// <summary>
        /// Updates all active projectiles and returns dead ones to the pool.
        /// </summary>
        public void Update(float deltaTime, SpatialGrid2D<MonsterEntity> monsterGrid)
        {
            for (int i = _activeProjectiles.Count - 1; i >= 0; i--)
            {
                var projectile = _activeProjectiles[i];
                projectile.Update(deltaTime, monsterGrid, _collisionQueryBuffer);

                if (!projectile.IsActive)
                {
                    _activeProjectiles.RemoveAt(i);
                    _pool.Despawn(projectile);
                }
            }
        }

        public void DespawnAll()
        {
            for (int i = 0; i < _activeProjectiles.Count; i++)
            {
                _pool.Despawn(_activeProjectiles[i]);
            }
            _activeProjectiles.Clear();
        }
    }
}
