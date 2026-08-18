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
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class ExpGemView : MonoBehaviour
    {
        private ExpGemEntity _entity;
        private SpriteRenderer _spriteRenderer;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        public void Bind(ExpGemEntity entity)
        {
            _entity = entity;
            transform.position = new Vector3(entity.Position.X, entity.Position.Y, 0f);

            if (_spriteRenderer.sprite == null)
            {
                _spriteRenderer.sprite = Utils.SpriteHelper.GetOrCreateGemSprite();
            }

            // Gem color tint based on value
            if (entity.ExpValue >= 20) _spriteRenderer.color = new Color(1.0f, 0.4f, 0.4f);
            else if (entity.ExpValue >= 5) _spriteRenderer.color = new Color(0.4f, 0.8f, 1.0f);
            else _spriteRenderer.color = Color.white;

            gameObject.SetActive(true);
        }

        public void UpdateView()
        {
            if (_entity == null || !_entity.IsActive)
            {
                gameObject.SetActive(false);
                return;
            }

            transform.position = new Vector3(_entity.Position.X, _entity.Position.Y, 0f);
        }
    }

    /// <summary>
    /// Synchronizes experience gem spawning and collection with the Unity scene.
    /// </summary>
    public class GemManagerView : MonoBehaviour
    {
        [SerializeField] private PlayerView _playerView;
        [SerializeField] private GameObject _gemPrefab;

        private GemManager _domainManager;
        private readonly List<ExpGemView> _viewPool = new List<ExpGemView>(256);

        public GemManager DomainManager => _domainManager;

        public void Initialize(EventBus eventBus, PlayerView playerView = null)
        {
            _playerView = playerView;
            _domainManager = new GemManager(eventBus, initialCapacity: 64);
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

            // Synchronize active gems with views
            var activeGems = _domainManager.ActiveGems;
            for (int i = 0; i < activeGems.Count; i++)
            {
                GetOrCreateView(activeGems[i]);
            }

            for (int i = 0; i < _viewPool.Count; i++)
            {
                if (_viewPool[i].gameObject.activeSelf)
                {
                    _viewPool[i].UpdateView();
                }
            }
        }

        private ExpGemView GetOrCreateView(ExpGemEntity entity)
        {
            for (int i = 0; i < _viewPool.Count; i++)
            {
                if (!_viewPool[i].gameObject.activeSelf)
                {
                    _viewPool[i].Bind(entity);
                    return _viewPool[i];
                }
            }

            GameObject go;
            if (_gemPrefab != null)
            {
                go = Instantiate(_gemPrefab, transform);
            }
            else
            {
                go = new GameObject($"GemView_{_viewPool.Count + 1}");
                go.transform.SetParent(transform);
                go.transform.localScale = Vector3.one * 1.0f;
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = Utils.SpriteHelper.GetOrCreateGemSprite();
                sr.color = Color.white;
            }

            var view = go.GetComponent<ExpGemView>() ?? go.AddComponent<ExpGemView>();
            view.Bind(entity);
            _viewPool.Add(view);
            return view;
        }
    }
}
