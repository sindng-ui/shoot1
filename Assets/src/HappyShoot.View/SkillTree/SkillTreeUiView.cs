using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using HappyShoot.Domain.Entities;
using HappyShoot.Domain.Progression;
using HappyShoot.View.Utils;

namespace HappyShoot.View.SkillTree
{
    /// <summary>
    /// Master Arcane Constellation Skill Tree Progression Screen (Wizard-Only).
    /// Features an expansive 360° celestial map with 18 Wizard nodes (6 Core + 3 Elemental Branches),
    /// persistent Gold currency wallet, and elemental branch awakening/50% refund reset.
    /// Strictly modular and under 500 lines.
    /// </summary>
    public class SkillTreeUiView : MonoBehaviour
    {
        private SkillTreeManager _manager;
        private CharacterClassType _selectedClass = CharacterClassType.Wizard;

        private GameObject _panelRoot;
        private Transform _treeContainer;
        private Text _goldText;
        private Text _awakeningStatusText;
        private Button _resetAwakeningBtn;

        // Detail panel
        private GameObject _detailPanel;
        private Text _detailTitleText;
        private Text _detailDescText;
        private Text _detailCostText;
        private Button _unlockBtn;
        private SkillTreeNodeDef _selectedNode;

        private readonly List<SkillTreeNodeView> _activeNodeViews = new List<SkillTreeNodeView>(32);
        private readonly List<GameObject> _activeLines = new List<GameObject>(32);

        public SkillTreeManager Manager => _manager;
        public event Action OnSkillTreeClosed;

        public void Initialize(SkillTreeManager manager)
        {
            _manager = manager;
            if (_manager != null)
            {
                _manager.OnTreeStateChanged += RefreshAll;
            }

            EnsureUiElements();
            RefreshAll();
            Hide();
        }

        private void OnDestroy()
        {
            if (_manager != null)
            {
                _manager.OnTreeStateChanged -= RefreshAll;
            }
        }

        private void Update()
        {
            if (_panelRoot == null || !_panelRoot.activeSelf) return;

            bool escPressed = false;
            #if ENABLE_INPUT_SYSTEM
            if (UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                escPressed = true;
            }
            #endif
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                escPressed = true;
            }

            if (escPressed)
            {
                Hide();
            }
        }

        public void Show(CharacterClassType defaultClass = CharacterClassType.Wizard)
        {
            _selectedClass = CharacterClassType.Wizard;
            EnsureUiElements();
            _panelRoot.SetActive(true);
            RefreshAll();
        }

        public void Hide()
        {
            if (_panelRoot != null)
            {
                _panelRoot.SetActive(false);
            }
            OnSkillTreeClosed?.Invoke();
        }

        public void RefreshAll()
        {
            if (_panelRoot == null || !_panelRoot.activeSelf) return;

            // 1. Update Gold Wallet
            if (_goldText != null && _manager != null)
            {
                _goldText.text = $"💰 보유 금화: {_manager.GetGoldCount():N0} G";
            }

            // 2. Update Awakening Status
            UpdateAwakeningStatus();

            // 3. Rebuild 360° Master Arcane Constellation
            RebuildTreeView();

            // 4. Update Detail Panel
            RefreshDetailPanel();
        }

        private void UpdateAwakeningStatus()
        {
            if (_awakeningStatusText == null || _manager == null) return;

            var awakened = _manager.GetAwakenedBranch(_selectedClass);

            if (awakened == BranchType.None)
            {
                _awakeningStatusText.text = "속성 각성: [미각성] (분기 첫 노드 해금 시 선택)";
                _awakeningStatusText.color = new Color(0.8f, 0.8f, 0.8f);
                if (_resetAwakeningBtn != null) _resetAwakeningBtn.gameObject.SetActive(false);
            }
            else
            {
                _awakeningStatusText.text = $"속성 각성: [{awakened.GetBranchDisplayName()} 각성 중]";
                _awakeningStatusText.color = new Color(1.0f, 0.85f, 0.2f);
                if (_resetAwakeningBtn != null) _resetAwakeningBtn.gameObject.SetActive(true);
            }
        }

        private void RebuildTreeView()
        {
            for (int i = 0; i < _activeLines.Count; i++) Destroy(_activeLines[i]);
            _activeLines.Clear();
            for (int i = 0; i < _activeNodeViews.Count; i++) Destroy(_activeNodeViews[i].gameObject);
            _activeNodeViews.Clear();

            if (_manager == null) return;

            // 1. Sector Divider Rays (120° intervals)
            SkillTreeLayoutHelper.CreateSectorDividers(_treeContainer);

            // 2. Central Arcane Core Emblem (Origin)
            CreateCentralHub();

            // 3. Spawn 3 Elemental Spire Labels
            CreateSectorLabels();

            // 4. Spawn all Wizard Nodes across the 360° tree
            foreach (var kvp in _manager.NodeDefs)
            {
                var def = kvp.Value;
                if (def.ClassType != CharacterClassType.Wizard) continue;

                var nodeGo = new GameObject($"Node_{def.Id}");
                nodeGo.transform.SetParent(_treeContainer, false);
                var view = nodeGo.AddComponent<SkillTreeNodeView>();
                view.Initialize(def, OnNodeClicked);
                view.RectTransform.anchoredPosition = SkillTreeLayoutHelper.GetNodePosition(def);

                int level = _manager.GetNodeLevel(def.Id);
                bool canUnlock = _manager.CanUnlockNode(def.Id);
                bool isBlocked = _manager.IsBranchLocked(def.Id);
                view.RefreshState(level, canUnlock, isBlocked);

                _activeNodeViews.Add(view);
            }

            // 5. Connect Prerequisite Lines
            for (int i = 0; i < _activeNodeViews.Count; i++)
            {
                var view = _activeNodeViews[i];
                var def = view.Def;

                if (def.PrerequisiteIds == null || def.PrerequisiteIds.Length == 0)
                {
                    // Connect tier-1 core nodes to Central Hub (0,0)
                    bool isUnlocked = _manager.GetNodeLevel(def.Id) > 0;
                    var centerLine = SkillTreeLayoutHelper.CreateConnectionLine(
                        _treeContainer,
                        Vector2.zero,
                        view.RectTransform.anchoredPosition,
                        isUnlocked,
                        false);
                    _activeLines.Add(centerLine);
                    continue;
                }

                for (int p = 0; p < def.PrerequisiteIds.Length; p++)
                {
                    var prereqView = _activeNodeViews.Find(v => v.Def.Id == def.PrerequisiteIds[p]);
                    if (prereqView != null)
                    {
                        bool isUnlocked = _manager.GetNodeLevel(def.Id) > 0;
                        bool isBlocked = _manager.IsBranchLocked(def.Id);
                        var line = SkillTreeLayoutHelper.CreateConnectionLine(
                            _treeContainer,
                            prereqView.RectTransform.anchoredPosition,
                            view.RectTransform.anchoredPosition,
                            isUnlocked,
                            isBlocked);
                        _activeLines.Add(line);
                    }
                }
            }
        }

        private void CreateCentralHub()
        {
            var hubGo = new GameObject("CentralHub");
            hubGo.transform.SetParent(_treeContainer, false);
            var rt = hubGo.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(76, 76);
            rt.anchoredPosition = Vector2.zero;

            var img = hubGo.AddComponent<Image>();
            img.sprite = SkillTreeSpriteHelper.GetOrCreateCentralHubSprite(76);

            var txt = CreateText(hubGo.transform, "🔮\n마력 코어", Vector2.zero, 12, new Color(0.20f, 0.15f, 0.05f));
            txt.fontStyle = FontStyle.Bold;
        }

        private void CreateSectorLabels()
        {
            // Fire Spire (Top: 90°)
            CreateSectorLabel("🔥 인페르노 화염 대마법사", new Vector2(0, 485), new Color(1.0f, 0.50f, 0.40f));
            // Ice Spire (Bottom-Left: 210°)
            CreateSectorLabel("❄️ 절대영도 빙결 비전", new Vector2(-420, -250), new Color(0.50f, 0.85f, 1.0f));
            // Lightning Spire (Bottom-Right: 330°)
            CreateSectorLabel("⚡ 폭풍현자 전격 비전", new Vector2(420, -250), new Color(0.92f, 0.65f, 1.0f));
        }

        private void CreateSectorLabel(string title, Vector2 pos, Color color)
        {
            var go = CreateUiObject("SectorLabel", _treeContainer, pos, new Vector2(280, 36));
            var txt = go.AddComponent<Text>();
            txt.font = FontHelper.GetKoreanFont();
            txt.text = title;
            txt.fontSize = 17;
            txt.fontStyle = FontStyle.Bold;
            txt.color = color;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.raycastTarget = false;
        }

        private void OnNodeClicked(SkillTreeNodeDef def)
        {
            _selectedNode = def;
            RefreshDetailPanel();
            UpdateAwakeningStatus();
        }

        private void RefreshDetailPanel()
        {
            if (_detailPanel == null) return;

            if (_selectedNode == null)
            {
                _detailTitleText.text = "💡 노드를 선택하세요";
                _detailDescText.text = "360° 비전 성좌의 노드를 클릭하면\n상세 효과와 해금 버튼이 표시됩니다.\n\n중앙 6개는 영구 기본 스탯 노드이며,\n바깥 3대 원소 분기(화염/빙결/전기)는\n첫 해금 시 해당 속성으로 각성됩니다.";
                _detailCostText.text = "선택된 노드 없음";
                _detailCostText.color = Color.gray;
                _unlockBtn.interactable = false;
                return;
            }

            _detailTitleText.text = $"{_selectedNode.Title}";
            _detailDescText.text = _selectedNode.Description;

            int level = _manager.GetNodeLevel(_selectedNode.Id);
            bool isMax = level >= _selectedNode.MaxLevel;
            bool isBlocked = _manager.IsBranchLocked(_selectedNode.Id);
            bool canUnlock = _manager.CanUnlockNode(_selectedNode.Id);

            if (isMax)
            {
                _detailCostText.text = "상태: 해금 완료 (MAX)";
                _detailCostText.color = new Color(0.4f, 1.0f, 0.5f);
                _unlockBtn.interactable = false;
            }
            else if (isBlocked)
            {
                _detailCostText.text = "상태: 다른 속성 각성으로 잠김 (리셋 필요)";
                _detailCostText.color = new Color(1.0f, 0.4f, 0.4f);
                _unlockBtn.interactable = false;
            }
            else
            {
                _detailCostText.text = $"필요 금화: {_selectedNode.GoldCost:N0} G";
                _detailCostText.color = canUnlock ? new Color(1.0f, 0.9f, 0.3f) : new Color(0.7f, 0.7f, 0.7f);
                _unlockBtn.interactable = canUnlock;
            }
        }

        private void OnUnlockClicked()
        {
            if (_selectedNode != null && _manager != null)
            {
                _manager.TryUnlockNode(_selectedNode.Id);
            }
        }

        private void OnResetAwakeningClicked()
        {
            if (_manager != null)
            {
                int refunded = _manager.ResetAwakening(_selectedClass);
                Debug.Log($"[SkillTreeUiView] ResetAwakening refunded {refunded} Gold!");
                RefreshAll();
            }
        }

        // ── UI Construction ──

        private void EnsureUiElements()
        {
            if (_panelRoot != null) return;

            var canvasGo = new GameObject("SkillTreeCanvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 200; // Above all normal HUDs
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            canvasGo.AddComponent<GraphicRaycaster>();

            _panelRoot = new GameObject("SkillTreePanel");
            _panelRoot.transform.SetParent(canvasGo.transform, false);
            var panelRt = _panelRoot.AddComponent<RectTransform>();
            panelRt.anchorMin = Vector2.zero;
            panelRt.anchorMax = Vector2.one;
            panelRt.offsetMin = Vector2.zero;
            panelRt.offsetMax = Vector2.zero;

            // 100% Solid Dark Nebula Background
            var bgImg = _panelRoot.AddComponent<Image>();
            bgImg.sprite = SpriteHelper.GetOrCreateWhiteSprite();
            bgImg.color = new Color(0.04f, 0.05f, 0.09f, 1.0f);

            BuildHeader();
            BuildTreeArea();
            BuildDetailPanel();
        }

        private void BuildHeader()
        {
            // Back Button (Top-Left)
            var backBtn = CreateButton(_panelRoot.transform, "◀ 뒤로 가기 (ESC)", new Vector2(-780, 470), new Vector2(210, 48), Hide);
            backBtn.image.color = new Color(0.22f, 0.28f, 0.40f, 1.0f);

            // Title Bar (Top-Center y: 470)
            var titleGo = CreateUiObject("TitleBar", _panelRoot.transform, new Vector2(-200, 470), new Vector2(400, 48));
            var titleTxt = titleGo.AddComponent<Text>();
            titleTxt.font = FontHelper.GetKoreanFont();
            titleTxt.text = "🌌 대마법사 비전 성좌 (영구 성장)";
            titleTxt.fontSize = 24;
            titleTxt.fontStyle = FontStyle.Bold;
            titleTxt.color = new Color(0.85f, 0.75f, 1.0f);
            titleTxt.alignment = TextAnchor.MiddleCenter;
            titleTxt.raycastTarget = false;

            // Gold Wallet Bar (Top-Right-Center y: 470)
            var walletGo = CreateUiObject("GoldBar", _panelRoot.transform, new Vector2(300, 470), new Vector2(380, 48));
            var walletBg = walletGo.AddComponent<Image>();
            walletBg.sprite = SpriteHelper.GetOrCreateWhiteSprite();
            walletBg.color = new Color(0.12f, 0.14f, 0.22f, 0.95f);

            _goldText = CreateText(walletGo.transform, "💰 보유 금화: 0 G", Vector2.zero, 20, new Color(1.0f, 0.88f, 0.35f));
            _goldText.alignment = TextAnchor.MiddleCenter;

            // Close Button (Top-Right)
            var closeBtn = CreateButton(_panelRoot.transform, "✕ 닫기 (ESC)", new Vector2(780, 470), new Vector2(160, 48), Hide);
            closeBtn.image.color = new Color(0.70f, 0.20f, 0.25f, 1.0f);
        }

        private void BuildTreeArea()
        {
            // Radial constellation tree container (center at x: -160, y: -25)
            var treeAreaGo = CreateUiObject("TreeArea", _panelRoot.transform, new Vector2(-160, -25), new Vector2(1000, 1000));
            var dialImg = treeAreaGo.AddComponent<Image>();
            dialImg.sprite = SkillTreeBackgroundHelper.GetOrCreateDialSprite(512);
            dialImg.color = Color.white;
            dialImg.raycastTarget = false;
            _treeContainer = treeAreaGo.transform;
        }

        private void BuildDetailPanel()
        {
            // Right-side inspector panel
            _detailPanel = CreateUiObject("DetailPanel", _panelRoot.transform, new Vector2(670, -20), new Vector2(400, 720));
            var bg = _detailPanel.AddComponent<Image>();
            bg.sprite = SpriteHelper.GetOrCreateWhiteSprite();
            bg.color = new Color(0.09f, 0.11f, 0.17f, 1.0f);

            _detailTitleText = CreateText(_detailPanel.transform, "💡 노드를 선택하세요", new Vector2(0, 310), 22, new Color(1.0f, 0.9f, 0.35f));
            _detailDescText = CreateText(_detailPanel.transform, "원형 성좌 노드를 클릭하면\n상세 효과와 해금 버튼이 표시됩니다.", new Vector2(0, 140), 16, new Color(0.85f, 0.90f, 0.95f));
            _detailDescText.rectTransform.sizeDelta = new Vector2(360, 240);

            // Awakening status & Reset button inside Detail Panel
            _awakeningStatusText = CreateText(_detailPanel.transform, "속성 상태: [미각성]", new Vector2(0, -60), 15, Color.yellow);
            _awakeningStatusText.rectTransform.sizeDelta = new Vector2(360, 40);
            _resetAwakeningBtn = CreateButton(_detailPanel.transform, "⚡ 각성 리셋 (50% 골드 환불)", new Vector2(0, -115), new Vector2(260, 40), OnResetAwakeningClicked);

            _detailCostText = CreateText(_detailPanel.transform, "선택된 노드 없음", new Vector2(0, -180), 18, Color.gray);
            _unlockBtn = CreateButton(_detailPanel.transform, "💰 노드 해금", new Vector2(0, -260), new Vector2(320, 56), OnUnlockClicked);
        }

        private GameObject CreateUiObject(string name, Transform parent, Vector2 pos, Vector2 size)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            return go;
        }

        private Text CreateText(Transform parent, string text, Vector2 pos, int fontSize, Color color)
        {
            var go = new GameObject("Text");
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(400, 40);
            var txt = go.AddComponent<Text>();
            txt.font = FontHelper.GetKoreanFont();
            txt.text = text;
            txt.fontSize = fontSize;
            txt.fontStyle = FontStyle.Bold;
            txt.color = color;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.raycastTarget = false;
            return txt;
        }

        private Button CreateButton(Transform parent, string label, Vector2 pos, Vector2 size, Action onClick)
        {
            var go = CreateUiObject("Btn_" + label, parent, pos, size);
            var img = go.AddComponent<Image>();
            img.sprite = SpriteHelper.GetOrCreateWhiteSprite();
            img.color = new Color(0.20f, 0.45f, 0.85f, 1.0f);

            var btn = go.AddComponent<Button>();
            btn.targetGraphic = img;
            if (onClick != null) btn.onClick.AddListener(() => onClick());

            var txt = CreateText(go.transform, label, Vector2.zero, 15, Color.white);
            txt.rectTransform.sizeDelta = size;
            return btn;
        }
    }
}
