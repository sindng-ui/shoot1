using UnityEngine;
using HappyShoot.Domain.Entities;
using HappyShoot.Domain.Events;
using HappyShoot.Domain.Projectiles;
using HappyShoot.Domain.Spatial;
using HappyShoot.View.Cameras;

namespace HappyShoot.View.Player
{
    /// <summary>
    /// Unity MonoBehaviour View that binds to a pure C# PlayerEntity.
    /// Handles visual rendering, sprite position updates, and event animations without holding business logic.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class PlayerView : MonoBehaviour
    {
        [Header("Character Configuration")]
        [SerializeField] private CharacterClassType _classType = CharacterClassType.Warrior;

        [Header("Visual Feedback")]
        [SerializeField] private Color _flashDamageColor = Color.red;
        [SerializeField] private float _flashDuration = 0.1f;

        [Header("External Systems (Optional in Scene)")]
        [SerializeField] private Monsters.MonsterSpawnerView _spawnerView;
        [SerializeField] private Projectiles.ProjectileManagerView _projectileManagerView;

        private PlayerEntity _entity;
        private EventBus _eventBus;
        private SpriteRenderer _spriteRenderer;
        private Color _originalColor;
        private float _flashTimer;

        public PlayerEntity Entity => _entity;
        public EventBus EventBus => _eventBus;
        public Vector2 CurrentMoveDirection { get; private set; } = Vector2.zero;

        public void SetExternalSystems(Monsters.MonsterSpawnerView spawnerView, Projectiles.ProjectileManagerView projectileManagerView)
        {
            _spawnerView = spawnerView;
            _projectileManagerView = projectileManagerView;
        }

        private GameObject _shadowGo;
        private GameObject _bodyVisualGo;
        private SpriteRenderer _bodySr;
        private GameObject _slashPivotGo;
        private SpriteRenderer _slashVisualSr;
        private GameObject _swordGo;
        private SpriteRenderer _swordSr;
        private float _slashVisualTimer;
        private float _slashBaseAngle;
        private float _slashHalfArc = 75f;
        private const float SlashDuration = 0.18f;

        private Projectiles.OrbitingBladeView _orbitingBladeView;
        private int _cachedOrbitalLevel = -1;
        private float _cachedOrbitalArea = -1f;

        private Vector3 _lastPos;
        private float _walkBobTimer;

        private void Awake()
        {
            // 1. Create 2.5D Blob Shadow at feet
            _shadowGo = new GameObject("BlobShadow");
            _shadowGo.transform.SetParent(transform);
            _shadowGo.transform.localPosition = new Vector3(0f, -0.42f, 0f);
            _shadowGo.transform.localScale = new Vector3(1.5f, 0.75f, 1f);
            var shadowSr = _shadowGo.AddComponent<SpriteRenderer>();
            shadowSr.sprite = Utils.SpriteHelper.GetOrCreateBlobShadowSprite();
            shadowSr.sortingOrder = -1; // Directly on ground

            // 2. Create child BodyVisual to prevent overriding root transform.position
            _bodyVisualGo = new GameObject("BodyVisual");
            _bodyVisualGo.transform.SetParent(transform);
            _bodyVisualGo.transform.localPosition = Vector3.zero;
            _bodyVisualGo.transform.localScale = Vector3.one * 1.5f;

            _bodySr = _bodyVisualGo.AddComponent<SpriteRenderer>();
            _bodySr.sprite = Utils.SpriteHelper.GetOrCreateWarriorSprite();
            _bodySr.color = Color.white;
            _bodySr.sortingOrder = 15; // In front of monsters (sortingOrder 10)
            _originalColor = Color.white;

            // Also check if root has SpriteRenderer and clean it up
            var rootSr = GetComponent<SpriteRenderer>();
            if (rootSr != null)
            {
                rootSr.sprite = null;
            }

            // Create Greatsword weapon object held by player
            var swordPivot = new GameObject("SwordPivot");
            swordPivot.transform.SetParent(transform);
            swordPivot.transform.localPosition = new Vector3(0.25f, -0.05f, 0f);

            _swordGo = new GameObject("Greatsword");
            _swordGo.transform.SetParent(swordPivot.transform);
            _swordGo.transform.localPosition = Vector3.zero;
            _swordGo.transform.localScale = Vector3.one * 1.3f;

            _swordSr = _swordGo.AddComponent<SpriteRenderer>();
            _swordSr.sprite = Utils.SpriteHelper.GetOrCreateSwordSprite();
            _swordSr.sortingOrder = 16; // In front of body

            // Create slash pivot & arc visual child
            _slashPivotGo = new GameObject("SlashPivot");
            _slashPivotGo.transform.SetParent(transform);
            _slashPivotGo.transform.localPosition = Vector3.zero;

            var slashGo = new GameObject("SlashArc");
            slashGo.transform.SetParent(_slashPivotGo.transform);
            slashGo.transform.localPosition = Vector3.zero;
            slashGo.transform.localScale = Vector3.one;

            _slashVisualSr = slashGo.AddComponent<SpriteRenderer>();
            _slashVisualSr.sprite = Utils.SpriteHelper.GetOrCreateSlashArcSprite();
            _slashVisualSr.color = Color.white;
            _slashVisualSr.sortingOrder = 30; // On top of all monsters during attack
            _slashPivotGo.SetActive(false);

            // Create Orbiting Blades visual container
            var orbitGo = new GameObject("OrbitingBladesVisual");
            orbitGo.transform.SetParent(transform, false);
            orbitGo.transform.localPosition = Vector3.zero;
            _orbitingBladeView = orbitGo.AddComponent<Projectiles.OrbitingBladeView>();
            _orbitingBladeView.Initialize(transform, bladeCount: 2, orbitRadius: 2.0f);
            orbitGo.SetActive(false);

            // Initialize domain event bus and player entity
            _eventBus = new EventBus();
            Vector2D startPos = new Vector2D(transform.position.x, transform.position.y);
            _entity = PlayerClassFactory.CreatePlayer(1, _classType, startPos, _eventBus);
            ApplyClassVisuals();

            // Subscribe to domain events
            _eventBus.Subscribe<PlayerMovedEvent>(OnPlayerMoved);
            _eventBus.Subscribe<PlayerDamagedEvent>(OnPlayerDamaged);
            _eventBus.Subscribe<PlayerDiedEvent>(OnPlayerDied);
            _eventBus.Subscribe<PlayerSlashExecutedEvent>(OnPlayerSlashExecuted);
            _eventBus.Subscribe<BloodEaterExecutedEvent>(OnBloodEaterExecuted);

            _lastPos = transform.position;
        }

        public void SetClassType(CharacterClassType classType)
        {
            _classType = classType;
            Vector2D currentPos = _entity != null ? _entity.Position : new Vector2D(transform.position.x, transform.position.y);
            _entity = PlayerClassFactory.CreatePlayer(1, _classType, currentPos, _eventBus);
            ApplyClassVisuals();
        }

        private void ApplyClassVisuals()
        {
            if (_bodySr != null)
            {
                if (_classType == CharacterClassType.Wizard)
                    _bodySr.sprite = Utils.WizardSpriteHelper.GetOrCreateWizardSprite();
                else if (_classType == CharacterClassType.Ranger)
                    _bodySr.sprite = Utils.SpriteHelper.GetOrCreateRangerSprite();
                else
                    _bodySr.sprite = Utils.SpriteHelper.GetOrCreateWarriorSprite();
            }

            if (_swordSr != null)
            {
                if (_classType == CharacterClassType.Wizard)
                    _swordSr.sprite = Utils.WizardSpriteHelper.GetOrCreateStaffSprite();
                else if (_classType == CharacterClassType.Ranger)
                    _swordSr.sprite = Utils.SpriteHelper.GetOrCreateBowSprite();
                else
                    _swordSr.sprite = Utils.SpriteHelper.GetOrCreateSwordSprite();
            }
        }

        private void Update()
        {
            if (_entity == null || _entity.IsDead)
                return;

            // Handle damage flash timer
            if (_flashTimer > 0f)
            {
                _flashTimer -= Time.deltaTime;
                if (_flashTimer <= 0f && _bodySr != null)
                {
                    _bodySr.color = _originalColor;
                }
            }

            // Calculate aim direction towards mouse cursor
            if (Camera.main != null)
            {
                Vector3 mouseScreenPos = UnityEngine.InputSystem.Mouse.current != null 
                    ? (Vector3)UnityEngine.InputSystem.Mouse.current.position.ReadValue()
                    : Input.mousePosition;
                mouseScreenPos.z = -Camera.main.transform.position.z;
                Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);
                _entity.AimTargetPosition = new Vector2D(mouseWorldPos.x, mouseWorldPos.y);
                Vector2 dir = new Vector2(mouseWorldPos.x - transform.position.x, mouseWorldPos.y - transform.position.y);
                if (dir.sqrMagnitude > 0.001f)
                {
                    _entity.AimDirection = new Vector2D(dir.normalized.x, dir.normalized.y);
                }
            }

            // Update domain skills against monsters
            ISpatialGrid2D monsterGrid = _spawnerView != null ? _spawnerView.MonsterGrid : null;
            ProjectileManager projManager = _projectileManagerView != null ? _projectileManagerView.DomainManager : null;
            _entity.Update(Time.deltaTime, monsterGrid, projManager);

            // Handle Brotato-style walking jelly physics, tilting & 2.5D shadow scale
            Vector3 deltaMove = transform.position - _lastPos;
            _lastPos = transform.position;
            float moveDist = deltaMove.magnitude;

            if (moveDist > 0.0001f)
            {
                CurrentMoveDirection = new Vector2(deltaMove.x, deltaMove.y).normalized;
                _walkBobTimer += Time.deltaTime * 16f;
                float hop = Mathf.Abs(Mathf.Sin(_walkBobTimer)) * 0.12f;
                float squashY = Mathf.Sin(_walkBobTimer * 2f) * 0.08f;
                float stretchX = -squashY * 0.5f;
                float tiltZ = Mathf.Clamp(-deltaMove.x * 35f, -9f, 9f);

                if (_bodyVisualGo != null)
                {
                    _bodyVisualGo.transform.localPosition = new Vector3(0f, hop, 0f);
                    _bodyVisualGo.transform.localScale = new Vector3(1.5f * (1f + stretchX), 1.5f * (1f + squashY), 1f);
                    _bodyVisualGo.transform.localRotation = Quaternion.Euler(0f, 0f, tiltZ);
                }

                if (_shadowGo != null)
                {
                    float shadowScale = 1f - (hop * 0.6f);
                    _shadowGo.transform.localScale = new Vector3(1.5f * shadowScale, 0.75f * shadowScale, 1f);
                }

                if (deltaMove.x > 0.001f && _bodySr != null)
                {
                    _bodySr.flipX = false;
                    if (_swordGo != null && _swordGo.transform.parent != null)
                    {
                        _swordGo.transform.parent.localPosition = new Vector3(0.25f, -0.05f, 0f);
                    }
                }
                else if (deltaMove.x < -0.001f && _bodySr != null)
                {
                    _bodySr.flipX = true;
                    if (_swordGo != null && _swordGo.transform.parent != null)
                    {
                        _swordGo.transform.parent.localPosition = new Vector3(-0.25f, -0.05f, 0f);
                    }
                }
            }
            else
            {
                CurrentMoveDirection = Vector2.zero;
                // Idle breathing jelly pulse
                _walkBobTimer += Time.deltaTime * 3.5f;
                float breatheY = Mathf.Sin(_walkBobTimer) * 0.035f;
                if (_bodyVisualGo != null)
                {
                    _bodyVisualGo.transform.localPosition = Vector3.zero;
                    _bodyVisualGo.transform.localScale = new Vector3(1.5f * (1f - breatheY * 0.5f), 1.5f * (1f + breatheY), 1f);
                    _bodyVisualGo.transform.localRotation = Quaternion.identity;
                }
                if (_shadowGo != null)
                {
                    _shadowGo.transform.localScale = new Vector3(1.5f, 0.75f, 1f);
                }
            }

            // Handle dynamic swinging slash & weapon swing animation
            if (_slashVisualTimer > 0f)
            {
                _slashVisualTimer -= Time.deltaTime;
                float progress = Mathf.Clamp01(1.0f - (_slashVisualTimer / SlashDuration)); // 0.0 -> 1.0

                // Swing smoothly through the full dynamic arc (-halfArc to +halfArc degrees)
                float currentAngle = _slashBaseAngle + Mathf.Lerp(-_slashHalfArc, _slashHalfArc, Mathf.SmoothStep(0f, 1f, progress));
                if (_slashPivotGo != null)
                {
                    _slashPivotGo.transform.rotation = Quaternion.Euler(0f, 0f, currentAngle);
                }

                // Weapon follows swing arc during attack
                if (_swordGo != null && _swordGo.transform.parent != null)
                {
                    _swordGo.transform.parent.rotation = Quaternion.Euler(0f, 0f, currentAngle);
                }

                // Smooth energetic fade out
                if (_slashVisualSr != null)
                {
                    Color c = _slashVisualSr.color;
                    c.a = Mathf.Sin(progress * Mathf.PI) * 0.95f;
                    _slashVisualSr.color = c;
                }

                if (_slashVisualTimer <= 0f)
                {
                    if (_slashPivotGo != null) _slashPivotGo.SetActive(false);
                    if (_swordSr != null) _swordSr.sortingOrder = 16;
                    // Reset sword idle resting angle
                    if (_swordGo != null && _swordGo.transform.parent != null)
                    {
                        bool isFlipped = _bodySr != null && _bodySr.flipX;
                        _swordGo.transform.parent.rotation = Quaternion.Euler(0f, 0f, isFlipped ? 135f : -45f);
                    }
                }
            }
            else
            {
                // Idle resting sword angle
                if (_swordGo != null && _swordGo.transform.parent != null)
                {
                    bool isFlipped = _bodySr != null && _bodySr.flipX;
                    _swordGo.transform.parent.rotation = Quaternion.Euler(0f, 0f, isFlipped ? 135f : -45f);
                }
            }

            // Update projectile views if present
            if (_projectileManagerView != null && _spawnerView != null)
            {
                _projectileManagerView.UpdateProjectiles(Time.deltaTime, _spawnerView.MonsterGrid);
            }

            // Synchronize Orbiting Blades visibility and blade count with domain skill state
            UpdateOrbitalBladesVisual();
        }

        private void UpdateOrbitalBladesVisual()
        {
            if (_entity == null || _orbitingBladeView == null) return;

            HappyShoot.Domain.Skills.ISkill orbitalSkill = null;
            var skills = _entity.Skills;
            for (int i = 0; i < skills.Count; i++)
            {
                if (skills[i].Id == "orbital")
                {
                    orbitalSkill = skills[i];
                    break;
                }
            }

            if (orbitalSkill != null)
            {
                if (!_orbitingBladeView.gameObject.activeSelf)
                {
                    _orbitingBladeView.gameObject.SetActive(true);
                }

                var orbitalEffect = (orbitalSkill as HappyShoot.Domain.Skills.CompositeSkill)?.Effect as HappyShoot.Domain.Skills.Effects.OrbitingBladesEffect;
                int bladeCount = orbitalEffect != null ? orbitalEffect.BladeCount : (2 + (orbitalSkill.Level - 1));
                float radius = (orbitalEffect != null ? orbitalEffect.OrbitRadius : 2.0f) * _entity.Stats.AreaMultiplier;

                if (_cachedOrbitalLevel != bladeCount || Mathf.Abs(_cachedOrbitalArea - radius) > 0.01f)
                {
                    _cachedOrbitalLevel = bladeCount;
                    _cachedOrbitalArea = radius;
                    _orbitingBladeView.SetBlades(bladeCount, radius);
                }
            }
            else
            {
                if (_orbitingBladeView.gameObject.activeSelf)
                {
                    _orbitingBladeView.gameObject.SetActive(false);
                }
            }
        }

        private void OnPlayerSlashExecuted(PlayerSlashExecutedEvent evt)
        {
            _slashBaseAngle = evt.DirectionAngleDegrees;
            _slashHalfArc = Mathf.Max(15f, evt.ArcAngleDegrees * 0.5f);
            _slashVisualTimer = SlashDuration;
            CameraFollowView.Instance?.TriggerShake("slash", duration: 0.12f, intensity: 0.18f);
            if (_slashVisualSr != null)
            {
                _slashVisualSr.sprite = Utils.SpriteHelper.GetOrCreateSlashArcSprite();
            }
            if (_swordSr != null)
            {
                _swordSr.color = Color.white;
                _swordSr.sortingOrder = 30; // Bring sword on top of monsters during swing
            }
            if (_slashPivotGo != null)
            {
                _slashPivotGo.SetActive(true);
                float initialAngle = _slashBaseAngle - _slashHalfArc;
                _slashPivotGo.transform.rotation = Quaternion.Euler(0f, 0f, initialAngle);
                if (_swordGo != null && _swordGo.transform.parent != null)
                {
                    _swordGo.transform.parent.rotation = Quaternion.Euler(0f, 0f, initialAngle);
                }

                // 128px sprite with maxRadius = 112.64px at 32 PPU = 3.52 units base reach.
                // Scale so that visual blade arc edge exactly matches domain effective radius.
                float baseRadius = 3.52f;
                float arcScale = Mathf.Max(0.5f, evt.Radius / baseRadius);
                if (_slashVisualSr != null)
                {
                    _slashVisualSr.transform.localScale = Vector3.one * arcScale;
                    _slashVisualSr.transform.localPosition = Vector3.zero;
                    Color c = Color.white;
                    c.a = 1.0f;
                    _slashVisualSr.color = c;
                }
            }
        }

        private void OnBloodEaterExecuted(BloodEaterExecutedEvent evt)
        {
            _slashBaseAngle = evt.DirectionAngleDegrees;
            _slashHalfArc = Mathf.Max(15f, evt.ArcAngleDegrees * 0.5f);
            _slashVisualTimer = SlashDuration;
            if (_slashVisualSr != null)
            {
                _slashVisualSr.sprite = Utils.SpriteHelper.GetOrCreateBloodSlashArcSprite();
            }
            if (_swordSr != null)
            {
                _swordSr.color = new Color(1.0f, 0.35f, 0.45f, 1f); // Glowing ruby blade
                _swordSr.sortingOrder = 30;
            }
            if (_slashPivotGo != null)
            {
                _slashPivotGo.SetActive(true);
                float initialAngle = _slashBaseAngle - _slashHalfArc;
                _slashPivotGo.transform.rotation = Quaternion.Euler(0f, 0f, initialAngle);
                if (_swordGo != null && _swordGo.transform.parent != null)
                {
                    _swordGo.transform.parent.rotation = Quaternion.Euler(0f, 0f, initialAngle);
                }

                float baseRadius = 3.52f;
                float arcScale = Mathf.Max(0.5f, evt.Radius / baseRadius);
                if (_slashVisualSr != null)
                {
                    _slashVisualSr.transform.localScale = Vector3.one * arcScale;
                    _slashVisualSr.transform.localPosition = Vector3.zero;
                    Color c = Color.white;
                    c.a = 1.0f;
                    _slashVisualSr.color = c;
                }
            }
        }

        private void OnPlayerMoved(PlayerMovedEvent evt)
        {
            transform.position = new Vector3(evt.Position.X, evt.Position.Y, transform.position.z);
        }

        private void OnPlayerDamaged(PlayerDamagedEvent evt)
        {
            if (_bodySr != null) _bodySr.color = _flashDamageColor;
            _flashTimer = _flashDuration;
        }

        private void OnPlayerDied(PlayerDiedEvent evt)
        {
            if (_bodySr != null) _bodySr.color = Color.gray;
            Debug.Log($"[PlayerView] Player has died!");
        }

        private void OnDestroy()
        {
            _eventBus?.Clear();
        }
    }
}
