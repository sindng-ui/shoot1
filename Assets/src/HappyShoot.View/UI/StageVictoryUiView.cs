using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using HappyShoot.Domain.Meta;
using HappyShoot.Domain.Progression;
using HappyShoot.Domain.Session;
using HappyShoot.View.Shop;
using HappyShoot.View.SkillTree;

namespace HappyShoot.View.UI
{
    /// <summary>
    /// Exclusive Stage Victory popup displayed ONLY when the final Boss 3 (Arch-Lich King) is defeated.
    /// Settles earned gold and gems into permanent storage, and unlocks the Skill Tree & Meta Shop.
    /// Strictly modular and under 500 lines.
    /// </summary>
    public class StageVictoryUiView : MonoBehaviour
    {
        [Header("UI References")]
        private GameObject _panelRoot;
        private Text _titleText;
        private Text _survivalTimeText;
        private Text _killCountText;
        private Text _goldEarnedText;
        private Text _gemsEarnedText;
        private Text _companionRewardText;
        private Button _openSkillTreeButton;
        private Button _retryButton;

        private GameSessionEntity _gameSession;
        private MetaShopManager _shopManager;
        private SkillTreeManager _skillTreeManager;
        private SkillTreeUiView _skillTreeUiView;
        private InGameGemCounterHudView _gemCounter;

        public void Initialize(
            GameSessionEntity gameSession,
            MetaShopManager shopManager,
            SkillTreeManager skillTreeManager,
            SkillTreeUiView skillTreeUiView,
            InGameGemCounterHudView gemCounter,
            Transform parentCanvasTf)
        {
            _gameSession = gameSession;
            _shopManager = shopManager;
            _skillTreeManager = skillTreeManager;
            _skillTreeUiView = skillTreeUiView;
            _gemCounter = gemCounter;

            if (parentCanvasTf != null)
            {
                transform.SetParent(parentCanvasTf, false);
            }

            BuildUi();
            if (_panelRoot != null) _panelRoot.SetActive(false);
        }

        public void ShowVictoryPopup()
        {
            Debug.Log("[StageVictoryUiView] ShowVictoryPopup called! Displaying Victory UI!");
            if (_panelRoot != null)
            {
                _panelRoot.SetActive(true);
                _panelRoot.transform.SetAsLastSibling();
            }

            // 1. Settle rewards ONLY on Stage Victory!
            if (_gameSession != null)
            {
                if (_survivalTimeText != null)
                    _survivalTimeText.text = $"⏱️ 클리어 시간: {_gameSession.GetFormattedTime()}";

                if (_killCountText != null)
                    _killCountText.text = $"💀 격파한 몬스터: {_gameSession.KillCount} 마리";

                if (_goldEarnedText != null)
                    _goldEarnedText.text = $"💰 획득한 전리품 골드: +{_gameSession.GoldEarned} G";

                // Settle gold into permanent shop and SkillTree storage
                if (_gameSession.GoldEarned > 0)
                {
                    if (_shopManager != null) _shopManager.AddGold(_gameSession.GoldEarned);
                    if (_skillTreeManager != null) _skillTreeManager.AddGold(_gameSession.GoldEarned);
                }

                // Settle gems into permanent SkillTree storage
                if (_gemCounter != null && _skillTreeManager != null)
                {
                    int r = _gemCounter.RunRubyCount;
                    int e = _gemCounter.RunEmeraldCount;
                    int a = _gemCounter.RunAmethystCount;

                    if (r > 0) _skillTreeManager.AddGems(GemType.Ruby, r);
                    if (e > 0) _skillTreeManager.AddGems(GemType.Emerald, e);
                    if (a > 0) _skillTreeManager.AddGems(GemType.Amethyst, a);

                    if (_gemsEarnedText != null)
                    {
                        _gemsEarnedText.text = $"💎 획득한 영구 보석: 🔴+{r}  🟢+{e}  🟣+{a}";
                    }
                }

                // Increment clear count and show companion unlock reward!
                if (_skillTreeManager != null)
                {
                    _skillTreeManager.IncrementClearCount();
                    int clears = _skillTreeManager.ClearCount;
                    if (_companionRewardText != null)
                    {
                        if (clears == 1)
                            _companionRewardText.text = "🎉 1회차 클리어 특전: [전사 동료 (Warrior)] 영구 해금!";
                        else if (clears == 2)
                            _companionRewardText.text = "🎉 2회차 클리어 특전: [궁수 동료 (Ranger)] 영구 해금!";
                        else
                            _companionRewardText.text = $"🏆 {clears}회차 정복 완료! (3인 마법 원정대 출격)";
                    }
                }
            }

            Utils.HitStopManager.Instance?.CancelHitStop();
            Time.timeScale = 0f;
        }

        private void BuildUi()
        {
            var canvasGo = new GameObject("VictoryCanvas");
            canvasGo.transform.SetParent(transform, false);
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 120; // Highest UI priority above all HUDs!
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1440, 810);
            scaler.matchWidthOrHeight = 1.0f;
            canvasGo.AddComponent<GraphicRaycaster>();

            _panelRoot = new GameObject("VictoryPanelRoot");
            _panelRoot.transform.SetParent(canvasGo.transform, false);

            var rt = _panelRoot.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            var bgImg = _panelRoot.AddComponent<Image>();
            bgImg.color = new Color(0.02f, 0.05f, 0.12f, 0.94f); // Majestic dark navy

            // Dialog Window
            var dialogGo = new GameObject("DialogFrame");
            dialogGo.transform.SetParent(_panelRoot.transform, false);
            var dialogRt = dialogGo.AddComponent<RectTransform>();
            dialogRt.sizeDelta = new Vector2(580f, 490f);
            var dialogImg = dialogGo.AddComponent<Image>();
            dialogImg.color = new Color(0.10f, 0.14f, 0.22f, 0.98f);

            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");

            // Crown / Title Text
            var titleGo = new GameObject("TitleText");
            titleGo.transform.SetParent(dialogGo.transform, false);
            var titleRt = titleGo.AddComponent<RectTransform>();
            titleRt.anchoredPosition = new Vector2(0f, 175f);
            titleRt.sizeDelta = new Vector2(520f, 60f);
            _titleText = titleGo.AddComponent<Text>();
            _titleText.font = font;
            _titleText.text = "🏆 STAGE CLEAR - VICTORY!";
            _titleText.fontSize = 32;
            _titleText.fontStyle = FontStyle.Bold;
            _titleText.alignment = TextAnchor.MiddleCenter;
            _titleText.color = new Color(1.0f, 0.85f, 0.25f, 1.0f); // Bright Gold

            // Stats lines
            _survivalTimeText = CreateStatText(dialogGo.transform, new Vector2(0f, 110f), font);
            _killCountText = CreateStatText(dialogGo.transform, new Vector2(0f, 77f), font);
            _goldEarnedText = CreateStatText(dialogGo.transform, new Vector2(0f, 44f), font, new Color(1.0f, 0.85f, 0.2f));
            _gemsEarnedText = CreateStatText(dialogGo.transform, new Vector2(0f, 11f), font, new Color(0.3f, 0.95f, 1.0f));
            _companionRewardText = CreateStatText(dialogGo.transform, new Vector2(0f, -24f), font, new Color(0.4f, 1.0f, 0.6f));

            // Exclusive Unlocked Button: Skill Tree
            var skillTreeBtnGo = new GameObject("BtnOpenSkillTree");
            skillTreeBtnGo.transform.SetParent(dialogGo.transform, false);
            var stRt = skillTreeBtnGo.AddComponent<RectTransform>();
            stRt.anchoredPosition = new Vector2(0f, -95f);
            stRt.sizeDelta = new Vector2(460f, 56f);
            var stImg = skillTreeBtnGo.AddComponent<Image>();
            stImg.color = new Color(0.85f, 0.65f, 0.10f, 1.0f); // Gold highlight
            _openSkillTreeButton = skillTreeBtnGo.AddComponent<Button>();
            _openSkillTreeButton.onClick.AddListener(OnOpenSkillTreeClicked);

            var stTextGo = new GameObject("Text");
            stTextGo.transform.SetParent(skillTreeBtnGo.transform, false);
            var stTextRt = stTextGo.AddComponent<RectTransform>();
            stTextRt.sizeDelta = stRt.sizeDelta;
            var stText = stTextGo.AddComponent<Text>();
            stText.font = font;
            stText.text = "💎 승리자의 특전: 영구 성장 & 스킬 트리 개방";
            stText.fontSize = 20;
            stText.fontStyle = FontStyle.Bold;
            stText.alignment = TextAnchor.MiddleCenter;
            stText.color = Color.black;

            // Retry / Replay Button
            var retryBtnGo = new GameObject("BtnRetry");
            retryBtnGo.transform.SetParent(dialogGo.transform, false);
            var reRt = retryBtnGo.AddComponent<RectTransform>();
            reRt.anchoredPosition = new Vector2(0f, -170f);
            reRt.sizeDelta = new Vector2(300f, 44f);
            var reImg = retryBtnGo.AddComponent<Image>();
            reImg.color = new Color(0.25f, 0.35f, 0.45f, 1.0f);
            _retryButton = retryBtnGo.AddComponent<Button>();
            _retryButton.onClick.AddListener(OnRetryClicked);

            var reTextGo = new GameObject("Text");
            reTextGo.transform.SetParent(retryBtnGo.transform, false);
            var reTextRt = reTextGo.AddComponent<RectTransform>();
            reTextRt.sizeDelta = reRt.sizeDelta;
            var reText = reTextGo.AddComponent<Text>();
            reText.font = font;
            reText.text = "🔄 새로운 모험 시작 (재도전)";
            reText.fontSize = 18;
            reText.alignment = TextAnchor.MiddleCenter;
            reText.color = Color.white;
        }

        private Text CreateStatText(Transform parent, Vector2 pos, Font font, Color? color = null)
        {
            var go = new GameObject("StatLine");
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(480f, 32f);
            var text = go.AddComponent<Text>();
            text.font = font;
            text.fontSize = 18;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = color ?? Color.white;
            return text;
        }

        private void OnOpenSkillTreeClicked()
        {
            if (_skillTreeUiView != null)
            {
                _skillTreeUiView.Show();
            }
        }

        private void OnRetryClicked()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
