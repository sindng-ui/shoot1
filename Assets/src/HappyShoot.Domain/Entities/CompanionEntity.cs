using System;
using HappyShoot.Domain.Spatial;

namespace HappyShoot.Domain.Entities
{
    public enum CompanionType
    {
        Warrior,
        Ranger
    }

    /// <summary>
    /// Pure domain entity representing an AI companion (Warrior or Ranger) escorting the Wizard.
    /// Synchronizes stats with the player entity with a fixed 1/3 (0.333x) damage scaling multiplier.
    /// Strictly modular and under 500 lines.
    /// </summary>
    public class CompanionEntity
    {
        public const float DamageMultiplier = 0.333f; // Exactly 1/3 of standard damage

        public CompanionType Type { get; }
        public PlayerEntity Owner { get; }
        public Vector2D Position { get; set; }

        private float _cooldownTimer;
        public float BaseCooldown { get; }

        public CompanionEntity(CompanionType type, PlayerEntity owner, Vector2D startPos)
        {
            Type = type;
            Owner = owner ?? throw new ArgumentNullException(nameof(owner));
            Position = startPos;

            // Base skill cooldowns
            BaseCooldown = (type == CompanionType.Warrior) ? 1.2f : 0.8f;
            _cooldownTimer = 0f;
        }

        public void Update(float deltaTime)
        {
            if (_cooldownTimer > 0f)
            {
                _cooldownTimer -= deltaTime;
            }
        }

        public bool CanAttack => _cooldownTimer <= 0f;

        public void TriggerAttack()
        {
            float cdr = Owner != null ? Owner.Stats.CooldownReduction : 0f;
            float effectiveCdr = Math.Min(0.75f, Math.Max(0f, cdr));
            _cooldownTimer = Math.Max(0.2f, BaseCooldown * (1f - effectiveCdr));
        }

        public float CalculateDamage(float baseDamage)
        {
            float playerAp = Owner != null ? Owner.Stats.AttackPowerMultiplier : 1.0f;
            return baseDamage * playerAp * DamageMultiplier;
        }

        public float CalculateArea(float baseArea)
        {
            float playerArea = Owner != null ? Owner.Stats.AreaMultiplier : 1.0f;
            return baseArea * playerArea;
        }
    }
}
