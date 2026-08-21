using UnityEngine;
using HappyShoot.Domain.Settings;

namespace HappyShoot.View.Settings
{
    /// <summary>
    /// Unity PlayerPrefs implementation of ISettingsStorage.
    /// </summary>
    public class UnityPlayerPrefsSettingsStorage : ISettingsStorage
    {
        public int GetInt(string key, int defaultValue)
        {
            return PlayerPrefs.GetInt(key, defaultValue);
        }

        public void SetInt(string key, int value)
        {
            PlayerPrefs.SetInt(key, value);
        }

        public float GetFloat(string key, float defaultValue)
        {
            return PlayerPrefs.GetFloat(key, defaultValue);
        }

        public void SetFloat(string key, float value)
        {
            PlayerPrefs.SetFloat(key, value);
        }

        public void Save()
        {
            PlayerPrefs.Save();
        }
    }
}
