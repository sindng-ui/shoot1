using System;
using System.Collections.Generic;
using System.Linq;
using HappyShoot.Domain.Entities;
using HappyShoot.Domain.Skills;

namespace HappyShoot.Domain.Leveling
{
    public enum RewardCategory
    {
        NewActiveSkill,
        UpgradeActiveSkill,
        PassiveStatBuff
    }

    public class SkillRewardOption
    {
        public string Id { get; }
        public string Title { get; }
        public string Description { get; }
        public RewardCategory Category { get; }
        public int CurrentLevel { get; }
        public int NextLevel { get; }
        public Func<ISkill> SkillFactory { get; }
        public Action<PlayerEntity> PassiveApplier { get; }

        public SkillRewardOption(
            string id,
            string title,
            string description,
            RewardCategory category,
            int currentLevel,
            int nextLevel,
            Func<ISkill> skillFactory = null,
            Action<PlayerEntity> passiveApplier = null)
        {
            Id = id;
            Title = title;
            Description = description;
            Category = category;
            CurrentLevel = currentLevel;
            NextLevel = nextLevel;
            SkillFactory = skillFactory;
            PassiveApplier = passiveApplier;
        }
    }

    /// <summary>
    /// Generates 3 random non-duplicate skill/passive upgrades on level up and applies selections.
    /// </summary>
    public class SkillRewardManager
    {
        private readonly Dictionary<string, (string title, string description, Func<ISkill> factory)> _allSkills
            = new Dictionary<string, (string, string, Func<ISkill>)>();

        private readonly Random _random;

        public SkillRewardManager(int? seed = null)
        {
            _random = seed.HasValue ? new Random(seed.Value) : new Random();
        }

        public void RegisterSkill(string id, string title, string description, Func<ISkill> factory)
        {
            _allSkills[id] = (title, description, factory);
        }

        /// <summary>
        /// Rolls a list of reward options (usually 3) for the player upon leveling up.
        /// </summary>
        public List<SkillRewardOption> RollRewards(PlayerEntity player, int count = 3)
        {
            if (player == null) throw new ArgumentNullException(nameof(player));

            var candidateOptions = new List<SkillRewardOption>();

            // 1. Evaluate registered active skills
            foreach (var kvp in _allSkills)
            {
                string skillId = kvp.Key;
                var info = kvp.Value;

                var existingSkill = player.Skills.FirstOrDefault(s => s.Id == skillId);
                if (existingSkill == null)
                {
                    // New skill option
                    candidateOptions.Add(new SkillRewardOption(
                        id: skillId,
                        title: $"[New] {info.title}",
                        description: info.description,
                        category: RewardCategory.NewActiveSkill,
                        currentLevel: 0,
                        nextLevel: 1,
                        skillFactory: info.factory
                    ));
                }
                else if (!existingSkill.IsMaxLevel)
                {
                    // Upgrade existing skill option
                    candidateOptions.Add(new SkillRewardOption(
                        id: skillId,
                        title: $"[Lv.{existingSkill.Level + 1}] {info.title}",
                        description: $"Enhances {info.title} to level {existingSkill.Level + 1}",
                        category: RewardCategory.UpgradeActiveSkill,
                        currentLevel: existingSkill.Level,
                        nextLevel: existingSkill.Level + 1,
                        passiveApplier: p => existingSkill.LevelUp()
                    ));
                }
                // If max level, exclude from candidate pool
            }

            // 2. Add fallback/passive stat options
            candidateOptions.Add(new SkillRewardOption(
                id: "passive_hp",
                title: "Vitality Boost",
                description: "+20 Max Health & Instant 20 Heal",
                category: RewardCategory.PassiveStatBuff,
                currentLevel: 0,
                nextLevel: 1,
                passiveApplier: p =>
                {
                    var cur = p.Stats;
                    p.Stats = new CharacterStats(
                        cur.MaxHealth + 20f, cur.HealthRegen, cur.MoveSpeed, cur.AttackPowerMultiplier,
                        cur.Armor, cur.CritChance, cur.CritDamageMultiplier, cur.CooldownReduction,
                        cur.AreaMultiplier, cur.ProjectileSpeedMultiplier, cur.ExtraProjectiles, cur.PickupRadius
                    );
                    p.Heal(20f);
                }
            ));

            candidateOptions.Add(new SkillRewardOption(
                id: "passive_speed",
                title: "Wind Step",
                description: "+10% Move Speed",
                category: RewardCategory.PassiveStatBuff,
                currentLevel: 0,
                nextLevel: 1,
                passiveApplier: p =>
                {
                    var cur = p.Stats;
                    p.Stats = new CharacterStats(
                        cur.MaxHealth, cur.HealthRegen, cur.MoveSpeed * 1.10f, cur.AttackPowerMultiplier,
                        cur.Armor, cur.CritChance, cur.CritDamageMultiplier, cur.CooldownReduction,
                        cur.AreaMultiplier, cur.ProjectileSpeedMultiplier, cur.ExtraProjectiles, cur.PickupRadius
                    );
                }
            ));

            // Shuffle and pick unique options up to count
            return candidateOptions.OrderBy(_ => _random.Next()).Take(count).ToList();
        }

        /// <summary>
        /// Applies the selected reward to the player entity.
        /// </summary>
        public void ApplyReward(PlayerEntity player, SkillRewardOption selectedOption)
        {
            if (player == null || selectedOption == null) return;

            if (selectedOption.Category == RewardCategory.NewActiveSkill && selectedOption.SkillFactory != null)
            {
                player.AddSkill(selectedOption.SkillFactory.Invoke());
            }
            else if (selectedOption.PassiveApplier != null)
            {
                selectedOption.PassiveApplier.Invoke(player);
            }
        }
    }
}
