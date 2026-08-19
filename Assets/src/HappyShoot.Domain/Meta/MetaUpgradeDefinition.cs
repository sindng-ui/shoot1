using System;
using System.Collections.Generic;

namespace HappyShoot.Domain.Meta
{
    public enum MetaUpgradeCategory
    {
        Survival,
        Offense,
        Utility,
        Strategy
    }

    /// <summary>
    /// Metadata definition for a permanent shop upgrade.
    /// </summary>
    public class MetaUpgradeDefinition
    {
        public string Id { get; }
        public string Title { get; }
        public string Name => Title;
        public string Description { get; }
        public MetaUpgradeCategory Category { get; }
        public int MaxLevel { get; }
        public int BaseCost { get; }
        public float CostMultiplierPerLevel { get; }
        public float BonusPerLevel { get; }

        public MetaUpgradeDefinition(
            string id,
            string title,
            string description,
            MetaUpgradeCategory category,
            int maxLevel,
            int baseCost,
            float costMultiplierPerLevel = 1.3f,
            float bonusPerLevel = 1.0f)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            Title = title ?? throw new ArgumentNullException(nameof(title));
            Description = description ?? throw new ArgumentNullException(nameof(description));
            Category = category;
            MaxLevel = Math.Max(1, maxLevel);
            BaseCost = Math.Max(1, baseCost);
            CostMultiplierPerLevel = Math.Max(1.0f, costMultiplierPerLevel);
            BonusPerLevel = bonusPerLevel;
        }

        /// <summary>
        /// Calculates the cost to buy the next level.
        /// </summary>
        public int GetCostForLevel(int currentLevel)
        {
            if (currentLevel >= MaxLevel) return int.MaxValue;
            return (int)(BaseCost * Math.Pow(CostMultiplierPerLevel, currentLevel));
        }

        /// <summary>
        /// Calculates total gold invested in this upgrade up to currentLevel.
        /// </summary>
        public int GetTotalInvestedGold(int currentLevel)
        {
            int total = 0;
            for (int lvl = 0; lvl < currentLevel; lvl++)
            {
                total += (int)(BaseCost * Math.Pow(CostMultiplierPerLevel, lvl));
            }
            return total;
        }
    }

    /// <summary>
    /// Serializable persistent player save data.
    /// </summary>
    [Serializable]
    public class MetaUpgradeSaveData
    {
        public int TotalGold = 0;
        public Dictionary<string, int> UpgradeLevels = new Dictionary<string, int>();

        public int GetLevel(string upgradeId)
        {
            if (UpgradeLevels.TryGetValue(upgradeId, out int lvl))
                return lvl;
            return 0;
        }

        public void SetLevel(string upgradeId, int level)
        {
            UpgradeLevels[upgradeId] = level;
        }
    }
}
