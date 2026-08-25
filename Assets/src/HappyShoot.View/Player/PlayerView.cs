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
    /// Features:
    /// - 100% original cute Chibi 9-directional sprites
    /// - Intelligent Aim/Look State Machine:
    ///   1. Mouse active -> Aim reticle shown, looks at Mouse Aim.
    ///   2. Mouse idle (4s) -> Aim reticle hidden, looks at WASD movement direction.
    ///   3. Move idle (4.5s) -> Smoothly auto-resets to default cute Front posture.
    /// - Warrior broadsword swing, Ranger bow recoil kickback, Wizard raised staff posture.
    /// Strictly modular and under 460 lines (500-line architecture rule).
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

        // Ranger bow recoil & snap
        private float _rangerRecoilTimer;
        private const float RangerRecoilDuration = 0.14f;
        private Vector2 _lastShootAimDir = Vector2.right;

        // Wizard raised staff casting pulse
        private float _wizardCastPulseTimer;
        private const float WizardCastPulseDuration = 0.18f;

        // Intelligent Aim & Look State Machine
        private float _keyboardIdleTimer;
        private const float KeyboardIdleThreshold = 4.5f;
        private HeroSpriteHelper.ViewDirection _lastMoveViewDir = HeroSpriteHelper.ViewDirection.Front;
        private bool _lastMoveFacingLeft = false;
        private Vector2 _lastActiveAimDir = Vector2.right;

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

            // 2. Child BodyVisual (Cute Chibi Hero)
            _bodyVisualGo = new GameObject("BodyVisual");
            _bodyVisualGo.transform.SetParent(transform, false);
            _bodyVisualGo.transform.localPosition = Vector3.zero;
            _bodyVisualGo.transform.localScale = Vector3.one * 1.5f;

            _bodySr = _bodyVisualGo.AddComponent<SpriteRenderer>();
            _bodySr.sortingOrder = 15;
            _bodySr.color = Color.white;

            var rootSr = GetComponent<SpriteRenderer>();
            if (rootSr != null) rootSr.sprite = null;

            // 3. Hand-Held Weapon Pivot
            _weaponPivotGo = new GameObject("WeaponPivot");
            _weaponPivotGo.transform.SetParent(transform, false);
            _weaponPivotGo.transform.localPosition = new Vector3(0.28f, -0.06f, 0f);

            _weaponGo = new GameObject("WeaponVisual");
            _weaponGo.transform.SetParent(_weaponPivotGo.transform, false);
            _weaponGo.transform.localPosition = Vector3.zero;
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

            // Ranger recoil events
            _eventBus.Subscribe<PiercingArrowExecutedEvent>(OnRangerShoot);
            _eventBus.Subscribe<StormBowExecutedEvent>(evt => TriggerRangerRecoil(new Vector2(evt.TargetDirection.X, evt.TargetDirection.Y)));
            _eventBus.Subscribe<WindGlaiveExecutedEvent>(evt => TriggerRangerRecoil(new Vector2(evt.TargetDirection.X, evt.TargetDirection.Y)));
            _eventBus.Subscribe<PhantomGlaiveExecutedEvent>(evt => TriggerRangerRecoil(new Vector2(evt.TargetDirection.X, evt.TargetDirection.Y)));
            _eventBus.Subscribe<StellarRainExecutedEvent>(evt => TriggerRangerRecoil(Vector2.down));
            _eventBus.Subscribe<ArrowRainExecutedEvent>(evt => TriggerRangerRecoil(Vector2.down));

            // Wizard cast pulse events
            _eventBus.Subscribe<FireballLaunchedEvent>(evt => TriggerWizardCastPulse());
            _eventBus.Subscribe<MeteorStrikeLaunchedEvent>(evt => TriggerWizardCastPulse());
            _eventBus.Subscribe<ChainLightningExecutedEvent>(evt => TriggerWizardCastPulse());
            _eventBus.Subscribe<GigastormLightningExecutedEvent>(evt => TriggerWizardCastPulse());
            _eventBus.Subscribe<FrostNovaExecutedEvent>(evt => TriggerWizardCastPulse());
            _eventBus.Subscribe<BlizzardNovaExecutedEvent>(evt => TriggerWizardCastPulse());

            _lastPos = transform.position;
        }

        public void SetClassType(CharacterClassType classType)
        {
            _classType = classType;
            Vector2D currentPos = _entity != null ? _entity.Position : new Vector2D(transform.position.x, transform.position.y);
            _entity = PlayerClassFactory.CreatePlayer(1, _classType, currentPos, _eventBus);

            _slashVisualTimer = 0f;
            _rangerRecoilTimer = 0f;
            _wizardCastPulseTimer = 0f;
            _keyboardIdleTimer = 0f;
            if (_slashPivotGo != null) _slashPivotGo.SetActive(false);
            if (_weaponPivotGo != null) _weaponPivotGo.transform.rotation = Quaternion.identity;

            ApplyClassVisuals();
            IsGameStarted = true;
        }

        private void ApplyClassVisuals()
        {
            if (_bodySr != null)
            {
                _bodySr.sprite = HeroSpriteHelper.GetHeroSprite(_classType, _currentViewDir, 32);
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
                if (_flashTimer <= 0f && _bodySr != null) _bodySr.color = _originalColor;
            }

            // 2. Compute Movement Vector
            Vector3 deltaMove = transform.position - _lastPos;
            _lastPos = transform.position;
            float moveDist = deltaMove.magnitude;
            Vector2 moveDir = moveDist > 0.0001f ? new Vector2(deltaMove.x, deltaMove.y).normalized : Vector2.zero;
            CurrentMoveDirection = moveDir;

            // 3. Mouse Coordinate Evaluation
            Vector2 mouseAimDir = Vector2.right;
            if (Camera.main != null)
            {
                Vector3 mouseScreenPos = UnityEngine.InputSystem.Mouse.current != null
                    ? (Vector3)UnityEngine.InputSystem.Mouse.current.position.ReadValue()
                    : Input.mousePosition;
                mouseScreenPos.z = -Camera.main.transform.position.z;
                Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);
                _entity.AimTargetPosition = new Vector2D(mouseWorldPos.x, mouseWorldPos.y);

                Vector2 offset = new Vector2(mouseWorldPos.x - transform.position.x, mouseWorldPos.y - transform.position.y);
                if (offset.sqrMagnitude > 0.001f) mouseAimDir = offset.normalized;
            }

            // 4. Update Hero Look & Aim State Machine
            UpdateHeroAimVisuals(mouseAimDir, moveDir, dt);

            // 5. Domain skills update
            ISpatialGrid2D monsterGrid = _spawnerView != null ? _spawnerView.MonsterGrid : null;
            ProjectileManager projManager = _projectileManagerView != null ? _projectileManagerView.DomainManager : null;
            _entity.Update(dt, monsterGrid, projManager);

            // 6. Movement Jelly Bobbing & Shadow scaling
            if (moveDist > 0.0001f)
            {
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
                _walkBobTimer += dt * 3.5f;
                float breatheY = Mathf.Sin(_walkBobTimer) * 0.03f;
                if (_bodyVisualGo != null)
                {
                    _bodyVisualGo.transform.localPosition = Vector3.zero;
                    _bodyVisualGo.transform.localScale = new Vector3(1.5f * (1f - breatheY * 0.5f), 1.5f * (1f + breatheY), 1f);
                    _bodyVisualGo.transform.localRotation = Quaternion.identity;
                }
            }

            // Apply recoil kickback to body visual if shooting
            if (_rangerRecoilTimer > 0f && _bodyVisualGo != null)
            {
                float p = Mathf.Clamp01(1.0f - (_rangerRecoilTimer / RangerRecoilDuration));
                float snap = Mathf.Sin(p * Mathf.PI);
                _bodyVisualGo.transform.localPosition += new Vector3(-_lastShootAimDir.x * snap * 0.06f, 0f, 0f);
            }

            // 7. Slash Arc animation
            UpdateSlashVisuals(dt);

            // 8. Projectiles & Orbital
            if (_projectileManagerView != null && _spawnerView != null)
            {
                _projectileManagerView.UpdateProjectiles(dt, _spawnerView.MonsterGrid);
            }
            UpdateOrbitalBladesVisual();
        }

        private void UpdateHeroAimVisuals(Vector2 mouseAimDir, Vector2 moveDir, float dt)
        {
            bool isMouseAimActive = Cameras.AimReticleView.IsMouseAimActive;

            HeroSpriteHelper.ViewDirection newDir;
            bool isFacingLeft = false;
            Vector2 finalSkillAimDir;

            if (isMouseAimActive)
            {
                // (1) Mouse Active -> Look & Aim towards Mouse (9-way)
                _keyboardIdleTimer = 0f;
                float angleDeg = Mathf.Atan2(mouseAimDir.y, mouseAimDir.x) * Mathf.Rad2Deg;
                isFacingLeft = mouseAimDir.x < -0.05f;
                newDir = EvaluateViewDirection(angleDeg);
                _lastMoveViewDir = newDir;
                _lastMoveFacingLeft = isFacingLeft;
                finalSkillAimDir = mouseAimDir;
            }
            else
            {
                // (2) Mouse Idle (Aim hidden)
                if (moveDir.sqrMagnitude > 0.001f)
                {
                    // (2-a) Moving with Keyboard -> Look towards WASD Move Direction (9-way)
                    _keyboardIdleTimer = 0f;
                    float moveAngleDeg = Mathf.Atan2(moveDir.y, moveDir.x) * Mathf.Rad2Deg;
                    isFacingLeft = moveDir.x < -0.05f;
                    newDir = EvaluateViewDirection(moveAngleDeg);
                    _lastMoveViewDir = newDir;
                    _lastMoveFacingLeft = isFacingLeft;
                    finalSkillAimDir = moveDir;
                }
                else
                {
                    // (2-b) Movement Stopped -> Look Front immediately when mouse aim is idle
                    newDir = HeroSpriteHelper.ViewDirection.Front;
                    isFacingLeft = false;
                    finalSkillAimDir = Vector2.down;
                    _lastMoveViewDir = newDir;
                    _lastMoveFacingLeft = false;
                }
            }

            _lastActiveAimDir = finalSkillAimDir;
            _entity.AimDirection = new Vector2D(finalSkillAimDir.x, finalSkillAimDir.y);

            if (_currentViewDir != newDir || _bodySr.sprite == null)
            {
                _currentViewDir = newDir;
                _bodySr.sprite = HeroSpriteHelper.GetHeroSprite(_classType, _currentViewDir, 32);
            }

            _bodySr.flipX = isFacingLeft;

            // Weapon Posture & Class-Specific Animation
            UpdateWeaponAndClassVisuals(dt, finalSkillAimDir, isFacingLeft, newDir);
        }

        private HeroSpriteHelper.ViewDirection EvaluateViewDirection(float angleDeg)
        {
            if (angleDeg >= -112.5f && angleDeg <= -67.5f) return HeroSpriteHelper.ViewDirection.Front; // South
            if ((angleDeg > -157.5f && angleDeg < -112.5f) || (angleDeg > -67.5f && angleDeg < -22.5f)) return HeroSpriteHelper.ViewDirection.FrontDiagonal; // SE/SW
            if (angleDeg >= 67.5f && angleDeg <= 112.5f) return HeroSpriteHelper.ViewDirection.Back; // North
            if ((angleDeg > 22.5f && angleDeg < 67.5f) || (angleDeg > 112.5f && angleDeg < 157.5f)) return HeroSpriteHelper.ViewDirection.BackDiagonal; // NE/NW
            return HeroSpriteHelper.ViewDirection.Side; // East/West
        }

        private void UpdateWeaponAndClassVisuals(float dt, Vector2 aimDir, bool isFacingLeft, HeroSpriteHelper.ViewDirection newDir)
        {
            if (_slashVisualTimer > 0f || _weaponPivotGo == null) return;

            if (_classType == CharacterClassType.Ranger)
            {
                // Ranger Recoil Spring Animation
                float recoilX = 0f;
                float recoilY = 0f;
                float bowScaleBonus = 0f;

                if (_rangerRecoilTimer > 0f)
                {
                    _rangerRecoilTimer -= dt;
                    float p = Mathf.Clamp01(1.0f - (_rangerRecoilTimer / RangerRecoilDuration));
                    float snap = Mathf.Sin(p * Mathf.PI);
                    recoilX = -_lastShootAimDir.x * snap * 0.12f;
                    recoilY = -_lastShootAimDir.y * snap * 0.08f;
                    bowScaleBonus = snap * 0.22f;
                }

                float sideOffset = isFacingLeft ? -0.25f : 0.25f;
                _weaponPivotGo.transform.localPosition = new Vector3(sideOffset + recoilX, -0.05f + recoilY, 0f);
                float restAngle = isFacingLeft ? 135f : -45f;
                _weaponPivotGo.transform.rotation = Quaternion.Euler(0f, 0f, restAngle);
                if (_weaponGo != null) _weaponGo.transform.localScale = Vector3.one * (1.3f + bowScaleBonus);
            }
            else if (_classType == CharacterClassType.Wizard)
            {
                // Wizard Raised Staff Pose (Pointing outward away from face, never covers eyes/hat)
                float castAngleOffset = 0f;
                float castHeightOffset = 0f;

                if (_wizardCastPulseTimer > 0f)
                {
                    _wizardCastPulseTimer -= dt;
                    float p = Mathf.Clamp01(1.0f - (_wizardCastPulseTimer / WizardCastPulseDuration));
                    float pulse = Mathf.Sin(p * Mathf.PI);
                    castAngleOffset = isFacingLeft ? pulse * 16f : -pulse * 16f;
                    castHeightOffset = pulse * 0.06f;
                }

                float sideOffset = isFacingLeft ? -0.28f : 0.28f;
                float baseAngle = isFacingLeft ? 28f : -28f;
                _weaponPivotGo.transform.localPosition = new Vector3(sideOffset, -0.06f + castHeightOffset, 0f);
                _weaponPivotGo.transform.rotation = Quaternion.Euler(0f, 0f, baseAngle + castAngleOffset);
                if (_weaponGo != null) _weaponGo.transform.localScale = Vector3.one * 1.3f;
            }
            else
            {
                // Warrior Classic Broadsword Pose
                float sideOffset = isFacingLeft ? -0.25f : 0.25f;
                _weaponPivotGo.transform.localPosition = new Vector3(sideOffset, -0.05f, 0f);
                float restAngle = isFacingLeft ? 135f : -45f;
                _weaponPivotGo.transform.rotation = Quaternion.Euler(0f, 0f, restAngle);
                if (_weaponGo != null) _weaponGo.transform.localScale = Vector3.one * 1.3f;
            }

            if (_weaponSr != null)
            {
                _weaponSr.flipY = false;
                _weaponSr.flipX = false;
                _weaponSr.sortingOrder = (newDir == HeroSpriteHelper.ViewDirection.Back || newDir == HeroSpriteHelper.ViewDirection.BackDiagonal) ? 14 : 16;
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
                if (_weaponPivotGo != null)
                {
                    bool isFlipped = _bodySr != null && _bodySr.flipX;
                    _weaponPivotGo.transform.rotation = Quaternion.Euler(0f, 0f, isFlipped ? 135f : -45f);
                }
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

        private void OnRangerShoot(PiercingArrowExecutedEvent evt) => TriggerRangerRecoil(new Vector2(evt.TargetDirection.X, evt.TargetDirection.Y));
        private void TriggerRangerRecoil(Vector2 dir)
        {
            _rangerRecoilTimer = RangerRecoilDuration;
            _lastShootAimDir = dir.sqrMagnitude > 0.001f ? dir.normalized : Vector2.right;
        }

        private void TriggerWizardCastPulse() => _wizardCastPulseTimer = WizardCastPulseDuration;

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
