using System;
using System.Collections.Generic;
using HappyShoot.Domain.Entities;

namespace HappyShoot.Domain.Progression
{
    /// <summary>
    /// Serializable persistent save data for the gem-based skill tree progression system.
    /// Stored via PlayerPrefs JSON.
    /// </summary>
    [Serializable]
    public class SkillTreeSaveData
    {
        /// <summary>Gem wallet: index by (int)GemType.</summary>
        public int RubyCount;
        public int EmeraldCount;
        public int AmethystCount;

        /// <summary>Permanent gold currency for skill tree progression.</summary>
        public int GoldCount;

        /// <summary>Total stage/boss victory clear count.</summary>
        public int ClearCount;

        public bool IsWarriorUnlocked => ClearCount >= 1;
        public bool IsRangerUnlocked => ClearCount >= 2;

        public void IncrementClearCount()
        {
            ClearCount++;
        }

        /// <summary>Node ID → unlocked level (0 = locked).</summary>
        public SerializableDict NodeLevels = new SerializableDict();

        /// <summary>Class → awakened branch type (0 = None).</summary>
        public int WarriorBranch;
        public int RangerBranch;
        public int WizardBranch;

        // ── Gold wallet helpers ──

        public int GetGold() => GoldCount;

        public void AddGold(int amount)
        {
            GoldCount = Math.Max(0, GoldCount + amount);
        }

        public bool TrySpendGold(int amount)
        {
            if (amount <= 0) return true;
            if (GoldCount < amount) return false;
            GoldCount -= amount;
            return true;
        }

        // ── Gem wallet helpers (zero-allocation) ──

        public int GetGemCount(GemType type)
        {
            switch (type)
            {
                case GemType.Ruby: return RubyCount;
                case GemType.Emerald: return EmeraldCount;
                case GemType.Amethyst: return AmethystCount;
                default: return 0;
            }
        }

        public void SetGemCount(GemType type, int value)
        {
            switch (type)
            {
                case GemType.Ruby: RubyCount = value; break;
                case GemType.Emerald: EmeraldCount = value; break;
                case GemType.Amethyst: AmethystCount = value; break;
            }
        }

        public void AddGems(GemType type, int amount)
        {
            SetGemCount(type, GetGemCount(type) + amount);
        }

        // ── Node level helpers ──

        public int GetNodeLevel(string nodeId)
        {
            return NodeLevels.Get(nodeId);
        }

        public void SetNodeLevel(string nodeId, int level)
        {
            NodeLevels.Set(nodeId, level);
        }

        // ── Branch awakening helpers ──

        public BranchType GetAwakenedBranch(CharacterClassType classType)
        {
            switch (classType)
            {
                case CharacterClassType.Warrior: return (BranchType)WarriorBranch;
                case CharacterClassType.Ranger: return (BranchType)RangerBranch;
                case CharacterClassType.Wizard: return (BranchType)WizardBranch;
                default: return BranchType.None;
            }
        }

        public void SetAwakenedBranch(CharacterClassType classType, BranchType branch)
        {
            switch (classType)
            {
                case CharacterClassType.Warrior: WarriorBranch = (int)branch; break;
                case CharacterClassType.Ranger: RangerBranch = (int)branch; break;
                case CharacterClassType.Wizard: WizardBranch = (int)branch; break;
            }
        }
    }

    /// <summary>
    /// Unity-serializable string→int dictionary wrapper (JsonUtility cannot serialize Dictionary).
    /// </summary>
    [Serializable]
    public class SerializableDict
    {
        public List<string> Keys = new List<string>();
        public List<int> Values = new List<int>();

        public int Get(string key)
        {
            int idx = Keys.IndexOf(key);
            return idx >= 0 ? Values[idx] : 0;
        }

        public void Set(string key, int value)
        {
            int idx = Keys.IndexOf(key);
            if (idx >= 0)
            {
                Values[idx] = value;
            }
            else
            {
                Keys.Add(key);
                Values.Add(value);
            }
        }

        public void Remove(string key)
        {
            int idx = Keys.IndexOf(key);
            if (idx >= 0)
            {
                Keys.RemoveAt(idx);
                Values.RemoveAt(idx);
            }
        }

        public void Clear()
        {
            Keys.Clear();
            Values.Clear();
        }

        public int Count => Keys.Count;
    }
}
