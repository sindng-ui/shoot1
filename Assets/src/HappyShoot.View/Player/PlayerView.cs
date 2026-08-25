using UnityEngine;
using HappyShoot.Domain.Entities;
using HappyShoot.Domain.Events;
using HappyShoot.Domain.Projectiles;
using HappyShoot.Domain.Spatial;
using HappyShoot.View.Cameras;
using HappyShoot.View.Utils;

namespace HappyShoot.View.Player
{
    /// <summary>
    /// Unity MonoBehaviour View for Player characters.
    /// Features: High-definition 64x64 cel-shaded 8-directional sprites, mouse aim tracking,
    /// dynamic weapon pivoting, Brotato squash/stretch physics, and event-driven animation.
    /// Strictly modular and under 400 lines (500-line architecture rule).
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
        private Color _originalColor = Color.white;
        private float _flashTimer;

        public PlayerEntity Entity => _entity;
        public EventBus EventBus => _eventBus;
        public Vector2 CurrentMoveDirection { get; private set; } = Vector2.zero;
        public bool IsGameStarted { get; set; } = false;

        private GameObject _shadowGo;
        private GameObject _bodyVisualGo;
        private SpriteRenderer _bodySr;
        private GameObject _weaponPivotGo;
        private GameObject _weaponGo;
        private SpriteRenderer _weaponSr;
        private GameObject _slashPivotGo;
        private SpriteRenderer _slashVisualSr;

        private float _slashVisualTimer;
        private float _slashBaseAngle;
        private float _slashHalfArc = 75f;
        private const float SlashDuration = 0.18f;

        private Projectiles.OrbitingBladeView _orbitingBladeView;
        private int _cachedOrbitalLevel = -1;
        private float _cachedOrbitalArea = -1f;

        private Vector3 _lastPos;
        private float _walkBobTimer;
        private HeroSpriteHelper.ViewDirection _currentViewDir = HeroSpriteHelper.ViewDirection.Front;

        public void SetExternalSystems(Monsters.MonsterSpawnerView spawnerView, Projectiles.ProjectileManagerView projectileManagerView)
        {
            _spawnerView = spawnerView;
            _projectileManagerView = projectileManagerView;
        }

        private void Awake()
        {
            // 1. 2.5D Blob Shadow at feet
            _shadowGo = new GameObject("BlobShadow");
            _shadowGo.transform.SetParent(transform, false);
            _shadowGo.transform.localPosition = new Vector3(0f, -0.42f, 0f);
            _shadowGo.transform.localScale = new Vector3(1.6f, 0.8f, 1f);
            var shadowSr = _shadowGo.AddComponent<SpriteRenderer>();
            shadowSr.sprite = SpriteHelper.GetOrCreateBlobShadowSprite();
            shadowSr.sortingOrder = -1;

            // 2. Child BodyVisual (Cute Chibi Hero with enhanced polish)
            _bodyVisualGo = new GameObject("BodyVisual");
            _bodyVisualGo.transform.SetParent(transform, false);
            _bodyVisualGo.transform.localPosition = Vector3.zero;
            _bodyVisualGo.transform.localScale = Vector3.one * 1.5f;

            _bodySr = _bodyVisualGo.AddComponent<SpriteRenderer>();
            _bodySr.sortingOrder = 15;
            _bodySr.color = Color.white;

            var rootSr = GetComponent<SpriteRenderer>();
            if (rootSr != null) rootSr.sprite = null;

            // 3. Dynamic Aim-tracking Weapon Pivot
            _weaponPivotGo = new GameObject("WeaponPivot");
            _weaponPivotGo.transform.SetParent(transform, false);
            _weaponPivotGo.transform.localPosition = new Vector3(0.25f, -0.05f, 0f);

            _weaponGo = new GameObject("WeaponVisual");
            _weaponGo.transform.SetParent(_weaponPivotGo.transform, false);
            _weaponGo.transform.localPosition = new Vector3(0.28f, 0f, 0f);
            _weaponGo.transform.localScale = Vector3.one * 1.3f;

            _weaponSr = _weaponGo.AddComponent<SpriteRenderer>();
            _weaponSr.sortingOrder = 16;

            // 4. Slash Visual Arc
            _slashPivotGo = new GameObject("SlashPivot");
            _slashPivotGo.transform.SetParent(transform, false);
            _slashPivotGo.transform.localPosition = Vector3.zero;

            var slashGo = new GameObject("SlashArc");
            slashGo.transform.SetParent(_slashPivotGo.transform, false);
            _slashVisualSr = slashGo.AddComponent<SpriteRenderer>();
            _slashVisualSr.sprite = SpriteHelper.GetOrCreateSlashArcSprite();
            _slashVisualSr.sortingOrder = 30;
            _slashPivotGo.SetActive(false);

            // 5. Orbiting Blades Visual
            var orbitGo = new GameObject("OrbitingBladesVisual");
            orbitGo.transform.SetParent(transform, false);
            _orbitingBladeView = orbitGo.AddComponent<Projectiles.OrbitingBladeView>();
            _orbitingBladeView.Initialize(transform, bladeCount: 2, orbitRadius: 2.0f);
            orbitGo.SetActive(false);

            // 6. Domain Setup
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

            _slashVisualTimer = 0f;
            if (_slashPivotGo != null) _slashPivotGo.SetActive(false);
            if (_weaponPivotGo != null) _weaponPivotGo.transform.rotation = Quaternion.identity;

            ApplyClassVisuals();
            IsGameStarted = true;
        }

        private void ApplyClassVisuals()
        {
            if (_bodySr != null)
            {
                _bodySr.sprite = HeroSpriteHelper.GetHeroSprite(_classType, _currentViewDir, 36);
            }
            if (_weaponSr != null)
            {
                _weaponSr.sprite = HeroSpriteHelper.GetWeaponSprite(_classType, 32);
            }
        }

        private void Update()
        {
            if (_entity == null || _entity.IsDead || !IsGameStarted)
                return;

            float dt = Time.deltaTime;

            // 1. Damage flash timer
            if (_flashTimer > 0f)
            {
                _flashTimer -= dt;
                if (_flashTimer <= 0f && _bodySr != null)
                {
                    _bodySr.color = _originalColor;
                }
            }

            // 2. Mouse Aim & 9-Directional Quarter-View Angle Evaluation
            Vector2 aimDir = Vector2.right;
            if (Camera.main != null)
            {
                Vector3 mouseScreenPos = UnityEngine.InputSystem.Mouse.current != null
                    ? (Vector3)UnityEngine.InputSystem.Mouse.current.position.ReadValue()
                    : Input.mousePosition;
                mouseScreenPos.z = -Camera.main.transform.position.z;
                Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);
                _entity.AimTargetPosition = new Vector2D(mouseWorldPos.x, mouseWorldPos.y);

                Vector2 offset = new Vector2(mouseWorldPos.x - transform.position.x, mouseWorldPos.y - transform.position.y);
                if (offset.sqrMagnitude > 0.001f)
                {
                    aimDir = offset.normalized;
                    _entity.AimDirection = new Vector2D(aimDir.x, aimDir.y);
                }
            }

            UpdateHeroAimVisuals(aimDir);

            // 3. Domain skills update
            ISpatialGrid2D monsterGrid = _spawnerView != null ? _spawnerView.MonsterGrid : null;
            ProjectileManager projManager = _projectileManagerView != null ? _projectileManagerView.DomainManager : null;
            _entity.Update(dt, monsterGrid, projManager);

            // 4. Movement Jelly Bobbing & Shadow scaling
            Vector3 deltaMove = transform.position - _lastPos;
            _lastPos = transform.position;
            float moveDist = deltaMove.magnitude;

            if (moveDist > 0.0001f)
            {
                CurrentMoveDirection = new Vector2(deltaMove.x, deltaMove.y).normalized;
                _walkBobTimer += dt * 16f;
                float hop = Mathf.Abs(Mathf.Sin(_walkBobTimer)) * 0.12f;
                float squashY = Mathf.Sin(_walkBobTimer * 2f) * 0.08f;
                float stretchX = -squashY * 0.5f;
                float tiltZ = Mathf.Clamp(-deltaMove.x * 30f, -8f, 8f);

                if (_bodyVisualGo != null)
                {
                    _bodyVisualGo.transform.localPosition = new Vector3(0f, hop, 0f);
                    _bodyVisualGo.transform.localScale = new Vector3(1.5f * (1f + stretchX), 1.5f * (1f + squashY), 1f);
                    _bodyVisualGo.transform.localRotation = Quaternion.Euler(0f, 0f, tiltZ);
                }
                if (_shadowGo != null)
                {
                    float shadowScale = 1f - (hop * 0.6f);
                    _shadowGo.transform.localScale = new Vector3(1.6f * shadowScale, 0.8f * shadowScale, 1f);
                }
            }
            else
            {
                CurrentMoveDirection = Vector2.zero;
                _walkBobTimer += dt * 3.5f;
                float breatheY = Mathf.Sin(_walkBobTimer) * 0.03f;
                if (_bodyVisualGo != null)
                {
                    _bodyVisualGo.transform.localPosition = Vector3.zero;
                    _bodyVisualGo.transform.localScale = new Vector3(1.5f * (1f - breatheY * 0.5f), 1.5f * (1f + breatheY), 1f);
                    _bodyVisualGo.transform.localRotation = Quaternion.identity;
                }
            }

            // 5. Slash Arc animation
            UpdateSlashVisuals(dt);

            // 6. Projectiles & Orbital
            if (_projectileManagerView != null && _spawnerView != null)
            {
                _projectileManagerView.UpdateProjectiles(dt, _spawnerView.MonsterGrid);
            }
            UpdateOrbitalBladesVisual();
        }

        private void UpdateHeroAimVisuals(Vector2 aimDir)
        {
            float angleDeg = Mathf.Atan2(aimDir.y, aimDir.x) * Mathf.Rad2Deg; // -180 to 180
            bool isFacingLeft = aimDir.x < -0.05f;

            // Determine 9-way direction from angle (S, SE/SW, E/W, NE/NW, N)
            HeroSpriteHelper.ViewDirection newDir;
            if (angleDeg >= -112.5f && angleDeg <= -67.5f)
            {
                newDir = HeroSpriteHelper.ViewDirection.Front; // South (Front)
            }
            else if ((angleDeg > -157.5f && angleDeg < -112.5f) || (angleDeg > -67.5f && angleDeg < -22.5f))
            {
                newDir = HeroSpriteHelper.ViewDirection.FrontDiagonal; // SE / SW
            }
            else if (angleDeg >= 67.5f && angleDeg <= 112.5f)
            {
                newDir = HeroSpriteHelper.ViewDirection.Back; // North (Back)
            }
            else if ((angleDeg > 22.5f && angleDeg < 67.5f) || (angleDeg > 112.5f && angleDeg < 157.5f))
            {
                newDir = HeroSpriteHelper.ViewDirection.BackDiagonal; // NE / NW
            }
            else
            {
                newDir = HeroSpriteHelper.ViewDirection.Side; // East / West
            }

            if (_currentViewDir != newDir || _bodySr.sprite == null)
            {
                _currentViewDir = newDir;
                _bodySr.sprite = HeroSpriteHelper.GetHeroSprite(_classType, _currentViewDir, 36);
            }

            _bodySr.flipX = isFacingLeft;

            // Weapon Pivot Positioning & Aim Rotation
            if (_weaponPivotGo != null && _slashVisualTimer <= 0f)
            {
                float sideOffset = isFacingLeft ? -0.25f : 0.25f;
                _weaponPivotGo.transform.localPosition = new Vector3(sideOffset, -0.05f, 0f);
                _weaponPivotGo.transform.rotation = Quaternion.Euler(0f, 0f, angleDeg);

                if (_weaponSr != null)
                {
                    _weaponSr.flipY = isFacingLeft;
                    _weaponSr.sortingOrder = (newDir == HeroSpriteHelper.ViewDirection.Back || newDir == HeroSpriteHelper.ViewDirection.BackDiagonal) ? 14 : 16;
                }
            }
        }

        private void UpdateSlashVisuals(float dt)
        {
            if (_slashVisualTimer <= 0f) return;

            _slashVisualTimer -= dt;
            float progress = Mathf.Clamp01(1.0f - (_slashVisualTimer / SlashDuration));
            float currentAngle = _slashBaseAngle + Mathf.Lerp(-_slashHalfArc, _slashHalfArc, Mathf.SmoothStep(0f, 1f, progress));

            if (_slashPivotGo != null) _slashPivotGo.transform.rotation = Quaternion.Euler(0f, 0f, currentAngle);
            if (_weaponPivotGo != null) _weaponPivotGo.transform.rotation = Quaternion.Euler(0f, 0f, currentAngle);

            if (_slashVisualSr != null)
            {
                Color c = _slashVisualSr.color;
                c.a = Mathf.Sin(progress * Mathf.PI) * 0.95f;
                _slashVisualSr.color = c;
            }

            if (_slashVisualTimer <= 0f)
            {
                if (_slashPivotGo != null) _slashPivotGo.SetActive(false);
                if (_weaponSr != null) _weaponSr.sortingOrder = 16;
            }
        }

        private void UpdateOrbitalBladesVisual()
        {
            if (_entity == null || _orbitingBladeView == null) return;

            HappyShoot.Domain.Skills.ISkill orbitalSkill = null;
            var skills = _entity.Skills;
            for (int i = 0; i < skills.Count; i++)
            {
                if (skills[i].Id == "orbital") { orbitalSkill = skills[i]; break; }
            }

            if (orbitalSkill != null)
            {
                if (!_orbitingBladeView.gameObject.activeSelf) _orbitingBladeView.gameObject.SetActive(true);
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
                if (_orbitingBladeView.gameObject.activeSelf) _orbitingBladeView.gameObject.SetActive(false);
            }
        }

        private void OnPlayerSlashExecuted(PlayerSlashExecutedEvent evt)
        {
            _slashBaseAngle = evt.DirectionAngleDegrees;
            _slashHalfArc = Mathf.Max(15f, evt.ArcAngleDegrees * 0.5f);
            _slashVisualTimer = SlashDuration;
            CameraFollowView.Instance?.TriggerShake("slash", duration: 0.12f, intensity: 0.18f);

            if (_slashVisualSr != null) _slashVisualSr.sprite = SpriteHelper.GetOrCreateSlashArcSprite();
            if (_weaponSr != null) { _weaponSr.color = Color.white; _weaponSr.sortingOrder = 30; }
            TriggerSlashPivot(evt.Radius);
        }

        private void OnBloodEaterExecuted(BloodEaterExecutedEvent evt)
        {
            _slashBaseAngle = evt.DirectionAngleDegrees;
            _slashHalfArc = Mathf.Max(15f, evt.ArcAngleDegrees * 0.5f);
            _slashVisualTimer = SlashDuration;

            if (_slashVisualSr != null) _slashVisualSr.sprite = SpriteHelper.GetOrCreateBloodSlashArcSprite();
            if (_weaponSr != null) { _weaponSr.color = new Color(1.0f, 0.35f, 0.45f, 1f); _weaponSr.sortingOrder = 30; }
            TriggerSlashPivot(evt.Radius);
        }

        private void TriggerSlashPivot(float radius)
        {
            if (_slashPivotGo != null)
            {
                _slashPivotGo.SetActive(true);
                float initialAngle = _slashBaseAngle - _slashHalfArc;
                _slashPivotGo.transform.rotation = Quaternion.Euler(0f, 0f, initialAngle);
                if (_weaponPivotGo != null) _weaponPivotGo.transform.rotation = Quaternion.Euler(0f, 0f, initialAngle);

                float baseRadius = 3.52f;
                float arcScale = Mathf.Max(0.5f, radius / baseRadius);
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

        private void OnPlayerMoved(PlayerMovedEvent evt) => transform.position = new Vector3(evt.Position.X, evt.Position.Y, transform.position.z);
        private void OnPlayerDamaged(PlayerDamagedEvent evt) { if (_bodySr != null) _bodySr.color = _flashDamageColor; _flashTimer = _flashDuration; }
        private void OnPlayerDied(PlayerDiedEvent evt) { if (_bodySr != null) _bodySr.color = Color.gray; }
        private void OnDestroy() => _eventBus?.Clear();
    }
}
