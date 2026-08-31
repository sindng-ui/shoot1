using UnityEngine;
using HappyShoot.Domain.Forge;

namespace HappyShoot.View.Forge
{
    /// <summary>
    /// PlayerPrefs JSON storage implementation for Magic Forge save data.
    /// Handles Runes, Crystals, and Skill Reforge states.
    /// Strictly modular and under 500 lines.
    /// </summary>
    public class JsonForgeStorage
    {
        private const string SaveKey = "HappyShoot_ForgeSave_v1";

        public ForgeSaveData Load()
        {
            if (PlayerPrefs.HasKey(SaveKey))
            {
                string json = PlayerPrefs.GetString(SaveKey);
                try
                {
                    var data = JsonUtility.FromJson<ForgeSaveData>(json);
                    if (data != null) return data;
                }
                catch
                {
                    // Fallback on parse failure
                }
            }

            return new ForgeSaveData();
        }

        public void Save(ForgeSaveData data)
        {
            if (data == null) return;
            string json = JsonUtility.ToJson(data);
            PlayerPrefs.SetString(SaveKey, json);
            PlayerPrefs.Save();
        }
    }
}
