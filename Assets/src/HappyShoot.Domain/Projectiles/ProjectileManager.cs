using System;
using System.Collections.Generic;
using HappyShoot.Domain.Entities;
using HappyShoot.Domain.Pool;
using HappyShoot.Domain.Spatial;

namespace HappyShoot.Domain.Projectiles
{
    /// <summary>
    /// Manages spawning, pooling, and tick updates for all active projectiles.
    /// </summary>
    public class ProjectileManager
    {
        private readonly ObjectPool<ProjectileEntity> _pool;
        private readonly List<ProjectileEntity> _activeProjectiles = new List<ProjectileEntity>(256);
        private readonly List<MonsterEntity> _collisionQueryBuffer = new List<MonsterEntity>(16);
        private int _idCounter = 5000;

        public IReadOnlyList<ProjectileEntity> ActiveProjectiles => _activeProjectiles;
        public int ActiveCount => _activeProjectiles.Count;
        public event Action<ProjectileEntity> OnProjectileSpawned;

        public ProjectileManager(int initialCapacity = 128)
        {
            _pool = new ObjectPool<ProjectileEntity>(() => new ProjectileEntity(), initialCapacity: initialCapacity);
        }

        /// <summary>
        /// Spawns a projectile from the pool.
        /// </summary>
        public ProjectileEntity LaunchProjectile(
            Vector2D origin,
            Vector2D direction,
            float speed = 12f,
            float damage = 20f,
            int pierceCount = 1,
            float lifetime = 3f)
        {
            var projectile = _pool.Spawn();
            projectile.Initialize(
                id: ++_idCounter,
                startPosition: origin,
                direction: direction,
                speed: speed,
                damage: damage,
                pierceCount: pierceCount,
                lifetime: lifetime
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
                var proj = _activeProjectiles[i];
                proj.Update(deltaTime, monsterGrid, _collisionQueryBuffer);

                if (!proj.IsActive)
                {
                    _activeProjectiles.RemoveAt(i);
                    _pool.Despawn(proj);
                }
            }
        }

        /// <summary>
        /// Despawns all projectiles and returns them to the pool.
        /// </summary>
        public void Clear()
        {
            for (int i = _activeProjectiles.Count - 1; i >= 0; i--)
            {
                _pool.Despawn(_activeProjectiles[i]);
            }
            _activeProjectiles.Clear();
        }
    }
}
