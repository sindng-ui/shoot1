using UnityEngine;
using HappyShoot.Domain.Progression;

namespace HappyShoot.View.SkillTree
{
    /// <summary>
    /// PlayerPrefs JSON storage implementation for skill tree save data.
    /// Strictly modular and under 500 lines.
    /// </summary>
    public class JsonSkillTreeStorage : ISkillTreeStorage
    {
        private const string SaveKey = "HappyShoot_SkillTreeSave_v1";

        public SkillTreeSaveData Load()
        {
            if (PlayerPrefs.HasKey(SaveKey))
            {
                string json = PlayerPrefs.GetString(SaveKey);
                try
                {
                    var data = JsonUtility.FromJson<SkillTreeSaveData>(json);
                    if (data != null) return data;
                }
                catch
                {
                    // Fallback to empty save data if corrupt
                }
            }

            return new SkillTreeSaveData();
        }

        public void Save(SkillTreeSaveData data)
        {
            if (data == null) return;
            string json = JsonUtility.ToJson(data);
            PlayerPrefs.SetString(SaveKey, json);
            PlayerPrefs.Save();
        }
    }
}
