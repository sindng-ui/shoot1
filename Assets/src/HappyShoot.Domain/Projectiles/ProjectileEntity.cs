using System;
using System.Collections.Generic;
using HappyShoot.Domain.Entities;
using HappyShoot.Domain.Events;
using HappyShoot.Domain.Pool;
using HappyShoot.Domain.Spatial;

namespace HappyShoot.Domain.Projectiles
{
    /// <summary>
    /// Pure C# Projectile entity with piercing, collision detection, mini AoE explosions on hit, and zero-allocation pooling.
    /// Strictly modular and under 500 lines.
    /// </summary>
    public class ProjectileEntity : ISpatialEntity, IPoolable
    {
        public int Id { get; private set; }
        public Vector2D Position { get; private set; }
        public float Radius { get; set; } = 0.35f;

        public bool IsActive { get; private set; }
        public Vector2D Direction { get; private set; }
        public float Speed { get; private set; }
        public float Damage { get; private set; }
        public int RemainingPierce { get; private set; }
        public float RemainingLifetime { get; private set; }

        public float ExplosionRadius { get; private set; }
        public float ExplosionDamage { get; private set; }
        public bool HasExplosionOnHit => ExplosionRadius > 0f && ExplosionDamage > 0f;

        private readonly HashSet<int> _hitMonsterIds = new HashSet<int>(8);
        private readonly List<MonsterEntity> _splashBuffer = new List<MonsterEntity>(16);
        private EventBus _eventBus;

        public ProjectileEntity()
        {
            IsActive = false;
        }

        public void Initialize(
            int id,
            Vector2D startPosition,
            Vector2D direction,
            float speed,
            float damage,
            int pierceCount,
            float lifetime,
            float explosionRadius = 0f,
            float explosionDamage = 0f,
            EventBus eventBus = null)
        {
            Id = id;
            Position = startPosition;
            Direction = direction.Normalized;
            Speed = speed;
            Damage = damage;
            RemainingPierce = Math.Max(1, pierceCount + 1);
            RemainingLifetime = Math.Max(0.1f, lifetime);
            ExplosionRadius = explosionRadius;
            ExplosionDamage = explosionDamage;
            _eventBus = eventBus;
            _hitMonsterIds.Clear();
            IsActive = true;
        }

        public void OnSpawn()
        {
            IsActive = true;
        }

        public void OnDespawn()
        {
            IsActive = false;
            _hitMonsterIds.Clear();
        }

        /// <summary>
        /// Moves the projectile and checks collision against monsters in spatial grid.
        /// </summary>
        public void Update(float deltaTime, SpatialGrid2D<MonsterEntity> monsterGrid, IList<MonsterEntity> queryBuffer)
        {
            if (!IsActive) return;

            RemainingLifetime -= deltaTime;
            if (RemainingLifetime <= 0f)
            {
                IsActive = false;
                return;
            }

            // Move
            Position += Direction * (Speed * deltaTime);

            // Check collision against monsters
            if (monsterGrid != null && queryBuffer != null)
            {
                int hitCount = monsterGrid.QueryRadiusNonAlloc(Position, Radius, queryBuffer);
                for (int i = 0; i < hitCount; i++)
                {
                    var monster = queryBuffer[i];
                    if (monster.IsActive && !monster.IsDead && _hitMonsterIds.Add(monster.Id))
                    {
                        // 1. Direct Pierce Damage
                        monster.TakeDamage(Damage);

                        // 2. Storm Bow Mini AoE Explosion at hit point!
                        if (HasExplosionOnHit)
                        {
                            _splashBuffer.Clear();
                            int splashCount = monsterGrid.QueryRadiusNonAlloc(Position, ExplosionRadius, _splashBuffer);
                            for (int s = 0; s < splashCount; s++)
                            {
                                var splashTarget = _splashBuffer[s];
                                if (splashTarget != null && splashTarget.IsActive && !splashTarget.IsDead && splashTarget.Id != monster.Id)
                                {
                                    splashTarget.TakeDamage(ExplosionDamage);
                                }
                            }

                            _eventBus?.Publish(new StormArrowHitExplosionEvent(Position, ExplosionRadius));
                        }

                        // Infinite pierce if RemainingPierce >= 900
                        if (RemainingPierce < 900)
                        {
                            RemainingPierce--;
                            if (RemainingPierce <= 0)
                            {
                                IsActive = false;
                                break;
                            }
                        }
                    }
                }
            }
        }
    }
}
