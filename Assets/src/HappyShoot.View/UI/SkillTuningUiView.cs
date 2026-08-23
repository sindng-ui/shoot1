using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using HappyShoot.Domain.Entities;
using HappyShoot.Domain.Gems;
using HappyShoot.Domain.Leveling;
using HappyShoot.Domain.Skills;
using HappyShoot.View.Config;
using HappyShoot.View.Monsters;
using HappyShoot.View.Player;
using HappyShoot.View.Utils;

namespace HappyShoot.View.UI
{
    /// <summary>
    /// Combat & Balance Sandbox UI for real-time live parameter adjustments of Skills, EXP system, and Monster Stats.
    /// Strictly modular and under 500 lines.
    /// </summary>
    public class SkillTuningUiView : MonoBehaviour
    {
        private PlayerView _playerView;
        private SkillRewardManager _rewardManager;
        private MonsterSpawnerView _spawnerView;
        private LevelSystem _levelSystem;
        private GemManager _gemManager;
        private SkillConfigData _config;

        private GameObject _panelRoot;
        private GameObject _contentBox;
        private Text _collapseBtnText;
        private bool _isCollapsed = false;

        private Transform _slidersContainer;
        private Text _statusNoticeText;
        private Text _spamBtnText;
        private Image _spamBtnImg;
        private bool _isInfiniteSpam = false;

        private bool _isAddMode = false;
        private Image _btnSwitchModeImg;
        private Image _btnAddModeImg;

        private string _selectedSkillId = "slash";
        private int _selectedMonsterIdx = 0;
        private readonly List<GameObject> _activeSliderRows = new List<GameObject>();
        private readonly Dictionary<string, Image> _tabButtons = new Dictionary<string, Image>();
        private List<Image> _levelButtonImgs = new List<Image>();
        private List<Image> _monsterSubTabImgs = new List<Image>();

        public void Initialize(
            PlayerView playerView,
            SkillRewardManager rewardManager,
            MonsterSpawnerView spawnerView = null,
            LevelSystem levelSystem = null,
            GemManager gemManager = null)
        {
            _playerView = playerView;
            _rewardManager = rewardManager;
            _spawnerView = spawnerView;
            _levelSystem = levelSystem;
            _gemManager = gemManager;
            _config = SkillConfigRepository.Instance.GetConfig();
            SkillTuningMemoryCache.ImportFromConfig(_config);

            BuildUi();
            SelectSkill("slash");
        }

        public void Show() => _panelRoot?.SetActive(true);
        public void Hide() => _panelRoot?.SetActive(false);

        private void BuildUi()
        {
            var canvas = FindFirstObjectByType<Canvas>() ?? FindAnyObjectByType<Canvas>();
            if (canvas == null) return;

            // 1. Root Container
            _panelRoot = new GameObject("CombatSandboxRoot");
            _panelRoot.transform.SetParent(canvas.transform, false);
            var rootRt = _panelRoot.AddComponent<RectTransform>();
            rootRt.anchorMin = new Vector2(0f, 0.5f);
            rootRt.anchorMax = new Vector2(0f, 0.5f);
            rootRt.pivot = new Vector2(0f, 0.5f);
            rootRt.anchoredPosition = new Vector2(16f, -35f);
            rootRt.sizeDelta = new Vector2(500f, 780f);

            // 2. Collapse Tab
            var tabGo = new GameObject("TuningCollapseTab");
            tabGo.transform.SetParent(_panelRoot.transform, false);
            var tabRt = tabGo.AddComponent<RectTransform>();
            tabRt.anchorMin = new Vector2(1f, 0.5f);
            tabRt.anchorMax = new Vector2(1f, 0.5f);
            tabRt.pivot = new Vector2(0f, 0.5f);
            tabRt.anchoredPosition = Vector2.zero;
            tabRt.sizeDelta = new Vector2(40f, 110f);

            var tabImg = tabGo.AddComponent<Image>();
            tabImg.sprite = SpriteHelper.GetOrCreateWhiteSprite();
            tabImg.color = new Color(0.10f, 0.15f, 0.22f, 0.95f);

            var tabBtn = tabGo.AddComponent<Button>();
            tabBtn.onClick.AddListener(ToggleCollapse);

            _collapseBtnText = SkillTuningSliderFactory.CreateText(tabGo.transform, "Label", "◀\n접\n기", 13, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, Color.cyan);

            // 3. Content Box
            _contentBox = new GameObject("TuningContentBox");
            _contentBox.transform.SetParent(_panelRoot.transform, false);
            var contentRt = _contentBox.AddComponent<RectTransform>();
            contentRt.anchorMin = Vector2.zero;
            contentRt.anchorMax = Vector2.one;
            contentRt.offsetMin = Vector2.zero;
            contentRt.offsetMax = Vector2.zero;

            var bgImg = _contentBox.AddComponent<Image>();
            bgImg.sprite = SpriteHelper.GetOrCreateWhiteSprite();
            bgImg.color = new Color(0.08f, 0.11f, 0.16f, 0.94f);

            SkillTuningSliderFactory.CreateText(_contentBox.transform, "Header", "🧪 [전투 & 밸런스 샌드박스]", 16, TextAnchor.MiddleCenter, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -8f), new Vector2(480f, 32f), new Color(0.3f, 1.0f, 0.5f));

            var modes = SkillTuningUiBuilder.CreateSelectionModeButtons(_contentBox.transform, SetSelectionMode);
            _btnSwitchModeImg = modes.switchImg;
            _btnAddModeImg = modes.addImg;

            SkillTuningUiBuilder.CreateSkillSelectButtons(_contentBox.transform, _tabButtons, SelectSkill, UnequipSkill);
            var utils = SkillTuningUiBuilder.CreateUtilityToolbar(_contentBox.transform, ToggleInfiniteSpam, ResetDummies, AddBatDummies);
            _spamBtnImg = utils.spamImg;
            _spamBtnText = utils.spamTxt;

            _levelButtonImgs = SkillTuningUiBuilder.CreateLevelSelectors(_contentBox.transform, SetSkillLevel);
            _monsterSubTabImers = SkillTuningUiBuilder.CreateMonsterSubTabs(_contentBox.transform, SelectMonsterSubTab);
            SetMonsterSubTabsVisible(false);

            // Scroll Sliders View
            var scrollGo = new GameObject("SlidersScrollView");
            scrollGo.transform.SetParent(_contentBox.transform, false);
            var scrollRt = scrollGo.AddComponent<RectTransform>();
            scrollRt.anchorMin = new Vector2(0.5f, 0.5f);
            scrollRt.anchorMax = new Vector2(0.5f, 0.5f);
            scrollRt.pivot = new Vector2(0.5f, 0.5f);
            scrollRt.anchoredPosition = new Vector2(0f, -30f);
            scrollRt.sizeDelta = new Vector2(476f, 370f);

            var scrollImg = scrollGo.AddComponent<Image>();
            scrollImg.sprite = SpriteHelper.GetOrCreateWhiteSprite();
            scrollImg.color = new Color(0.05f, 0.07f, 0.10f, 0.85f);

            var contentObj = new GameObject("ScrollContent");
            contentObj.transform.SetParent(scrollGo.transform, false);
            var contentRect = contentObj.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot = new Vector2(0f, 1f);
            contentRect.anchoredPosition = Vector2.zero;
            contentRect.sizeDelta = new Vector2(0f, 400f);
            _slidersContainer = contentObj.transform;

            CreateBottomActionButtons();
        }

        private List<Image> _monsterSubTabImers = new List<Image>();

        private void SetMonsterSubTabsVisible(bool visible)
        {
            for (int i = 0; i < _monsterSubTabImers.Count; i++)
                if (_monsterSubTabImers[i] != null) _monsterSubTabImers[i].gameObject.SetActive(visible);
            for (int i = 0; i < _levelButtonImgs.Count; i++)
                if (_levelButtonImgs[i] != null) _levelButtonImgs[i].gameObject.SetActive(!visible);
        }

        private void SelectMonsterSubTab(int idx)
        {
            _selectedMonsterIdx = idx;
            for (int i = 0; i < _monsterSubTabImers.Count; i++)
                _monsterSubTabImers[i].color = (i == idx) ? new Color(0.8f, 0.3f, 0.2f, 1f) : new Color(0.18f, 0.24f, 0.35f, 1f);
            RebuildSlidersForSelectedSkill();
        }

        private void SetSelectionMode(bool isAddMode)
        {
            _isAddMode = isAddMode;
            if (_btnSwitchModeImg != null) _btnSwitchModeImg.color = !_isAddMode ? new Color(0.15f, 0.60f, 0.85f, 1f) : new Color(0.18f, 0.24f, 0.32f, 0.95f);
            if (_btnAddModeImg != null) _btnAddModeImg.color = _isAddMode ? new Color(0.20f, 0.75f, 0.35f, 1f) : new Color(0.18f, 0.24f, 0.32f, 0.95f);

            if (_statusNoticeText != null)
            {
                _statusNoticeText.text = _isAddMode 
                    ? "➕ [누적 추가 모드] 스킬 탭 클릭 시 기존 스킬을 유지하고 추가 장착합니다."
                    : "🔁 [단독 교체 모드] 스킬 탭 클릭 시 다른 스킬을 해제하고 1개만 장착합니다.";
                _statusNoticeText.color = _isAddMode ? Color.green : Color.cyan;
            }
        }

        private void CreateBottomActionButtons()
        {
            float botY = 24f;
            var statusGo = new GameObject("StatusNoticeText");
            statusGo.transform.SetParent(_contentBox.transform, false);
            var sRt = statusGo.AddComponent<RectTransform>();
            sRt.anchorMin = sRt.anchorMax = sRt.pivot = new Vector2(0.5f, 0f);
            sRt.anchoredPosition = new Vector2(0f, botY + 44f);
            sRt.sizeDelta = new Vector2(460f, 28f);

            _statusNoticeText = statusGo.AddComponent<Text>();
            _statusNoticeText.font = FontHelper.GetKoreanFont();
            _statusNoticeText.fontSize = 11;
            _statusNoticeText.alignment = TextAnchor.MiddleCenter;
            _statusNoticeText.color = new Color(0.6f, 0.85f, 1f, 0.9f);
            _statusNoticeText.text = "💡 스킬/경험치/치명타/몬스터 스탯을 실시간 조절하고 파일에 저장할 수 있습니다.";

            CreateActionButton("Btn_SaveToFile", -115f, botY, new Color(0.18f, 0.65f, 0.35f, 1f), "💾 파일에 반영 (Save Config)", OnSaveClicked);
            CreateActionButton("Btn_ResetDefaults", 115f, botY, new Color(0.65f, 0.25f, 0.20f, 1f), "🔄 기본값 복원 (Restore)", OnResetClicked);
        }

        private void CreateActionButton(string name, float x, float y, Color col, string label, UnityEngine.Events.UnityAction onClick)
        {
            var go = new GameObject(name);
            go.transform.SetParent(_contentBox.transform, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0f);
            rt.anchoredPosition = new Vector2(x, y);
            rt.sizeDelta = new Vector2(220f, 36f);

            var img = go.AddComponent<Image>();
            img.sprite = SpriteHelper.GetOrCreateWhiteSprite();
            img.color = col;

            var btn = go.AddComponent<Button>();
            btn.targetGraphic = img;
            btn.onClick.AddListener(onClick);
            SkillTuningSliderFactory.CreateText(go.transform, "Txt", label, 13, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, Color.white);
        }

        private void SelectSkill(string skillId)
        {
            _selectedSkillId = skillId;

            foreach (var kvp in _tabButtons)
            {
                kvp.Value.color = (kvp.Key == skillId) ? new Color(0.2f, 0.6f, 0.95f, 1f) : new Color(0.14f, 0.18f, 0.26f, 0.95f);
            }

            if (skillId == "monster_tuning")
            {
                SetMonsterSubTabsVisible(true);
                RebuildSlidersForSelectedSkill();
                return;
            }

            SetMonsterSubTabsVisible(false);

            if (skillId == "exp_tuning" || skillId == "crit_tuning")
            {
                for (int i = 0; i < _levelButtonImgs.Count; i++)
                {
                    if (_levelButtonImgs[i] != null) _levelButtonImgs[i].gameObject.SetActive(false);
                }
                RebuildSlidersForSelectedSkill();
                return;
            }

            for (int i = 0; i < _levelButtonImgs.Count; i++)
            {
                if (_levelButtonImgs[i] != null) _levelButtonImgs[i].gameObject.SetActive(true);
            }

            if (_playerView?.Entity != null && _rewardManager != null)
            {
                var p = _playerView.Entity;

                if (!_isAddMode)
                {
                    var skillsToRemove = new List<string>();
                    foreach (var s in p.Skills)
                    {
                        if (s.Id != skillId) skillsToRemove.Add(s.Id);
                    }
                    for (int i = 0; i < skillsToRemove.Count; i++)
                    {
                        p.RemoveSkill(skillsToRemove[i]);
                    }
                }

                if (!p.HasSkill(skillId))
                {
                    _rewardManager.GrantOrLevelUpSkillDirectly(p, skillId);
                }

                var activeSkill = p.GetSkill(skillId) as CompositeSkill;
                int curLv = activeSkill != null ? activeSkill.Level : 1;
                SkillTuningMemoryCache.RestoreToInstance(skillId, curLv, activeSkill);

                for (int i = 0; i < _levelButtonImgs.Count; i++)
                {
                    _levelButtonImgs[i].color = (i + 1 == curLv) ? new Color(0.2f, 0.8f, 0.4f, 1f) : new Color(0.18f, 0.24f, 0.35f, 1f);
                }
            }

            RebuildSlidersForSelectedSkill();
        }

        private void UnequipSkill(string skillId)
        {
            if (_playerView?.Entity == null || _selectedSkillId == "exp_tuning" || _selectedSkillId == "monster_tuning") return;
            var p = _playerView.Entity;
            if (p.HasSkill(skillId))
            {
                p.RemoveSkill(skillId);
                if (_statusNoticeText != null)
                {
                    _statusNoticeText.text = $"🗑️ {skillId} 스킬이 장착 해제되었습니다!";
                    _statusNoticeText.color = Color.red;
                }
                RebuildSlidersForSelectedSkill();
            }
        }

        private void SetSkillLevel(int targetLevel)
        {
            if (_selectedSkillId == "exp_tuning" || _selectedSkillId == "monster_tuning") return;
            if (_playerView?.Entity == null || _rewardManager == null) return;
            var p = _playerView.Entity;
            p.RemoveSkill(_selectedSkillId);

            for (int i = 0; i < targetLevel; i++)
            {
                _rewardManager.GrantOrLevelUpSkillDirectly(p, _selectedSkillId);
            }

            var activeSkill = p.GetSkill(_selectedSkillId) as CompositeSkill;
            bool hadMemory = SkillTuningMemoryCache.HasMemory(_selectedSkillId, targetLevel);
            SkillTuningMemoryCache.RestoreToInstance(_selectedSkillId, targetLevel, activeSkill);

            for (int i = 0; i < _levelButtonImgs.Count; i++)
            {
                _levelButtonImgs[i].color = (i + 1 == targetLevel) ? new Color(0.2f, 0.8f, 0.4f, 1f) : new Color(0.18f, 0.24f, 0.35f, 1f);
            }

            RebuildSlidersForSelectedSkill();

            if (_statusNoticeText != null)
            {
                _statusNoticeText.text = hadMemory 
                    ? $"🌟 {_selectedSkillId} Lv.{targetLevel} (튜닝된 메모리 수치 복원됨!)"
                    : $"🌟 {_selectedSkillId} Lv.{targetLevel} 순수 공식 스펙으로 설정되었습니다!";
                _statusNoticeText.color = hadMemory ? Color.yellow : Color.green;
            }
        }

        private void ToggleInfiniteSpam()
        {
            _isInfiniteSpam = !_isInfiniteSpam;
            _spamBtnText.text = _isInfiniteSpam ? "⚡ 난사 모드: ON" : "⚡ 난사 모드: OFF";
            _spamBtnImg.color = _isInfiniteSpam ? new Color(0.95f, 0.55f, 0.1f, 1f) : new Color(0.25f, 0.28f, 0.35f, 1f);
            RebuildSlidersForSelectedSkill();
        }

        private void ResetDummies()
        {
            if (_spawnerView != null && _playerView?.Entity != null)
            {
                _spawnerView.SpawnTrainingDummies(_playerView.Entity.Position, 5);
                if (_statusNoticeText != null)
                {
                    _statusNoticeText.text = "🎯 허수아비 5마리가 재소환되었습니다!";
                    _statusNoticeText.color = Color.cyan;
                }
            }
        }

        private void AddBatDummies()
        {
            if (_spawnerView != null && _playerView?.Entity != null)
            {
                _spawnerView.SpawnBatDummies(_playerView.Entity.Position, 20);
                if (_statusNoticeText != null)
                {
                    _statusNoticeText.text = "🦇 움직이는 비행 박쥐 20마리가 추가 소환되었습니다!";
                    _statusNoticeText.color = new Color(0.85f, 0.45f, 1f);
                }
            }
        }

        private void RebuildSlidersForSelectedSkill()
        {
            for (int i = 0; i < _activeSliderRows.Count; i++)
            {
                Destroy(_activeSliderRows[i]);
            }
            _activeSliderRows.Clear();

            if (_selectedSkillId == "monster_tuning")
            {
                SkillTuningRowConfigurator.ConfigureMonsterRows(
                    _selectedMonsterIdx,
                    _slidersContainer,
                    _config?.Monsters,
                    row => _activeSliderRows.Add(row));
                return;
            }

            SkillTuningRowConfigurator.ConfigureRows(
                _playerView?.Entity,
                _selectedSkillId,
                _slidersContainer,
                _isInfiniteSpam,
                _config,
                _levelSystem,
                _gemManager,
                row => _activeSliderRows.Add(row));
        }

        private void OnSaveClicked()
        {
            if (_playerView?.Entity != null)
            {
                if (_selectedSkillId != "exp_tuning" && _selectedSkillId != "monster_tuning" && _selectedSkillId != "crit_tuning")
                {
                    var activeSkill = _playerView.Entity.GetSkill(_selectedSkillId);
                    if (activeSkill != null && activeSkill.Level == 1)
                        SkillLiveApplier.PullSkillStatsToConfig(_playerView.Entity, _selectedSkillId, _config);
                }

                if (_config.CritStat == null) _config.CritStat = new HappyShoot.Domain.Skills.CritStatConfig();
                var pStat = _playerView.Entity.Stats;
                _config.CritStat.CritChance = pStat.CritChance;
                _config.CritStat.CritDamageMultiplier = pStat.CritDamageMultiplier;
                _config.CritStat.AttackPowerMultiplier = pStat.AttackPowerMultiplier;
                _config.CritStat.MoveSpeed = pStat.MoveSpeed;
                _config.CritStat.Armor = pStat.Armor;
                _config.CritStat.CooldownReduction = pStat.CooldownReduction;
                _config.CritStat.IsCustom = true;
            }

            SkillTuningMemoryCache.ExportToConfig(_config);
            bool success = SkillConfigRepository.Instance.Save(_config);

            if (_statusNoticeText != null)
            {
                _statusNoticeText.text = success ? "✅ 모든 스킬/치명타/경험치/몬스터 스탯이 파일에 저장되었습니다!" : "❌ 저장 실패!";
                _statusNoticeText.color = success ? Color.green : Color.red;
            }
        }

        private void OnResetClicked()
        {
            bool hasFile = SkillConfigRepository.Instance.HasSavedConfigFile();
            _config = SkillConfigRepository.Instance.ReloadFromFileOrDefaults();
            
            SkillTuningMemoryCache.ImportFromConfig(_config);
            if (_levelSystem != null) _levelSystem.Config = _config.Exp;
            if (_gemManager != null) _gemManager.Config = _config.Exp;

            if (_config.CritStat != null && _config.CritStat.IsCustom && _playerView?.Entity != null)
            {
                var s = _playerView.Entity.Stats;
                var c = _config.CritStat;
                _playerView.Entity.Stats = new CharacterStats(s.MaxHealth, s.HealthRegen, c.MoveSpeed, c.AttackPowerMultiplier, c.Armor, c.CritChance, c.CritDamageMultiplier, c.CooldownReduction, s.AreaMultiplier, s.ProjectileSpeedMultiplier, s.ExtraProjectiles, s.PickupRadius);
            }

            if (_selectedSkillId == "exp_tuning" || _selectedSkillId == "monster_tuning" || _selectedSkillId == "crit_tuning")
                RebuildSlidersForSelectedSkill();
            else
                SetSkillLevel(1);

            if (_statusNoticeText != null)
            {
                _statusNoticeText.text = hasFile 
                    ? "🔄 파일에 저장된 설정 값으로 복원되었습니다!" 
                    : "🔄 저장 파일이 없어 기본(Pre-defined) 설정으로 복원되었습니다!";
                _statusNoticeText.color = Color.cyan;
            }
        }

        private void ToggleCollapse()
        {
            _isCollapsed = !_isCollapsed;
            _contentBox.SetActive(!_isCollapsed);
            _collapseBtnText.text = _isCollapsed ? "▶\n펼\n치\n기" : "◀\n접\n기";
            _collapseBtnText.color = _isCollapsed ? Color.green : Color.cyan;
        }
    }
}
