using System;

namespace HappyShoot.Domain.Entities
{
    /// <summary>
    /// Represents an active skill instance learned and upgraded by a companion.
    /// Tracks skill level (1~5) and independent cooldown timer.
    /// Strictly modular and under 500 lines.
    /// </summary>
    public class CompanionSkillInstance
    {
        public string SkillId { get; }
        public int Level { get; private set; }
        public float CooldownTimer { get; private set; }
        public bool IsMaxLevel => Level >= 5;

        public CompanionSkillInstance(string skillId, int initialLevel = 1)
        {
            SkillId = skillId ?? throw new ArgumentNullException(nameof(skillId));
            Level = Math.Max(1, Math.Min(5, initialLevel));
            CooldownTimer = 0f; // Ready immediately on start
        }

        public void Tick(float deltaTime)
        {
            if (CooldownTimer > 0f)
            {
                CooldownTimer -= deltaTime;
                if (CooldownTimer < 0f) CooldownTimer = 0f;
            }
        }

        public bool IsReady => CooldownTimer <= 0f;

        public void Trigger(float cooldownDuration)
        {
            CooldownTimer = Math.Max(0.1f, cooldownDuration);
        }

        public bool LevelUp()
        {
            if (IsMaxLevel) return false;
            Level++;
            return true;
        }
    }
}
