using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using HappyShoot.Domain.Forge;
using HappyShoot.Domain.Progression;

namespace HappyShoot.View.Forge
{
    /// <summary>
    /// UI Tab View for Rune Inscription.
    /// Manages equipped skill slots, rune catalog (Common, Rare, Legendary),
    /// rune unlocking/upgrading, and socketing runes into skills.
    /// Strictly modular and under 500 lines.
    /// </summary>
    public class RuneInscriptionTabView : MonoBehaviour
    {
        private RuneManager _runeManager;
        private SkillTreeManager _walletManager;
        private Action _onDataChanged;

        private GameObject _rootGo;
        private Transform _slotContainer;
        private Transform _runeGridContainer;

        // Detail panel
        private Text _detailTitleText;
        private Text _detailDescText;
        private Text _detailStatsText;
        private Text _detailCostText;
        private Button _actionButton;
        private Text _actionButtonText;
        private Transform _equipTargetButtonsRow;

        private string _selectedRuneId = "rune_rapid";
        private readonly List<string> _activeSkills = new List<string> { "fireball", "frost_nova", "chain_lightning" };
        private readonly Dictionary<string, string> _skillDisplayNames = new Dictionary<string, string>
        {
            { "fireball", "🔥 화염구" },
            { "frost_nova", "❄️ 서리 폭발" },
            { "chain_lightning", "⚡ 연쇄 번개" }
        };

        public void Initialize(RuneManager runeManager, SkillTreeManager walletManager, Action onDataChanged)
        {
            _runeManager = runeManager;
            _walletManager = walletManager;
            _onDataChanged = onDataChanged;

            BuildUI();
            Refresh();
        }

        private void BuildUI()
        {
            _rootGo = gameObject;
            var rootRt = _rootGo.GetComponent<RectTransform>();
            if (rootRt != null)
            {
                rootRt.anchorMin = Vector2.zero;
                rootRt.anchorMax = Vector2.one;
                rootRt.sizeDelta = Vector2.zero;
            }

            // ── Top: Equipped Skill Slots (Y: 250) ──
            var slotsSection = CreatePanel(transform, "SlotsSection", new Vector2(0f, 250f), new Vector2(1520f, 110f));
            _slotContainer = slotsSection.transform;

            // ── Middle-Left: Rune Catalog Grid (X: -330, Y: -65) ──
            var gridSection = CreatePanel(transform, "GridSection", new Vector2(-330f, -65f), new Vector2(840f, 460f));
            _runeGridContainer = gridSection.transform;

            // ── Middle-Right: Detail & Upgrade Panel (X: 450, Y: -65) ──
            BuildDetailPanel();
        }

        private void BuildDetailPanel()
        {
            var detailGo = CreatePanel(transform, "DetailPanel", new Vector2(450f, -65f), new Vector2(640f, 460f));
            detailGo.GetComponent<Image>().color = new Color(0.12f, 0.13f, 0.20f, 0.98f);

            _detailTitleText = CreateText(detailGo.transform, "Title", "룬 이름", 22, FontStyle.Bold,
                new Vector2(0f, 180f), new Vector2(600f, 40f), Color.white);

            _detailDescText = CreateText(detailGo.transform, "Desc", "설명", 15, FontStyle.Normal,
                new Vector2(0f, 125f), new Vector2(600f, 55f), new Color(0.85f, 0.85f, 0.90f));

            _detailStatsText = CreateText(detailGo.transform, "Stats", "수치", 16, FontStyle.Normal,
                new Vector2(0f, 50f), new Vector2(600f, 60f), new Color(0.45f, 0.95f, 0.60f));

            _detailCostText = CreateText(detailGo.transform, "Cost", "비용", 19, FontStyle.Bold,
                new Vector2(0f, -15f), new Vector2(600f, 40f), new Color(1f, 0.85f, 0.35f));

            // Upgrade / Unlock Button (Huge Touch Area: 560x56)
            var actionBtnGo = CreateButton(detailGo.transform, "ActionBtn", new Vector2(0f, -80f), new Vector2(560f, 56f),
                new Color(0.20f, 0.65f, 0.35f, 1f));
            _actionButton = actionBtnGo.GetComponent<Button>();
            _actionButton.onClick.AddListener(OnActionButtonClicked);
            _actionButtonText = CreateText(actionBtnGo.transform, "BtnText", "강화하기", 18, FontStyle.Bold,
                Vector2.zero, new Vector2(560f, 56f), Color.white);

            // Equip to Skill Slot buttons row
            var equipRow = CreatePanel(detailGo.transform, "EquipRow", new Vector2(0f, -150f), new Vector2(560f, 44f));
            _equipTargetButtonsRow = equipRow.transform;
            equipRow.GetComponent<Image>().color = Color.clear;

            for (int i = 0; i < _activeSkills.Count; i++)
            {
                string sId = _activeSkills[i];
                float x = -190f + i * 190f;
                var eqBtn = CreateButton(_equipTargetButtonsRow, $"Eq_{sId}", new Vector2(x, 0f), new Vector2(175f, 44f),
                    new Color(0.35f, 0.25f, 0.55f, 1f));
                eqBtn.GetComponent<Button>().onClick.AddListener(() => OnEquipToSkill(sId));
                CreateText(eqBtn.transform, "Label", _skillDisplayNames[sId], 14, FontStyle.Bold,
                    Vector2.zero, new Vector2(175f, 44f), Color.white);
            }
        }

        public void Refresh()
        {
            if (_runeManager == null) return;

            RefreshSlots();
            RefreshRuneGrid();
            RefreshDetail();
        }

        private void RefreshSlots()
        {
            for (int i = _slotContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(_slotContainer.GetChild(i).gameObject);
            }

            for (int i = 0; i < _activeSkills.Count; i++)
            {
                string skillId = _activeSkills[i];
                string runeId = _runeManager.GetEquippedRuneId(skillId);
                var runeDef = !string.IsNullOrEmpty(runeId) ? _runeManager.GetDefinition(runeId) : null;
                int level = runeDef != null ? _runeManager.GetLevel(runeId) : 0;

                float x = -510f + i * 510f;
                var slotGo = CreatePanel(_slotContainer, $"Slot_{skillId}", new Vector2(x, 0f), new Vector2(480f, 96f));
                slotGo.GetComponent<Image>().color = new Color(0.15f, 0.16f, 0.24f, 0.95f);

                string skillLabel = _skillDisplayNames.TryGetValue(skillId, out var name) ? name : skillId;
                CreateText(slotGo.transform, "SkillName", skillLabel, 18, FontStyle.Bold,
                    new Vector2(-90f, 20f), new Vector2(260f, 32f), new Color(1.0f, 0.9f, 0.4f));

                if (runeDef != null)
                {
                    string runeInfo = $"{runeDef.Name} Lv.{level}";
                    CreateText(slotGo.transform, "RuneName", runeInfo, 15, FontStyle.Bold,
                        new Vector2(-90f, -18f), new Vector2(260f, 28f), new Color(0.45f, 0.90f, 1f));

                    // Unequip button (✕)
                    var unequipBtn = CreateButton(slotGo.transform, "UnequipBtn", new Vector2(190f, 0f), new Vector2(48f, 48f),
                        new Color(0.75f, 0.25f, 0.25f, 1f));
                    unequipBtn.GetComponent<Button>().onClick.AddListener(() => OnUnequipSlot(skillId));
                    CreateText(unequipBtn.transform, "Label", "✕", 20, FontStyle.Bold, Vector2.zero, new Vector2(48f, 48f), Color.white);
                }
                else
                {
                    CreateText(slotGo.transform, "EmptyNotice", "미장착 (우측에서 룬 선택 후 장착)", 13, FontStyle.Italic,
                        new Vector2(-40f, -18f), new Vector2(340f, 28f), new Color(0.6f, 0.6f, 0.7f));
                }
            }
        }

        private void RefreshRuneGrid()
        {
            for (int i = _runeGridContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(_runeGridContainer.GetChild(i).gameObject);
            }

            int index = 0;
            foreach (var kvp in _runeManager.Definitions)
            {
                var def = kvp.Value;
                bool isUnlocked = _runeManager.IsUnlocked(def.Id);
                int level = _runeManager.GetLevel(def.Id);
                bool isSelected = def.Id == _selectedRuneId;

                int col = index % 4;
                int row = index / 4;
                float x = -315f + col * 210f;
                float y = 140f - row * 140f;

                var cardGo = CreateButton(_runeGridContainer, $"Rune_{def.Id}", new Vector2(x, y), new Vector2(195f, 120f),
                    isSelected ? new Color(0.25f, 0.50f, 0.85f, 1f) : (isUnlocked ? new Color(0.18f, 0.20f, 0.28f, 1f) : new Color(0.11f, 0.12f, 0.16f, 0.85f)));

                string rId = def.Id;
                cardGo.GetComponent<Button>().onClick.AddListener(() =>
                {
                    _selectedRuneId = rId;
                    Refresh();
                });

                Color gradeColor = def.Grade switch
                {
                    RuneGrade.Legendary => new Color(0.90f, 0.50f, 1.0f, 1f),
                    RuneGrade.Rare => new Color(0.40f, 0.80f, 1.0f, 1f),
                    _ => new Color(0.45f, 0.95f, 0.55f, 1f)
                };

                CreateText(cardGo.transform, "Name", def.Name, 16, FontStyle.Bold,
                    new Vector2(0f, 24f), new Vector2(185f, 26f), gradeColor);

                string gradeBadge = def.Grade switch
                {
                    RuneGrade.Legendary => "★ 전설",
                    RuneGrade.Rare => "◆ 희귀",
                    _ => "● 일반"
                };
                string subText = isUnlocked ? $"{gradeBadge} │ Lv.{level}" : $"{gradeBadge} │ 🔒 미해금";
                Color subCol = isUnlocked ? new Color(0.9f, 0.9f, 0.9f) : new Color(0.6f, 0.6f, 0.6f);
                CreateText(cardGo.transform, "Sub", subText, 13, FontStyle.Normal,
                    new Vector2(0f, -20f), new Vector2(185f, 26f), subCol);

                index++;
            }
        }

        private void RefreshDetail()
        {
            var def = _runeManager.GetDefinition(_selectedRuneId);
            if (def == null) return;

            bool isUnlocked = _runeManager.IsUnlocked(def.Id);
            int level = _runeManager.GetLevel(def.Id);

            string gradeBadge = def.Grade switch
            {
                RuneGrade.Legendary => "<color=#E080FF>[전설 룬]</color>",
                RuneGrade.Rare => "<color=#66CCFF>[희귀 룬]</color>",
                _ => "<color=#77FF88>[일반 룬]</color>"
            };

            _detailTitleText.text = $"{gradeBadge} {def.Name} {(isUnlocked ? $"<color=#FFD700>Lv.{level}</color>" : "<color=#888888>(잠김)</color>")}";
            _detailDescText.text = def.Description;

            if (isUnlocked)
            {
                var curMods = def.CalculateModifiers(level);
                var nextMods = def.CalculateModifiers(level + 1);
                _detailStatsText.text = $"현재: 쿨감 {(1f - curMods.CooldownMultiplier) * 100f:+0.#;-0.#;0}% │ 피해 {(curMods.DamageMultiplier - 1f) * 100f:+0.#;-0.#;0}%\n" +
                                        $"다음: 쿨감 {(1f - nextMods.CooldownMultiplier) * 100f:+0.#;-0.#;0}% │ 피해 {(nextMods.DamageMultiplier - 1f) * 100f:+0.#;-0.#;0}%";

                int cost = def.GetUpgradeCost(level);
                string gemBadge = def.PrimaryGem switch
                {
                    GemType.Ruby => $"<color=#FF5555>◆ 루비 {cost}개</color>",
                    GemType.Emerald => $"<color=#44FF77>◆ 에메랄드 {cost}개</color>",
                    _ => $"<color=#D477FF>◆ 아메시스트 {cost}개</color>"
                };
                _detailCostText.text = $"강화 비용: {gemBadge}";
                _actionButtonText.text = $"🔨 Lv.{level + 1} 강화하기";
                _equipTargetButtonsRow.gameObject.SetActive(true);
            }
            else
            {
                _detailStatsText.text = $"기본: 쿨감 {(1f - def.BaseCooldownMultiplier) * 100f:+0.#;-0.#;0}% │ 피해 {(def.BaseDamageMultiplier - 1f) * 100f:+0.#;-0.#;0}%";
                string costStr = "";
                if (def.UnlockRubyCost > 0) costStr += $"<color=#FF5555>◆ 루비 {def.UnlockRubyCost}개</color> ";
                if (def.UnlockEmeraldCost > 0) costStr += $"<color=#44FF77>◆ 에메랄드 {def.UnlockEmeraldCost}개</color> ";
                if (def.UnlockAmethystCost > 0) costStr += $"<color=#D477FF>◆ 아메시스트 {def.UnlockAmethystCost}개</color> ";
                _detailCostText.text = $"해금 비용: {costStr}";
                _actionButtonText.text = "🔓 룬 해금하기";
                _equipTargetButtonsRow.gameObject.SetActive(false);
            }
        }

        private void OnActionButtonClicked()
        {
            var def = _runeManager.GetDefinition(_selectedRuneId);
            if (def == null || _walletManager == null) return;

            var wallet = _walletManager.SaveData;
            bool success = false;

            if (_runeManager.IsUnlocked(def.Id))
            {
                success = _runeManager.TryUpgradeRune(def.Id, wallet);
            }
            else
            {
                success = _runeManager.TryUnlockRune(def.Id, wallet);
            }

            if (success)
            {
                _walletManager.Save();
                _onDataChanged?.Invoke();
                Refresh();
            }
        }

        private void OnEquipToSkill(string skillId)
        {
            if (_runeManager.EquipRune(skillId, _selectedRuneId))
            {
                _onDataChanged?.Invoke();
                Refresh();
            }
        }

        private void OnUnequipSlot(string skillId)
        {
            _runeManager.UnequipRune(skillId);
            _onDataChanged?.Invoke();
            Refresh();
        }

        // ── UI Helper Methods (Standard zero GC Canvas elements) ──

        private GameObject CreatePanel(Transform parent, string name, Vector2 pos, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            go.GetComponent<Image>().color = new Color(0.08f, 0.08f, 0.12f, 0.9f);
            return go;
        }

        private GameObject CreateButton(Transform parent, string name, Vector2 pos, Vector2 size, Color color)
        {
            var go = CreatePanel(parent, name, pos, size);
            go.GetComponent<Image>().color = color;
            var btn = go.AddComponent<Button>();
            btn.targetGraphic = go.GetComponent<Image>();
            return go;
        }

        private Text CreateText(Transform parent, string name, string text, int fontSize, FontStyle style,
            Vector2 pos, Vector2 size, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            var t = go.GetComponent<Text>();
            t.text = text;
            t.font = Utils.FontHelper.GetKoreanFont();
            t.fontSize = fontSize;
            t.fontStyle = style;
            t.alignment = TextAnchor.MiddleCenter;
            t.color = color;
            t.supportRichText = true;
            return t;
        }
    }
}
