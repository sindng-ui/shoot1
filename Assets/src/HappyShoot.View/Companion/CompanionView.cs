using System.Collections.Generic;
using UnityEngine;
using HappyShoot.Domain.Entities;
using HappyShoot.Domain.Events;
using HappyShoot.Domain.Skills;
using HappyShoot.Domain.Spatial;
using HappyShoot.View.Audio;
using HappyShoot.View.Config;
using HappyShoot.View.Monsters;
using HappyShoot.View.Projectiles;
using HappyShoot.View.Utils;

namespace HappyShoot.View.Companion
{
    /// <summary>
    /// View component for an AI companion (Warrior or Ranger) escorting the Wizard.
    /// Manages formation tracking, walking bobbing, auto-targeting, and combat skills.
    /// Strictly modular and under 500 lines.
    /// </summary>
    public class CompanionView : MonoBehaviour
    {
        public CompanionEntity Entity { get; private set; }

        private Player.PlayerView _playerView;
        private MonsterSpawnerView _spawnerView;
        private ProjectileManagerView _projManager;
        private EventBus _eventBus;

        private GameObject _bodyVisualGo;
        private SpriteRenderer _bodySr;
        private GameObject _shadowGo;
        private SpriteRenderer _shadowSr;
        private GameObject _weaponPivotGo;
        private SpriteRenderer _weaponSr;
        private GameObject _slashPivotGo;
        private SpriteRenderer _slashVisualSr;

        private Vector2 _formationOffset;
        private float _slashVisualTimer;
        private float _slashDuration = 0.18f;
        private float _slashBaseAngle;
        private Vector3 _lastPos;
        private float _walkBobTimer;
        private MonsterEntity _currentTarget;
        private HeroSpriteHelper.ViewDirection _currentViewDir = HeroSpriteHelper.ViewDirection.Front;
        private bool _isRegrouping = false;
        private Vector3 _regroupTarget;

        public void Initialize(
            CompanionEntity entity,
            Player.PlayerView playerView,
            MonsterSpawnerView spawnerView,
            ProjectileManagerView projManager,
            EventBus eventBus)
        {
            Entity = entity;
            _playerView = playerView;
            _spawnerView = spawnerView;
            _projManager = projManager;
            _eventBus = eventBus;

            _formationOffset = (Entity.Type == CompanionType.Warrior)
                ? new Vector2(-1.95f, 0.55f)   // Front-Left (Escort Guard - Widen spread)
                : new Vector2(1.95f, -0.65f);  // Back-Right (Cover Archer - Widen spread)

            BuildVisuals();
        }

        private void BuildVisuals()
        {
            var charClass = (Entity.Type == CompanionType.Warrior) ? CharacterClassType.Warrior : CharacterClassType.Ranger;

            // 1. Root Body Visual (Scale 1.5x - Identical to PlayerView)
            _bodyVisualGo = new GameObject("BodyVisual");
            _bodyVisualGo.transform.SetParent(transform, false);
            _bodyVisualGo.transform.localScale = Vector3.one * 1.5f;

            _bodySr = _bodyVisualGo.AddComponent<SpriteRenderer>();
            _bodySr.sprite = HeroSpriteHelper.GetHeroSprite(charClass, HeroSpriteHelper.ViewDirection.Front, 32);
            _bodySr.sortingOrder = 15;

            // 2. 2.5D Blob Shadow (Identical to PlayerView)
            _shadowGo = new GameObject("BlobShadow");
            _shadowGo.transform.SetParent(transform, false);
            _shadowGo.transform.localPosition = new Vector3(0f, -0.36f, 0f);
            _shadowGo.transform.localScale = new Vector3(1.6f, 0.8f, 1f);
            _shadowSr = _shadowGo.AddComponent<SpriteRenderer>();
            _shadowSr.sprite = SpriteHelper.GetOrCreateBlobShadowSprite();
            _shadowSr.sortingOrder = 8;

            // 3. Weapon Visual (Scale 1.3x - Identical to PlayerView)
            _weaponPivotGo = new GameObject("WeaponPivot");
            _weaponPivotGo.transform.SetParent(transform, false);
            _weaponPivotGo.transform.localPosition = new Vector3(0.25f, -0.05f, 0f);

            var weaponGo = new GameObject("WeaponSprite");
            weaponGo.transform.SetParent(_weaponPivotGo.transform, false);
            weaponGo.transform.localPosition = Vector3.zero;
            weaponGo.transform.localScale = Vector3.one * 1.3f;

            _weaponSr = weaponGo.AddComponent<SpriteRenderer>();
            _weaponSr.sprite = HeroSpriteHelper.GetWeaponSprite(charClass, 32);
            _weaponSr.sortingOrder = 16;

            // 4. Slash Visualizer for Warrior
            if (Entity.Type == CompanionType.Warrior)
            {
                _slashPivotGo = new GameObject("SlashPivot");
                _slashPivotGo.transform.SetParent(transform, false);
                _slashPivotGo.transform.localPosition = Vector3.zero;

                var slashSpriteGo = new GameObject("SlashArc");
                slashSpriteGo.transform.SetParent(_slashPivotGo.transform, false);
                slashSpriteGo.transform.localPosition = new Vector3(1.35f, 0f, 0f);
                slashSpriteGo.transform.localRotation = Quaternion.Euler(0f, 0f, -90f);
                slashSpriteGo.transform.localScale = new Vector3(1.5f, 1.5f, 1.0f);

                _slashVisualSr = slashSpriteGo.AddComponent<SpriteRenderer>();
                _slashVisualSr.sprite = WarriorSkillSpriteHelper.GetOrCreateSlashArcSprite(128);
                _slashVisualSr.color = new Color(1.0f, 0.95f, 0.4f, 0f);
                _slashVisualSr.sortingOrder = 25;
                _slashPivotGo.SetActive(false);
            }

            _lastPos = transform.position;
        }

        private void Update()
        {
            if (Entity == null || _playerView == null) return;

            float dt = Time.deltaTime;
            var cfg = SkillConfigRepository.Instance.GetConfig();
            var compCfg = cfg?.Companion;
            if (compCfg != null)
            {
                Entity.FinalDamageScale = compCfg.FinalDamageScale;
                Entity.PassiveScale = compCfg.PassiveScale;
            }

            Entity.Update(dt);

            UpdateCombat(dt, compCfg);
            UpdatePositionAndAnimation(dt, compCfg);
            UpdateSlashVisuals(dt);
        }

        private void UpdatePositionAndAnimation(float dt, CompanionTuningConfig compCfg)
        {
            Vector3 playerPos = _playerView.transform.position;
            float distToPlayer = Vector3.Distance(playerPos, transform.position);

            // 1. 긴급 워프 (너무 멀리 화면 밖으로 벗어난 경우에만)
            if (distToPlayer > 16f)
            {
                transform.position = playerPos + (Vector3)_formationOffset;
                _lastPos = transform.position;
                _isRegrouping = false;
                return;
            }

            // 샌드박스 튜닝 파라미터 적용 (이속 배율, 재합류 반경, 안착 거리)
            float speedMult = compCfg != null ? compCfg.MoveSpeedMultiplier : 1.0f;
            float moveSpeed = ((_playerView.Entity != null) ? _playerView.Entity.Stats.MoveSpeed : 5.0f) * speedMult;
            float regroupRadius = compCfg != null ? compCfg.RegroupRadius : 5.0f;
            float arrivalDist = compCfg != null ? compCfg.RegroupArrivalDistance : 2.6f;

            // 2. 마법사 재합류(Regroup) 상태 판정
            if (!_isRegrouping && distToPlayer > regroupRadius)
            {
                _isRegrouping = true;
                _regroupTarget = playerPos + (Vector3)_formationOffset;
            }
            else if (_isRegrouping && distToPlayer < arrivalDist)
            {
                _isRegrouping = false;
            }

            Vector3 moveTargetPos = transform.position; // 기본은 제자리 대기
            bool shouldMove = false;

            if (_isRegrouping)
            {
                // [마법사가 멀리 갔을 때만 마법사 근처로 걸어감 (이동속도는 마법사와 100% 동일)]
                _regroupTarget = playerPos + (Vector3)_formationOffset;
                moveTargetPos = _regroupTarget;
                shouldMove = true;
            }
            else
            {
                // [평소: 유저가 조종하지 않는 완전 독립 AI 캐릭터 - 절대 마법사에게 딸려가지 않음]
                if (_currentTarget != null && !_currentTarget.IsDead)
                {
                    Vector3 monsterPos = new Vector3((float)_currentTarget.Position.X, (float)_currentTarget.Position.Y, 0f);
                    float distToMonster = Vector3.Distance(monsterPos, transform.position);

                    if (Entity.Type == CompanionType.Warrior)
                    {
                        // 전사: 몬스터가 1.5m보다 멀면 몬스터를 향해 스스로 걸어가서 공격
                        if (distToMonster > 1.5f)
                        {
                            moveTargetPos = monsterPos;
                            shouldMove = true;
                        }
                    }
                    // 궁수는 제자리에서 조준 사격
                }
            }

            // 3. 독립적 정속 보행 (마법사 걷는 속도와 100% 동일, 딸려오지 않음)
            if (shouldMove)
            {
                transform.position = Vector3.MoveTowards(transform.position, moveTargetPos, moveSpeed * dt);
            }
            Entity.Position = new Vector2D(transform.position.x, transform.position.y);

            // 4. 이동 감지 및 애니메이션
            Vector3 deltaMove = transform.position - _lastPos;
            float moveDist = deltaMove.sqrMagnitude;
            _lastPos = transform.position;
            bool isMoving = moveDist > 0.0001f;

            // 5. Brotato Movement Bobbing (마법사와 100% 동일한 통통 튀는 보빙)
            float hop = 0f;
            if (isMoving)
            {
                _walkBobTimer += dt * 16f; // 마법사와 동일한 16f 템포
                hop = Mathf.Abs(Mathf.Sin(_walkBobTimer)) * 0.14f; // 통통 튀어오름
                float squashY = Mathf.Sin(_walkBobTimer * 2f) * 0.10f; // 착지 탄성
                float tiltZ = Mathf.Sin(_walkBobTimer) * 8.5f; // 발 디딜 때마다 좌우 락킹

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
                if (_shadowGo != null)
                {
                    _shadowGo.transform.localScale = new Vector3(1.6f, 0.8f, 1f);
                }
            }

            // 6. 시선 및 5방향 스프라이트 전환
            // ★ 핵심: 걸어갈 때는 무조건 걸어가는 방향(deltaMove)을 바라봄! (미끄러짐 방지)
            Vector2 aimOrMoveDir;
            bool faceLeft;

            if (isMoving && deltaMove.sqrMagnitude > 0.0001f)
            {
                aimOrMoveDir = new Vector2(deltaMove.x, deltaMove.y);
                faceLeft = (deltaMove.x < -0.01f);
            }
            else if (_currentTarget != null && !_currentTarget.IsDead)
            {
                aimOrMoveDir = new Vector2((float)(_currentTarget.Position.X - transform.position.x), (float)(_currentTarget.Position.Y - transform.position.y));
                faceLeft = (_currentTarget.Position.X < transform.position.x);
            }
            else
            {
                aimOrMoveDir = Vector2.down;
                faceLeft = false;
            }

            if (aimOrMoveDir.sqrMagnitude > 0.0001f)
            {
                float angleDeg = Mathf.Atan2(aimOrMoveDir.y, aimOrMoveDir.x) * Mathf.Rad2Deg;
                var newDir = EvaluateViewDirection(angleDeg);
                if (newDir != _currentViewDir || _bodySr.sprite == null)
                {
                    _currentViewDir = newDir;
                    var charClass = (Entity.Type == CompanionType.Warrior) ? CharacterClassType.Warrior : CharacterClassType.Ranger;
                    _bodySr.sprite = HeroSpriteHelper.GetHeroSprite(charClass, _currentViewDir, 32);
                }
            }

            if (_bodySr != null) _bodySr.flipX = faceLeft;

            // 7. 무기 포즈 (무기도 몸통 hop에 맞춰 통통 튐!)
            if (_slashVisualTimer <= 0f)
            {
                if (_weaponPivotGo != null)
                {
                    _weaponPivotGo.transform.localPosition = new Vector3(faceLeft ? -0.25f : 0.25f, -0.05f + hop, 0f);
                    float weaponWobble = isMoving ? Mathf.Sin(_walkBobTimer) * 8f : 0f;
                    float baseRestAngle = (Entity.Type == CompanionType.Warrior)
                        ? (faceLeft ? 135f : -45f)
                        : (faceLeft ? 25f : -25f);
                    _weaponPivotGo.transform.localRotation = Quaternion.Euler(0f, 0f, baseRestAngle + weaponWobble);
                }

                if (_weaponSr != null)
                {
                    _weaponSr.flipX = faceLeft;
                    bool isBack = (_currentViewDir == HeroSpriteHelper.ViewDirection.Back || _currentViewDir == HeroSpriteHelper.ViewDirection.BackDiagonal);
                    _weaponSr.sortingOrder = isBack ? 14 : 16;
                }
            }
        }

        private HeroSpriteHelper.ViewDirection EvaluateViewDirection(float angleDeg)
        {
            if (angleDeg >= -112.5f && angleDeg <= -67.5f) return HeroSpriteHelper.ViewDirection.Front;
            if ((angleDeg > -157.5f && angleDeg < -112.5f) || (angleDeg > -67.5f && angleDeg < -22.5f)) return HeroSpriteHelper.ViewDirection.FrontDiagonal;
            if (angleDeg >= 67.5f && angleDeg <= 112.5f) return HeroSpriteHelper.ViewDirection.Back;
            if ((angleDeg > 22.5f && angleDeg < 67.5f) || (angleDeg > 112.5f && angleDeg < 157.5f)) return HeroSpriteHelper.ViewDirection.BackDiagonal;
            return HeroSpriteHelper.ViewDirection.Side;
        }

        private void UpdateCombat(float dt, CompanionTuningConfig compCfg)
        {
            if (_spawnerView == null || Entity == null || Entity.Skills == null || Entity.Skills.Count == 0) return;

            float range = (Entity.Type == CompanionType.Warrior)
                ? (compCfg != null ? compCfg.WarriorEngageRange : 3.8f)
                : (compCfg != null ? compCfg.RangerSnipingRange : 12.0f);

            bool protectWizard = compCfg != null && compCfg.PrioritizeProtectWizard;
            _currentTarget = FindClosestMonster(range, protectWizard);
            if (_currentTarget == null || _currentTarget.IsDead) return;

            var cfg = SkillConfigRepository.Instance.GetConfig();
            Vector2 attackDir = new Vector2(
                (float)(_currentTarget.Position.X - Entity.Position.X),
                (float)(_currentTarget.Position.Y - Entity.Position.Y)).normalized;

            for (int i = 0; i < Entity.Skills.Count; i++)
            {
                var skillInstance = Entity.Skills[i];
                if (!skillInstance.IsReady) continue;

                float baseCd = GetSkillBaseCooldown(skillInstance.SkillId, cfg);
                float effectiveCd = Entity.CalculateEffectiveCooldown(baseCd);
                skillInstance.Trigger(effectiveCd);

                CompanionSkillExecutor.ExecuteSkill(
                    skillInstance.SkillId,
                    skillInstance.Level,
                    Entity,
                    attackDir,
                    _spawnerView,
                    _projManager,
                    _eventBus,
                    onSlashTriggered: TriggerSlashVisual,
                    companionTransform: transform);

                break;
            }
        }

        private float GetSkillBaseCooldown(string skillId, SkillConfigData cfg)
        {
            switch (skillId)
            {
                case "slash": return cfg.Slash.Cooldown;
                case "ground_stomp": return cfg.GroundStomp.Cooldown;
                case "whirlwind": return cfg.Whirlwind.Cooldown;
                case "bow": return cfg.Bow.Cooldown;
                case "glaive": return cfg.Glaive.Cooldown;
                case "arrow_rain": return cfg.ArrowRain.Cooldown;
                default: return 1.0f;
            }
        }

        private void TriggerSlashVisual(float baseAngle)
        {
            _slashBaseAngle = baseAngle;
            _slashVisualTimer = _slashDuration;
            if (_weaponSr != null) _weaponSr.sortingOrder = 30;
            if (_slashPivotGo != null)
            {
                _slashPivotGo.SetActive(true);
                float initialAngle = _slashBaseAngle - 60f;
                _slashPivotGo.transform.rotation = Quaternion.Euler(0f, 0f, initialAngle);
                if (_weaponPivotGo != null) _weaponPivotGo.transform.rotation = Quaternion.Euler(0f, 0f, initialAngle);
                if (_slashVisualSr != null) _slashVisualSr.color = Color.white;
            }
        }

        private void UpdateSlashVisuals(float dt)
        {
            if (_slashVisualTimer <= 0f) return;
            _slashVisualTimer -= dt;
            float p = Mathf.Clamp01(1.0f - (_slashVisualTimer / _slashDuration));
            float currentAngle = _slashBaseAngle + Mathf.Lerp(-60f, 60f, Mathf.SmoothStep(0f, 1f, p));

            if (_slashPivotGo != null)
                _slashPivotGo.transform.rotation = Quaternion.Euler(0f, 0f, currentAngle);

            if (_weaponPivotGo != null)
                _weaponPivotGo.transform.rotation = Quaternion.Euler(0f, 0f, currentAngle);

            if (_slashVisualSr != null)
            {
                Color c = _slashVisualSr.color;
                c.a = Mathf.Sin(p * Mathf.PI) * 0.95f;
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

        private MonsterEntity FindClosestMonster(float maxRange, bool prioritizeProtectWizard)
        {
            if (_spawnerView == null) return null;
            var activeList = _spawnerView.DomainSpawner?.ActiveMonsters;
            if (activeList == null || activeList.Count == 0) return null;

            MonsterEntity closest = null;
            float minSqrDist = float.MaxValue;
            Vector2D myPos = Entity.Position;
            Vector2D wizardPos = (_playerView != null && _playerView.Entity != null) ? _playerView.Entity.Position : myPos;
            Vector2D evalPos = prioritizeProtectWizard ? wizardPos : myPos;
            float maxRangeSqr = maxRange * maxRange;

            for (int i = 0; i < activeList.Count; i++)
            {
                var m = activeList[i];
                if (m == null || m.IsDead) continue;
                float sqrDist = (m.Position - myPos).SqrMagnitude;
                if (sqrDist < minSqrDist)
                {
                    minSqrDist = sqrDist;
                    closest = m;
                }
            }
            return closest;
        }
    }
}
