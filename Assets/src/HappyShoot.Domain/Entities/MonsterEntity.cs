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

        // Status Effects
        public bool IsChilled => ChillTimer > 0f;
        public float ChillTimer { get; private set; }
        public float ChillSlowFactor { get; private set; }

        public bool IsBurning => BurnTimer > 0f;
        public float BurnTimer { get; private set; }
        private float _burnTickTimer;
        public float BurnDamagePerTick { get; private set; }

        public bool IsShocked => ShockTimer > 0f;
        public float ShockTimer { get; private set; }
        private float _shockTickTimer;
        public float ShockDamagePerTick { get; private set; }

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
            Velocity = Vector2D.Zero;
            ChillTimer = 0f;
            ChillSlowFactor = 0f;
            BurnTimer = 0f;
            _burnTickTimer = 0f;
            BurnDamagePerTick = 0f;
            ShockTimer = 0f;
            _shockTickTimer = 0f;
            ShockDamagePerTick = 0f;
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
        /// Applies Chill (Slow) debuff to the monster.
        /// </summary>
        public void ApplyChill(float duration, float slowFactor = 0.40f)
        {
            ChillTimer = Math.Max(ChillTimer, duration);
            ChillSlowFactor = slowFactor;
        }

        /// <summary>
        /// Applies Burn (Fire DoT) to the monster for duration (default 7s, ticks every 0.5s).
        /// </summary>
        public void ApplyBurn(float duration = 7.0f, float damagePerTick = 4.0f)
        {
            BurnTimer = Math.Max(BurnTimer, duration);
            BurnDamagePerTick = Math.Max(BurnDamagePerTick, damagePerTick);
        }

        /// <summary>
        /// Applies Shock (Electric DoT) to the monster for duration (default 7s, ticks every 0.7s).
        /// </summary>
        public void ApplyShock(float duration = 7.0f, float damagePerTick = 5.0f)
        {
            ShockTimer = Math.Max(ShockTimer, duration);
            ShockDamagePerTick = Math.Max(ShockDamagePerTick, damagePerTick);
        }

        /// <summary>
        /// Updates active status effects (Chill, Burn, Shock DoT ticks).
        /// </summary>
        public void UpdateStatusEffects(float deltaTime)
        {
            if (!IsActive || IsDead || deltaTime <= 0f) return;

            if (ChillTimer > 0f)
            {
                ChillTimer -= deltaTime;
            }

            if (BurnTimer > 0f)
            {
                BurnTimer -= deltaTime;
                _burnTickTimer += deltaTime;
                if (_burnTickTimer >= 0.5f)
                {
                    _burnTickTimer = 0f;
                    TakeDamage(BurnDamagePerTick);
                }
            }

            if (ShockTimer > 0f)
            {
                ShockTimer -= deltaTime;
                _shockTickTimer += deltaTime;
                if (_shockTickTimer >= 0.7f)
                {
                    _shockTickTimer = 0f;
                    TakeDamage(ShockDamagePerTick);
                }
            }
        }

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

        public Vector2D Velocity { get; set; }

        /// <summary>
        /// Updates AI movement (melee chase vs ranged kite) and attack timers.
        /// </summary>
        public void UpdateAI(Vector2D targetPosition, float deltaTime)
        {
            if (!IsActive || IsDead || deltaTime <= 0f) return;

            UpdateStatusEffects(deltaTime);
            if (IsDead) return;

            float effectiveMoveSpeed = MoveSpeed * (IsChilled ? Math.Max(0.1f, 1f - ChillSlowFactor) : 1f);

            Vector2D diff = targetPosition - Position;
            float distSqr = diff.SqrMagnitude;

            if (IsRanged)
            {
                float prefDistSqr = PreferredDistance * PreferredDistance;
                if (distSqr > prefDistSqr + 0.5f)
                {
                    float invDist = 1.0f / (float)Math.Sqrt(distSqr);
                    Position += new Vector2D(diff.X * invDist * (effectiveMoveSpeed * deltaTime), diff.Y * invDist * (effectiveMoveSpeed * deltaTime));
                }
                else if (distSqr < prefDistSqr - 1.0f && distSqr > 0.01f)
                {
                    float invDist = 1.0f / (float)Math.Sqrt(distSqr);
                    Position -= new Vector2D(diff.X * invDist * (effectiveMoveSpeed * 0.7f * deltaTime), diff.Y * invDist * (effectiveMoveSpeed * 0.7f * deltaTime));
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
                // Bat: Fast flying with steering inertia (smooth curve turning)
                if (Type == MonsterType.Bat && distSqr > 0.0001f)
                {
                    float invDist = 1.0f / (float)Math.Sqrt(distSqr);
                    Vector2D desiredDir = new Vector2D(diff.X * invDist, diff.Y * invDist);
                    Vector2D currentDir = Velocity.SqrMagnitude > 0.001f ? Velocity.Normalized : desiredDir;

                    float steerSpeed = 2.56f; // Turning speed (20% higher inertia)
                    float t = Math.Min(1.0f, steerSpeed * deltaTime);
                    Vector2D newDir = (currentDir + (desiredDir - currentDir) * t).Normalized;

                    Velocity = newDir * effectiveMoveSpeed;
                    Position += Velocity * deltaTime;
                }
                // Standard melee chase (Slime, Golem, Boss)
                else if (distSqr > 0.0001f)
                {
                    float dist = (float)Math.Sqrt(distSqr);
                    float step = effectiveMoveSpeed * deltaTime;
                    if (step >= dist)
                    {
                        Position = targetPosition;
                    }
                    else
                    {
                        float invDist = 1.0f / dist;
                        Position += new Vector2D(diff.X * invDist * step, diff.Y * invDist * step);
                    }
                }
            }
        }

        public void ConsumePendingAttack()
        {
            HasPendingRangedAttack = false;
        }

        /// <summary>
        /// Applies damage to the monster and triggers events (regular, boss, ice shatter).
        /// </summary>
        public void TakeDamage(float damage, bool isCritical = false)
        {
            if (!IsActive || IsDead || damage <= 0f) return;

            bool wasChilled = IsChilled;
            CurrentHealth = Math.Max(0f, CurrentHealth - damage);
            _eventBus?.Publish(new MonsterDamagedEvent(Id, damage, CurrentHealth, MaxHealth, Position, isCritical));

            if (IsBoss)
            {
                _eventBus?.Publish(new BossHealthUpdatedEvent(Id, CurrentHealth, MaxHealth));
            }

            if (CurrentHealth <= 0f)
            {
                _eventBus?.Publish(new MonsterDiedEvent(Id, Position, ExpValue, GoldValue));

                if (wasChilled)
                {
                    _eventBus?.Publish(new MonsterShatteredEvent(Id, Position, Radius * 2.5f));
                }

                if (IsBoss)
                {
                    _eventBus?.Publish(new BossDiedEvent(Id, TypeName, Position, GoldValue * 5));
                }
            }
        }
    }
}
