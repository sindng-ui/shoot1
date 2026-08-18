using System.Collections.Generic;
using UnityEngine;
using HappyShoot.Domain.Entities;
using HappyShoot.Domain.Projectiles;
using HappyShoot.Domain.Spatial;

namespace HappyShoot.View.Projectiles
{
    /// <summary>
    /// Unity MonoBehaviour representing a single projectile in the scene.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class ProjectileView : MonoBehaviour
    {
        private ProjectileEntity _entity;
        private SpriteRenderer _spriteRenderer;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        public void Bind(ProjectileEntity entity)
        {
            _entity = entity;
            transform.position = new Vector3(entity.Position.X, entity.Position.Y, 0f);

            // Rotate towards direction
            float angle = Mathf.Atan2(entity.Direction.Y, entity.Direction.X) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);

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
    /// Unity MonoBehaviour that synchronizes ProjectileManager domain updates and views.
    /// </summary>
    public class ProjectileManagerView : MonoBehaviour
    {
        [SerializeField] private GameObject _projectilePrefab;

        private ProjectileManager _domainManager;
        private readonly List<ProjectileView> _viewPool = new List<ProjectileView>(128);

        public ProjectileManager DomainManager => _domainManager;

        private void Awake()
        {
            _domainManager = new ProjectileManager(initialCapacity: 64);
        }

        public void UpdateProjectiles(float deltaTime, SpatialGrid2D<MonsterEntity> monsterGrid)
        {
            if (_domainManager == null) return;

            _domainManager.Update(deltaTime, monsterGrid);

            // Synchronize active views
            var activeDomainProjectiles = _domainManager.ActiveProjectiles;
            for (int i = 0; i < activeDomainProjectiles.Count; i++)
            {
                GetOrCreateView(activeDomainProjectiles[i]);
            }

            for (int i = 0; i < _viewPool.Count; i++)
            {
                if (_viewPool[i].gameObject.activeSelf)
                {
                    _viewPool[i].UpdateView();
                }
            }
        }

        private ProjectileView GetOrCreateView(ProjectileEntity entity)
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
            if (_projectilePrefab != null)
            {
                go = Instantiate(_projectilePrefab, transform);
            }
            else
            {
                go = new GameObject($"ProjectileView_{_viewPool.Count + 1}");
                go.transform.SetParent(transform);
                go.transform.localScale = new Vector3(0.4f, 0.15f, 1f);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = Utils.SpriteHelper.GetOrCreateSquareSprite();
                sr.color = Color.yellow; // Default yellow bullet/arrow
            }

            var view = go.GetComponent<ProjectileView>() ?? go.AddComponent<ProjectileView>();
            view.Bind(entity);
            _viewPool.Add(view);
            return view;
        }
    }
}
