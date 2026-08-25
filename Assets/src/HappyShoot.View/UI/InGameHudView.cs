using System.Text;
using UnityEngine;
using UnityEngine.UI;
using HappyShoot.Domain.Entities;
using HappyShoot.Domain.Events;
using HappyShoot.Domain.Leveling;
using HappyShoot.Domain.Session;
using HappyShoot.Domain.Skills;
using HappyShoot.Domain.Skills.Triggers;
using HappyShoot.View.Player;

namespace HappyShoot.View.UI
{
    /// <summary>
    /// Master in-game persistent HUD managing bottom 3-layer HUD:
    /// Layer 1: 10-segmented EXP bar + Level badge
    /// Layer 2: 6 Skill slots + Dash slot + 360° Clockwise Radial Cooldown
    /// Layer 3: Horned Helmet Emblem + Wide HP bar
    /// Top: Timer, Kills, Gold.
    /// Strictly modular and under 500 lines.
    /// </summary>
    public class InGameHudView : MonoBehaviour
    {
        private Slider _expSlider;
        private Text _levelText;
        private Text _expText;
        private Slider _healthSlider;
        private Text _healthText;
        private Text _timerText;
        private Text _killCountText;
        private Text _goldText;

        private PlayerView _playerView;
        private LevelSystem _levelSystem;
        private GameSessionEntity _gameSession;
        private EventBus _eventBus;

        private float _targetExpFill;
        private float _currentExpFill;
        private float _targetHpFill;
        private float _currentHpFill;

        private readonly StringBuilder _sb = new StringBuilder(32);
        private int _cachedKills = -1;
        private int _cachedGold = -1;
        private int _cachedLevel = -1;

        private Image[] _skillSlotIcons;
        private Image[] _skillSlotCooldownMasks;
        private Text[] _skillSlotLevelTexts;
        private Text[] _skillSlotCountTexts;
        private GameObject[] _skillSlotRoots;
        private Image[] _passiveSlotIcons;
        private Text[] _passiveSlotLevelTexts;
        private Text[] _passiveSlotValueTexts;
        private GameObject[] _passiveSlotRoots;
        private Image _dashCooldownMask;
        private SettingsDialogUiView _settingsDialog;
        private CanvasScaler _scaler;

        public void SetSettingsDialog(SettingsDialogUiView dialog)
        {
            _settingsDialog = dialog;
        }

        public void Initialize(PlayerView playerView, LevelSystem levelSystem, GameSessionEntity gameSession)
        {
            _playerView = playerView;
            _levelSystem = levelSystem;
            _gameSession = gameSession;

            if (_playerView != null)
            {
                _eventBus = _playerView.EventBus;
            }

            EnsureUiElements();
            SubscribeEvents();
            ApplyUiScale();
            UpdateAllUi(immediate: true);

            Domain.Settings.GameSettings.OnSettingsChanged += ApplyUiScale;
        }

        private void OnDestroy()
        {
            Domain.Settings.GameSettings.OnSettingsChanged -= ApplyUiScale;
        }

        private void ApplyUiScale()
        {
            if (_scaler != null)
            {
                float scale = Domain.Settings.GameSettings.UiScale;
                _scaler.referenceResolution = new Vector2(1920f / scale, 1080f / scale);
            }
        }

        private void SubscribeEvents()
        {
            if (_eventBus == null) return;

            _eventBus.Subscribe<PlayerDamagedEvent>(OnPlayerDamaged);
            _eventBus.Subscribe<PlayerHealedEvent>(OnPlayerHealed);
            _eventBus.Subscribe<PlayerLevelUpEvent>(OnLevelUp);
            _eventBus.Subscribe<ExpGainedEvent>(OnExpGained);
            _eventBus.Subscribe<KillCountUpdatedEvent>(OnKillCountUpdated);
            _eventBus.Subscribe<GoldGainedEvent>(OnGoldGained);
        }

        private void Update()
        {
            // Smoothly interpolate Exp Bar and HP Bar
            if (_expSlider != null && Mathf.Abs(_currentExpFill - _targetExpFill) > 0.001f)
            {
                _currentExpFill = Mathf.Lerp(_currentExpFill, _targetExpFill, Time.unscaledDeltaTime * 10f);
                _expSlider.value = _currentExpFill;
            }

            if (_healthSlider != null && Mathf.Abs(_currentHpFill - _targetHpFill) > 0.001f)
            {
                _currentHpFill = Mathf.Lerp(_currentHpFill, _targetHpFill, Time.unscaledDeltaTime * 12f);
                _healthSlider.value = _currentHpFill;
            }

            // Update timer display from game session
            if (_timerText != null && _gameSession != null)
            {
                _timerText.text = _gameSession.GetFormattedTime();
            }

            // Sync skills display & radial cooldown progress
            UpdateSkillsDisplay();
        }

        private void OnPlayerDamaged(PlayerDamagedEvent evt)
        {
            UpdateHpDisplay(evt.RemainingHealth, evt.MaxHealth);
        }

        private void OnPlayerHealed(PlayerHealedEvent evt)
        {
            UpdateHpDisplay(evt.CurrentHealth, evt.MaxHealth);
        }

        private void OnLevelUp(PlayerLevelUpEvent evt)
        {
            UpdateLevelDisplay(evt.NewLevel);
            if (_levelSystem != null)
            {
                _targetExpFill = _levelSystem.RequiredExp > 0 ? (float)_levelSystem.CurrentExp / _levelSystem.RequiredExp : 0f;
                UpdateExpTextDisplay(_levelSystem.CurrentExp, _levelSystem.RequiredExp);
            }
            UpdateSkillsDisplay();
        }

        private void OnExpGained(ExpGainedEvent evt)
        {
            _targetExpFill = evt.RequiredExp > 0 ? (float)evt.CurrentExp / evt.RequiredExp : 0f;
            UpdateExpTextDisplay(evt.CurrentExp, evt.RequiredExp);
        }

        private void OnKillCountUpdated(KillCountUpdatedEvent evt)
        {
            UpdateKillCountDisplay(evt.TotalKills);
        }

        private void OnGoldGained(GoldGainedEvent evt)
        {
            UpdateGoldDisplay(evt.TotalGold);
        }

        public void UpdateAllUi(bool immediate = false)
        {
            if (_playerView != null && _playerView.Entity != null)
            {
                var entity = _playerView.Entity;
                UpdateHpDisplay(entity.CurrentHealth, entity.Stats.MaxHealth, immediate);
            }

            if (_levelSystem != null)
            {
                UpdateLevelDisplay(_levelSystem.Level);
                _targetExpFill = _levelSystem.RequiredExp > 0 ? (float)_levelSystem.CurrentExp / _levelSystem.RequiredExp : 0f;
                UpdateExpTextDisplay(_levelSystem.CurrentExp, _levelSystem.RequiredExp);
                if (immediate)
                {
                    _currentExpFill = _targetExpFill;
                    if (_expSlider != null) _expSlider.value = _currentExpFill;
                }
            }

            if (_gameSession != null)
            {
                UpdateKillCountDisplay(_gameSession.KillCount);
                UpdateGoldDisplay(_gameSession.GoldEarned);
            }

            UpdateSkillsDisplay();
        }

        public void UpdateSkillsDisplay()
        {
            if (_playerView == null || _playerView.Entity == null || _skillSlotRoots == null) return;

            var allSkills = _playerView.Entity.Skills;
            int total = allSkills != null ? allSkills.Count : 0;

            // Filter only active skills that have an actual cooldown trigger (exclude orbital blades, passives, etc.)
            int slotIdx = 0;
            for (int s = 0; s < total && slotIdx < InGameHudBuilder.MaxSkillSlots; s++)
            {
                var skill = allSkills[s];
                if (skill == null) continue;

                // Exclude orbital blade (constant orbit with no cooldown) and passives
                if (skill.Id == "orbital" || skill.Id.StartsWith("passive_")) continue;

                if (skill is CompositeSkill comp && comp.Trigger is CooldownTrigger cd)
                {
                    if (_skillSlotRoots[slotIdx] != null) _skillSlotRoots[slotIdx].SetActive(true);
                    if (_skillSlotIcons[slotIdx] != null)
                    {
                        _skillSlotIcons[slotIdx].sprite = Utils.RewardIconHelper.GetOrCreateRewardIcon(skill.Id);
                    }
                    if (_skillSlotLevelTexts[slotIdx] != null)
                    {
                        _skillSlotLevelTexts[slotIdx].text = $"Lv.{skill.Level}";
                    }

                    // Projectile / Count Badge (e.g., 2, 3 fireballs)
                    if (_skillSlotCountTexts != null && slotIdx < _skillSlotCountTexts.Length && _skillSlotCountTexts[slotIdx] != null)
                    {
                        int projCount = GetSkillProjectileCount(skill);
                        _skillSlotCountTexts[slotIdx].text = projCount > 1 ? projCount.ToString() : "";
                    }

                    // 360° Clockwise Radial Cooldown Progress (1.0 = on cooldown, 0.0 = ready)
                    if (_skillSlotCooldownMasks[slotIdx] != null)
                    {
                        _skillSlotCooldownMasks[slotIdx].fillAmount = Mathf.Clamp01(1.0f - cd.NormalizedProgress);
                    }

                    slotIdx++;
                }
            }

            // Hide unused slots
            for (int i = slotIdx; i < InGameHudBuilder.MaxSkillSlots; i++)
            {
                if (_skillSlotRoots[i] != null) _skillSlotRoots[i].SetActive(false);
            }

            // Sync Left-side Passive List
            UpdatePassivesDisplay();
        }

        public void UpdatePassivesDisplay()
        {
            if (_playerView == null || _playerView.Entity == null || _passiveSlotRoots == null) return;

            var passives = _playerView.Entity.PassiveLevels;
            int slotIdx = 0;

            if (passives != null)
            {
                foreach (var kvp in passives)
                {
                    if (slotIdx >= InGameHudBuilder.MaxPassiveSlots) break;

                    string id = kvp.Key;
                    int level = kvp.Value;
                    if (level <= 0) continue;

                    if (_passiveSlotRoots[slotIdx] != null) _passiveSlotRoots[slotIdx].SetActive(true);
                    if (_passiveSlotIcons[slotIdx] != null)
                    {
                        _passiveSlotIcons[slotIdx].sprite = Utils.RewardIconHelper.GetOrCreateRewardIcon(id, 80);
                    }
                    if (_passiveSlotLevelTexts[slotIdx] != null)
                    {
                        _passiveSlotLevelTexts[slotIdx].text = level.ToString();
                    }
                    if (_passiveSlotValueTexts[slotIdx] != null)
                    {
                        _passiveSlotValueTexts[slotIdx].text = GetPassiveValueText(id, level);
                    }

                    slotIdx++;
                }
            }

            // Hide unused slots
            for (int i = slotIdx; i < InGameHudBuilder.MaxPassiveSlots; i++)
            {
                if (_passiveSlotRoots[i] != null) _passiveSlotRoots[i].SetActive(false);
            }
        }

        private string GetPassiveValueText(string passiveId, int level)
        {
            switch (passiveId)
            {
                case "passive_fang": return $"+{level * 15}% ATK";
                case "passive_feather": return $"+{level * 12}% SPD";
                case "passive_rune": return $"+{level * 15}% RNG";
                case "passive_armor": return $"+{level * 5} ARM";
                case "passive_ring": return $"+{level * 10}% EXP";
                case "passive_heart": return $"+{level * 20} HP";
                case "passive_crit": return $"+{level * 8}% CRT";
                case "passive_ignition": return "🔥 화염";
                case "passive_overcharge": return "⚡ 감전";
                default: return $"Lv.{level}";
            }
        }

        private int GetSkillProjectileCount(ISkill skill)
        {
            if (skill == null) return 0;
            int extraProj = _playerView != null && _playerView.Entity != null ? _playerView.Entity.Stats.ExtraProjectiles : 0;

            if (skill is CompositeSkill comp)
            {
                if (comp.Effect is Domain.Skills.Effects.FireballEffect fb) return fb.FireballCount + extraProj;
                if (comp.Effect is Domain.Skills.Effects.MeteorStrikeEffect ms) return ms.FireballCount + extraProj;
                if (comp.Effect is Domain.Skills.Effects.PiercingArrowEffect pa) return pa.ArrowCount + extraProj;
                if (comp.Effect is Domain.Skills.Effects.StormArrowEffect sb) return sb.BaseArrowCount + extraProj;
                if (comp.Effect is Domain.Skills.Effects.WindGlaiveEffect wg) return wg.GlaiveCount + extraProj;
                if (comp.Effect is Domain.Skills.Effects.PhantomGlaiveEffect pg) return pg.PhantomCount + extraProj;
                if (comp.Effect is Domain.Skills.Effects.ChainLightningEffect cl) return cl.ChainCount;
                if (comp.Effect is Domain.Skills.Effects.GigastormLightningEffect gsl) return gsl.ChainCount;
                if (comp.Effect is Domain.Skills.Effects.BlizzardNovaEffect bn) return bn.ShardCount;
                if (comp.Effect is Domain.Skills.Effects.TempestWhirlwindEffect tw) return tw.SlashWaveCount;
            }
            return 0;
        }

        private void UpdateHpDisplay(float currentHp, float maxHp, bool immediate = false)
        {
            float fill = maxHp > 0f ? Mathf.Clamp01(currentHp / maxHp) : 0f;
            _targetHpFill = fill;
            if (immediate)
            {
                _currentHpFill = fill;
                if (_healthSlider != null) _healthSlider.value = fill;
            }

            if (_healthText != null)
            {
                _sb.Clear();
                _sb.Append((int)currentHp).Append(" / ").Append((int)maxHp);
                _healthText.text = _sb.ToString();
            }
        }

        private void UpdateLevelDisplay(int level)
        {
            if (_cachedLevel == level) return;
            _cachedLevel = level;

            if (_levelText != null)
            {
                _levelText.text = $"{level}";
            }
        }

        private void UpdateExpTextDisplay(int currentExp, int requiredExp)
        {
            if (_expText != null)
            {
                int percent = requiredExp > 0 ? (int)((float)currentExp / requiredExp * 100f) : 100;
                _sb.Clear();
                _sb.Append("EXP ").Append(currentExp).Append(" / ").Append(requiredExp).Append(" (").Append(percent).Append("%)");
                _expText.text = _sb.ToString();
            }
        }

        private void UpdateKillCountDisplay(int kills)
        {
            if (_cachedKills == kills) return;
            _cachedKills = kills;

            if (_killCountText != null)
            {
                _sb.Clear();
                _sb.Append("💀 ").Append(kills);
                _killCountText.text = _sb.ToString();
            }
        }

        private void UpdateGoldDisplay(int gold)
        {
            if (_cachedGold == gold) return;
            _cachedGold = gold;

            if (_goldText != null)
            {
                _sb.Clear();
                _sb.Append("💰 ").Append(gold);
                _goldText.text = _sb.ToString();
            }
        }

        private void EnsureUiElements()
        {
            if (_expSlider != null && _healthSlider != null) return;

            // Ensure EventSystem exists in scene
            if (FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                var eventSystemGo = new GameObject("EventSystem");
                eventSystemGo.AddComponent<UnityEngine.EventSystems.EventSystem>();
                eventSystemGo.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            }

            // Build full HUD via modular InGameHudBuilder
            var hud = InGameHudBuilder.BuildHud(this.transform);

            _scaler = hud.Scaler;
            _expSlider = hud.ExpSlider;
            _levelText = hud.LevelText;
            _expText = hud.ExpText;
            _healthSlider = hud.HealthSlider;
            _healthText = hud.HealthText;
            _timerText = hud.TimerText;
            _killCountText = hud.KillCountText;
            _goldText = hud.GoldText;

            _skillSlotIcons = hud.SkillSlotIcons;
            _skillSlotCooldownMasks = hud.SkillSlotCooldownMasks;
            _skillSlotLevelTexts = hud.SkillSlotLevelTexts;
            _skillSlotCountTexts = hud.SkillSlotCountTexts;
            _skillSlotRoots = hud.SkillSlotRoots;
            _passiveSlotIcons = hud.PassiveSlotIcons;
            _passiveSlotLevelTexts = hud.PassiveSlotLevelTexts;
            _passiveSlotValueTexts = hud.PassiveSlotValueTexts;
            _passiveSlotRoots = hud.PassiveSlotRoots;
            _dashCooldownMask = hud.DashCooldownMask;
        }
    }
}
