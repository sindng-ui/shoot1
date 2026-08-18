using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using HappyShoot.Domain.Entities;
using HappyShoot.Domain.Leveling;
using HappyShoot.View.Player;

namespace HappyShoot.View.UI
{
    /// <summary>
    /// Unity UI View that manages the 3-choice skill selection popup on level up.
    /// </summary>
    public class LevelUpUiView : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerView _playerView;
        [SerializeField] private GameObject _panelRoot;
        [SerializeField] private Transform _cardsContainer;
        [SerializeField] private GameObject _rewardCardPrefab;

        private LevelSystem _levelSystem;
        private SkillRewardManager _rewardManager;
        private List<SkillRewardOption> _currentOptions;

        public LevelSystem LevelSystem => _levelSystem;
        public SkillRewardManager RewardManager => _rewardManager;

        public void Initialize(LevelSystem levelSystem, SkillRewardManager rewardManager)
        {
            _levelSystem = levelSystem;
            _rewardManager = rewardManager;

            if (_levelSystem != null)
            {
                _levelSystem.OnLevelUp += ShowLevelUpPopup;
            }

            if (_panelRoot != null)
            {
                _panelRoot.SetActive(false);
            }
        }

        public void ShowLevelUpPopup(int newLevel)
        {
            if (_rewardManager == null || _playerView == null || _playerView.Entity == null)
                return;

            _currentOptions = _rewardManager.RollRewards(_playerView.Entity, count: 3);

            if (_panelRoot != null)
            {
                _panelRoot.SetActive(true);
            }

            // Pause game
            Time.timeScale = 0f;

            Debug.Log($"[LevelUpUiView] Level Up to Lv.{newLevel}! Showing {_currentOptions.Count} reward cards.");
        }

        /// <summary>
        /// Called when the player selects a card option (0, 1, or 2).
        /// </summary>
        public void SelectOption(int optionIndex)
        {
            if (_currentOptions == null || optionIndex < 0 || optionIndex >= _currentOptions.Count)
                return;

            var selected = _currentOptions[optionIndex];
            _rewardManager.ApplyReward(_playerView.Entity, selected);

            Debug.Log($"[LevelUpUiView] Selected reward: {selected.Title}");

            // Close UI and resume game
            if (_panelRoot != null)
            {
                _panelRoot.SetActive(false);
            }

            Time.timeScale = 1f;
        }

        private void OnDestroy()
        {
            if (_levelSystem != null)
            {
                _levelSystem.OnLevelUp -= ShowLevelUpPopup;
            }
        }
    }
}
