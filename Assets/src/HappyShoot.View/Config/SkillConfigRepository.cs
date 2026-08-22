using System;
using System.IO;
using UnityEngine;
using HappyShoot.Domain.Skills;
using HappyShoot.Domain.Skills.Effects;
using HappyShoot.Domain.Skills.Triggers;

namespace HappyShoot.View.Config
{
    /// <summary>
    /// Presentation storage repository responsible for loading, saving, and applying real-time tuned skill configurations.
    /// Persists configurations in JSON format to Application.persistentDataPath/skill_configs.json.
    /// Strictly modular and under 500 lines.
    /// </summary>
    public class SkillConfigRepository
    {
        private static SkillConfigRepository _instance;
        public static SkillConfigRepository Instance => _instance ?? (_instance = new SkillConfigRepository());

        private SkillConfigData _cachedConfig;
        private const string PrefsKey = "HappyShoot_SkillConfigsJson";

        public bool HasSavedConfigFile()
        {
            try
            {
                string filePath = GetConfigFilePath();
                return File.Exists(filePath) || PlayerPrefs.HasKey(PrefsKey);
            }
            catch
            {
                return PlayerPrefs.HasKey(PrefsKey);
            }
        }

        public SkillConfigData GetConfig()
        {
            if (_cachedConfig == null)
            {
                _cachedConfig = Load();
            }
            return _cachedConfig;
        }

        public SkillConfigData Load()
        {
            try
            {
                string filePath = GetConfigFilePath();
                if (File.Exists(filePath))
                {
                    string json = File.ReadAllText(filePath);
                    var loaded = JsonUtility.FromJson<SkillConfigData>(json);
                    if (loaded != null) return loaded;
                }
                else if (PlayerPrefs.HasKey(PrefsKey))
                {
                    string json = PlayerPrefs.GetString(PrefsKey);
                    var loaded = JsonUtility.FromJson<SkillConfigData>(json);
                    if (loaded != null) return loaded;
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[SkillConfigRepository] Failed to load config: {ex.Message}. Using defaults.");
            }

            return new SkillConfigData();
        }

        public bool Save(SkillConfigData data)
        {
            if (data == null) return false;
            _cachedConfig = data;

            try
            {
                string json = JsonUtility.ToJson(data, true);
                string filePath = GetConfigFilePath();
                string dir = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                File.WriteAllText(filePath, json);
                PlayerPrefs.SetString(PrefsKey, json);
                PlayerPrefs.Save();
                Debug.Log($"[SkillConfigRepository] Saved skill configs successfully to: {filePath}");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SkillConfigRepository] Error saving configs: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Restores configs from saved file if present, otherwise returns pre-defined defaults.
        /// </summary>
        public SkillConfigData ReloadFromFileOrDefaults()
        {
            _cachedConfig = Load();
            return _cachedConfig;
        }

        public SkillConfigData ResetToDefaults()
        {
            var fresh = new SkillConfigData();
            Save(fresh);
            return fresh;
        }

        /// <summary>
        /// Injects loaded level-specific tuned parameters (L1~L5) into the active skill instance.
        /// </summary>
        public void ApplyConfigToSkillLevel(ISkill iskill, int level)
        {
            if (iskill is not CompositeSkill skill) return;
            var config = GetConfig();
            if (config?.LevelTunings == null) return;

            SkillLevelCustomData match = null;
            for (int i = 0; i < config.LevelTunings.Count; i++)
            {
                var t = config.LevelTunings[i];
                if (t != null && t.SkillId == skill.Id && t.Level == level)
                {
                    match = t;
                    break;
                }
            }

            if (match == null) return;

            var cdTrigger = skill.Trigger as CooldownTrigger;
            if (match.Cooldown >= 0f && cdTrigger != null)
            {
                cdTrigger.Cooldown = match.Cooldown;
            }

            switch (skill.Id)
            {
                case "slash":
                    if (skill.Effect is GreatswordSlashEffect slash)
                    {
                        if (match.Damage >= 0f) slash.BaseDamage = match.Damage;
                        if (match.Radius >= 0f) slash.Radius = match.Radius;
                        if (match.ExtraParam1 >= 0f) slash.ArcAngleDegrees = match.ExtraParam1;
                    }
                    break;

                case "ground_stomp":
                    if (skill.Effect is GroundStompEffect stomp)
                    {
                        if (match.Damage >= 0f) stomp.BaseDamage = match.Damage;
                        if (match.Radius >= 0f) stomp.StompRadius = match.Radius;
                    }
                    break;

                case "whirlwind":
                    if (skill.Effect is WhirlwindEffect ww)
                    {
                        if (match.Damage >= 0f) ww.BaseDamage = match.Damage;
                        if (match.Radius >= 0f) ww.Radius = match.Radius;
                    }
                    break;

                case "bow":
                    if (skill.Effect is PiercingArrowEffect bow)
                    {
                        if (match.Damage >= 0f) bow.BaseDamage = match.Damage;
                        if (match.Speed >= 0f) bow.Speed = match.Speed;
                        if (match.Count >= 0) bow.ArrowCount = match.Count;
                        if (match.ExtraParam1 >= 0f) bow.SpreadAngleDeg = match.ExtraParam1;
                    }
                    break;

                case "glaive":
                    if (skill.Effect is WindGlaiveEffect glaive)
                    {
                        if (match.Damage >= 0f) glaive.BaseDamage = match.Damage;
                        if (match.Radius >= 0f) glaive.MaxDistance = match.Radius;
                        if (match.Speed >= 0f) glaive.Speed = match.Speed;
                        if (match.Count >= 0) glaive.GlaiveCount = match.Count;
                    }
                    break;

                case "arrow_rain":
                    if (skill.Effect is ArrowRainEffect ar)
                    {
                        if (match.Damage >= 0f) ar.BaseDamage = match.Damage;
                        if (match.Radius >= 0f) ar.Radius = match.Radius;
                        if (match.Duration >= 0f) ar.Duration = match.Duration;
                        if (match.Count >= 0) ar.ArrowCount = match.Count;
                    }
                    break;

                case "fireball":
                    if (skill.Effect is FireballEffect fb)
                    {
                        if (match.Damage >= 0f) fb.BaseDamage = match.Damage;
                        if (match.Radius >= 0f) fb.Radius = match.Radius;
                        if (match.Speed >= 0f) fb.Speed = match.Speed;
                        if (match.Count >= 0) fb.FireballCount = match.Count;
                    }
                    break;

                case "frost_nova":
                    if (skill.Effect is FrostNovaEffect fn)
                    {
                        if (match.Damage >= 0f) fn.BaseDamage = match.Damage;
                        if (match.Radius >= 0f) fn.Radius = match.Radius;
                        if (match.Duration >= 0f) fn.ChillDuration = match.Duration;
                    }
                    break;

                case "chain_lightning":
                    if (skill.Effect is ChainLightningEffect cl)
                    {
                        if (match.Damage >= 0f) cl.BaseDamage = match.Damage;
                        if (match.Count >= 0) cl.ChainCount = match.Count;
                        if (match.Radius >= 0f) cl.JumpRadius = match.Radius;
                    }
                    break;

                case "orbital":
                    if (skill.Effect is OrbitingBladesEffect orb)
                    {
                        if (match.Damage >= 0f) orb.BaseDamage = match.Damage;
                        if (match.Radius >= 0f) orb.OrbitRadius = match.Radius;
                        if (match.Speed >= 0f) orb.RotationSpeed = match.Speed;
                        if (match.Count >= 0) orb.BladeCount = match.Count;
                    }
                    break;

                case "blood_eater":
                    if (skill.Effect is BloodEaterEffect be)
                    {
                        if (match.Damage >= 0f) be.BaseDamage = match.Damage;
                        if (match.Radius >= 0f) be.Radius = match.Radius;
                        if (match.ExtraParam1 >= 0f) be.HealAmount = match.ExtraParam1;
                    }
                    break;

                case "storm_bow":
                    if (skill.Effect is StormArrowEffect sb)
                    {
                        if (match.Damage >= 0f) sb.ArrowDamage = match.Damage;
                        if (match.ExtraParam1 >= 0f) sb.ExplosionDamage = match.ExtraParam1;
                        if (match.Radius >= 0f) sb.ExplosionRadius = match.Radius;
                        if (match.Speed >= 0f) sb.Speed = match.Speed;
                        if (match.Count >= 0) sb.BaseArrowCount = match.Count;
                    }
                    break;

                case "meteor_strike":
                    if (skill.Effect is MeteorStrikeEffect ms)
                    {
                        if (match.Damage >= 0f) ms.BaseDamage = match.Damage;
                        if (match.Radius >= 0f) ms.ExplosionRadius = match.Radius;
                    }
                    break;
            }
        }

        private string GetConfigFilePath()
        {
            return Path.Combine(Application.persistentDataPath, "skill_configs.json");
        }
    }
}
