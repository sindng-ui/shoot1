using System.Collections.Generic;
using UnityEngine;
using HappyShoot.Domain.Events;
using HappyShoot.Domain.Gems;
using HappyShoot.Domain.Spatial;
using HappyShoot.View.Player;

namespace HappyShoot.View.Gems
{
    /// <summary>
    /// Unity MonoBehaviour representing a single experience gem.
    /// Enhanced with color coding by exp value and high performance rendering.
    /// Strictly modular and under 500 lines.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class ExpGemView : MonoBehaviour
    {
        private ExpGemEntity _entity;
        private SpriteRenderer _spriteRenderer;
        private Transform _transform;

        private void Awake()
        {
            _transform = transform;
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        public void Bind(ExpGemEntity entity)
        {
            _entity = entity;
            _transform.position = new Vector3(entity.Position.X, entity.Position.Y, 0f);

            if (_spriteRenderer.sprite == null)
            {
                _spriteRenderer.sprite = Utils.SpriteHelper.GetOrCreateGemSprite();
            }

            // Gem color tint based on value
            if (entity.ExpValue >= 20)
            {
                _spriteRenderer.color = new Color(1.0f, 0.35f, 0.35f); // Red high-value gem
                _transform.localScale = Vector3.one * 0.95f;
            }
            else if (entity.ExpValue >= 5)
            {
                _spriteRenderer.color = new Color(0.35f, 0.85f, 1.0f); // Blue medium gem
                _transform.localScale = Vector3.one * 0.80f;
            }
            else
            {
                _spriteRenderer.color = Color.white; // Standard green/white gem
                _transform.localScale = Vector3.one * 0.70f;
            }

            gameObject.SetActive(true);
        }

        public void UpdateView()
        {
            if (_entity == null || !_entity.IsActive)
            {
                gameObject.SetActive(false);
                return;
            }

            _transform.position = new Vector3(_entity.Position.X, _entity.Position.Y, 0f);
        }
    }

    /// <summary>
    /// Synchronizes experience gem spawning and collection with the Unity scene using a massive 1,500+ prewarmed zero-allocation pool.
    /// Automatically expands dynamically if player defeats massive hordes across the entire arena.
    /// Strictly modular and under 500 lines.
    /// </summary>
    public class GemManagerView : MonoBehaviour
    {
        [SerializeField] private PlayerView _playerView;

        private const int MaxPrewarmedCapacity = 1500;
        private GemManager _domainManager;
        private readonly List<ExpGemView> _viewPool = new List<ExpGemView>(MaxPrewarmedCapacity);

        public GemManager DomainManager => _domainManager;

        public void Initialize(EventBus eventBus, PlayerView playerView = null)
        {
            _playerView = playerView;
            _domainManager = new GemManager(eventBus, initialCapacity: MaxPrewarmedCapacity);
            _domainManager.OnGemSpawned += SpawnGemView;
            PrewarmViewPool(MaxPrewarmedCapacity);
        }

        private void PrewarmViewPool(int count)
        {
            if (_viewPool.Count > 0) return;

            var gemSprite = Utils.SpriteHelper.GetOrCreateGemSprite();

            for (int i = 0; i < count; i++)
            {
                CreateNewGemView(i + 1, gemSprite);
            }
        }

        private ExpGemView CreateNewGemView(int index, Sprite gemSprite)
        {
            var go = new GameObject($"GemView_{index}");
            go.transform.SetParent(transform, false);
            go.transform.localScale = Vector3.one * 0.70f;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = gemSprite;
            sr.sortingOrder = 3;

            var view = go.AddComponent<ExpGemView>();
            go.SetActive(false);
            _viewPool.Add(view);
            return view;
        }

        public void SetPlayerView(PlayerView playerView)
        {
            _playerView = playerView;
        }

        private void Update()
        {
            if (_domainManager == null || _playerView == null || _playerView.Entity == null || _playerView.Entity.IsDead)
                return;

            Vector2D playerPos = _playerView.Entity.Position;
            float pickupRadius = _playerView.Entity.Stats.PickupRadius;

            _domainManager.Update(playerPos, pickupRadius, Time.deltaTime);

            for (int i = 0; i < _viewPool.Count; i++)
            {
                var view = _viewPool[i];
                if (view.gameObject.activeSelf)
                {
                    view.UpdateView();
                }
            }
        }

        public void SpawnGemView(ExpGemEntity entity)
        {
            // 1. Search for inactive view in prewarmed pool
            for (int i = 0; i < _viewPool.Count; i++)
            {
                if (!_viewPool[i].gameObject.activeSelf)
                {
                    _viewPool[i].Bind(entity);
                    return;
                }
            }

            // 2. Dynamic expansion fallback: Never drop a gem visually!
            var gemSprite = Utils.SpriteHelper.GetOrCreateGemSprite();
            var newView = CreateNewGemView(_viewPool.Count + 1, gemSprite);
            newView.Bind(entity);
        }
    }
}
