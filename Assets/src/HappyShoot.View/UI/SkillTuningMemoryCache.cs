using System;
using System.Collections.Generic;
using HappyShoot.Domain.Skills;
using HappyShoot.Domain.Skills.Effects;
using HappyShoot.Domain.Skills.Triggers;

namespace HappyShoot.View.UI
{
    /// <summary>
    /// In-memory and persistent level-by-level (L1~L5) parameter working cache.
    /// Preserves exact tuned values per level when switching levels, saving to file, or resetting.
    /// Strictly modular and under 500 lines.
    /// </summary>
    public static class SkillTuningMemoryCache
    {
        public class LevelMemory
        {
            public float? Damage;
            public float? Radius;
            public float? Cooldown;
            public float? Speed;
            public int? Count;
            public float? Duration;
            public float? ExtraParam1; // ArcAngle / ChillDuration / JumpRadius / SpreadAngle
        }

        private static readonly Dictionary<string, Dictionary<int, LevelMemory>> _cache = new Dictionary<string, Dictionary<int, LevelMemory>>();

        public static bool HasMemory(string skillId, int level)
        {
            return _cache.TryGetValue(skillId, out var levels) && levels.ContainsKey(level);
        }

        public static LevelMemory GetOrCreate(string skillId, int level)
        {
            if (!_cache.TryGetValue(skillId, out var levels))
            {
                levels = new Dictionary<int, LevelMemory>();
                _cache[skillId] = levels;
            }

            if (!levels.TryGetValue(level, out var memory))
            {
                memory = new LevelMemory();
                levels[level] = memory;
            }

            return memory;
        }

        /// <summary>
        /// Restores saved in-memory tuned parameters into the newly instantiated skill level instance.
        /// </summary>
        public static void RestoreToInstance(string skillId, int level, CompositeSkill skill)
        {
            if (skill == null || !HasMemory(skillId, level)) return;
            var mem = GetOrCreate(skillId, level);
            var cdTrigger = skill.Trigger as CooldownTrigger;

            if (mem.Cooldown.HasValue && cdTrigger != null)
            {
                cdTrigger.Cooldown = mem.Cooldown.Value;
            }

            switch (skillId)
            {
                case "slash":
                    if (skill.Effect is GreatswordSlashEffect slash)
                    {
                        if (mem.Damage.HasValue) slash.BaseDamage = mem.Damage.Value;
                        if (mem.Radius.HasValue) slash.Radius = mem.Radius.Value;
                        if (mem.ExtraParam1.HasValue) slash.ArcAngleDegrees = mem.ExtraParam1.Value;
                    }
                    break;

                case "ground_stomp":
                    if (skill.Effect is GroundStompEffect stomp)
                    {
                        if (mem.Damage.HasValue) stomp.BaseDamage = mem.Damage.Value;
                        if (mem.Radius.HasValue) stomp.StompRadius = mem.Radius.Value;
                    }
                    break;

                case "whirlwind":
                    if (skill.Effect is WhirlwindEffect ww)
                    {
                        if (mem.Damage.HasValue) ww.BaseDamage = mem.Damage.Value;
                        if (mem.Radius.HasValue) ww.Radius = mem.Radius.Value;
                    }
                    break;

                case "bow":
                    if (skill.Effect is PiercingArrowEffect bow)
                    {
                        if (mem.Damage.HasValue) bow.BaseDamage = mem.Damage.Value;
                        if (mem.Speed.HasValue) bow.Speed = mem.Speed.Value;
                        if (mem.Count.HasValue) bow.ArrowCount = mem.Count.Value;
                        if (mem.ExtraParam1.HasValue) bow.SpreadAngleDeg = mem.ExtraParam1.Value;
                    }
                    break;

                case "glaive":
                    if (skill.Effect is WindGlaiveEffect glaive)
                    {
                        if (mem.Damage.HasValue) glaive.BaseDamage = mem.Damage.Value;
                        if (mem.Radius.HasValue) glaive.MaxDistance = mem.Radius.Value;
                        if (mem.Speed.HasValue) glaive.Speed = mem.Speed.Value;
                        if (mem.Count.HasValue) glaive.GlaiveCount = mem.Count.Value;
                    }
                    break;

                case "arrow_rain":
                    if (skill.Effect is ArrowRainEffect ar)
                    {
                        if (mem.Damage.HasValue) ar.BaseDamage = mem.Damage.Value;
                        if (mem.Radius.HasValue) ar.Radius = mem.Radius.Value;
                        if (mem.Duration.HasValue) ar.Duration = mem.Duration.Value;
                        if (mem.Count.HasValue) ar.ArrowCount = mem.Count.Value;
                    }
                    break;

                case "fireball":
                    if (skill.Effect is FireballEffect fb)
                    {
                        if (mem.Damage.HasValue) fb.BaseDamage = mem.Damage.Value;
                        if (mem.Radius.HasValue) fb.Radius = mem.Radius.Value;
                        if (mem.Speed.HasValue) fb.Speed = mem.Speed.Value;
                        if (mem.Count.HasValue) fb.FireballCount = mem.Count.Value;
                    }
                    break;

                case "frost_nova":
                    if (skill.Effect is FrostNovaEffect fn)
                    {
                        if (mem.Damage.HasValue) fn.BaseDamage = mem.Damage.Value;
                        if (mem.Radius.HasValue) fn.Radius = mem.Radius.Value;
                        if (mem.Duration.HasValue) fn.ChillDuration = mem.Duration.Value;
                    }
                    break;

                case "chain_lightning":
                    if (skill.Effect is ChainLightningEffect cl)
                    {
                        if (mem.Damage.HasValue) cl.BaseDamage = mem.Damage.Value;
                        if (mem.Count.HasValue) cl.ChainCount = mem.Count.Value;
                        if (mem.Radius.HasValue) cl.JumpRadius = mem.Radius.Value;
                    }
                    break;

                case "orbital":
                    if (skill.Effect is OrbitingBladesEffect orb)
                    {
                        if (mem.Damage.HasValue) orb.BaseDamage = mem.Damage.Value;
                        if (mem.Radius.HasValue) orb.OrbitRadius = mem.Radius.Value;
                        if (mem.Speed.HasValue) orb.RotationSpeed = mem.Speed.Value;
                        if (mem.Count.HasValue) orb.BladeCount = mem.Count.Value;
                    }
                    break;
            }
        }

        /// <summary>
        /// Exports all tuned levels from memory cache into SkillConfigData for file persistence.
        /// </summary>
        public static void ExportToConfig(SkillConfigData config)
        {
            if (config == null) return;
            config.LevelTunings.Clear();

            foreach (var skillKvp in _cache)
            {
                string skillId = skillKvp.Key;
                foreach (var levelKvp in skillKvp.Value)
                {
                    int level = levelKvp.Key;
                    var mem = levelKvp.Value;

                    config.LevelTunings.Add(new SkillLevelCustomData
                    {
                        SkillId = skillId,
                        Level = level,
                        Damage = mem.Damage ?? -1f,
                        Radius = mem.Radius ?? -1f,
                        Cooldown = mem.Cooldown ?? -1f,
                        Speed = mem.Speed ?? -1f,
                        Count = mem.Count ?? -1,
                        Duration = mem.Duration ?? -1f,
                        ExtraParam1 = mem.ExtraParam1 ?? -1f,
                        HasCustomValues = true
                    });
                }
            }
        }

        /// <summary>
        /// Imports and restores level tunings from loaded SkillConfigData into working memory cache.
        /// </summary>
        public static void ImportFromConfig(SkillConfigData config)
        {
            _cache.Clear();
            if (config == null || config.LevelTunings == null) return;

            for (int i = 0; i < config.LevelTunings.Count; i++)
            {
                var data = config.LevelTunings[i];
                if (string.IsNullOrEmpty(data.SkillId) || data.Level < 1) continue;

                var mem = GetOrCreate(data.SkillId, data.Level);
                if (data.Damage >= 0f) mem.Damage = data.Damage;
                if (data.Radius >= 0f) mem.Radius = data.Radius;
                if (data.Cooldown >= 0f) mem.Cooldown = data.Cooldown;
                if (data.Speed >= 0f) mem.Speed = data.Speed;
                if (data.Count >= 0) mem.Count = data.Count;
                if (data.Duration >= 0f) mem.Duration = data.Duration;
                if (data.ExtraParam1 >= 0f) mem.ExtraParam1 = data.ExtraParam1;
            }
        }

        public static void ClearSkill(string skillId)
        {
            if (_cache.ContainsKey(skillId))
            {
                _cache.Remove(skillId);
            }
        }

        public static void ClearAll()
        {
            _cache.Clear();
        }
    }
}
