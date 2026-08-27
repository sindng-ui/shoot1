using System.Collections.Generic;
using UnityEngine;
using HappyShoot.Domain.Events;
using HappyShoot.Domain.Gems;
using HappyShoot.Domain.Progression;
using HappyShoot.View.SkillTree;

namespace HappyShoot.View.Gems
{
    /// <summary>
    /// Unity MonoBehaviour representing a single dropped progression gem stone (Ruby, Emerald, Amethyst).
    /// Strictly modular and under 500 lines.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class GemStoneView : MonoBehaviour
    {
        private GemStoneEntity _entity;
        private SpriteRenderer _spriteRenderer;
        private Transform _transform;
        private float _bobTimer;

        private void Awake()
        {
            _transform = transform;
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _spriteRenderer.sortingOrder = 4;
        }

        public void Bind(GemStoneEntity entity)
        {
            _entity = entity;
            _transform.position = new Vector3(entity.Position.X, entity.Position.Y, 0f);
            _spriteRenderer.sprite = SkillTreeSpriteHelper.GetGemSprite(entity.GemType);
            _transform.localScale = Vector3.one * 0.90f;
            _bobTimer = (entity.Id % 10) * 0.5f;
            gameObject.SetActive(true);
        }

        public void UpdateView(float deltaTime)
        {
            if (_entity == null || !_entity.IsActive)
            {
                gameObject.SetActive(false);
                return;
            }

            _bobTimer += deltaTime * 4f;
            float scaleBob = 0.90f + (Mathf.Sin(_bobTimer) * 0.08f);
            _transform.localScale = Vector3.one * scaleBob;
            _transform.position = new Vector3(_entity.Position.X, _entity.Position.Y, 0f);
        }
    }

    /// <summary>
    /// Manages visual synchronization and pooling for all dropped permanent progression gem stones.
    /// Strictly modular and under 500 lines.
    /// </summary>
    public class GemStoneManagerView : MonoBehaviour
    {
        private GemManager _gemManager;
        private readonly List<GemStoneView> _pool = new List<GemStoneView>(64);

        public void Initialize(GemManager gemManager)
        {
            _gemManager = gemManager;
            if (_gemManager != null)
            {
                _gemManager.OnGemStoneSpawned += SpawnStoneView;
            }

            PrewarmPool(32);
        }

        private void OnDestroy()
        {
            if (_gemManager != null)
            {
                _gemManager.OnGemStoneSpawned -= SpawnStoneView;
            }
        }

        private void PrewarmPool(int count)
        {
            for (int i = 0; i < count; i++)
            {
                CreateNewView(i + 1);
            }
        }

        private GemStoneView CreateNewView(int index)
        {
            var go = new GameObject($"GemStoneView_{index}");
            go.transform.SetParent(transform, false);
            var view = go.AddComponent<GemStoneView>();
            go.SetActive(false);
            _pool.Add(view);
            return view;
        }

        public void SpawnStoneView(GemStoneEntity entity)
        {
            for (int i = 0; i < _pool.Count; i++)
            {
                if (!_pool[i].gameObject.activeSelf)
                {
                    _pool[i].Bind(entity);
                    return;
                }
            }

            var newView = CreateNewView(_pool.Count + 1);
            newView.Bind(entity);
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            for (int i = 0; i < _pool.Count; i++)
            {
                if (_pool[i].gameObject.activeSelf)
                {
                    _pool[i].UpdateView(dt);
                }
            }
        }
    }
}
