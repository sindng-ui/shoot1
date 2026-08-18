using System;
using HappyShoot.Domain.Events;
using HappyShoot.Domain.Pool;
using HappyShoot.Domain.Spatial;

namespace HappyShoot.Domain.Entities
{
    /// <summary>
    /// Pure C# Monster entity supporting pooling and spatial grid indexing.
    /// </summary>
    public class MonsterEntity : ISpatialEntity, IPoolable
    {
        public int Id { get; private set; }
        public string TypeName { get; private set; }
        public Vector2D Position { get; private set; }
        public float Radius { get; set; } = 0.4f;

        public bool IsActive { get; private set; }
        public float MaxHealth { get; private set; }
        public float CurrentHealth { get; private set; }
        public float MoveSpeed { get; private set; }
        public float ContactDamage { get; private set; }
        public int ExpValue { get; private set; }
        public int GoldValue { get; private set; }
        public bool IsDead => CurrentHealth <= 0f;

        private EventBus _eventBus;

        public MonsterEntity()
        {
            IsActive = false;
        }

        public void Initialize(
            int id,
            string typeName,
            float maxHealth,
            float moveSpeed,
            float contactDamage,
            int expValue,
            int goldValue,
            Vector2D startPosition,
            EventBus eventBus = null)
        {
            Id = id;
            TypeName = typeName;
            MaxHealth = maxHealth;
            CurrentHealth = maxHealth;
            MoveSpeed = moveSpeed;
            ContactDamage = contactDamage;
            ExpValue = expValue;
            GoldValue = goldValue;
            Position = startPosition;
            _eventBus = eventBus;
            IsActive = true;
        }

        public void OnSpawn()
        {
            IsActive = true;
        }

        public void OnDespawn()
        {
            IsActive = false;
            _eventBus = null;
        }

        /// <summary>
        /// Moves toward the target position (Player).
        /// </summary>
        public void UpdateAI(Vector2D targetPosition, float deltaTime)
        {
            if (!IsActive || IsDead || deltaTime <= 0f) return;

            Vector2D direction = targetPosition - Position;
            if (direction.SqrMagnitude > 0.01f)
            {
                Position += direction.Normalized * (MoveSpeed * deltaTime);
            }
        }

        /// <summary>
        /// Applies damage to the monster and triggers events.
        /// </summary>
        public void TakeDamage(float damage)
        {
            if (!IsActive || IsDead || damage <= 0f) return;

            CurrentHealth = Math.Max(0f, CurrentHealth - damage);
            _eventBus?.Publish(new MonsterDamagedEvent(Id, damage, CurrentHealth, MaxHealth, Position));

            if (CurrentHealth <= 0f)
            {
                _eventBus?.Publish(new MonsterDiedEvent(Id, Position, ExpValue, GoldValue));
            }
        }
    }
}
