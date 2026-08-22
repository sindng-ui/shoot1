using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using HappyShoot.Domain.Entities;
using HappyShoot.Domain.Leveling;
using HappyShoot.Domain.Session;
using HappyShoot.View.Monsters;
using HappyShoot.View.Player;
using HappyShoot.View.Utils;

namespace HappyShoot.View.UI
{
    /// <summary>
    /// Developer Mode In-Game Skill Selector and Cheat Console UI.
    /// Allows real-time toggling, equipping, and leveling of all active skills (Warrior, Ranger, Wizard),
    /// evolutions, passives, and debug cheats. Supports Right-Click to immediately reset to Lv.0.
    /// Strictly modular and under 500 lines.
    /// </summary>
    public class DevSkillSelectorUiView : MonoBehaviour
    {
        private PlayerView _playerView;
        private SkillRewardManager _rewardManager;
        private LevelSystem _levelSystem;
        private GameSessionEntity _gameSession;
        private MonsterSpawnerView _spawnerView;

        private GameObject _panelRoot;
        private GameObject _contentBox;
        private bool _isCollapsed;
        private Text _collapseBtnText;
        private Text _godModeBtnText;
        private Text _timeScaleBtnText;
        private float _currentTimeScale = 1.0f;

        private readonly Dictionary<string, (Button btn, Text text, Image bg)> _skillButtons
            = new Dictionary<string, (Button, Text, Image)>();
        private readonly Dictionary<string, (Button btn, Text text, Image bg)> _passiveButtons
            = new Dictionary<string, (Button, Text, Image)>();

        public void Initialize(
            PlayerView playerView,
            SkillRewardManager rewardManager,
            LevelSystem levelSystem,
            GameSessionEntity gameSession,
            MonsterSpawnerView spawnerView)
        {
            _playerView = playerView;
            _rewardManager = rewardManager;
            _levelSystem = levelSystem;
            _gameSession = gameSession;
            _spawnerView = spawnerView;

            BuildUi();
            RefreshAllButtons();
        }

        public void Show() => _panelRoot?.SetActive(true);
        public void Hide() => _panelRoot?.SetActive(false);

        private void BuildUi()
        {
            var canvas = FindFirstObjectByType<Canvas>() ?? FindAnyObjectByType<Canvas>();
            if (canvas == null) return;

            // 1. Root Container (Docked to Right Center)
            _panelRoot = new GameObject("DevSkillSelectorRoot");
            _panelRoot.transform.SetParent(canvas.transform, false);
            var rootRt = _panelRoot.AddComponent<RectTransform>();
            rootRt.anchorMin = new Vector2(1f, 0.5f);
            rootRt.anchorMax = new Vector2(1f, 0.5f);
            rootRt.pivot = new Vector2(1f, 0.5f);
            rootRt.anchoredPosition = new Vector2(-15f, 0f);
            rootRt.sizeDelta = new Vector2(360f, 660f);

            // 2. Collapse Toggle Tab (Left border tab)
            var tabGo = new GameObject("CollapseTab");
            tabGo.transform.SetParent(_panelRoot.transform, false);
            var tabRt = tabGo.AddComponent<RectTransform>();
            tabRt.anchorMin = new Vector2(0f, 0.5f);
            tabRt.anchorMax = new Vector2(0f, 0.5f);
            tabRt.pivot = new Vector2(1f, 0.5f);
            tabRt.anchoredPosition = new Vector2(0f, 0f);
            tabRt.sizeDelta = new Vector2(36f, 100f);

            var tabImg = tabGo.AddComponent<Image>();
            tabImg.sprite = SpriteHelper.GetOrCreateWhiteSprite();
            tabImg.color = new Color(0.12f, 0.16f, 0.22f, 0.95f);

            var tabBtn = tabGo.AddComponent<Button>();
            tabBtn.onClick.AddListener(ToggleCollapse);

            _collapseBtnText = CreateText(tabGo.transform, "Label", "▶\n접\n기", 12, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, Color.yellow);

            // 3. Main Content Box
            _contentBox = new GameObject("ContentBox");
            _contentBox.transform.SetParent(_panelRoot.transform, false);
            var contentRt = _contentBox.AddComponent<RectTransform>();
            contentRt.anchorMin = Vector2.zero;
            contentRt.anchorMax = Vector2.one;
            contentRt.pivot = new Vector2(1f, 0.5f);
            contentRt.anchoredPosition = Vector2.zero;
            contentRt.sizeDelta = Vector2.zero;

            var bg = _contentBox.AddComponent<Image>();
            bg.sprite = SpriteHelper.GetOrCreateWhiteSprite();
            bg.color = new Color(0.08f, 0.10f, 0.15f, 0.96f);

            var outline = _contentBox.AddComponent<Outline>();
            outline.effectColor = new Color(0.2f, 0.8f, 0.4f, 0.8f);
            outline.effectDistance = new Vector2(2f, -2f);

            float currentY = -18f;

            // Header Title
            CreateText(_contentBox.transform, "Title", "🛠️ 개발자 치트 & 스킬 콘솔", 14, TextAnchor.MiddleCenter, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, currentY), new Vector2(340f, 22f), new Color(0.3f, 1f, 0.5f));
            currentY -= 28f;

            // Cheat Buttons (GodMode, FullHeal, LevelUp, KillAll, Speed, Gold)
            CreateSmallButton(_contentBox.transform, "BtnGod", "🛡️ 무적 모드: OFF", new Vector2(-85f, currentY), new Vector2(160f, 26f), new Color(0.3f, 0.3f, 0.35f, 1f), ToggleGodMode, out _godModeBtnText);
            CreateSmallButton(_contentBox.transform, "BtnHeal", "💖 체력 풀회복", new Vector2(85f, currentY), new Vector2(160f, 26f), new Color(0.2f, 0.6f, 0.3f, 1f), HealFull, out _);
            currentY -= 30f;

            CreateSmallButton(_contentBox.transform, "BtnLevelUp", "🌟 레벨 +1", new Vector2(-85f, currentY), new Vector2(160f, 26f), new Color(0.6f, 0.4f, 0.15f, 1f), GiveLevelUp, out _);
            CreateSmallButton(_contentBox.transform, "BtnKillAll", "💀 몬스터 전멸", new Vector2(85f, currentY), new Vector2(160f, 26f), new Color(0.7f, 0.2f, 0.2f, 1f), KillAllMonsters, out _);
            currentY -= 30f;

            CreateSmallButton(_contentBox.transform, "BtnSpeed", "⏩ 속도: 1x", new Vector2(-85f, currentY), new Vector2(160f, 26f), new Color(0.3f, 0.3f, 0.6f, 1f), ToggleTimeScale, out _timeScaleBtnText);
            CreateSmallButton(_contentBox.transform, "BtnGold", "💰 골드 +1000", new Vector2(85f, currentY), new Vector2(160f, 26f), new Color(0.75f, 0.65f, 0.1f, 1f), AddGold1000, out _);
            currentY -= 32f;

            // 1. ACTIVE SKILLS (좌클릭: +1 Lv / 우클릭: Lv.0 해제)
            CreateText(_contentBox.transform, "Sec_Active", "🗡️ 액티브 무기 (좌: +1Lv / 우: Lv.0 해제)", 12, TextAnchor.MiddleLeft, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, currentY), new Vector2(330f, 18f), new Color(1f, 0.65f, 0.65f, 1f));
            currentY -= 22f;

            // Warrior
            AddSkillButton("slash", "🗡️ 대검 베기 [전사]", ref currentY);
            AddSkillButton("ground_stomp", "💥 지면 강타 [전사]", ref currentY);
            AddSkillButton("whirlwind", "🌀 휠윈드 [전사]", ref currentY);
            // Ranger
            AddSkillButton("bow", "🏹 관통 화살 [궁수]", ref currentY);
            AddSkillButton("glaive", "🪓 칼바람 글레이브 [궁수]", ref currentY);
            AddSkillButton("arrow_rain", "🌧️ 화살 비 [궁수]", ref currentY);
            // Wizard
            AddSkillButton("fireball", "🔥 화염구 [마법사]", ref currentY);
            AddSkillButton("frost_nova", "❄️ 프로스트 노바 [마법사]", ref currentY);
            AddSkillButton("chain_lightning", "⚡ 체인 라이트닝 [마법사]", ref currentY);
            // Shared
            AddSkillButton("orbital", "⚔️ 오비탈 블레이드 [공용]", ref currentY);
            currentY -= 6f;

            // 2. EVOLVED SKILLS
            CreateText(_contentBox.transform, "Sec_Evo", "✨ 진화 궁극기 (좌: 장착 / 우: 해제)", 12, TextAnchor.MiddleLeft, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, currentY), new Vector2(330f, 18f), new Color(1f, 0.85f, 0.2f, 1f));
            currentY -= 22f;

            AddSkillButton("blood_eater", "🩸 블러드 이터", ref currentY);
            AddSkillButton("storm_bow", "⚡ 폭풍의 활", ref currentY);
            AddSkillButton("meteor_strike", "☄️ 메테오 스트라이크", ref currentY);
            currentY -= 6f;

            // 3. PASSIVES
            CreateText(_contentBox.transform, "Sec_Passive", "💎 패시브 아이템 (좌: +1Lv / 우: Lv.0 해제)", 12, TextAnchor.MiddleLeft, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, currentY), new Vector2(330f, 18f), new Color(0.4f, 0.85f, 1f, 1f));
            currentY -= 22f;

            AddPassiveButton("passive_fang", "흡혈귀의 이빨 (공격력 +15%)", ref currentY);
            AddPassiveButton("passive_feather", "바람의 깃털 (이속+12%, 투속+15%)", ref currentY);
            AddPassiveButton("passive_rune", "마나 룬 (범위+15%, 쿨감-6%)", ref currentY);
            AddPassiveButton("passive_armor", "강철 갑옷 (방어력 +5)", ref currentY);
            AddPassiveButton("passive_ring", "황금 반지 (골드 획득량 +25%)", ref currentY);
            AddPassiveButton("passive_heart", "생명의 펜던트 (최대 체력 +20)", ref currentY);
        }

        private void ToggleCollapse()
        {
            _isCollapsed = !_isCollapsed;
            _contentBox.SetActive(!_isCollapsed);
            _collapseBtnText.text = _isCollapsed ? "◀\n펼\n치\n기" : "▶\n접\n기";
            _collapseBtnText.color = _isCollapsed ? Color.green : Color.yellow;
        }

        private void AddSkillButton(string skillId, string label, ref float currentY)
        {
            var btnGo = new GameObject($"BtnSkill_{skillId}");
            btnGo.transform.SetParent(_contentBox.transform, false);
            var rt = btnGo.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0f, currentY);
            rt.sizeDelta = new Vector2(330f, 22f);

            var img = btnGo.AddComponent<Image>();
            img.color = new Color(0.18f, 0.22f, 0.28f, 0.9f);

            var btn = btnGo.AddComponent<Button>();
            btn.targetGraphic = img;

            AttachClickHandlers(btnGo, () => OnSkillClicked(skillId), () => OnSkillRightClicked(skillId));

            var txt = CreateText(btnGo.transform, "Label", label, 11, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, Color.white);

            _skillButtons[skillId] = (btn, txt, img);
            currentY -= 24f;
        }

        private void AddPassiveButton(string passiveId, string label, ref float currentY)
        {
            var btnGo = new GameObject($"BtnPassive_{passiveId}");
            btnGo.transform.SetParent(_contentBox.transform, false);
            var rt = btnGo.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0f, currentY);
            rt.sizeDelta = new Vector2(330f, 20f);

            var img = btnGo.AddComponent<Image>();
            img.color = new Color(0.16f, 0.24f, 0.20f, 1f);

            var btn = btnGo.AddComponent<Button>();
            btn.targetGraphic = img;

            AttachClickHandlers(btnGo, () => OnPassiveClicked(passiveId), () => OnPassiveRightClicked(passiveId));

            var txt = CreateText(btnGo.transform, "Label", label, 10, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, Color.white);

            _passiveButtons[passiveId] = (btn, txt, img);
            currentY -= 22f;
        }

        private void AttachClickHandlers(GameObject go, Action onLeftClick, Action onRightClick)
        {
            var trigger = go.AddComponent<EventTrigger>();
            var entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
            entry.callback.AddListener((data) =>
            {
                var pointerData = (PointerEventData)data;
                if (pointerData.button == PointerEventData.InputButton.Right)
                {
                    onRightClick?.Invoke();
                }
                else if (pointerData.button == PointerEventData.InputButton.Left)
                {
                    onLeftClick?.Invoke();
                }
            });
            trigger.triggers.Add(entry);
        }

        private void OnSkillClicked(string skillId)
        {
            if (_playerView?.Entity == null || _rewardManager == null) return;
            var player = _playerView.Entity;
            var existing = player.GetSkill(skillId);

            if (existing == null) _rewardManager.GrantOrLevelUpSkillDirectly(player, skillId);
            else if (!existing.IsMaxLevel) existing.LevelUp();
            else player.RemoveSkill(skillId);

            RefreshAllButtons();
        }

        private void OnSkillRightClicked(string skillId)
        {
            if (_playerView?.Entity == null) return;
            _playerView.Entity.RemoveSkill(skillId);
            RefreshAllButtons();
        }

        private void OnPassiveClicked(string passiveId)
        {
            if (_playerView?.Entity == null || _rewardManager == null) return;
            _rewardManager.GrantOrUpgradePassiveDirectly(_playerView.Entity, passiveId);
            RefreshAllButtons();
        }

        private void OnPassiveRightClicked(string passiveId)
        {
            if (_playerView?.Entity == null) return;
            _playerView.Entity.RemovePassive(passiveId);
            RefreshAllButtons();
        }

        private void RefreshAllButtons()
        {
            if (_playerView?.Entity == null) return;
            var player = _playerView.Entity;

            foreach (var kvp in _skillButtons)
            {
                string id = kvp.Key;
                var (_, txt, img) = kvp.Value;
                var existing = player.GetSkill(id);

                if (existing != null)
                {
                    img.color = new Color(0.15f, 0.55f, 0.30f, 1f);
                    txt.text = $"✅ [Lv.{existing.Level}] {existing.Name}" + (existing.IsMaxLevel ? " (MAX)" : " (+1)");
                }
                else
                {
                    img.color = new Color(0.18f, 0.22f, 0.28f, 0.9f);
                    txt.text = $"➕ [장착] {GetSkillNameById(id)}";
                }
            }

            foreach (var kvp in _passiveButtons)
            {
                string id = kvp.Key;
                var (_, txt, img) = kvp.Value;
                int level = player.GetPassiveLevel(id);

                if (level > 0)
                {
                    img.color = new Color(0.18f, 0.45f, 0.50f, 1f);
                    txt.text = $"✅ [Lv.{level}/5] {GetPassiveNameById(id)} (+1)";
                }
                else
                {
                    img.color = new Color(0.16f, 0.20f, 0.22f, 0.9f);
                    txt.text = $"➕ [Lv.1] {GetPassiveNameById(id)}";
                }
            }
        }

        private string GetSkillNameById(string id)
        {
            switch (id)
            {
                case "slash": return "대검 베기 [전사]";
                case "ground_stomp": return "지면 강타 [전사]";
                case "whirlwind": return "휠윈드 [전사]";
                case "bow": return "관통 화살 [궁수]";
                case "glaive": return "칼바람 글레이브 [궁수]";
                case "arrow_rain": return "화살 비 [궁수]";
                case "fireball": return "화염구 [마법사]";
                case "frost_nova": return "프로스트 노바 [마법사]";
                case "chain_lightning": return "체인 라이트닝 [마법사]";
                case "orbital": return "오비탈 블레이드 [공용]";
                case "blood_eater": return "✨ 블러드 이터";
                case "storm_bow": return "✨ 폭풍의 활";
                case "meteor_strike": return "☄️ 메테오 스트라이크";
                default: return id;
            }
        }

        private string GetPassiveNameById(string id)
        {
            switch (id)
            {
                case "passive_fang": return "흡혈귀의 이빨";
                case "passive_feather": return "바람의 깃털";
                case "passive_rune": return "마나 룬";
                case "passive_armor": return "강철 갑옷";
                case "passive_ring": return "황금 반지";
                case "passive_heart": return "생명의 펜던트";
                default: return id;
            }
        }

        private void ToggleGodMode()
        {
            if (_playerView?.Entity == null) return;
            var p = _playerView.Entity;
            p.IsGodMode = !p.IsGodMode;
            _godModeBtnText.text = p.IsGodMode ? "🛡️ 무적 모드: ON" : "🛡️ 무적 모드: OFF";
        }

        private void HealFull()
        {
            if (_playerView?.Entity == null) return;
            _playerView.Entity.Heal(999999f);
        }

        private void GiveLevelUp()
        {
            if (_levelSystem == null) return;
            _levelSystem.AddExp(_levelSystem.RequiredExp);
        }

        private void KillAllMonsters()
        {
            if (_spawnerView == null) return;
            var activeList = _spawnerView.DomainSpawner?.ActiveMonsters;
            if (activeList == null) return;
            for (int i = activeList.Count - 1; i >= 0; i--)
            {
                if (activeList[i].IsActive && !activeList[i].IsDead)
                {
                    activeList[i].TakeDamage(999999f);
                }
            }
        }

        private void ToggleTimeScale()
        {
            if (Mathf.Approximately(_currentTimeScale, 1.0f)) _currentTimeScale = 2.0f;
            else if (Mathf.Approximately(_currentTimeScale, 2.0f)) _currentTimeScale = 4.0f;
            else if (Mathf.Approximately(_currentTimeScale, 4.0f)) _currentTimeScale = 0.5f;
            else _currentTimeScale = 1.0f;

            Time.timeScale = _currentTimeScale;
            if (_timeScaleBtnText != null) _timeScaleBtnText.text = $"⏩ 속도: {_currentTimeScale}x";
        }

        private void AddGold1000()
        {
            _gameSession?.AddGold(1000);
        }

        private void CreateSmallButton(Transform parent, string name, string label, Vector2 pos, Vector2 size, Color color, Action onClick, out Text outText)
        {
            var btnGo = new GameObject(name);
            btnGo.transform.SetParent(parent, false);
            var rt = btnGo.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            var img = btnGo.AddComponent<Image>();
            img.sprite = SpriteHelper.GetOrCreateWhiteSprite();
            img.color = color;

            var btn = btnGo.AddComponent<Button>();
            btn.targetGraphic = img;
            btn.onClick.AddListener(() => onClick?.Invoke());

            outText = CreateText(btnGo.transform, "Label", label, 11, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, Color.white);
        }

        private Text CreateText(Transform parent, string name, string text, int fontSize, TextAnchor alignment, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 pos, Vector2 size, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = pivot;
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            var txt = go.AddComponent<Text>();
            txt.text = text;
            txt.fontSize = fontSize;
            txt.fontStyle = FontStyle.Bold;
            txt.alignment = alignment;
            txt.color = color;
            txt.font = FontHelper.GetKoreanFont();
            return txt;
        }
    }
}
