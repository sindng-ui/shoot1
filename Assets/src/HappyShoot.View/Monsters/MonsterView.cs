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
        private float _lastX;

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
            _lastX = entity.Position.X;
            if (_spriteRenderer != null) _spriteRenderer.flipX = false;

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
                        _spriteRenderer.sprite = BossSpriteHelper.GetOrCreateBoss1Sprite();
                        _baseScale = 3.2f;
                        if (_shadowGo != null)
                        {
                            _shadowGo.transform.localPosition = new Vector3(0f, -0.85f, 0f);
                            _shadowGo.transform.localScale = new Vector3(3.2f, 1.4f, 1f);
                        }
                        break;
                                        case MonsterType.Boss2:
                        _spriteRenderer.sprite = BossSpriteHelper.GetOrCreateBoss2Sprite();
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
                    case MonsterType.Wraith:
                        _spriteRenderer.sprite = Phase3MonsterSpriteHelper.GetOrCreateWraithSprite();
                        _baseScale = 1.4f;
                        if (_shadowGo != null)
                        {
                            _shadowGo.transform.localPosition = new Vector3(0f, -0.35f, 0f);
                            _shadowGo.transform.localScale = new Vector3(1.1f, 0.5f, 1f);
                        }
                        break;
                    case MonsterType.Necromancer:
                        _spriteRenderer.sprite = Phase3MonsterSpriteHelper.GetOrCreateNecromancerSprite();
                        _baseScale = 1.6f;
                        if (_shadowGo != null)
                        {
                            _shadowGo.transform.localPosition = new Vector3(0f, -0.40f, 0f);
                            _shadowGo.transform.localScale = new Vector3(1.3f, 0.6f, 1f);
                        }
                        break;
                    case MonsterType.Abomination:
                        _spriteRenderer.sprite = Phase3MonsterSpriteHelper.GetOrCreateAbominationSprite();
                        _baseScale = 2.3f;
                        if (_shadowGo != null)
                        {
                            _shadowGo.transform.localPosition = new Vector3(0f, -0.60f, 0f);
                            _shadowGo.transform.localScale = new Vector3(2.2f, 1.0f, 1f);
                        }
                        break;
                    case MonsterType.Reaper:
                        _spriteRenderer.sprite = Phase3MonsterSpriteHelper.GetOrCreateReaperSprite();
                        _baseScale = 1.8f;
                        if (_shadowGo != null)
                        {
                            _shadowGo.transform.localPosition = new Vector3(0f, -0.45f, 0f);
                            _shadowGo.transform.localScale = new Vector3(1.5f, 0.7f, 1f);
                        }
                        break;
                    case MonsterType.Boss3:
                        _spriteRenderer.sprite = BossSpriteHelper.GetOrCreateBoss3Sprite();
                        _baseScale = 3.6f;
                        if (_shadowGo != null)
                        {
                            _shadowGo.transform.localPosition = new Vector3(0f, -0.90f, 0f);
                            _shadowGo.transform.localScale = new Vector3(3.6f, 1.5f, 1f);
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

            // Handle horizontal direction flip based on each monster's native art orientation
            float deltaX = _entity.Position.X - _lastX;
            _lastX = _entity.Position.X;
            bool origRight = IsOriginalArtFacingRight();

            if (deltaX > 0.005f)
            {
                // Moving Right: face right
                if (_spriteRenderer != null) _spriteRenderer.flipX = !origRight;
            }
            else if (deltaX < -0.005f)
            {
                // Moving Left: face left
                if (_spriteRenderer != null) _spriteRenderer.flipX = origRight;
            }

            // Brotato-style Squash/Stretch Jelly Physics per Monster Type
            float dt = Time.deltaTime;
            _animTimer += dt * 8f;

            float squashX = 0f;
            float squashY = 0f;
            float tiltZ = 0f;

            if (_hurtJoltTimer > 0f)
            {
                _hurtJoltTimer -= dt;
                if (_isCritHit)
                {
                    // Stronger Critical Jolt & Violent Shake
                    squashX = 0.48f;
                    squashY = -0.36f;
                    tiltZ = Mathf.Sin(_animTimer * 24f) * 16f;
                }
                else
                {
                    // Crisp, elastic hit jolt & micro-shake for satisfying melee punch
                    squashX = 0.28f;
                    squashY = -0.22f;
                    tiltZ = Mathf.Sin(_animTimer * 20f) * 7f;
                }
            }
            else
            {
                _isCritHit = false;
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
                        // Skeleton trot: light jaunty bone swagger
                        float waddle = Mathf.Sin(_animTimer * 1.1f);
                        tiltZ = waddle * 4.0f;
                        squashY = Mathf.Abs(waddle) * 0.05f;
                        squashX = -squashY * 0.4f;
                        break;

                    case MonsterType.Golem:
                        // Heavy rock stomp: slow, massive step impact (no waddling!)
                        float golemStep = Mathf.Sin(_animTimer * 0.65f);
                        tiltZ = golemStep * 2.0f;
                        float stompImpact = Mathf.Max(0f, Mathf.Sin(_animTimer * 1.3f));
                        squashY = -stompImpact * 0.05f;
                        squashX = stompImpact * 0.04f;
                        break;

                    case MonsterType.Boss:
                    case MonsterType.Boss2:
                    case MonsterType.Boss3:
                        // Heavy breathing pulsation
                        float pulse = Mathf.Sin(_animTimer * 0.75f);
                        squashY = pulse * 0.09f;
                        squashX = -pulse * 0.06f;
                        break;
                    case MonsterType.FireImp:
                        // Fiery hovering float: smooth floating bobbing with gentle flame sway (no frantic shaking!)
                        float impBob = Mathf.Sin(_animTimer * 1.2f);
                        squashY = impBob * 0.06f;
                        squashX = -squashY * 0.5f;
                        tiltZ = Mathf.Sin(_animTimer * 0.9f) * 3.5f;
                        break;

                    case MonsterType.ToxicSpider:
                        // Ground-creeping crawl: subtle, firm low-stance scuttle without weird jelly stretching
                        float crawl = Mathf.Sin(_animTimer * 1.6f);
                        squashY = Mathf.Abs(crawl) * 0.03f;
                        squashX = -squashY * 0.3f;
                        tiltZ = crawl * 1.5f;
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

                        float visualScale = GetTuningVisualScale(_entity.Type);
            float finalScale = _baseScale * visualScale;
            _transform.localScale = new Vector3(finalScale * (1f + squashX), finalScale * (1f + squashY), 1f);
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

                /// <summary>
        /// Returns true if the native monster sprite art is facing Right.
        /// Right: Skeleton, Golem, FireImp.
        /// Left: DarkKnight, LichKing, ToxicSpider, VampireBat, Slime.
        /// </summary>
                private float GetTuningVisualScale(MonsterType type)
        {
            var cfg = Config.SkillConfigRepository.Instance?.GetConfig()?.Monsters;
            return cfg != null ? cfg.GetVisualScale(type) : 1.0f;
        }

        private bool IsOriginalArtFacingRight()
        {
            if (_entity == null) return false;
            return _entity.Type == MonsterType.Skeleton
                || _entity.Type == MonsterType.Golem
                || _entity.Type == MonsterType.FireImp;
        }

        private bool _isCritHit = false;

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
        /// Triggered when the monster takes damage. Flash with elasticity and shake.
        /// Critical hits trigger stronger squash/stretch, golden flash, and noticeable shake.
        /// </summary>
        public void OnHitFeedback(bool isCritical = false)
        {
            if (_spriteRenderer != null)
            {
                _isCritHit = isCritical;
                if (isCritical)
                {
                    // Golden Flash with prominent violent shake & deep squash
                    _spriteRenderer.color = new Color(1.0f, 0.95f, 0.35f, 1f);
                    _flashTimer = 0.14f;
                    _hurtJoltTimer = 0.14f;
                }
                else
                {
                    // Flash White with crisp micro-jolt
                    _spriteRenderer.color = _flashColor;
                    _flashTimer = _flashDuration;
                    _hurtJoltTimer = 0.06f;
                }
            }
        }
    }
}
