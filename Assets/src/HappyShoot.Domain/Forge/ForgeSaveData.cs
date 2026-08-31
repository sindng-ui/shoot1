using System;
using System.Collections.Generic;
using HappyShoot.Domain.Progression;

namespace HappyShoot.Domain.Forge
{
    /// <summary>
    /// Serializable persistent save data for the Magic Forge system.
    /// Covers Runes, Crystals, and Skill Reforges.
    /// Stored via PlayerPrefs JSON (same pattern as SkillTreeSaveData).
    /// </summary>
    [Serializable]
    public class ForgeSaveData
    {
        // ══════════════════════════════════════
        //  🔮 Rune Data
        // ══════════════════════════════════════

        /// <summary>runeId → level (0 = not unlocked, 1+ = unlocked at that level)</summary>
        public SerializableDict RuneLevels = new SerializableDict();

        /// <summary>skillId → runeId ("" or missing = no rune equipped)</summary>
        public SerializableStringDict RuneSlotBindings = new SerializableStringDict();

        // ══════════════════════════════════════
        //  💎 Crystal Data (Phase 2)
        // ══════════════════════════════════════

        /// <summary>crystalId → level (0 = not synthesized, 1+ = level)</summary>
        public SerializableDict CrystalLevels = new SerializableDict();

        /// <summary>Up to 3 equipped crystal IDs.</summary>
        public List<string> EquippedCrystals = new List<string>();

        // ══════════════════════════════════════
        //  🔥 Skill Reforge Data (Phase 3)
        // ══════════════════════════════════════

        /// <summary>skillId_variant → 1 (unlocked)</summary>
        public SerializableDict ReforgeUnlocks = new SerializableDict();

        /// <summary>skillId → variant suffix ("" = original)</summary>
        public SerializableStringDict ActiveReforges = new SerializableStringDict();

        // ── Rune Helpers ──

        public bool IsRuneUnlocked(string runeId) => RuneLevels.Get(runeId) > 0;
        public int GetRuneLevel(string runeId) => RuneLevels.Get(runeId);

        public void SetRuneLevel(string runeId, int level)
        {
            RuneLevels.Set(runeId, Math.Max(0, level));
        }

        public string GetEquippedRuneId(string skillId)
        {
            return RuneSlotBindings.GetValue(skillId);
        }

        public void SetEquippedRune(string skillId, string runeId)
        {
            RuneSlotBindings.SetValue(skillId, runeId ?? "");
        }

        public void ClearEquippedRune(string skillId)
        {
            RuneSlotBindings.SetValue(skillId, "");
        }

        // ── Crystal Helpers (Phase 2) ──

        public bool IsCrystalSynthesized(string crystalId) => CrystalLevels.Get(crystalId) > 0;
        public int GetCrystalLevel(string crystalId) => CrystalLevels.Get(crystalId);

        // ── Reforge Helpers (Phase 3) ──

        public bool IsReforgeUnlocked(string skillId, string variant)
        {
            return ReforgeUnlocks.Get(skillId + "_" + variant) > 0;
        }

        public string GetActiveReforge(string skillId)
        {
            return ActiveReforges.GetValue(skillId);
        }
    }

    /// <summary>
    /// Unity-serializable string→string dictionary wrapper.
    /// (JsonUtility cannot serialize Dictionary directly.)
    /// </summary>
    [Serializable]
    public class SerializableStringDict
    {
        public List<string> Keys = new List<string>();
        public List<string> Values = new List<string>();

        public string GetValue(string key)
        {
            int idx = Keys.IndexOf(key);
            return idx >= 0 ? Values[idx] : "";
        }

        public void SetValue(string key, string value)
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

        public int Count => Keys.Count;
    }
}
