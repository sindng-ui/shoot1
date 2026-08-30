using System;
using UnityEngine;
using UnityEngine.UI;
using HappyShoot.Domain.Progression;

namespace HappyShoot.View.SkillTree
{
    /// <summary>
    /// UI component representing a single interactive skill tree node on the graph.
    /// Strictly modular and under 500 lines.
    /// </summary>
    public class SkillTreeNodeView : MonoBehaviour
    {
        public SkillTreeNodeDef Def { get; private set; }

        private Image _frameImg;
        private Image _iconImg;
        private Text _levelText;
        private Button _btn;
        private Action<SkillTreeNodeDef> _onClick;

        public RectTransform RectTransform => (RectTransform)transform;

        public void Initialize(SkillTreeNodeDef def, Action<SkillTreeNodeDef> onClick)
        {
            Def = def;
            _onClick = onClick;

            BuildUi();
        }

        private void BuildUi()
        {
            var rt = gameObject.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(66, 66);

            // Frame Image
            _frameImg = gameObject.AddComponent<Image>();
            _frameImg.type = Image.Type.Simple;

            // Icon Image
            var iconGo = new GameObject("Icon");
            iconGo.transform.SetParent(transform, false);
            var iconRt = iconGo.AddComponent<RectTransform>();
            iconRt.sizeDelta = new Vector2(36, 36);
            iconRt.anchoredPosition = new Vector2(0, 7);
            _iconImg = iconGo.AddComponent<Image>();
            _iconImg.sprite = SkillTreeSpriteHelper.GetBranchIcon(Def.Branch);
            _iconImg.raycastTarget = false;

            // Level / Status Text
            var textGo = new GameObject("LevelText");
            textGo.transform.SetParent(transform, false);
            var textRt = textGo.AddComponent<RectTransform>();
            textRt.sizeDelta = new Vector2(64, 18);
            textRt.anchoredPosition = new Vector2(0, -22);
            _levelText = textGo.AddComponent<Text>();
            _levelText.font = Utils.FontHelper.GetKoreanFont();
            _levelText.fontSize = 12;
            _levelText.fontStyle = FontStyle.Bold;
            _levelText.alignment = TextAnchor.MiddleCenter;
            _levelText.color = Color.white;
            _levelText.raycastTarget = false;

            // Button Click
            _btn = gameObject.AddComponent<Button>();
            _btn.transition = Selectable.Transition.ColorTint;
            _btn.onClick.AddListener(() => _onClick?.Invoke(Def));
        }

        /// <summary>
        /// Updates the visual state of the node (frame, text, interactability).
        /// </summary>
        public void RefreshState(int currentLevel, bool canUnlock, bool isBlocked)
        {
            bool isUnlocked = currentLevel >= Def.MaxLevel;
            _frameImg.sprite = SkillTreeSpriteHelper.GetNodeFrame(isUnlocked, canUnlock, isBlocked);

            if (isUnlocked)
            {
                _levelText.text = "✓ 해금";
                _levelText.color = new Color(0.4f, 1.0f, 0.5f);
                _btn.interactable = true; // Still clickable to view info
            }
            else if (isBlocked)
            {
                _levelText.text = "✖ 잠김";
                _levelText.color = new Color(1.0f, 0.4f, 0.4f);
                _btn.interactable = true;
            }
            else if (canUnlock)
            {
                _levelText.text = $"{Def.GoldCost}G";
                _levelText.color = new Color(1.0f, 0.9f, 0.3f);
                _btn.interactable = true;
            }
            else
            {
                _levelText.text = $"{Def.GoldCost}G";
                _levelText.color = new Color(0.6f, 0.6f, 0.65f);
                _btn.interactable = true;
            }
        }
    }
}
