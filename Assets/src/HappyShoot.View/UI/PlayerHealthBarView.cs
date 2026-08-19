using UnityEngine;
using HappyShoot.Domain.Events;
using HappyShoot.View.Player;
using HappyShoot.View.Utils;

namespace HappyShoot.View.UI
{
    /// <summary>
    /// Smooth floating mini health bar directly above the player's head.
    /// Built using lightweight SpriteRenderers with zero Canvas overhead.
    /// </summary>
    public class PlayerHealthBarView : MonoBehaviour
    {
        [SerializeField] private Vector3 _offset = new Vector3(0f, 0.75f, 0f);
        [SerializeField] private Vector2 _barSize = new Vector2(0.9f, 0.12f);

        private SpriteRenderer _backgroundSr;
        private SpriteRenderer _fillSr;
        private Transform _fillTransform;

        private float _targetFill = 1f;
        private float _currentFill = 1f;
        private EventBus _eventBus;

        public void Initialize(PlayerView playerView)
        {
            if (playerView == null) return;

            transform.SetParent(playerView.transform);
            transform.localPosition = _offset;
            transform.localScale = Vector3.one;

            CreateBarSprites();

            _eventBus = playerView.EventBus;
            if (_eventBus != null)
            {
                _eventBus.Subscribe<PlayerDamagedEvent>(OnPlayerDamaged);
                _eventBus.Subscribe<PlayerHealedEvent>(OnPlayerHealed);
            }

            if (playerView.Entity != null)
            {
                var stats = playerView.Entity.Stats;
                UpdateHealth(playerView.Entity.CurrentHealth, stats.MaxHealth, immediate: true);
            }
        }

        private void CreateBarSprites()
        {
            var bgGo = new GameObject("HpBg");
            bgGo.transform.SetParent(transform, false);
            bgGo.transform.localPosition = Vector3.zero;
            bgGo.transform.localScale = new Vector3(_barSize.x + 0.04f, _barSize.y + 0.04f, 1f);

            _backgroundSr = bgGo.AddComponent<SpriteRenderer>();
            _backgroundSr.sprite = SpriteHelper.GetOrCreateSquareSprite();
            _backgroundSr.color = new Color(0.1f, 0.1f, 0.12f, 0.85f);
            _backgroundSr.sortingOrder = 10;

            var fillPivot = new GameObject("HpFillPivot");
            fillPivot.transform.SetParent(transform, false);
            fillPivot.transform.localPosition = new Vector3(-_barSize.x * 0.5f, 0f, 0f); // Pivot on left

            var fillGo = new GameObject("HpFill");
            fillGo.transform.SetParent(fillPivot.transform, false);
            fillGo.transform.localPosition = new Vector3(_barSize.x * 0.5f, 0f, 0f);
            fillGo.transform.localScale = new Vector3(_barSize.x, _barSize.y, 1f);

            _fillSr = fillGo.AddComponent<SpriteRenderer>();
            _fillSr.sprite = SpriteHelper.GetOrCreateSquareSprite();
            _fillSr.color = new Color(0.2f, 0.85f, 0.35f, 0.95f);
            _fillSr.sortingOrder = 11;

            _fillTransform = fillPivot.transform;
        }

        private void Update()
        {
            if (_fillTransform != null && Mathf.Abs(_currentFill - _targetFill) > 0.001f)
            {
                _currentFill = Mathf.Lerp(_currentFill, _targetFill, Time.unscaledDeltaTime * 14f);
                _fillTransform.localScale = new Vector3(_currentFill, 1f, 1f);

                // Dynamically color: Green -> Yellow -> Red
                if (_fillSr != null)
                {
                    if (_currentFill > 0.5f)
                    {
                        _fillSr.color = Color.Lerp(Color.yellow, new Color(0.2f, 0.85f, 0.35f, 0.95f), (_currentFill - 0.5f) * 2f);
                    }
                    else
                    {
                        _fillSr.color = Color.Lerp(new Color(0.9f, 0.15f, 0.15f, 0.95f), Color.yellow, _currentFill * 2f);
                    }
                }
            }
        }

        private void OnPlayerDamaged(PlayerDamagedEvent evt)
        {
            UpdateHealth(evt.RemainingHealth, evt.MaxHealth);
        }

        private void OnPlayerHealed(PlayerHealedEvent evt)
        {
            UpdateHealth(evt.CurrentHealth, evt.MaxHealth);
        }

        private void UpdateHealth(float current, float max, bool immediate = false)
        {
            float fill = max > 0f ? Mathf.Clamp01(current / max) : 0f;
            _targetFill = fill;
            if (immediate)
            {
                _currentFill = fill;
                if (_fillTransform != null)
                {
                    _fillTransform.localScale = new Vector3(fill, 1f, 1f);
                }
            }
        }
    }
}
