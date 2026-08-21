using System.Collections.Generic;
using UnityEngine;
using HappyShoot.Domain.Chests;
using HappyShoot.Domain.Leveling;
using HappyShoot.View.Player;

namespace HappyShoot.View.Chests
{
    /// <summary>
    /// Manages spawning and visual pooling of treasure chest views in the Unity scene.
    /// </summary>
    public class TreasureChestManagerView : MonoBehaviour
    {
        private TreasureChestManager _domainManager;
        private PlayerView _playerView;
        private SkillRewardManager _rewardManager;
        private readonly List<TreasureChestView> _viewPool = new List<TreasureChestView>(16);

        public TreasureChestManager DomainManager => _domainManager;

        public void Initialize(PlayerView playerView, SkillRewardManager rewardManager)
        {
            _playerView = playerView;
            _rewardManager = rewardManager;
            _domainManager = new TreasureChestManager(playerView != null ? playerView.EventBus : null);
        }

        private void Update()
        {
            if (_domainManager == null || _playerView == null || _playerView.Entity == null || _playerView.Entity.IsDead)
                return;

            _domainManager.Update(_playerView.Entity, _rewardManager, pickupRadius: 0.9f);

            // Synchronize active views
            var activeChests = _domainManager.ActiveChests;
            for (int i = 0; i < activeChests.Count; i++)
            {
                var chest = activeChests[i];
                GetOrCreateView(chest);
            }

            // Update all views in pool so opened/despawned chests are immediately hidden
            for (int i = 0; i < _viewPool.Count; i++)
            {
                if (_viewPool[i].gameObject.activeSelf)
                {
                    _viewPool[i].UpdateView();
                }
            }
        }

        private TreasureChestView GetOrCreateView(TreasureChestEntity entity)
        {
            for (int i = 0; i < _viewPool.Count; i++)
            {
                if (!_viewPool[i].gameObject.activeSelf)
                {
                    _viewPool[i].Bind(entity);
                    return _viewPool[i];
                }
            }

            var go = new GameObject($"ChestView_{_viewPool.Count + 1}");
            go.transform.SetParent(transform);
            var view = go.AddComponent<TreasureChestView>();
            view.Bind(entity);
            _viewPool.Add(view);
            return view;
        }
    }
}
