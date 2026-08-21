using System;

namespace HappyShoot.Domain.Settings
{
    public interface ISettingsStorage
    {
        int GetInt(string key, int defaultValue);
        void SetInt(string key, int value);
        float GetFloat(string key, float defaultValue);
        void SetFloat(string key, float value);
        void Save();
    }

    /// <summary>
    /// Master Game Settings domain model.
    /// Pure C# decoupled from UnityEngine.
    /// </summary>
    public static class GameSettings
    {
        public static event Action OnSettingsChanged;

        private const string KeyAutoTarget = "Settings_AutoTarget";
        private const string KeyBgmVolume = "Settings_BgmVolume";
        private const string KeySfxVolume = "Settings_SfxVolume";
        private const string KeyIsMuted = "Settings_IsMuted";
        private const string KeyUiScale = "Settings_UiScale";
        private const string KeyShowDamage = "Settings_ShowDamage";
        private const string KeyScreenShake = "Settings_ScreenShake";
        private const string KeyFullscreen = "Settings_Fullscreen";

        public static bool AutoTargeting { get; private set; } = true;
        public static float BgmVolume { get; private set; } = 0.6f;
        public static float SfxVolume { get; private set; } = 0.8f;
        public static bool IsMuted { get; private set; } = false;
        public static float UiScale { get; private set; } = 1.0f;
        public static bool ShowDamageText { get; private set; } = true;
        public static bool ScreenShake { get; private set; } = true;
        public static bool IsFullscreen { get; private set; } = true;

        private static ISettingsStorage _storage;

        public static void InitializeStorage(ISettingsStorage storage)
        {
            _storage = storage;
            Load();
        }

        public static void Load()
        {
            if (_storage == null) return;

            AutoTargeting = _storage.GetInt(KeyAutoTarget, 1) == 1;
            BgmVolume = _storage.GetFloat(KeyBgmVolume, 0.6f);
            SfxVolume = _storage.GetFloat(KeySfxVolume, 0.8f);
            IsMuted = _storage.GetInt(KeyIsMuted, 0) == 1;
            UiScale = _storage.GetFloat(KeyUiScale, 1.0f);
            ShowDamageText = _storage.GetInt(KeyShowDamage, 1) == 1;
            ScreenShake = _storage.GetInt(KeyScreenShake, 1) == 1;
            IsFullscreen = _storage.GetInt(KeyFullscreen, 1) == 1;
        }

        public static void SetAutoTargeting(bool enable)
        {
            AutoTargeting = enable;
            _storage?.SetInt(KeyAutoTarget, enable ? 1 : 0);
            _storage?.Save();
            OnSettingsChanged?.Invoke();
        }

        public static void SetBgmVolume(float volume)
        {
            BgmVolume = Math.Max(0f, Math.Min(1f, volume));
            _storage?.SetFloat(KeyBgmVolume, BgmVolume);
            _storage?.Save();
            OnSettingsChanged?.Invoke();
        }

        public static void SetSfxVolume(float volume)
        {
            SfxVolume = Math.Max(0f, Math.Min(1f, volume));
            _storage?.SetFloat(KeySfxVolume, SfxVolume);
            _storage?.Save();
            OnSettingsChanged?.Invoke();
        }

        public static void SetMuted(bool mute)
        {
            IsMuted = mute;
            _storage?.SetInt(KeyIsMuted, mute ? 1 : 0);
            _storage?.Save();
            OnSettingsChanged?.Invoke();
        }

        public static void SetUiScale(float scale)
        {
            UiScale = Math.Max(0.8f, Math.Min(1.25f, scale));
            _storage?.SetFloat(KeyUiScale, UiScale);
            _storage?.Save();
            OnSettingsChanged?.Invoke();
        }

        public static void SetShowDamageText(bool show)
        {
            ShowDamageText = show;
            _storage?.SetInt(KeyShowDamage, show ? 1 : 0);
            _storage?.Save();
            OnSettingsChanged?.Invoke();
        }

        public static void SetScreenShake(bool enable)
        {
            ScreenShake = enable;
            _storage?.SetInt(KeyScreenShake, enable ? 1 : 0);
            _storage?.Save();
            OnSettingsChanged?.Invoke();
        }

        public static void SetFullscreen(bool fullscreen)
        {
            IsFullscreen = fullscreen;
            _storage?.SetInt(KeyFullscreen, fullscreen ? 1 : 0);
            _storage?.Save();
            OnSettingsChanged?.Invoke();
        }
    }
}
