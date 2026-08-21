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
        private Transform _transform;
        private Color _originalColor;
        private float _flashTimer;

        public MonsterEntity Entity => _entity;

        private float _animTimer;
        private float _baseScale = 1.4f;

        private void Awake()
        {
            _transform = transform;
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _spriteRenderer.sprite = Utils.SpriteHelper.GetOrCreateSlimeSprite();
            _spriteRenderer.color = Color.white;
            _transform.localScale = Vector3.one * 1.4f;
            _originalColor = Color.white;
            _animTimer = Random.Range(0f, 10f);
        }

        public void Bind(MonsterEntity entity)
        {
            _entity = entity;
            _transform.position = new Vector3(entity.Position.X, entity.Position.Y, 0f);

            if (_spriteRenderer != null)
            {
                switch (entity.Type)
                {
                    case MonsterType.Bat:
                        _spriteRenderer.sprite = Utils.SpriteHelper.GetOrCreateBatSprite();
                        _baseScale = 1.2f;
                        break;
                    case MonsterType.Skeleton:
                        _spriteRenderer.sprite = Utils.SpriteHelper.GetOrCreateSkeletonSprite();
                        _baseScale = 1.4f;
                        break;
                    case MonsterType.Golem:
                        _spriteRenderer.sprite = Utils.SpriteHelper.GetOrCreateGolemSprite();
                        _baseScale = 2.0f;
                        break;
                    case MonsterType.Boss:
                        _spriteRenderer.sprite = Utils.SpriteHelper.GetOrCreateBossSprite();
                        _baseScale = 3.2f;
                        break;
                    case MonsterType.Slime:
                    default:
                        _spriteRenderer.sprite = Utils.SpriteHelper.GetOrCreateSlimeSprite();
                        _baseScale = 1.4f;
                        break;
                }
                _spriteRenderer.color = Color.white;
            }

            _transform.localScale = Vector3.one * _baseScale;
            gameObject.SetActive(true);
        }

        public void UpdateView()
        {
            if (_entity == null || !_entity.IsActive || _entity.IsDead)
            {
                gameObject.SetActive(false);
                return;
            }

            _transform.position = new Vector3(_entity.Position.X, _entity.Position.Y, 0f);

            // Boss & Golem squash animation only (keeps 500+ regular mob updates ultra lightweight)
            if (_entity.Type == MonsterType.Boss || _entity.Type == MonsterType.Golem)
            {
                _animTimer += Time.deltaTime * 6f;
                float squash = Mathf.Sin(_animTimer) * 0.08f;
                _transform.localScale = new Vector3(_baseScale * (1f + squash), _baseScale * (1f - squash), 1f);
            }

            if (_flashTimer > 0f)
            {
                _flashTimer -= Time.deltaTime;
                if (_flashTimer <= 0f && _spriteRenderer != null)
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
