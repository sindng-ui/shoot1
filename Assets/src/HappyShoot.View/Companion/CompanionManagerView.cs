using System;
using UnityEngine;
using HappyShoot.Domain.Entities;
using HappyShoot.Domain.Events;
using HappyShoot.Domain.Progression;
using HappyShoot.Domain.Spatial;
using HappyShoot.View.Monsters;
using HappyShoot.View.Player;
using HappyShoot.View.Projectiles;

namespace HappyShoot.View.Companion
{
    /// <summary>
    /// Manages the spawning, lifecycle, and runtime toggling (via cheats or progress) of AI companions.
    /// Strictly modular and under 500 lines.
    /// </summary>
    public class CompanionManagerView : MonoBehaviour
    {
        private PlayerView _playerView;
        private MonsterSpawnerView _spawnerView;
        private ProjectileManagerView _projManager;
        private EventBus _eventBus;
        private SkillTreeManager _skillTreeManager;

        private CompanionView _warriorView;
        private CompanionView _rangerView;

        public bool IsWarriorActive => _warriorView != null && _warriorView.gameObject.activeSelf;
        public bool IsRangerActive => _rangerView != null && _rangerView.gameObject.activeSelf;

        public event Action OnCompanionsChanged;

        public void Initialize(
            PlayerView playerView,
            MonsterSpawnerView spawnerView,
            ProjectileManagerView projManager,
            EventBus eventBus,
            SkillTreeManager skillTreeManager)
        {
            _playerView = playerView;
            _spawnerView = spawnerView;
            _projManager = projManager;
            _eventBus = eventBus;
            _skillTreeManager = skillTreeManager;

            // Spawn companions based on permanent progression clear count
            if (_skillTreeManager != null)
            {
                if (_skillTreeManager.IsWarriorUnlocked)
                {
                    SetWarriorActive(true);
                }
                if (_skillTreeManager.IsRangerUnlocked)
                {
                    SetRangerActive(true);
                }
            }
        }

        public void ToggleWarrior()
        {
            SetWarriorActive(!IsWarriorActive);
        }

        public void ToggleRanger()
        {
            SetRangerActive(!IsRangerActive);
        }

        public void SetWarriorActive(bool active)
        {
            if (active)
            {
                if (_warriorView == null)
                {
                    _warriorView = CreateCompanion(CompanionType.Warrior);
                }
                _warriorView.gameObject.SetActive(true);
            }
            else if (_warriorView != null)
            {
                _warriorView.gameObject.SetActive(false);
            }

            OnCompanionsChanged?.Invoke();
        }

        public void SetRangerActive(bool active)
        {
            if (active)
            {
                if (_rangerView == null)
                {
                    _rangerView = CreateCompanion(CompanionType.Ranger);
                }
                _rangerView.gameObject.SetActive(true);
            }
            else if (_rangerView != null)
            {
                _rangerView.gameObject.SetActive(false);
            }

            OnCompanionsChanged?.Invoke();
        }

        private CompanionView CreateCompanion(CompanionType type)
        {
            var go = new GameObject($"Companion_{type}");
            go.transform.SetParent(transform, false);
            go.transform.position = _playerView != null ? _playerView.transform.position : Vector3.zero;

            var entity = new CompanionEntity(
                type,
                _playerView?.Entity,
                new Vector2D(go.transform.position.x, go.transform.position.y));

            var view = go.AddComponent<CompanionView>();
            view.Initialize(entity, _playerView, _spawnerView, _projManager, _eventBus);
            return view;
        }
    }
}
