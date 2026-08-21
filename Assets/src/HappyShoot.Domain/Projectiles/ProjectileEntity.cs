using System;
using System.Collections.Generic;
using HappyShoot.Domain.Entities;
using HappyShoot.Domain.Pool;
using HappyShoot.Domain.Spatial;

namespace HappyShoot.Domain.Projectiles
{
    /// <summary>
    /// Pure C# Projectile entity with piercing, collision detection, and zero-allocation pooling.
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

        private readonly HashSet<int> _hitMonsterIds = new HashSet<int>(8);

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
            float lifetime)
        {
            Id = id;
            Position = startPosition;
            Direction = direction.Normalized;
            Speed = speed;
            Damage = damage;
            // pierceCount represents pierce count: total hits = pierceCount + 1 (e.g. 3 pierces = 4 hits)
            RemainingPierce = Math.Max(1, pierceCount + 1);
            RemainingLifetime = Math.Max(0.1f, lifetime);
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
                        monster.TakeDamage(Damage);

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
