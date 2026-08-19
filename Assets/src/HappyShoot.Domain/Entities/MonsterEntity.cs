using System;
using HappyShoot.Domain.Events;
using HappyShoot.Domain.Pool;
using HappyShoot.Domain.Spatial;

namespace HappyShoot.Domain.Entities
{
    /// <summary>
    /// Pure C# Monster entity supporting archetypes, boss patterns, and zero-allocation pooling.
    /// </summary>
    public class MonsterEntity : ISpatialEntity, IPoolable
    {
        public int Id { get; private set; }
        public MonsterType Type { get; private set; }
        public string TypeName { get; private set; }
        public Vector2D Position { get; private set; }
        public float Radius { get; set; } = 0.4f;

        public bool IsActive { get; private set; }
        public bool IsBoss => Type == MonsterType.Boss;
        public float MaxHealth { get; private set; }
        public float CurrentHealth { get; private set; }
        public float MoveSpeed { get; private set; }
        public float ContactDamage { get; private set; }
        public int ExpValue { get; private set; }
        public int GoldValue { get; private set; }
        public bool IsDead => CurrentHealth <= 0f;

        // Ranged / Special attack state
        public bool IsRanged { get; private set; }
        public float PreferredDistance { get; private set; }
        public float AttackInterval { get; private set; }
        public float AttackTimer { get; private set; }
        public bool HasPendingRangedAttack { get; private set; }

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
            Initialize(id, typeName, maxHealth, moveSpeed, contactDamage, expValue, goldValue, startPosition, eventBus, MonsterType.Slime);
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
            EventBus eventBus,
            MonsterType type,
            float radius = 0.4f,
            bool isRanged = false,
            float preferredDistance = 0f,
            float attackInterval = 1.5f)
        {
            Id = id;
            TypeName = typeName;
            Type = type;
            MaxHealth = maxHealth;
            CurrentHealth = maxHealth;
            MoveSpeed = moveSpeed;
            ContactDamage = contactDamage;
            ExpValue = expValue;
            GoldValue = goldValue;
            Position = startPosition;
            Radius = radius;
            _eventBus = eventBus;
            IsRanged = isRanged;
            PreferredDistance = preferredDistance;
            AttackInterval = attackInterval;
            AttackTimer = 0f;
            HasPendingRangedAttack = false;
            IsActive = true;

            if (IsBoss)
            {
                _eventBus?.Publish(new BossSpawnedEvent(Id, TypeName, MaxHealth, Position));
            }
        }

        public void InitializeFromDefinition(
            int id,
            MonsterDefinition def,
            Vector2D startPosition,
            EventBus eventBus = null,
            float hpMultiplier = 1.0f,
            float damageMultiplier = 1.0f)
        {
            Initialize(
                id,
                def.Name,
                def.BaseMaxHealth * hpMultiplier,
                def.BaseMoveSpeed,
                def.BaseDamage * damageMultiplier,
                def.ExpValue,
                def.GoldValue,
                startPosition,
                eventBus,
                def.Type,
                def.Radius,
                def.IsRanged,
                def.PreferredDistance,
                def.AttackInterval
            );
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

        private float _contactAttackTimer = 0.5f;
        public float ContactAttackInterval { get; set; } = 0.5f;

        /// <summary>
        /// Updates AI movement and applies contact damage to the player if within range.
        /// </summary>
        public void UpdateAI(PlayerEntity player, float deltaTime)
        {
            if (!IsActive || IsDead || deltaTime <= 0f || player == null || player.IsDead) return;

            Vector2D diff = player.Position - Position;
            float contactDist = Radius + player.Radius;
            bool wasInContact = diff.SqrMagnitude <= contactDist * contactDist;

            UpdateAI(player.Position, deltaTime);

            Vector2D newDiff = player.Position - Position;
            bool isInContact = newDiff.SqrMagnitude <= contactDist * contactDist;

            if (wasInContact || isInContact)
            {
                _contactAttackTimer += deltaTime;
                if (_contactAttackTimer >= ContactAttackInterval)
                {
                    _contactAttackTimer = 0f;
                    player.TakeDamage(ContactDamage);
                }
            }
            else
            {
                // Reset timer so the next contact triggers damage promptly
                _contactAttackTimer = ContactAttackInterval;
            }
        }

        /// <summary>
        /// Updates AI movement (melee chase vs ranged kite) and attack timers.
        /// </summary>
        public void UpdateAI(Vector2D targetPosition, float deltaTime)
        {
            if (!IsActive || IsDead || deltaTime <= 0f) return;

            Vector2D diff = targetPosition - Position;
            float distSqr = diff.SqrMagnitude;

            if (IsRanged)
            {
                float prefDistSqr = PreferredDistance * PreferredDistance;
                // If too far, close in. If too close, back away slightly.
                if (distSqr > prefDistSqr + 0.5f)
                {
                    Position += diff.Normalized * (MoveSpeed * deltaTime);
                }
                else if (distSqr < prefDistSqr - 1.0f && distSqr > 0.01f)
                {
                    Position -= diff.Normalized * (MoveSpeed * 0.7f * deltaTime);
                }

                // Tick ranged shoot timer
                AttackTimer += deltaTime;
                if (AttackTimer >= AttackInterval)
                {
                    AttackTimer = 0f;
                    HasPendingRangedAttack = true;
                }
            }
            else
            {
                // Standard melee chase
                if (distSqr > 0.0001f)
                {
                    float dist = (float)Math.Sqrt(distSqr);
                    float step = MoveSpeed * deltaTime;
                    if (step >= dist)
                    {
                        Position = targetPosition;
                    }
                    else
                    {
                        Position += (diff / dist) * step;
                    }
                }
            }
        }

        public void ConsumePendingAttack()
        {
            HasPendingRangedAttack = false;
        }

        /// <summary>
        /// Applies damage to the monster and triggers events (regular & boss).
        /// </summary>
        public void TakeDamage(float damage)
        {
            if (!IsActive || IsDead || damage <= 0f) return;

            CurrentHealth = Math.Max(0f, CurrentHealth - damage);
            _eventBus?.Publish(new MonsterDamagedEvent(Id, damage, CurrentHealth, MaxHealth, Position));

            if (IsBoss)
            {
                _eventBus?.Publish(new BossHealthUpdatedEvent(Id, CurrentHealth, MaxHealth));
            }

            if (CurrentHealth <= 0f)
            {
                _eventBus?.Publish(new MonsterDiedEvent(Id, Position, ExpValue, GoldValue));

                if (IsBoss)
                {
                    _eventBus?.Publish(new BossDiedEvent(Id, TypeName, Position, GoldValue * 5));
                }
            }
        }
    }
}
