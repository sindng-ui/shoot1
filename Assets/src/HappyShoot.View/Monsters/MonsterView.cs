using UnityEngine;
using HappyShoot.Domain.Entities;
using HappyShoot.View.Cameras;
using HappyShoot.View.Utils;

namespace HappyShoot.View.Monsters
{
    /// <summary>
    /// Unity MonoBehaviour representing a single monster in the scene.
    /// Enhanced with Brotato-style 2.5D blob shadow, squash/stretch bouncy jelly physics,
    /// and crisp Flash White / Hit-Stop juice feedback.
    /// Strictly modular and under 500 lines.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class MonsterView : MonoBehaviour
    {
        [Header("Visual Feedback")]
        [SerializeField] private Color _flashColor = new Color(2.0f, 2.0f, 2.0f, 1.0f); // Bright Flash White
        [SerializeField] private float _flashDuration = 0.07f;

        private MonsterEntity _entity;
        private SpriteRenderer _spriteRenderer;
        private Transform _transform;
        private Color _originalColor;
        private float _flashTimer;
        private float _hurtJoltTimer;

        private GameObject _shadowGo;
        private SpriteRenderer _shadowSr;

        public MonsterEntity Entity => _entity;

        private float _animTimer;
        private float _baseScale = 1.4f;

        private void Awake()
        {
            _transform = transform;
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _spriteRenderer.sprite = SpriteHelper.GetOrCreateSlimeSprite();
            _spriteRenderer.color = Color.white;
            _spriteRenderer.sortingOrder = 10;
            _originalColor = Color.white;
            _transform.localScale = Vector3.one * 1.4f;
            _animTimer = Random.Range(0f, 10f);

            // Create 2.5D Blob Shadow child
            _shadowGo = new GameObject("BlobShadow");
            _shadowGo.transform.SetParent(_transform, false);
            _shadowGo.transform.localPosition = new Vector3(0f, -0.32f, 0f);
            _shadowGo.transform.localScale = new Vector3(1.2f, 0.6f, 1f);

            _shadowSr = _shadowGo.AddComponent<SpriteRenderer>();
            _shadowSr.sprite = SpriteHelper.GetOrCreateBlobShadowSprite();
            _shadowSr.sortingOrder = 9; // Directly under monster
        }

        public void Bind(MonsterEntity entity)
        {
            _entity = entity;
            _transform.position = new Vector3(entity.Position.X, entity.Position.Y, 0f);
            _hurtJoltTimer = 0f;

            if (_spriteRenderer != null)
            {
                switch (entity.Type)
                {
                    case MonsterType.Bat:
                        _spriteRenderer.sprite = SpriteHelper.GetOrCreateBatSprite();
                        _baseScale = 1.2f;
                        if (_shadowGo != null)
                        {
                            _shadowGo.transform.localPosition = new Vector3(0f, -0.45f, 0f);
                            _shadowGo.transform.localScale = new Vector3(0.9f, 0.45f, 1f);
                        }
                        break;
                    case MonsterType.Skeleton:
                        _spriteRenderer.sprite = SpriteHelper.GetOrCreateSkeletonSprite();
                        _baseScale = 1.4f;
                        if (_shadowGo != null)
                        {
                            _shadowGo.transform.localPosition = new Vector3(0f, -0.36f, 0f);
                            _shadowGo.transform.localScale = new Vector3(1.2f, 0.6f, 1f);
                        }
                        break;
                    case MonsterType.Golem:
                        _spriteRenderer.sprite = SpriteHelper.GetOrCreateGolemSprite();
                        _baseScale = 2.0f;
                        if (_shadowGo != null)
                        {
                            _shadowGo.transform.localPosition = new Vector3(0f, -0.55f, 0f);
                            _shadowGo.transform.localScale = new Vector3(2.0f, 0.95f, 1f);
                        }
                        break;
                    case MonsterType.Boss:
                        _spriteRenderer.sprite = SpriteHelper.GetOrCreateBossSprite();
                        _baseScale = 3.2f;
                        if (_shadowGo != null)
                        {
                            _shadowGo.transform.localPosition = new Vector3(0f, -0.85f, 0f);
                            _shadowGo.transform.localScale = new Vector3(3.2f, 1.4f, 1f);
                        }
                        break;
                    case MonsterType.FireImp:
                        _spriteRenderer.sprite = SpriteHelper.GetOrCreateFireImpSprite();
                        _baseScale = 1.5f;
                        if (_shadowGo != null)
                        {
                            _shadowGo.transform.localPosition = new Vector3(0f, -0.35f, 0f);
                            _shadowGo.transform.localScale = new Vector3(1.1f, 0.55f, 1f);
                        }
                        break;
                    case MonsterType.ToxicSpider:
                        _spriteRenderer.sprite = SpriteHelper.GetOrCreateToxicSpiderSprite();
                        _baseScale = 1.8f;
                        if (_shadowGo != null)
                        {
                            _shadowGo.transform.localPosition = new Vector3(0f, -0.40f, 0f);
                            _shadowGo.transform.localScale = new Vector3(2.0f, 0.80f, 1f);
                        }
                        break;
                    case MonsterType.DarkKnight:
                        _spriteRenderer.sprite = SpriteHelper.GetOrCreateDarkKnightSprite();
                        _baseScale = 2.4f;
                        if (_shadowGo != null)
                        {
                            _shadowGo.transform.localPosition = new Vector3(0f, -0.60f, 0f);
                            _shadowGo.transform.localScale = new Vector3(2.2f, 1.0f, 1f);
                        }
                        break;
                    case MonsterType.Slime:
                    default:
                        _spriteRenderer.sprite = SpriteHelper.GetOrCreateSlimeSprite();
                        _baseScale = 1.4f;
                        if (_shadowGo != null)
                        {
                            _shadowGo.transform.localPosition = new Vector3(0f, -0.32f, 0f);
                            _shadowGo.transform.localScale = new Vector3(1.2f, 0.6f, 1f);
                        }
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

            // Brotato-style Squash/Stretch Jelly Physics per Monster Type
            float dt = Time.deltaTime;
            _animTimer += dt * 8f;

            float squashX = 0f;
            float squashY = 0f;
            float tiltZ = 0f;

            if (_hurtJoltTimer > 0f)
            {
                _hurtJoltTimer -= dt;
                // Jolt elasticity on hit
                squashX = 0.22f;
                squashY = -0.18f;
            }
            else
            {
                switch (_entity.Type)
                {
                    case MonsterType.Slime:
                        // Bouncy hop
                        float hop = Mathf.Abs(Mathf.Sin(_animTimer * 1.4f));
                        squashY = (hop - 0.5f) * 0.16f;
                        squashX = -squashY * 0.7f;
                        break;

                    case MonsterType.Bat:
                        // Floating wing flutter
                        float flutter = Mathf.Sin(_animTimer * 2.2f);
                        squashY = flutter * 0.12f;
                        squashX = -squashY * 0.5f;
                        tiltZ = flutter * 6f;
                        break;

                    case MonsterType.Skeleton:
                    case MonsterType.Golem:
                        // Waddling trot
                        float waddle = Mathf.Sin(_animTimer);
                        tiltZ = waddle * 5.5f;
                        squashY = Mathf.Abs(waddle) * 0.08f;
                        squashX = -squashY * 0.5f;
                        break;

                    case MonsterType.Boss:
                        // Heavy breathing pulsation
                        float pulse = Mathf.Sin(_animTimer * 0.75f);
                        squashY = pulse * 0.09f;
                        squashX = -pulse * 0.06f;
                        break;

                    case MonsterType.FireImp:
                        // Frantic dart - fast erratic tilt
                        float impDart = Mathf.Sin(_animTimer * 3.5f);
                        tiltZ = impDart * 12f;
                        squashY = impDart * 0.10f;
                        squashX = -squashY * 0.8f;
                        break;

                    case MonsterType.ToxicSpider:
                        // Creepy crawl - multi-leg scuttle
                        float scuttle = Mathf.Sin(_animTimer * 2.8f);
                        squashY = Mathf.Abs(scuttle) * 0.12f;
                        squashX = -scuttle * 0.18f;
                        tiltZ = scuttle * 4f;
                        break;

                    case MonsterType.DarkKnight:
                        // Heavy stomp march
                        float stomp = Mathf.Sin(_animTimer * 1.2f);
                        squashY = Mathf.Abs(stomp) * 0.06f;
                        squashX = -squashY * 0.4f;
                        tiltZ = stomp * 3f;
                        break;
                }
            }

            _transform.localScale = new Vector3(_baseScale * (1f + squashX), _baseScale * (1f + squashY), 1f);
            _transform.localRotation = Quaternion.Euler(0f, 0f, tiltZ);

            if (_flashTimer > 0f)
            {
                _flashTimer -= dt;
                if (_flashTimer <= 0f && _spriteRenderer != null)
                {
                    UpdateStatusTint();
                }
            }
            else
            {
                UpdateStatusTint();
            }
        }

        private void UpdateStatusTint()
        {
            if (_spriteRenderer == null || _entity == null) return;

            if (_entity.IsChilled)
            {
                _spriteRenderer.color = new Color(0.45f, 0.85f, 1.0f); // Icy blue tint
            }
            else if (_entity.IsBurning)
            {
                _spriteRenderer.color = new Color(1.0f, 0.55f, 0.35f); // Burning flame tint
            }
            else if (_entity.IsShocked)
            {
                _spriteRenderer.color = new Color(1.0f, 0.95f, 0.45f); // Electric yellow tint
            }
            else
            {
                _spriteRenderer.color = _originalColor;
            }
        }

        /// <summary>
        /// Triggered when the monster takes damage. Flash white with lightweight jolt.
        /// Zero time-scale disruption so rapid continuous AoE skills (e.g. Orbital Blades) stay silky smooth.
        /// </summary>
        public void OnHitFeedback()
        {
            if (_spriteRenderer != null)
            {
                // Flash White with crisp micro-jolt
                _spriteRenderer.color = _flashColor;
                _flashTimer = _flashDuration;
                _hurtJoltTimer = 0.06f;
            }
        }
    }
}
