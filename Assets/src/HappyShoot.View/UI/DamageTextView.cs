using System.Collections.Generic;
using UnityEngine;
using HappyShoot.Domain.Events;
using HappyShoot.Domain.UI;

namespace HappyShoot.View.UI
{
    /// <summary>
    /// Lightweight 3D/2D TextMesh view for floating damage numbers.
    /// </summary>
    [RequireComponent(typeof(TextMesh))]
    public class DamageTextView : MonoBehaviour
    {
        private DamageTextEntity _entity;
        private TextMesh _textMesh;

        private void Awake()
        {
            _textMesh = GetComponent<TextMesh>();
            _textMesh.alignment = TextAlignment.Center;
            _textMesh.anchor = TextAnchor.MiddleCenter;
        }

        public void Bind(DamageTextEntity entity)
        {
            _entity = entity;
            transform.position = new Vector3(entity.Position.X, entity.Position.Y, -1f);

            _textMesh.text = entity.DamageValue.ToString("0");
            _textMesh.fontSize = entity.IsCritical ? 32 : 24;
            _textMesh.color = entity.IsCritical ? new Color(1f, 0.85f, 0.2f, 1f) : Color.white;

            gameObject.SetActive(true);
        }

        public void UpdateView()
        {
            if (_entity == null || !_entity.IsActive)
            {
                gameObject.SetActive(false);
                return;
            }

            transform.position = new Vector3(_entity.Position.X, _entity.Position.Y, -1f);

            Color c = _textMesh.color;
            c.a = _entity.Alpha;
            _textMesh.color = c;
        }
    }

    /// <summary>
    /// Synchronizes DamageTextManager with Unity scene view pool.
    /// </summary>
    public class DamageTextManagerView : MonoBehaviour
    {
        private DamageTextManager _domainManager;
        private readonly List<DamageTextView> _viewPool = new List<DamageTextView>(64);

        public DamageTextManager DomainManager => _domainManager;

        public void Initialize(EventBus eventBus)
        {
            _domainManager = new DamageTextManager(eventBus, initialCapacity: 32);
            _domainManager.OnTextSpawned += SpawnTextView;
        }

        private void Update()
        {
            if (_domainManager == null) return;

            _domainManager.Update(Time.deltaTime);

            for (int i = 0; i < _viewPool.Count; i++)
            {
                var view = _viewPool[i];
                if (view.gameObject.activeSelf)
                {
                    view.UpdateView();
                }
            }
        }

        public void SpawnTextView(DamageTextEntity entity)
        {
            if (!HappyShoot.Domain.Settings.GameSettings.ShowDamageText) return;

            for (int i = 0; i < _viewPool.Count; i++)
            {
                if (!_viewPool[i].gameObject.activeSelf)
                {
                    _viewPool[i].Bind(entity);
                    return;
                }
            }

            if (_viewPool.Count < 32)
            {
                var go = new GameObject($"DamageText_{_viewPool.Count + 1}");
                go.transform.SetParent(transform);
                go.transform.localScale = Vector3.one * 0.1f;

                var view = go.AddComponent<DamageTextView>();
                view.Bind(entity);
                _viewPool.Add(view);
            }
        }
    }
}
