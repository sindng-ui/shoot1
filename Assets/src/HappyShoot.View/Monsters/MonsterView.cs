using UnityEngine;
using HappyShoot.Domain.Entities;
using HappyShoot.Domain.Events;

namespace HappyShoot.View.Monsters
{
    /// <summary>
    /// Unity MonoBehaviour representing a single monster in the scene.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class MonsterView : MonoBehaviour
    {
        [Header("Visual Feedback")]
        [SerializeField] private Color _flashColor = Color.white;
        [SerializeField] private float _flashDuration = 0.08f;

        private MonsterEntity _entity;
        private SpriteRenderer _spriteRenderer;
        private Color _originalColor;
        private float _flashTimer;

        public MonsterEntity Entity => _entity;

        private float _animTimer;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _spriteRenderer.sprite = Utils.SpriteHelper.GetOrCreateSlimeSprite();
            _spriteRenderer.color = Color.white;
            transform.localScale = Vector3.one * 1.4f;
            _originalColor = Color.white;
            _animTimer = Random.Range(0f, 10f);
        }

        public void Bind(MonsterEntity entity)
        {
            _entity = entity;
            transform.position = new Vector3(entity.Position.X, entity.Position.Y, 0f);
            gameObject.SetActive(true);
        }

        public void UpdateView()
        {
            if (_entity == null || !_entity.IsActive || _entity.IsDead)
            {
                gameObject.SetActive(false);
                return;
            }

            transform.position = new Vector3(_entity.Position.X, _entity.Position.Y, 0f);

            // Cute jelly squash & stretch bounce
            _animTimer += Time.deltaTime * 6f;
            float squash = Mathf.Sin(_animTimer) * 0.12f;
            transform.localScale = new Vector3(1.4f * (1f + squash), 1.4f * (1f - squash), 1f);

            if (_flashTimer > 0f)
            {
                _flashTimer -= Time.deltaTime;
                if (_flashTimer <= 0f)
                {
                    _spriteRenderer.color = _originalColor;
                }
            }
        }

        public void OnHitFeedback()
        {
            _spriteRenderer.color = _flashColor;
            _flashTimer = _flashDuration;
        }
    }
}
