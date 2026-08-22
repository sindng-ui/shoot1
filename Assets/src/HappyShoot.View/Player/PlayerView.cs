using UnityEngine;
using HappyShoot.Domain.Entities;
using HappyShoot.Domain.Events;
using HappyShoot.Domain.Projectiles;
using HappyShoot.Domain.Spatial;

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

        public void SetExternalSystems(Monsters.MonsterSpawnerView spawnerView, Projectiles.ProjectileManagerView projectileManagerView)
        {
            _spawnerView = spawnerView;
            _projectileManagerView = projectileManagerView;
        }

        private GameObject _bodyVisualGo;
        private SpriteRenderer _bodySr;
        private GameObject _slashPivotGo;
        private SpriteRenderer _slashVisualSr;
        private GameObject _swordGo;
        private SpriteRenderer _swordSr;
        private float _slashVisualTimer;
        private float _slashBaseAngle;
        private const float SlashDuration = 0.18f;

        private Projectiles.OrbitingBladeView _orbitingBladeView;
        private int _cachedOrbitalLevel = -1;
        private float _cachedOrbitalArea = -1f;

        private Vector3 _lastPos;
        private float _walkBobTimer;

        private void Awake()
        {
            // Create child BodyVisual to prevent overriding root transform.position
            _bodyVisualGo = new GameObject("BodyVisual");
            _bodyVisualGo.transform.SetParent(transform);
            _bodyVisualGo.transform.localPosition = Vector3.zero;
            _bodyVisualGo.transform.localScale = Vector3.one * 1.5f;

            _bodySr = _bodyVisualGo.AddComponent<SpriteRenderer>();
            _bodySr.sprite = Utils.SpriteHelper.GetOrCreateWarriorSprite();
            _bodySr.color = Color.white;
            _bodySr.sortingOrder = 0;
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
            _swordSr.sortingOrder = 2; // In front of body

            // Create slash pivot & arc visual child
            _slashPivotGo = new GameObject("SlashPivot");
            _slashPivotGo.transform.SetParent(transform);
            _slashPivotGo.transform.localPosition = Vector3.zero;

            var slashGo = new GameObject("SlashArc");
            slashGo.transform.SetParent(_slashPivotGo.transform);
            slashGo.transform.localPosition = new Vector3(0.8f, 0f, 0f);
            slashGo.transform.localScale = Vector3.one * 2.5f;

            _slashVisualSr = slashGo.AddComponent<SpriteRenderer>();
            _slashVisualSr.sprite = Utils.SpriteHelper.GetOrCreateSlashArcSprite();
            _slashVisualSr.color = new Color(1f, 0.95f, 0.35f, 0.95f);
            _slashVisualSr.sortingOrder = 3;
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

            // Handle walking bobbing & flip direction
            Vector3 deltaMove = transform.position - _lastPos;
            _lastPos = transform.position;

            if (deltaMove.sqrMagnitude > 0.0001f)
            {
                _walkBobTimer += Time.deltaTime * 12f;
                float bobOffset = Mathf.Sin(_walkBobTimer) * 0.04f;
                if (_bodyVisualGo != null)
                {
                    _bodyVisualGo.transform.localPosition = new Vector3(0f, bobOffset, 0f);
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
                if (_bodyVisualGo != null)
                {
                    _bodyVisualGo.transform.localPosition = Vector3.zero;
                }
            }

            // Handle dynamic swinging slash & weapon swing animation
            if (_slashVisualTimer > 0f)
            {
                _slashVisualTimer -= Time.deltaTime;
                float progress = Mathf.Clamp01(1.0f - (_slashVisualTimer / SlashDuration)); // 0.0 -> 1.0

                // Swing smoothly from -60 to +60 degrees
                float currentAngle = _slashBaseAngle + Mathf.Lerp(-60f, 60f, progress);
                if (_slashPivotGo != null)
                {
                    _slashPivotGo.transform.rotation = Quaternion.Euler(0f, 0f, currentAngle);
                }

                // Weapon follows swing arc during attack
                if (_swordGo != null && _swordGo.transform.parent != null)
                {
                    _swordGo.transform.parent.rotation = Quaternion.Euler(0f, 0f, currentAngle);
                }

                // Fade out smoothly
                if (_slashVisualSr != null)
                {
                    Color c = _slashVisualSr.color;
                    c.a = Mathf.Sin(progress * Mathf.PI) * 0.95f;
                    _slashVisualSr.color = c;
                }

                if (_slashVisualTimer <= 0f)
                {
                    if (_slashPivotGo != null) _slashPivotGo.SetActive(false);
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

                int currentLevel = orbitalSkill.Level;
                float currentArea = _entity.Stats.AreaMultiplier;

                if (_cachedOrbitalLevel != currentLevel || Mathf.Abs(_cachedOrbitalArea - currentArea) > 0.01f)
                {
                    _cachedOrbitalLevel = currentLevel;
                    _cachedOrbitalArea = currentArea;
                    int bladeCount = 2 + (currentLevel - 1);
                    float radius = 2.0f * currentArea;
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
            _slashVisualTimer = SlashDuration;
            if (_slashPivotGo != null)
            {
                _slashPivotGo.SetActive(true);
                float initialAngle = _slashBaseAngle - 60f;
                _slashPivotGo.transform.rotation = Quaternion.Euler(0f, 0f, initialAngle);
                if (_swordGo != null && _swordGo.transform.parent != null)
                {
                    _swordGo.transform.parent.rotation = Quaternion.Euler(0f, 0f, initialAngle);
                }

                // Dynamic scale of the slash arc visual based on slash skill level & Area multiplier
                int slashLevel = 1;
                if (_entity != null)
                {
                    var skills = _entity.Skills;
                    for (int i = 0; i < skills.Count; i++)
                    {
                        if (skills[i].Id == "slash") { slashLevel = skills[i].Level; break; }
                    }
                }
                float areaMult = _entity != null ? _entity.Stats.AreaMultiplier : 1.0f;
                float arcScale = (2.5f + 0.85f * (slashLevel - 1)) * areaMult;
                if (_slashVisualSr != null)
                {
                    _slashVisualSr.transform.localScale = Vector3.one * arcScale;
                    _slashVisualSr.transform.localPosition = new Vector3(arcScale * 0.35f, 0f, 0f);
                    Color c = _slashVisualSr.color;
                    c.a = 0.95f;
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
