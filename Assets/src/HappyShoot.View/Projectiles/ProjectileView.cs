using System.Collections.Generic;
using UnityEngine;
using HappyShoot.Domain.Entities;
using HappyShoot.Domain.Events;
using HappyShoot.Domain.Projectiles;
using HappyShoot.Domain.Spatial;

namespace HappyShoot.View.Projectiles
{
    /// <summary>
    /// Unity MonoBehaviour representing a single projectile in the scene.
    /// Strictly modular and under 500 lines.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class ProjectileView : MonoBehaviour
    {
        private ProjectileEntity _entity;
        private SpriteRenderer _spriteRenderer;
        private Transform _transform;

        private void Awake()
        {
            _transform = transform;
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        public void Bind(ProjectileEntity entity)
        {
            _entity = entity;
            _transform.position = new Vector3(entity.Position.X, entity.Position.Y, 0f);

            // Set rotation towards projectile direction
            float angle = Mathf.Atan2(entity.Direction.Y, entity.Direction.X) * Mathf.Rad2Deg;
            _transform.rotation = Quaternion.Euler(0f, 0f, angle);

            if (_spriteRenderer != null)
            {
                if (entity.HasExplosionOnHit)
                {
                    _spriteRenderer.color = new Color(0.2f, 1.0f, 0.95f, 1.0f); // Glowing Cyan Storm Arrow
                    _transform.localScale = new Vector3(0.55f, 0.20f, 1f);
                }
                else
                {
                    _spriteRenderer.color = entity.Damage >= 40f ? Color.cyan : (entity.RemainingPierce > 1 ? new Color(1f, 0.6f, 0.2f) : Color.yellow);
                    _transform.localScale = new Vector3(0.4f, 0.15f, 1f);
                }
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
    /// Synchronizes ProjectileManager with Unity scene view pool using 128 prewarmed zero-allocation pool.
    /// </summary>
    public class ProjectileManagerView : MonoBehaviour
    {
        [SerializeField] private GameObject _projectilePrefab;

        private const int MaxPoolCapacity = 128;
        private ProjectileManager _domainManager;
        private readonly List<ProjectileView> _viewPool = new List<ProjectileView>(MaxPoolCapacity);

        public ProjectileManager DomainManager => _domainManager;

        private void Awake()
        {
            _domainManager = new ProjectileManager(initialCapacity: MaxPoolCapacity);
            _domainManager.OnProjectileSpawned += SpawnProjectileView;
            PrewarmViewPool(MaxPoolCapacity);
        }

        public void Initialize(EventBus eventBus)
        {
            _domainManager?.SetEventBus(eventBus);
        }

        private void PrewarmViewPool(int count)
        {
            if (_viewPool.Count > 0) return;

            var squareSprite = Utils.SpriteHelper.GetOrCreateSquareSprite();

            for (int i = 0; i < count; i++)
            {
                var go = new GameObject($"ProjectileView_{i + 1}");
                go.transform.SetParent(transform, false);
                go.transform.localScale = new Vector3(0.4f, 0.15f, 1f);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = squareSprite;
                sr.color = Color.yellow;
                sr.sortingOrder = 4;

                var view = go.AddComponent<ProjectileView>();
                go.SetActive(false);
                _viewPool.Add(view);
            }
        }

        public void UpdateProjectiles(float deltaTime, SpatialGrid2D<MonsterEntity> monsterGrid)
        {
            if (_domainManager == null) return;

            _domainManager.Update(deltaTime, monsterGrid);

            for (int i = 0; i < _viewPool.Count; i++)
            {
                var view = _viewPool[i];
                if (view.gameObject.activeSelf)
                {
                    view.UpdateView();
                }
            }
        }

        public void SpawnProjectileView(ProjectileEntity entity)
        {
            for (int i = 0; i < _viewPool.Count; i++)
            {
                if (!_viewPool[i].gameObject.activeSelf)
                {
                    _viewPool[i].Bind(entity);
                    return;
                }
            }

            if (_viewPool.Count < MaxPoolCapacity)
            {
                var go = new GameObject($"ProjectileView_{_viewPool.Count + 1}");
                go.transform.SetParent(transform, false);
                go.transform.localScale = new Vector3(0.4f, 0.15f, 1f);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = Utils.SpriteHelper.GetOrCreateSquareSprite();
                sr.sortingOrder = 4;

                var view = go.AddComponent<ProjectileView>();
                view.Bind(entity);
                _viewPool.Add(view);
            }
        }
    }
}
