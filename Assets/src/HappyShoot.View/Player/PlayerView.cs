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
    /// Intelligent hybrid aim/movement direction state machine, 9-dir high-res sprites,
    /// dynamic weapon poses & recoil animations. Strictly modular under 500 lines.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class PlayerView : MonoBehaviour
    {
        [Header("Character Configuration")]
        [SerializeField] private CharacterClassType _classType = CharacterClassType.Warrior;
        [SerializeField] private Color _flashDamageColor = Color.red;
        [SerializeField] private float _flashDuration = 0.1f;
        [SerializeField] private Monsters.MonsterSpawnerView _spawnerView;
        [SerializeField] private Projectiles.ProjectileManagerView _projectileManagerView;

        private PlayerEntity _entity;
        private EventBus _eventBus;
        private Color _originalColor = Color.white;
        private float _flashTimer;

        public PlayerEntity Entity => _entity;
        public SpriteRenderer BodyRenderer => _bodySr;
        public EventBus EventBus => _eventBus;
        public Vector2 CurrentMoveDirection { get; private set; } = Vector2.zero;
        public bool IsGameStarted { get; set; } = false;

        private GameObject _shadowGo, _bodyVisualGo, _weaponPivotGo, _weaponGo, _slashPivotGo, _slashVisualGo;
        private SpriteRenderer _bodySr, _shadowSr, _weaponSr, _slashVisualSr;

        private float _slashVisualTimer, _slashBaseAngle, _slashHalfArc = 75f;
        private const float SlashDuration = 0.18f;

        // Ranger bow recoil & snap
        private float _rangerRecoilTimer;
        private const float RangerRecoilDuration = 0.14f;
        private Vector2 _lastShootAimDir = Vector2.right;

        // Wizard raised staff casting pulse
        private float _wizardCastPulseTimer;
        private const float WizardCastPulseDuration = 0.18f;

        private Projectiles.OrbitingBladeView _orbitingBladeView;
        private int _cachedOrbitalLevel = -1;
        private float _cachedOrbitalArea = -1f;

        private Vector3 _lastPos;
        private float _walkBobTimer;
        private HeroSpriteHelper.ViewDirection _currentViewDir = HeroSpriteHelper.ViewDirection.Front;

        public CharacterClassType ClassType => _classType;

        public void SetExternalSystems(Monsters.MonsterSpawnerView spawnerView, Projectiles.ProjectileManagerView projectileManagerView)
        {
            _spawnerView = spawnerView;
            _projectileManagerView = projectileManagerView;
        }

        private void Awake()
        {
            // 1. Root & Base visuals
            _bodyVisualGo = new GameObject("BodyVisual");
            _bodyVisualGo.transform.SetParent(transform, false);
            _bodySr = _bodyVisualGo.AddComponent<SpriteRenderer>();
            _bodySr.sortingOrder = 15;
            _originalColor = _bodySr.color;
            _bodyVisualGo.transform.localScale = Vector3.one * 1.5f;

            // 2. 2.5D Blob Shadow
            _shadowGo = new GameObject("BlobShadow");
            _shadowGo.transform.SetParent(transform, false);
            _shadowGo.transform.localPosition = new Vector3(0f, -0.36f, 0f);
            _shadowGo.transform.localScale = new Vector3(1.6f, 0.8f, 1f);
            _shadowSr = _shadowGo.AddComponent<SpriteRenderer>();
            _shadowSr.sprite = SpriteHelper.GetOrCreateBlobShadowSprite();
            _shadowSr.sortingOrder = 8;

            // 3. Weapon Pivot & Visual
            _weaponPivotGo = new GameObject("WeaponPivot");
            _weaponPivotGo.transform.SetParent(transform, false);
            _weaponPivotGo.transform.localPosition = new Vector3(0.25f, -0.05f, 0f);
            _weaponGo = new GameObject("WeaponVisual");
            _weaponGo.transform.SetParent(_weaponPivotGo.transform, false);
            _weaponGo.transform.localPosition = Vector3.zero;
            _weaponGo.transform.localScale = Vector3.one * 1.3f;
            _weaponSr = _weaponGo.AddComponent<SpriteRenderer>();
            _weaponSr.sortingOrder = 16;

            // 4. Slash Effect Pivot & Visual
            _slashPivotGo = new GameObject("SlashPivot");
            _slashPivotGo.transform.SetParent(transform, false);
            _slashPivotGo.transform.localPosition = Vector3.zero;
            _slashVisualGo = new GameObject("SlashVisual");
            _slashVisualGo.transform.SetParent(_slashPivotGo.transform, false);
            _slashVisualGo.transform.localPosition = Vector3.zero;
            _slashVisualSr = _slashVisualGo.AddComponent<SpriteRenderer>();
            _slashVisualSr.sprite = SpriteHelper.GetOrCreateSlashArcSprite();
            _slashVisualSr.sortingOrder = 20;
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
            _eventBus.Subscribe<StormBowExecutedEvent>(e => TriggerRangerRecoil(new Vector2(e.TargetDirection.X, e.TargetDirection.Y)));
            _eventBus.Subscribe<WindGlaiveExecutedEvent>(e => TriggerRangerRecoil(new Vector2(e.TargetDirection.X, e.TargetDirection.Y)));
            _eventBus.Subscribe<PhantomGlaiveExecutedEvent>(e => TriggerRangerRecoil(new Vector2(e.TargetDirection.X, e.TargetDirection.Y)));
            _eventBus.Subscribe<StellarRainExecutedEvent>(_ => TriggerRangerRecoil(Vector2.down));
            _eventBus.Subscribe<ArrowRainExecutedEvent>(_ => TriggerRangerRecoil(Vector2.down));

            // Wizard cast pulse events
            _eventBus.Subscribe<FireballLaunchedEvent>(_ => TriggerWizardCastPulse());
            _eventBus.Subscribe<MeteorStrikeLaunchedEvent>(_ => TriggerWizardCastPulse());
            _eventBus.Subscribe<ChainLightningExecutedEvent>(_ => TriggerWizardCastPulse());
            _eventBus.Subscribe<GigastormLightningExecutedEvent>(_ => TriggerWizardCastPulse());
            _eventBus.Subscribe<FrostNovaExecutedEvent>(_ => TriggerWizardCastPulse());
            _eventBus.Subscribe<BlizzardNovaExecutedEvent>(_ => TriggerWizardCastPulse());

            _lastPos = transform.position;
        }

        public void SetClassType(CharacterClassType classType, string startSkillId = null)
        {
            _classType = classType;
            Vector2D currentPos = _entity != null ? _entity.Position : new Vector2D(transform.position.x, transform.position.y);
            _entity = PlayerClassFactory.CreatePlayer(1, _classType, currentPos, _eventBus, startSkillId);

            _slashVisualTimer = 0f;
            _rangerRecoilTimer = 0f;
            _wizardCastPulseTimer = 0f;
            if (_slashPivotGo != null) _slashPivotGo.SetActive(false);
            if (_weaponPivotGo != null) _weaponPivotGo.transform.rotation = Quaternion.identity;

            ApplyClassVisuals();
            IsGameStarted = true;
        }

        private void ApplyClassVisuals()
        {
            if (_bodySr != null) _bodySr.sprite = HeroSpriteHelper.GetHeroSprite(_classType, _currentViewDir, 32);
            if (_weaponSr != null) _weaponSr.sprite = HeroSpriteHelper.GetWeaponSprite(_classType, 32);
        }

        private void Update()
        {
            if (_entity == null || _entity.IsDead || !IsGameStarted) return;
            float dt = Time.deltaTime;

            // 1. Damage flash timer
            if (_flashTimer > 0f)
            {
                _flashTimer -= dt;
                if (_flashTimer <= 0f && _bodySr != null) _bodySr.color = _originalColor;
            }

            // 2. Movement & Direction
            Vector3 deltaMove = transform.position - _lastPos;
            _lastPos = transform.position;
            float moveDist = deltaMove.magnitude;
            Vector2 moveDir = moveDist > 0.0001f ? new Vector2(deltaMove.x, deltaMove.y).normalized : Vector2.zero;
            CurrentMoveDirection = moveDir;

            // 3. Mouse Aim Target Position
            Vector2 mouseAimDir = Vector2.right;
            if (Camera.main != null)
            {
                Vector3 mouseScreenPos = UnityEngine.InputSystem.Mouse.current != null ? (Vector3)UnityEngine.InputSystem.Mouse.current.position.ReadValue() : Input.mousePosition;
                mouseScreenPos.z = -Camera.main.transform.position.z;
                Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);
                _entity.AimTargetPosition = new Vector2D(mouseWorldPos.x, mouseWorldPos.y);
                Vector2 offset = new Vector2(mouseWorldPos.x - transform.position.x, mouseWorldPos.y - transform.position.y);
                if (offset.sqrMagnitude > 0.001f) mouseAimDir = offset.normalized;
            }

            // 4. Update Hero Look & Aim State Machine
            UpdateHeroAimVisuals(mouseAimDir, moveDir, dt);

            // 5. Domain skills update
            _entity.Update(dt, _spawnerView?.MonsterGrid, _projectileManagerView?.DomainManager);

            // 6. Movement Jelly Bobbing & Shadow scaling
            if (moveDist > 0.0001f)
            {
                _walkBobTimer += dt * 16f;
                float hop = Mathf.Abs(Mathf.Sin(_walkBobTimer)) * 0.12f;
                float squashY = Mathf.Sin(_walkBobTimer * 2f) * 0.08f;
                float tiltZ = Mathf.Clamp(-deltaMove.x * 30f, -8f, 8f);
                if (_bodyVisualGo != null)
                {
                    _bodyVisualGo.transform.localPosition = new Vector3(0f, hop, 0f);
                    _bodyVisualGo.transform.localScale = new Vector3(1.5f * (1f - squashY * 0.5f), 1.5f * (1f + squashY), 1f);
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
            bool isMoving = moveDir.sqrMagnitude > 0.001f;
            bool isMouseActive = Cameras.AimReticleView.IsMouseAimActive;
            bool isMouseMoving = Cameras.AimReticleView.IsMouseActivelyMoving;

            HeroSpriteHelper.ViewDirection newDir;
            bool isFacingLeft = false;
            Vector2 finalSkillAimDir;

            // (1) If moving with Keyboard and mouse is NOT actively moving -> Look towards Movement direction IMMEDIATELY!
            if (isMoving && !isMouseMoving)
            {
                float moveAngleDeg = Mathf.Atan2(moveDir.y, moveDir.x) * Mathf.Rad2Deg;
                isFacingLeft = moveDir.x < -0.05f;
                newDir = EvaluateViewDirection(moveAngleDeg);
                finalSkillAimDir = isMouseActive ? mouseAimDir : moveDir;
            }
            // (2) If mouse is actively moving or aiming -> Look & Aim towards Mouse (9-way)
            else if (isMouseActive)
            {
                float angleDeg = Mathf.Atan2(mouseAimDir.y, mouseAimDir.x) * Mathf.Rad2Deg;
                isFacingLeft = mouseAimDir.x < -0.05f;
                newDir = EvaluateViewDirection(angleDeg);
                finalSkillAimDir = mouseAimDir;
            }
            // (3) Fallback keyboard movement
            else if (isMoving)
            {
                float moveAngleDeg = Mathf.Atan2(moveDir.y, moveDir.x) * Mathf.Rad2Deg;
                isFacingLeft = moveDir.x < -0.05f;
                newDir = EvaluateViewDirection(moveAngleDeg);
                finalSkillAimDir = moveDir;
            }
            // (4) Completely idle -> Look Front
            else
            {
                newDir = HeroSpriteHelper.ViewDirection.Front;
                isFacingLeft = false;
                finalSkillAimDir = Vector2.down;
            }

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

                float sideOffset = isFacingLeft ? -0.20f : 0.20f;
                float handY = -0.16f;
                _weaponPivotGo.transform.localPosition = new Vector3(sideOffset + recoilX, handY + recoilY, 0f);

                float restAngle = isFacingLeft ? 25f : -25f;
                _weaponPivotGo.transform.rotation = Quaternion.Euler(0f, 0f, restAngle);
                if (_weaponSr != null) _weaponSr.flipX = isFacingLeft;
                if (_weaponGo != null) _weaponGo.transform.localScale = Vector3.one * (1.2f + bowScaleBonus);
            }
            else if (_classType == CharacterClassType.Wizard)
            {
                float castProgress = (_wizardCastPulseTimer > 0f) ? Mathf.Clamp01(1.0f - (_wizardCastPulseTimer / WizardCastPulseDuration)) : 0f;
                if (_wizardCastPulseTimer > 0f) _wizardCastPulseTimer -= dt;

                var placement = WizardWeaponPlacementHelper.CalculatePlacement(newDir, isFacingLeft, castProgress);
                _weaponPivotGo.transform.localPosition = placement.LocalPosition;
                _weaponPivotGo.transform.rotation = Quaternion.Euler(0f, 0f, placement.RotationZ);
                if (_weaponSr != null)
                {
                    _weaponSr.flipX = placement.FlipX;
                    _weaponSr.sortingOrder = placement.SortingOrder;
                }
                if (_weaponGo != null) _weaponGo.transform.localScale = placement.Scale;
            }
            else
            {
                // Warrior Classic Broadsword Pose
                _weaponPivotGo.transform.localPosition = new Vector3(isFacingLeft ? -0.25f : 0.25f, -0.05f, 0f);
                _weaponPivotGo.transform.rotation = Quaternion.Euler(0f, 0f, isFacingLeft ? 135f : -45f);
                if (_weaponGo != null) _weaponGo.transform.localScale = Vector3.one * 1.3f;
            }

            if (_weaponSr != null)
            {
                _weaponSr.flipY = false;
                if (_classType != CharacterClassType.Wizard)
                {
                    if (_classType == CharacterClassType.Warrior) _weaponSr.flipX = false;
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
            else if (_orbitingBladeView.gameObject.activeSelf) _orbitingBladeView.gameObject.SetActive(false);
        }

        private void OnRangerShoot(PiercingArrowExecutedEvent evt) => TriggerRangerRecoil(new Vector2(evt.TargetDirection.X, evt.TargetDirection.Y));
        private void TriggerRangerRecoil(Vector2 dir) { _rangerRecoilTimer = RangerRecoilDuration; _lastShootAimDir = dir.sqrMagnitude > 0.001f ? dir.normalized : Vector2.right; }
        private void TriggerWizardCastPulse() => _wizardCastPulseTimer = WizardCastPulseDuration;

        private void OnPlayerSlashExecuted(PlayerSlashExecutedEvent evt)
        {
            _slashBaseAngle = evt.DirectionAngleDegrees;
            _slashHalfArc = Mathf.Max(15f, evt.ArcAngleDegrees * 0.5f);
            _slashVisualTimer = SlashDuration;
            CameraFollowView.Instance?.TriggerShake("slash", 0.12f, 0.18f);
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
            if (_slashPivotGo == null) return;
            _slashPivotGo.SetActive(true);
            float initialAngle = _slashBaseAngle - _slashHalfArc;
            _slashPivotGo.transform.rotation = Quaternion.Euler(0f, 0f, initialAngle);
            if (_weaponPivotGo != null) _weaponPivotGo.transform.rotation = Quaternion.Euler(0f, 0f, initialAngle);
            if (_slashVisualSr != null)
            {
                _slashVisualSr.transform.localScale = Vector3.one * Mathf.Max(0.5f, radius / 3.52f);
                _slashVisualSr.transform.localPosition = Vector3.zero;
                _slashVisualSr.color = Color.white;
            }
        }
        private void OnPlayerMoved(PlayerMovedEvent evt) => transform.position = new Vector3(evt.Position.X, evt.Position.Y, transform.position.z);
        private void OnPlayerDamaged(PlayerDamagedEvent evt) { if (_bodySr != null) _bodySr.color = _flashDamageColor; _flashTimer = _flashDuration; }
        private void OnPlayerDied(PlayerDiedEvent evt) { if (_bodySr != null) _bodySr.color = Color.gray; }
        private void OnDestroy() => _eventBus?.Clear();
    }
}
