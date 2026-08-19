using System;
using System.Collections.Generic;
using System.Linq;
using HappyShoot.Domain.Entities;
using HappyShoot.Domain.Skills;
using HappyShoot.Domain.Skills.Evolution;

namespace HappyShoot.Domain.Leveling
{
    public enum RewardCategory
    {
        NewActiveSkill,
        UpgradeActiveSkill,
        NewPassive,
        UpgradePassive,
        EvolveSkill
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
        public Action<PlayerEntity> CustomApplier { get; }
        public SkillEvolutionRecipe EvolutionRecipe { get; }

        public SkillRewardOption(
            string id,
            string title,
            string description,
            RewardCategory category,
            int currentLevel,
            int nextLevel,
            Func<ISkill> skillFactory = null,
            Action<PlayerEntity> customApplier = null,
            SkillEvolutionRecipe evolutionRecipe = null,
            Action<PlayerEntity> passiveApplier = null)
        {
            Id = id;
            Title = title;
            Description = description;
            Category = category;
            CurrentLevel = currentLevel;
            NextLevel = nextLevel;
            SkillFactory = skillFactory;
            CustomApplier = customApplier ?? passiveApplier;
            EvolutionRecipe = evolutionRecipe;
        }
    }

    public class PassiveDefinition
    {
        public string Id { get; }
        public string Title { get; }
        public string Description { get; }
        public int MaxLevel { get; }
        public Action<PlayerEntity, int> ApplyLevel { get; }

        public PassiveDefinition(string id, string title, string description, int maxLevel, Action<PlayerEntity, int> applyLevel)
        {
            Id = id;
            Title = title;
            Description = description;
            MaxLevel = maxLevel;
            ApplyLevel = applyLevel;
        }
    }

    /// <summary>
    /// Generates 3-4 random non-duplicate skill/passive/evolution upgrades on level up and applies selections.
    /// </summary>
    public class SkillRewardManager
    {
        private readonly Dictionary<string, (string title, string description, Func<ISkill> factory)> _allSkills
            = new Dictionary<string, (string, string, Func<ISkill>)>();

        private readonly Dictionary<string, PassiveDefinition> _allPassives
            = new Dictionary<string, PassiveDefinition>();

        private readonly SkillEvolutionManager _evolutionManager;
        private readonly Random _random;

        public SkillRewardManager(SkillEvolutionManager evolutionManager = null, int? seed = null)
        {
            _evolutionManager = evolutionManager;
            _random = seed.HasValue ? new Random(seed.Value) : new Random();
        }

        public void RegisterSkill(string id, string title, string description, Func<ISkill> factory)
        {
            _allSkills[id] = (title, description, factory);
        }

        public void RegisterPassive(string id, string title, string description, int maxLevel, Action<PlayerEntity, int> applyLevel)
        {
            _allPassives[id] = new PassiveDefinition(id, title, description, maxLevel, applyLevel);
        }

        /// <summary>
        /// Rolls a list of reward options (usually 3) for the player upon leveling up.
        /// Evolutions are prioritized first if conditions are met.
        /// </summary>
        public List<SkillRewardOption> RollRewards(PlayerEntity player, int count = 3)
        {
            if (player == null) throw new ArgumentNullException(nameof(player));

            var result = new List<SkillRewardOption>();

            // 1. Check for ready Skill Evolutions (Top Priority)
            if (_evolutionManager != null)
            {
                var readyEvolutions = _evolutionManager.GetAvailableEvolutions(player);
                for (int i = 0; i < readyEvolutions.Count; i++)
                {
                    var evo = readyEvolutions[i];
                    result.Add(new SkillRewardOption(
                        id: evo.EvolvedSkillId,
                        title: $"✨ [EVOLVE] {evo.EvolvedSkillName}",
                        description: $"Combines {evo.BaseSkillId} with {evo.RequiredPassiveId} into supreme weapon!",
                        category: RewardCategory.EvolveSkill,
                        currentLevel: 8,
                        nextLevel: 9,
                        evolutionRecipe: evo
                    ));
                }
            }

            if (result.Count >= count)
            {
                return result.Take(count).ToList();
            }

            var pool = new List<SkillRewardOption>();

            // 2. Evaluate active skills
            foreach (var kvp in _allSkills)
            {
                string skillId = kvp.Key;
                var info = kvp.Value;

                var existingSkill = player.Skills.FirstOrDefault(s => s.Id == skillId);
                if (existingSkill == null)
                {
                    pool.Add(new SkillRewardOption(
                        id: skillId,
                        title: $"🗡️ [New] {info.title}",
                        description: info.description,
                        category: RewardCategory.NewActiveSkill,
                        currentLevel: 0,
                        nextLevel: 1,
                        skillFactory: info.factory
                    ));
                }
                else if (!existingSkill.IsMaxLevel)
                {
                    pool.Add(new SkillRewardOption(
                        id: skillId,
                        title: $"🗡️ [Lv.{existingSkill.Level + 1}] {info.title}",
                        description: $"Enhances {info.title} to level {existingSkill.Level + 1}",
                        category: RewardCategory.UpgradeActiveSkill,
                        currentLevel: existingSkill.Level,
                        nextLevel: existingSkill.Level + 1,
                        customApplier: p => existingSkill.LevelUp()
                    ));
                }
            }

            // 3. Evaluate passives
            foreach (var kvp in _allPassives)
            {
                string passiveId = kvp.Key;
                var def = kvp.Value;
                int currentLevel = player.GetPassiveLevel(passiveId);

                if (currentLevel == 0)
                {
                    pool.Add(new SkillRewardOption(
                        id: passiveId,
                        title: $"🛡️ [New] {def.Title}",
                        description: def.Description,
                        category: RewardCategory.NewPassive,
                        currentLevel: 0,
                        nextLevel: 1,
                        customApplier: p =>
                        {
                            int newLv = p.AddOrUpgradePassive(def.Id, def.MaxLevel);
                            def.ApplyLevel(p, newLv);
                        }
                    ));
                }
                else if (currentLevel < def.MaxLevel)
                {
                    pool.Add(new SkillRewardOption(
                        id: passiveId,
                        title: $"🛡️ [Lv.{currentLevel + 1}] {def.Title}",
                        description: $"{def.Description} (Level {currentLevel + 1})",
                        category: RewardCategory.UpgradePassive,
                        currentLevel: currentLevel,
                        nextLevel: currentLevel + 1,
                        customApplier: p =>
                        {
                            int newLv = p.AddOrUpgradePassive(def.Id, def.MaxLevel);
                            def.ApplyLevel(p, newLv);
                        }
                    ));
                }
            }

            // Shuffle remaining candidates and fill up to count
            var shuffled = pool.OrderBy(_ => _random.Next()).Take(count - result.Count);
            result.AddRange(shuffled);

            return result;
        }

        /// <summary>
        /// Applies the selected reward to the player entity.
        /// </summary>
        public void ApplyReward(PlayerEntity player, SkillRewardOption selectedOption)
        {
            if (player == null || selectedOption == null) return;

            if (selectedOption.Category == RewardCategory.EvolveSkill && selectedOption.EvolutionRecipe != null)
            {
                _evolutionManager?.EvolveSkill(player, selectedOption.EvolutionRecipe);
            }
            else if (selectedOption.Category == RewardCategory.NewActiveSkill && selectedOption.SkillFactory != null)
            {
                player.AddSkill(selectedOption.SkillFactory.Invoke());
            }
            else if (selectedOption.CustomApplier != null)
            {
                selectedOption.CustomApplier.Invoke(player);
            }
        }
    }
}
