using System;
using System.Collections.Generic;
using HappyShoot.Domain.Events;
using HappyShoot.Domain.Skills;
using HappyShoot.Domain.Spatial;

namespace HappyShoot.Domain.Entities
{
    /// <summary>
    /// Pure C# Player entity containing all player domain logic, stats, passives, and skill execution.
    /// </summary>
    public class PlayerEntity : ISpatialEntity
    {
        public int Id { get; }
        public CharacterClassType ClassType { get; set; } = CharacterClassType.Warrior;
        public Vector2D AimDirection { get; set; } = Vector2D.Right;
        public Vector2D AimTargetPosition { get; set; } = Vector2D.Zero;
        public Vector2D Position { get; private set; }
        public float Radius { get; set; } = 0.5f;
        public bool IsActive => !IsDead;

        public CharacterStats Stats { get; set; }
        public float CurrentHealth { get; private set; }
        public bool IsDead => CurrentHealth <= 0f;

        private readonly List<ISkill> _skills = new List<ISkill>(8);
        public IReadOnlyList<ISkill> Skills => _skills;

        private readonly Dictionary<string, int> _passiveLevels = new Dictionary<string, int>(8);
        public IReadOnlyDictionary<string, int> PassiveLevels => _passiveLevels;
        public IReadOnlyCollection<string> OwnedPassives => _passiveLevels.Keys;

        private readonly EventBus _eventBus;
        private readonly SkillContext _skillContext;

        public PlayerEntity(int id, CharacterStats stats, Vector2D startPosition, EventBus eventBus = null)
        {
            Id = id;
            Stats = stats;
            Position = startPosition;
            CurrentHealth = stats.MaxHealth;
            _eventBus = eventBus;

            _skillContext = new SkillContext
            {
                CasterId = id,
                CasterEntity = this,
                CasterPosition = startPosition,
                BaseDamage = 10f,
                AreaMultiplier = stats.AreaMultiplier,
                SpeedMultiplier = stats.ProjectileSpeedMultiplier,
                EventBus = eventBus
            };
        }

        /// <summary>
        /// Moves the player by the given direction vector (normalized automatically).
        /// </summary>
        public void Move(Vector2D direction, float deltaTime)
        {
            if (IsDead || deltaTime <= 0f) return;

            Vector2D norm = direction.Normalized;
            if (norm.SqrMagnitude > 0f)
            {
                Position += norm * (Stats.MoveSpeed * deltaTime);
                _skillContext.CasterPosition = Position;
                _eventBus?.Publish(new PlayerMovedEvent(Id, Position));
            }
        }

        public bool IsGodMode { get; set; }

        /// <summary>
        /// Applies mitigated damage based on Armor and publishes PlayerDamagedEvent or PlayerDiedEvent.
        /// </summary>
        public void TakeDamage(float rawDamage)
        {
            if (IsDead || rawDamage <= 0f || IsGodMode) return;

            float damageToApply = Stats.CalculateMitigatedDamage(rawDamage);
            CurrentHealth = Math.Max(0f, CurrentHealth - damageToApply);

            _eventBus?.Publish(new PlayerDamagedEvent(Id, damageToApply, CurrentHealth, Stats.MaxHealth));

            if (CurrentHealth <= 0f)
            {
                _eventBus?.Publish(new PlayerDiedEvent(Id));
            }
        }

        /// <summary>
        /// Restores player health capped at MaxHealth.
        /// </summary>
        public void Heal(float amount)
        {
            if (IsDead || amount <= 0f) return;

            float previousHealth = CurrentHealth;
            CurrentHealth = Math.Min(Stats.MaxHealth, CurrentHealth + amount);
            float actualHealed = CurrentHealth - previousHealth;

            if (actualHealed > 0f)
            {
                _eventBus?.Publish(new PlayerHealedEvent(Id, actualHealed, CurrentHealth, Stats.MaxHealth));
            }
        }

        private readonly Random _random = new Random();

        /// <summary>
        /// Evaluates critical strike chance and returns final calculated damage and critical status flag.
        /// </summary>
        public (float damage, bool isCritical) RollDamage(float rawDamage)
        {
            if (rawDamage <= 0f) return (0f, false);

            bool isCrit = Stats.CritChance > 0f && (_random.NextDouble() < Stats.CritChance);
            float finalDmg = isCrit ? rawDamage * Stats.CritDamageMultiplier : rawDamage;
            return (finalDmg, isCrit);
        }

        /// <summary>
        /// Equips a skill to the player.
        /// </summary>
        public void AddSkill(ISkill skill)
        {
            if (skill == null) throw new ArgumentNullException(nameof(skill));
            if (!_skills.Contains(skill))
            {
                _skills.Add(skill);
            }
        }

        /// <summary>
        /// Checks if the player currently possesses a skill with the specified ID.
        /// </summary>
        public bool HasSkill(string skillId)
        {
            if (string.IsNullOrEmpty(skillId)) return false;
            for (int i = 0; i < _skills.Count; i++)
            {
                if (_skills[i].Id == skillId) return true;
            }
            return false;
        }

        /// <summary>
        /// Retrieves a skill with the specified ID or null if not equipped.
        /// </summary>
        public ISkill GetSkill(string skillId)
        {
            if (string.IsNullOrEmpty(skillId)) return null;
            for (int i = 0; i < _skills.Count; i++)
            {
                if (_skills[i].Id == skillId) return _skills[i];
            }
            return null;
        }

        /// <summary>
        /// Unequips / removes a skill from the player.
        /// </summary>
        public bool RemoveSkill(string skillId)
        {
            if (string.IsNullOrEmpty(skillId)) return false;
            for (int i = 0; i < _skills.Count; i++)
            {
                if (_skills[i].Id == skillId)
                {
                    _skills.RemoveAt(i);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Replaces an existing skill (used for skill evolution).
        /// </summary>
        public bool ReplaceSkill(string oldSkillId, ISkill newSkill)
        {
            if (newSkill == null) throw new ArgumentNullException(nameof(newSkill));

            for (int i = 0; i < _skills.Count; i++)
            {
                if (_skills[i].Id == oldSkillId)
                {
                    _skills[i] = newSkill;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Registers or upgrades a passive item level.
        /// </summary>
        public int AddOrUpgradePassive(string passiveId, int maxLevel = 5)
        {
            if (string.IsNullOrEmpty(passiveId)) return 0;

            if (_passiveLevels.TryGetValue(passiveId, out int currentLevel))
            {
                if (currentLevel < maxLevel)
                {
                    currentLevel++;
                    _passiveLevels[passiveId] = currentLevel;
                }
                return currentLevel;
            }

            _passiveLevels[passiveId] = 1;
            return 1;
        }

        public void AddPassive(string passiveId) => AddOrUpgradePassive(passiveId);

        public bool HasPassive(string passiveId) => _passiveLevels.ContainsKey(passiveId);

        public bool RemovePassive(string passiveId)
        {
            if (string.IsNullOrEmpty(passiveId)) return false;
            return _passiveLevels.Remove(passiveId);
        }

        public int GetPassiveLevel(string passiveId)
        {
            return _passiveLevels.TryGetValue(passiveId, out int level) ? level : 0;
        }

        /// <summary>
        /// Regular domain tick to handle health regeneration and active skill cycles.
        /// </summary>
        public void Update(float deltaTime, ISpatialGrid2D enemyGrid, Projectiles.ProjectileManager projectileManager = null)
        {
            if (IsDead || deltaTime <= 0f) return;

            // Health regen
            if (Stats.HealthRegen > 0f && CurrentHealth < Stats.MaxHealth)
            {
                Heal(Stats.HealthRegen * deltaTime);
            }

            // Update skill context
            _skillContext.CasterPosition = Position;
            _skillContext.AimDirection = AimDirection;
            _skillContext.AimTargetPosition = AimTargetPosition;
            _skillContext.AreaMultiplier = Stats.AreaMultiplier;
            _skillContext.SpeedMultiplier = Stats.ProjectileSpeedMultiplier;
            _skillContext.DeltaTime = deltaTime;
            _skillContext.TargetGrid = enemyGrid;
            _skillContext.ProjectileManager = projectileManager;

            // Execute skills
            for (int i = 0; i < _skills.Count; i++)
            {
                _skills[i].Update(deltaTime, _skillContext);
            }
        }
    }
}
