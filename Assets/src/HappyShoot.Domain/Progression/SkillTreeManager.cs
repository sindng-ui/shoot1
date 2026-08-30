using System;
using System.Collections.Generic;
using HappyShoot.Domain.Entities;

namespace HappyShoot.Domain.Progression
{
    /// <summary>
    /// Core manager for the gem-based skill tree progression system.
    /// Handles gem wallet, node unlocking, exclusive branch awakening, gem exchange, and refunds.
    /// </summary>
    public class SkillTreeManager
    {
        private readonly ISkillTreeStorage _storage;
        private readonly SkillTreeSaveData _saveData;
        private readonly Dictionary<string, SkillTreeNodeDef> _nodeDefs = new Dictionary<string, SkillTreeNodeDef>(64);

        public SkillTreeSaveData SaveData => _saveData;
        public IReadOnlyDictionary<string, SkillTreeNodeDef> NodeDefs => _nodeDefs;

        public event Action OnTreeStateChanged;

        public SkillTreeManager(ISkillTreeStorage storage)
        {
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
            _saveData = _storage.Load() ?? new SkillTreeSaveData();
        }

        // ── Node Registration ──

        public void RegisterNode(SkillTreeNodeDef nodeDef)
        {
            if (nodeDef == null) throw new ArgumentNullException(nameof(nodeDef));
            _nodeDefs[nodeDef.Id] = nodeDef;
        }

        // ── Gold Wallet ──

        public int GetGoldCount() => _saveData.GetGold();

        public void AddGold(int amount)
        {
            if (amount <= 0) return;
            _saveData.AddGold(amount);
            Save();
        }

        // ── Clear Count & Companions ──

        public int ClearCount => _saveData.ClearCount;
        public bool IsWarriorUnlocked => _saveData.IsWarriorUnlocked;
        public bool IsRangerUnlocked => _saveData.IsRangerUnlocked;

        public void IncrementClearCount()
        {
            _saveData.IncrementClearCount();
            Save();
        }

        // ── Gem Wallet (Legacy / Exchange) ──

        public int GetGemCount(GemType type) => _saveData.GetGemCount(type);

        public void AddGems(GemType type, int amount)
        {
            if (amount <= 0) return;
            _saveData.AddGems(type, amount);
            Save();
        }

        /// <summary>
        /// Exchange gems at 2:1 ratio. Spends 2 of sourceType, gains 1 of targetType.
        /// </summary>
        public bool TryExchangeGems(GemType sourceType, GemType targetType)
        {
            if (sourceType == targetType) return false;
            if (_saveData.GetGemCount(sourceType) < 2) return false;

            _saveData.AddGems(sourceType, -2);
            _saveData.AddGems(targetType, 1);
            Save();
            return true;
        }

        // ── Node Unlocking (Gold-Based) ──

        /// <summary>
        /// Attempts to unlock or level up a skill tree node using Gold.
        /// Validates gold cost, prerequisites, max level, and branch exclusivity.
        /// </summary>
        public bool TryUnlockNode(string nodeId)
        {
            if (!_nodeDefs.TryGetValue(nodeId, out var def))
                return false;

            int currentLevel = _saveData.GetNodeLevel(nodeId);
            if (currentLevel >= def.MaxLevel)
                return false;

            // Check gold cost
            if (_saveData.GetGold() < def.GoldCost)
                return false;

            // Check prerequisites
            if (!ArePrerequisitesMet(def))
                return false;

            // Check branch exclusivity
            if (def.Branch != BranchType.None)
            {
                var currentAwakened = _saveData.GetAwakenedBranch(def.ClassType);
                if (currentAwakened != BranchType.None && currentAwakened != def.Branch)
                    return false; // Another branch is already awakened

                // First branch node → awaken this branch
                if (currentAwakened == BranchType.None)
                {
                    _saveData.SetAwakenedBranch(def.ClassType, def.Branch);
                }
            }

            // Deduct gold and increase level
            _saveData.TrySpendGold(def.GoldCost);
            _saveData.SetNodeLevel(nodeId, currentLevel + 1);
            Save();
            return true;
        }

        /// <summary>
        /// Returns true if all prerequisite nodes are at least level 1.
        /// </summary>
        public bool ArePrerequisitesMet(SkillTreeNodeDef def)
        {
            if (def.PrerequisiteIds == null || def.PrerequisiteIds.Length == 0)
                return true;

            for (int i = 0; i < def.PrerequisiteIds.Length; i++)
            {
                if (_saveData.GetNodeLevel(def.PrerequisiteIds[i]) < 1)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Returns true if the node can be unlocked right now.
        /// </summary>
        public bool CanUnlockNode(string nodeId)
        {
            if (!_nodeDefs.TryGetValue(nodeId, out var def))
                return false;

            int currentLevel = _saveData.GetNodeLevel(nodeId);
            if (currentLevel >= def.MaxLevel)
                return false;

            if (_saveData.GetGold() < def.GoldCost)
                return false;

            if (!ArePrerequisitesMet(def))
                return false;

            if (def.Branch != BranchType.None)
            {
                var currentAwakened = _saveData.GetAwakenedBranch(def.ClassType);
                if (currentAwakened != BranchType.None && currentAwakened != def.Branch)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Checks if a branch node is locked due to another branch being awakened.
        /// </summary>
        public bool IsBranchLocked(string nodeId)
        {
            if (!_nodeDefs.TryGetValue(nodeId, out var def))
                return true;

            if (def.Branch == BranchType.None)
                return false;

            var currentAwakened = _saveData.GetAwakenedBranch(def.ClassType);
            return currentAwakened != BranchType.None && currentAwakened != def.Branch;
        }

        // ── Awakening Reset (50% Gold Refund) ──

        /// <summary>
        /// Resets the awakened branch for a class, refunding 50% of invested gold.
        /// Returns the amount of gold refunded.
        /// </summary>
        public int ResetAwakening(CharacterClassType classType)
        {
            var awakenedBranch = _saveData.GetAwakenedBranch(classType);
            if (awakenedBranch == BranchType.None)
                return 0;

            int totalRefunded = 0;

            // Find all branch nodes for this class+branch and reset them
            foreach (var kvp in _nodeDefs)
            {
                var def = kvp.Value;
                if (def.ClassType != classType || def.Branch != awakenedBranch)
                    continue;

                int level = _saveData.GetNodeLevel(def.Id);
                if (level > 0)
                {
                    // Refund 50% of total gold invested
                    int invested = level * def.GoldCost;
                    int refund = invested / 2; // Integer division → floor
                    totalRefunded += refund;
                    _saveData.SetNodeLevel(def.Id, 0);
                }
            }

            _saveData.AddGold(totalRefunded);
            _saveData.SetAwakenedBranch(classType, BranchType.None);
            Save();
            return totalRefunded;
        }

        /// <summary>
        /// Full reset: refund ALL nodes (core + branch) at 50% gold rate, clear all awakenings.
        /// </summary>
        public int ResetAll(CharacterClassType classType)
        {
            int totalRefunded = 0;

            foreach (var kvp in _nodeDefs)
            {
                var def = kvp.Value;
                if (def.ClassType != classType)
                    continue;

                int level = _saveData.GetNodeLevel(def.Id);
                if (level > 0)
                {
                    int invested = level * def.GoldCost;
                    int refund = invested / 2;
                    totalRefunded += refund;
                    _saveData.SetNodeLevel(def.Id, 0);
                }
            }

            _saveData.AddGold(totalRefunded);
            _saveData.SetAwakenedBranch(classType, BranchType.None);
            Save();
            return totalRefunded;
        }

        // ── Query Helpers ──

        public int GetNodeLevel(string nodeId) => _saveData.GetNodeLevel(nodeId);

        public BranchType GetAwakenedBranch(CharacterClassType classType) =>
            _saveData.GetAwakenedBranch(classType);

        private void Save()
        {
            _storage.Save(_saveData);
            OnTreeStateChanged?.Invoke();
        }
    }

    /// <summary>
    /// Abstract storage provider for skill tree save data.
    /// </summary>
    public interface ISkillTreeStorage
    {
        SkillTreeSaveData Load();
        void Save(SkillTreeSaveData data);
    }

    /// <summary>
    /// In-memory storage for testing.
    /// </summary>
    public class MemorySkillTreeStorage : ISkillTreeStorage
    {
        private SkillTreeSaveData _data;
        public MemorySkillTreeStorage(SkillTreeSaveData data = null) { _data = data ?? new SkillTreeSaveData(); }
        public SkillTreeSaveData Load() => _data;
        public void Save(SkillTreeSaveData data) { _data = data; }
    }
}
