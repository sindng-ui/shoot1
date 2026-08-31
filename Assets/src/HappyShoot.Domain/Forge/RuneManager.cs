using System;
using System.Collections.Generic;
using HappyShoot.Domain.Progression;

namespace HappyShoot.Domain.Forge
{
    /// <summary>
    /// Core business logic for Rune Inscription.
    /// Manages unlocking, upgrading, equipping, and calculating composite RuneModifiers per skill slot.
    /// Zero-allocation queries at runtime for pure performance.
    /// </summary>
    public class RuneManager
    {
        private readonly Dictionary<string, RuneDefinition> _runeDefs = new Dictionary<string, RuneDefinition>(16);
        private ForgeSaveData _saveData;
        private Action _onSaveCallback;

        public IReadOnlyDictionary<string, RuneDefinition> Definitions => _runeDefs;
        public ForgeSaveData SaveData => _saveData;

        public event Action OnStateChanged;

        public RuneManager(ForgeSaveData saveData = null, Action onSaveCallback = null)
        {
            _saveData = saveData ?? new ForgeSaveData();
            _onSaveCallback = onSaveCallback;
        }

        public void SetSaveData(ForgeSaveData saveData, Action onSaveCallback = null)
        {
            _saveData = saveData ?? new ForgeSaveData();
            _onSaveCallback = onSaveCallback ?? _onSaveCallback;
            OnStateChanged?.Invoke();
        }

        public void RegisterRune(RuneDefinition def)
        {
            if (def == null) throw new ArgumentNullException(nameof(def));
            _runeDefs[def.Id] = def;
        }

        public RuneDefinition GetDefinition(string runeId)
        {
            if (string.IsNullOrEmpty(runeId)) return null;
            _runeDefs.TryGetValue(runeId, out var def);
            return def;
        }

        public bool IsUnlocked(string runeId) => _saveData.IsRuneUnlocked(runeId);
        public int GetLevel(string runeId) => _saveData.GetRuneLevel(runeId);
        public string GetEquippedRuneId(string skillId) => _saveData.GetEquippedRuneId(skillId);

        /// <summary>
        /// Attempts to unlock a locked rune using gems from the persistent wallet.
        /// </summary>
        public bool TryUnlockRune(string runeId, SkillTreeSaveData wallet)
        {
            if (wallet == null || !_runeDefs.TryGetValue(runeId, out var def)) return false;
            if (_saveData.IsRuneUnlocked(runeId)) return false;

            // Check gem costs
            if (wallet.GetGems(GemType.Ruby) < def.UnlockRubyCost ||
                wallet.GetGems(GemType.Emerald) < def.UnlockEmeraldCost ||
                wallet.GetGems(GemType.Amethyst) < def.UnlockAmethystCost)
            {
                return false;
            }

            // Deduct gems
            if (def.UnlockRubyCost > 0) wallet.SpendGems(GemType.Ruby, def.UnlockRubyCost);
            if (def.UnlockEmeraldCost > 0) wallet.SpendGems(GemType.Emerald, def.UnlockEmeraldCost);
            if (def.UnlockAmethystCost > 0) wallet.SpendGems(GemType.Amethyst, def.UnlockAmethystCost);

            _saveData.SetRuneLevel(runeId, 1);
            _onSaveCallback?.Invoke();
            OnStateChanged?.Invoke();
            return true;
        }

        /// <summary>
        /// Attempts to upgrade an unlocked rune by 1 level using its primary gem type.
        /// </summary>
        public bool TryUpgradeRune(string runeId, SkillTreeSaveData wallet)
        {
            if (wallet == null || !_runeDefs.TryGetValue(runeId, out var def)) return false;
            int currentLevel = _saveData.GetRuneLevel(runeId);
            if (currentLevel <= 0) return false;

            int cost = def.GetUpgradeCost(currentLevel);
            if (wallet.GetGems(def.PrimaryGem) < cost)
            {
                return false;
            }

            wallet.SpendGems(def.PrimaryGem, cost);
            _saveData.SetRuneLevel(runeId, currentLevel + 1);
            _onSaveCallback?.Invoke();
            OnStateChanged?.Invoke();
            return true;
        }

        /// <summary>
        /// Equips a rune into a specific skill slot.
        /// Multiple skill slots can equip the same rune if unlocked.
        /// </summary>
        public bool EquipRune(string skillId, string runeId)
        {
            if (string.IsNullOrEmpty(skillId)) return false;
            if (!string.IsNullOrEmpty(runeId) && !_saveData.IsRuneUnlocked(runeId)) return false;

            _saveData.SetEquippedRune(skillId, runeId);
            _onSaveCallback?.Invoke();
            OnStateChanged?.Invoke();
            return true;
        }

        /// <summary>
        /// Unequips any rune from the specified skill slot.
        /// </summary>
        public void UnequipRune(string skillId)
        {
            if (string.IsNullOrEmpty(skillId)) return;
            _saveData.ClearEquippedRune(skillId);
            _onSaveCallback?.Invoke();
            OnStateChanged?.Invoke();
        }

        /// <summary>
        /// Calculates the final RuneModifiers for a given skill ID.
        /// Evaluates resonance if multiple equipped runes share the same rune ID.
        /// Returns RuneModifiers.None if no rune is equipped.
        /// </summary>
        public RuneModifiers GetModifiersForSkill(string skillId)
        {
            if (string.IsNullOrEmpty(skillId)) return RuneModifiers.None;

            string runeId = _saveData.GetEquippedRuneId(skillId);
            if (string.IsNullOrEmpty(runeId)) return RuneModifiers.None;

            if (!_runeDefs.TryGetValue(runeId, out var def)) return RuneModifiers.None;
            int level = _saveData.GetRuneLevel(runeId);
            if (level <= 0) return RuneModifiers.None;

            var mods = def.CalculateModifiers(level);

            // ── Check Resonance ──
            if (mods.ResonanceMultiplier > 1.0f)
            {
                int matchCount = 0;
                for (int i = 0; i < _saveData.RuneSlotBindings.Keys.Count; i++)
                {
                    if (_saveData.RuneSlotBindings.Values[i] == runeId)
                    {
                        matchCount++;
                    }
                }

                if (matchCount >= 2)
                {
                    mods.DamageMultiplier *= mods.ResonanceMultiplier;
                    mods.AreaMultiplier *= mods.ResonanceMultiplier;
                }
            }

            return mods;
        }
    }
}
