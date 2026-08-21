# ⚔️ 칼(대검) 휘두르기 궤적 판정 및 공격 동기화 개선 계획서

## 🔍 문제 원인 정밀 분석

### 1. 문제 2 원인: 칼 궤적 반대편/360도 무관 피격 현상
- **도메인 판정 로직 결함**: `GreatswordSlashEffect.ApplyEffect`에서 타겟 방향 벡터나 부채꼴 각도(Arc/Sector Angle)를 전혀 계산하지 않고, `context.TargetGrid.QueryRadiusNonAlloc`를 통해 플레이어 중심 반경 내의 **360도 전 방향 모든 몬스터에게 무차별 데미지**를 주고 있었습니다.
- **결과**: 칼을 전방으로 휘두르는데도 등 뒤나 반대편에 있는 적들이 모두 피격되는 현상 발생.

### 2. 문제 1 원인: 칼 휘두르는 동작이 끝난 후 데미지가 들어가는 현상
- **View와 Domain 타이머의 완전한 비동기/불일치**:
  - `PlayerView`는 자체 `_slashCooldownTimer` (1.0초)로 혼자서 `_slashVisualTimer` (0.18초) 애니메이션을 돌리고 있었습니다.
  - 반면 실제 데미지를 주는 도메인 `CompositeSkill`은 `CooldownTrigger(1.2초)`로 1.2초마다 발동하고 있었습니다.
- **결과**: View에서 칼을 휙 휘두르고 애니메이션이 끝난(0.18초) 뒤, 0.2초 이상 지나서 도메인의 쿨타임이 차며 데미지가 들어가는 심각한 싱크 어긋남 발생.

---

## 🛠️ 해결 방안 및 설계

### 1. 전방 부채꼴 궤적 판정 (실제 보이는 것보다 살짝 넓은 150도 여유 범위)
- **공격 방향 벡터 계산**: 타겟 위치(또는 가장 가까운 적 방향)를 기준으로 공격 진행 방향 벡터 `forward` 산출.
- **부채꼴(Sector / Arc) 각도 판정**:
  - 시각적 스윙각: 120도 (-60° ~ +60°)
  - **실제 히트 판정각**: **150도 부채꼴** (전방 기준 ±75°, $\cos(75^\circ) \approx 0.2588$)
  - 보이는 궤적보다 살짝 넓게 설정하여 궤적 끝자락에 걸치거나 스치는 적도 시원하게 피격되도록 보장!
  - 몬스터와의 방향 벡터 내적(`dot = Dot(forward, dirToMonster)`)이 `0.2588` 이상인 전방 궤적상의 적만 데미지 적용.
  - **궤적 반대편(뒤쪽 $\text{dot} < 0$, 측면 뒤쪽)에 있는 적은 절대 피격되지 않음!**

### 2. 도메인 이벤트 기반 100% 완벽 동기화 (문제 1 해결)
- `PlayerSlashExecutedEvent` 도메인 이벤트 추가 (`PlayerId`, `CenterPosition`, `DirectionAngleDegrees`, `Radius`, `ArcAngleDegrees`).
- `SkillContext`에 `EventBus`를 연결하여, `GreatswordSlashEffect`가 발동되는 **정확한 프레임**에 `PlayerSlashExecutedEvent` 및 `PlaySoundEvent(SlashAttack)` 발행.
- `PlayerView`의 독립적인 1.0초 자체 쿨타임 타이머 제거 ➡️ `PlayerSlashExecutedEvent` 수신 시 정확한 방향으로 0.18초간 스윙 애니메이션 구동!
- **결과**: 칼을 휘두르는 바로 그 순간에만 데미지와 애니메이션이 100% 일치하여 발동!

---

## 📁 변경 대상 파일 및 컴포넌트

### 1. Domain Layer (`HappyShoot.Domain`)
- **[MODIFY]** `Assets/src/HappyShoot.Domain/Skills/ISkillComponents.cs`
- **[MODIFY]** `Assets/src/HappyShoot.Domain/Events/PlayerEvents.cs`
- **[MODIFY]** `Assets/src/HappyShoot.Domain/Skills/Effects/GreatswordSlashEffect.cs`
- **[MODIFY]** `Assets/src/HappyShoot.Domain/Entities/PlayerEntity.cs`
- **[MODIFY]** `Assets/src/HappyShoot.Domain/Entities/PlayerClassFactory.cs`

### 2. Presentation Layer (`HappyShoot.View`)
- **[MODIFY]** `Assets/src/HappyShoot.View/Player/PlayerView.cs`

### 3. Tests Layer (`HappyShoot.Domain.Tests`)
- **[NEW]** `Assets/tests/HappyShoot.Domain.Tests/Skills/GreatswordSlashTests.cs`

### 4. Docs & Architecture
- **[MODIFY]** `APP_MAP.md`
