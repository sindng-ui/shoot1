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
        private readonly Dictionary<string, (string title, string description, Func<ISkill> factory, CharacterClassType[] allowedClasses)> _allSkills
            = new Dictionary<string, (string, string, Func<ISkill>, CharacterClassType[])>();

        private readonly Dictionary<string, PassiveDefinition> _allPassives
            = new Dictionary<string, PassiveDefinition>();

        private readonly SkillEvolutionManager _evolutionManager;
        private readonly Random _random;
        private int _totalRollCount = 0;

        public IReadOnlyDictionary<string, (string title, string description, Func<ISkill> factory, CharacterClassType[] allowedClasses)> AllSkills => _allSkills;
        public IReadOnlyDictionary<string, PassiveDefinition> AllPassives => _allPassives;
        public SkillEvolutionManager EvolutionManager => _evolutionManager;
        public Action<ISkill, int> SkillLevelHook { get; set; }

        public SkillRewardManager(SkillEvolutionManager evolutionManager = null, int? seed = null)
        {
            _evolutionManager = evolutionManager;
            _random = seed.HasValue ? new Random(seed.Value) : new Random();
        }

        public void RegisterSkill(string id, string title, string description, Func<ISkill> factory, CharacterClassType[] allowedClasses = null)
        {
            _allSkills[id] = (title, description, factory, allowedClasses);
        }

        public void RegisterPassive(string id, string title, string description, int maxLevel, Action<PlayerEntity, int> applyLevel)
        {
            _allPassives[id] = new PassiveDefinition(id, title, description, maxLevel, applyLevel);
        }

        /// <summary>
        /// Grants or levels up a skill directly on the player entity (Used by Dev Mode / Debug tools).
        /// </summary>
        public bool GrantOrLevelUpSkillDirectly(PlayerEntity player, string skillId)
        {
            if (player == null || string.IsNullOrEmpty(skillId)) return false;

            var existing = player.GetSkill(skillId);
            if (existing != null)
            {
                if (!existing.IsMaxLevel)
                {
                    existing.LevelUp();
                    SkillLevelHook?.Invoke(existing, existing.Level);
                    return true;
                }
                return false;
            }

            if (_allSkills.TryGetValue(skillId, out var info) && info.factory != null)
            {
                var skill = info.factory.Invoke();
                SkillLevelHook?.Invoke(skill, 1);
                player.AddSkill(skill);
                return true;
            }

            // Check if it's an evolved skill
            if (_evolutionManager != null)
            {
                for (int i = 0; i < _evolutionManager.Recipes.Count; i++)
                {
                    var recipe = _evolutionManager.Recipes[i];
                    if (recipe.EvolvedSkillId == skillId && recipe.EvolvedSkillFactory != null)
                    {
                        var evolvedSkill = recipe.EvolvedSkillFactory.Invoke();
                        SkillLevelHook?.Invoke(evolvedSkill, 1);
                        player.AddSkill(evolvedSkill);
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Grants or upgrades a passive item directly on the player entity (Used by Dev Mode / Debug tools).
        /// </summary>
        public bool GrantOrUpgradePassiveDirectly(PlayerEntity player, string passiveId)
        {
            if (player == null || string.IsNullOrEmpty(passiveId)) return false;

            if (_allPassives.TryGetValue(passiveId, out var def))
            {
                int newLv = player.AddOrUpgradePassive(def.Id, def.MaxLevel);
                def.ApplyLevel(player, newLv);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Rolls a list of reward options (usually 3) for the player upon leveling up.
        /// Evolutions are prioritized first if conditions are met.
        /// </summary>
        public List<SkillRewardOption> RollRewards(PlayerEntity player, int count = 3)
        {
            if (player == null) throw new ArgumentNullException(nameof(player));
            _totalRollCount++;

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
                        title: $"✨ [진화] {evo.EvolvedSkillName}",
                        description: $"{evo.BaseSkillId} + {evo.RequiredPassiveId} 합성! 궁극 무기로 진화합니다.",
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

            // 2. Evaluate active skills with Class Restrictions
            foreach (var kvp in _allSkills)
            {
                string skillId = kvp.Key;
                var info = kvp.Value;

                // Filter out skills not allowed for this player's class
                if (info.allowedClasses != null && info.allowedClasses.Length > 0)
                {
                    if (!info.allowedClasses.Contains(player.ClassType))
                    {
                        continue;
                    }
                }

                var existingSkill = player.Skills.FirstOrDefault(s => s.Id == skillId);
                if (existingSkill == null)
                {
                    pool.Add(new SkillRewardOption(
                        id: skillId,
                        title: $"🗡️ {info.title}",
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
                        title: $"🗡️ {info.title} (Lv.{existingSkill.Level + 1})",
                        description: $"{info.title}의 위력과 성능을 Lv.{existingSkill.Level + 1}(으)로 강화합니다.",
                        category: RewardCategory.UpgradeActiveSkill,
                        currentLevel: existingSkill.Level,
                        nextLevel: existingSkill.Level + 1,
                        customApplier: p =>
                        {
                            existingSkill.LevelUp();
                            SkillLevelHook?.Invoke(existingSkill, existingSkill.Level);
                        }
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
                        title: $"🛡️ {def.Title}",
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
                        title: $"🛡️ {def.Title} (Lv.{currentLevel + 1})",
                        description: $"{def.Description} (현재 효과 강화)",
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

            // 4. Guarantee at least 1 Active Skill in the first 3 rolls (or when player has few skills)
            bool isEarlyRoll = _totalRollCount <= 3 || player.Skills.Count <= 1;
            var activeOptions = pool.Where(o => o.Category == RewardCategory.NewActiveSkill || o.Category == RewardCategory.UpgradeActiveSkill).ToList();
            var passiveOptions = pool.Where(o => o.Category == RewardCategory.NewPassive || o.Category == RewardCategory.UpgradePassive).ToList();

            if (isEarlyRoll && activeOptions.Count > 0 && !result.Any(r => r.Category == RewardCategory.NewActiveSkill || r.Category == RewardCategory.UpgradeActiveSkill))
            {
                // Force pick at least one random active skill
                int pickIdx = _random.Next(activeOptions.Count);
                var guaranteedActive = activeOptions[pickIdx];
                result.Add(guaranteedActive);
                activeOptions.RemoveAt(pickIdx);
            }

            // Pool together remaining candidates and fill up to count
            var remainingPool = activeOptions.Concat(passiveOptions).OrderBy(_ => _random.Next()).ToList();
            int needed = count - result.Count;
            if (needed > 0)
            {
                result.AddRange(remainingPool.Take(needed));
            }

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
                var skill = selectedOption.SkillFactory.Invoke();
                SkillLevelHook?.Invoke(skill, 1);
                player.AddSkill(skill);
            }
            else if (selectedOption.CustomApplier != null)
            {
                selectedOption.CustomApplier.Invoke(player);
            }
        }
    }
}
