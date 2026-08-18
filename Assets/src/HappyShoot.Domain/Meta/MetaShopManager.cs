using System;
using System.Collections.Generic;

namespace HappyShoot.Domain.Meta
{
    /// <summary>
    /// Manages purchasing, refunds, and state serialization for permanent meta progression upgrades.
    /// </summary>
    public class MetaShopManager
    {
        private readonly ISaveStorage _storage;
        private readonly MetaUpgradeSaveData _saveData;
        private readonly Dictionary<string, MetaUpgradeDefinition> _upgradeDefinitions = new Dictionary<string, MetaUpgradeDefinition>();

        public MetaUpgradeSaveData SaveData => _saveData;
        public int TotalGold => _saveData.TotalGold;
        public IReadOnlyDictionary<string, MetaUpgradeDefinition> Definitions => _upgradeDefinitions;

        public event Action OnShopStateChanged;

        public MetaShopManager(ISaveStorage storage)
        {
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
            _saveData = _storage.Load() ?? new MetaUpgradeSaveData();

            RegisterDefaultUpgrades();
        }

        private void RegisterDefaultUpgrades()
        {
            Register(new MetaUpgradeDefinition(MetaUpgradeApplier.UpgradeHealth, "Max Health", "+10 Max HP per level", MetaUpgradeCategory.Survival, maxLevel: 10, baseCost: 100, costMultiplierPerLevel: 1.25f, bonusPerLevel: 10f));
            Register(new MetaUpgradeDefinition(MetaUpgradeApplier.UpgradeArmor, "Armor", "+2 Armor per level", MetaUpgradeCategory.Survival, maxLevel: 5, baseCost: 150, costMultiplierPerLevel: 1.35f, bonusPerLevel: 2f));
            Register(new MetaUpgradeDefinition(MetaUpgradeApplier.UpgradeRegen, "Recovery", "+0.2 HP/sec regen", MetaUpgradeCategory.Survival, maxLevel: 5, baseCost: 200, costMultiplierPerLevel: 1.4f, bonusPerLevel: 0.2f));

            Register(new MetaUpgradeDefinition(MetaUpgradeApplier.UpgradeDamage, "Might", "+5% Total Damage", MetaUpgradeCategory.Offense, maxLevel: 10, baseCost: 120, costMultiplierPerLevel: 1.3f, bonusPerLevel: 0.05f));
            Register(new MetaUpgradeDefinition(MetaUpgradeApplier.UpgradeCritChance, "Critical Strike", "+2% Crit Chance", MetaUpgradeCategory.Offense, maxLevel: 5, baseCost: 200, costMultiplierPerLevel: 1.4f, bonusPerLevel: 0.02f));
            Register(new MetaUpgradeDefinition(MetaUpgradeApplier.UpgradeExtraProjectile, "Amount (Duplicator)", "+1 Extra Projectile", MetaUpgradeCategory.Offense, maxLevel: 2, baseCost: 1000, costMultiplierPerLevel: 2.5f, bonusPerLevel: 1f));

            Register(new MetaUpgradeDefinition(MetaUpgradeApplier.UpgradeSpeed, "Haste", "+5% Move Speed", MetaUpgradeCategory.Utility, maxLevel: 5, baseCost: 100, costMultiplierPerLevel: 1.25f, bonusPerLevel: 0.05f));
            Register(new MetaUpgradeDefinition(MetaUpgradeApplier.UpgradeMagnet, "Magnet Range", "+0.5m Pickup Radius", MetaUpgradeCategory.Utility, maxLevel: 5, baseCost: 80, costMultiplierPerLevel: 1.2f, bonusPerLevel: 0.5f));
        }

        public void Register(MetaUpgradeDefinition definition)
        {
            if (definition == null) throw new ArgumentNullException(nameof(definition));
            _upgradeDefinitions[definition.Id] = definition;
        }

        public void AddGold(int amount)
        {
            if (amount <= 0) return;
            _saveData.TotalGold += amount;
            _storage.Save(_saveData);
            OnShopStateChanged?.Invoke();
        }

        /// <summary>
        /// Attempts to purchase the next level of an upgrade.
        /// </summary>
        public bool TryPurchaseUpgrade(string upgradeId)
        {
            if (!_upgradeDefinitions.TryGetValue(upgradeId, out var def))
                return false;

            int currentLevel = _saveData.GetLevel(upgradeId);
            if (currentLevel >= def.MaxLevel)
                return false;

            int cost = def.GetCostForLevel(currentLevel);
            if (_saveData.TotalGold < cost)
                return false;

            // Deduct gold and increase level
            _saveData.TotalGold -= cost;
            _saveData.SetLevel(upgradeId, currentLevel + 1);

            _storage.Save(_saveData);
            OnShopStateChanged?.Invoke();
            return true;
        }

        /// <summary>
        /// 100% free refund: resets all upgrades to 0 and restores all invested gold.
        /// </summary>
        public void RefundAll()
        {
            int totalRefundedGold = 0;

            foreach (var kvp in _upgradeDefinitions)
            {
                string id = kvp.Key;
                var def = kvp.Value;
                int currentLevel = _saveData.GetLevel(id);

                if (currentLevel > 0)
                {
                    totalRefundedGold += def.GetTotalInvestedGold(currentLevel);
                    _saveData.SetLevel(id, 0);
                }
            }

            _saveData.TotalGold += totalRefundedGold;
            _storage.Save(_saveData);
            OnShopStateChanged?.Invoke();
        }
    }
}
