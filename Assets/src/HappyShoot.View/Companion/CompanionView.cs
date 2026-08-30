using System.Collections.Generic;
using UnityEngine;
using HappyShoot.Domain.Entities;
using HappyShoot.Domain.Events;
using HappyShoot.Domain.Spatial;
using HappyShoot.View.Audio;
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
            Entity.Update(dt);

            UpdateCombat(dt);
            UpdatePositionAndAnimation(dt);
            UpdateSlashVisuals(dt);
        }

        private void UpdatePositionAndAnimation(float dt)
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

            // 마법사의 기본 이동 속도 (완전히 동일한 속도로 이동)
            float moveSpeed = (_playerView.Entity != null) ? _playerView.Entity.Stats.MoveSpeed : 5.0f;

            // 2. 마법사 재합류(Regroup) 상태 판정
            // - 마법사가 6.0m 이상 멀리 가버렸을 때만 재합류 시작!
            // - 마법사 근처 2.6m 이내로 도달하면 즉시 멈추고 독립 전투 모드로 복귀!
            if (!_isRegrouping && distToPlayer > 6.0f)
            {
                _isRegrouping = true;
                _regroupTarget = playerPos + (Vector3)_formationOffset;
            }
            else if (_isRegrouping && distToPlayer < 2.6f)
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

        private void UpdateCombat(float dt)
        {
            if (_spawnerView == null) return;

            float range = (Entity.Type == CompanionType.Warrior) ? 3.6f : 12.0f;
            _currentTarget = FindClosestMonster(range);
            if (_currentTarget == null || !Entity.CanAttack) return;

            Entity.TriggerAttack();

            Vector2 attackDir = new Vector2(
                (float)(_currentTarget.Position.X - Entity.Position.X),
                (float)(_currentTarget.Position.Y - Entity.Position.Y)).normalized;

            if (Entity.Type == CompanionType.Warrior)
            {
                ExecuteWarriorSlash(attackDir);
            }
            else
            {
                ExecuteRangerArrow(attackDir);
            }
        }

        private void ExecuteWarriorSlash(Vector2 attackDir)
        {
            float baseDamage = 35f;
            float actualDamage = Entity.CalculateDamage(baseDamage);
            float radius = 2.9f * (Entity.Owner != null ? Entity.Owner.Stats.AreaMultiplier : 1.0f);

            // Trigger Visual Arc & Greatsword Swing
            _slashBaseAngle = Mathf.Atan2(attackDir.y, attackDir.x) * Mathf.Rad2Deg;
            _slashVisualTimer = _slashDuration;
            if (_weaponSr != null) _weaponSr.sortingOrder = 30; // 몸 앞으로 올려서 대검 휘두름!
            if (_slashPivotGo != null)
            {
                _slashPivotGo.SetActive(true);
                float initialAngle = _slashBaseAngle - 60f;
                _slashPivotGo.transform.rotation = Quaternion.Euler(0f, 0f, initialAngle);
                if (_weaponPivotGo != null) _weaponPivotGo.transform.rotation = Quaternion.Euler(0f, 0f, initialAngle);
                if (_slashVisualSr != null) _slashVisualSr.color = Color.white;
            }

            var activeMonsters = _spawnerView.DomainSpawner?.ActiveMonsters;
            if (activeMonsters != null)
            {
                Vector2D myPos = Entity.Position;
                for (int i = 0; i < activeMonsters.Count; i++)
                {
                    var m = activeMonsters[i];
                    if (m == null || m.IsDead) continue;
                    if ((m.Position - myPos).SqrMagnitude <= radius * radius)
                    {
                        Vector2 toM = new Vector2((float)(m.Position.X - myPos.X), (float)(m.Position.Y - myPos.Y)).normalized;
                        if (Vector2.Dot(attackDir, toM) >= 0.25f)
                            m.TakeDamage(actualDamage, isCritical: false);
                    }
                }
            }
            _eventBus?.Publish(new PlaySoundEvent(SoundEffectType.SlashAttack));
        }

        private void ExecuteRangerArrow(Vector2 attackDir)
        {
            float actualDamage = Entity.CalculateDamage(22f);
            if (_projManager?.DomainManager != null)
            {
                _projManager.DomainManager.LaunchProjectile(
                    Entity.Position,
                    new Vector2D(attackDir.x, attackDir.y),
                    speed: 16f,
                    damage: actualDamage,
                    pierceCount: 999);
            }

            _eventBus?.Publish(new PlaySoundEvent(SoundEffectType.BowShoot));
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

        private MonsterEntity FindClosestMonster(float maxRange)
        {
            if (_spawnerView == null) return null;
            var activeList = _spawnerView.DomainSpawner?.ActiveMonsters;
            if (activeList == null || activeList.Count == 0) return null;

            MonsterEntity closest = null;
            float minSqrDist = maxRange * maxRange;
            Vector2D myPos = Entity.Position;

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
