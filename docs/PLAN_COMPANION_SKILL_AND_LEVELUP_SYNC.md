# [구현 계획서] 컴패니언 샌드박스 스킬 연동 및 마법사 레벨업 성장 동기화 시스템

형님! 지시하신 **컴패니언 스킬/성장/샌드박스 밸런스 연동**에 대한 정밀 구현 계획서입니다.  
본 계획서는 컴패니언의 공격 속도와 데미지가 샌드박스(`SkillConfig`)를 100% 따르도록 정규화하고, 마법사의 액티브/패시브 성장에 맞춰 동료들도 유기적으로 함께 성장하는 완전한 파티 시스템을 구축합니다.

---

## 1. 요구사항 및 해결 방안

### ① 기본 스킬 공속 및 설정값 ➔ 샌드박스(SkillConfig) 연동
- **문제**: 기존 컴패니언의 쿨다운(`Warrior: 1.0s`, `Ranger: 0.75s`)과 데미지가 하드코딩되어 있어 샌드박스 튜닝이 반영되지 않고 공속이 지나치게 빨랐음.
- **해결**:
  - `SkillConfigRepository.Instance.GetConfig()`를 실시간 참조합니다.
  - 🛡️ **전사 (대검 베기)**: `cfg.Slash.Cooldown` (기본 **1.2초**), `cfg.Slash.Damage` (기본 **35f**), `cfg.Slash.Radius` (기본 **2.5f**), `cfg.Slash.ArcAngle` (기본 **150°**)
  - 🏹 **궁수 (관통 화살)**: `cfg.Bow.Cooldown` (기본 **0.8초**), `cfg.Bow.Damage` (기본 **25f**), `cfg.Bow.Speed` (기본 **16f**), `cfg.Bow.ArrowCount` (기본 **1개**)
  - 샌드박스에서 수치를 조정하면 동료들의 스킬에도 실시간으로 100% 즉시 반영됩니다.

### ② 마법사 신규 액티브 스킬 획득 시 ➔ 컴패니언 신규 스킬 랜덤 1개 획득
- 마법사가 `NewActiveSkill` (또는 `EvolveSkill`)을 획득했을 때:
  - 🛡️ **전사 액티브 풀**: `slash`(대검 베기, 기본 보유) ➔ 미보유 스킬인 `ground_stomp`(지면 강타), `whirlwind`(휠윈드) 중 **1개를 무작위 획득**!
  - 🏹 **궁수 액티브 풀**: `bow`(관통 화살, 기본 보유) ➔ 미보유 스킬인 `glaive`(칼바람 글레이브), `arrow_rain`(화살비) 중 **1개를 무작위 획득**!
  - 만약 3개 액티브를 이미 전부 보유했다면, 보유 중인 스킬 중 1개를 레벨업합니다.

### ③ 마법사 액티브 스킬 레벨업 시 ➔ 컴패니언 보유 액티브 스킬 1개 레벨업
- 마법사가 `UpgradeActiveSkill`을 선택했을 때:
  - 컴패니언이 현재 보유하고 있는 액티브 스킬 중 아직 MAX 레벨(Lv.5)이 아닌 스킬 1개를 무작위로 선택하여 **레벨업(Lv.1 ➔ Lv.2 ➔ ... ➔ Lv.5)** 시킵니다.

### ④ 마법사 패시브 스킬 획득/레벨업 시 ➔ 컴패니언에게 1/3 효과만 적용
- 마법사가 패시브(`NewPassive`, `UpgradePassive`)를 획득하거나 레벨업하면:
  - 마법사의 패시브 증강 수치(공격력, 쿨감, 이속, 치명타, 범위 등) 중 **정확히 1/3 (0.333x)** 만큼만 컴패니언의 스탯에 보너스로 가산됩니다.
  - 예: 마법사 공격력 패시브 +15% ➔ 컴패니언 공격력 +5%

### ⑤ 컴패니언 최종 데미지 = [샌드박스 기본 데미지] × [패시브(1/3) & 보너스] × 1/3
- `actualDamage = (baseSkillDamage * (1.0f + companionBonusAP)) * (1.0f / 3.0f)`
- 샌드박스 기본 스킬 데미지에 패시브(1/3 효과)를 적용한 뒤, **최종적으로 1/3을 곱하여 밸런싱된 최최종 데미지를 산출**합니다.

---

## 2. 아키텍처 및 모듈화 설계 (500줄 규칙 준수)

파일이 거대해지는 것을 철저히 방지하기 위해 역할을 명확히 분리합니다:

```
Assets/src/HappyShoot.Domain/
  ├── Entities/
  │     ├── CompanionEntity.cs (스킬 보유 목록, 레벨, 쿨다운, 패시브 1/3 스탯 계산 - 130줄)
  │     └── CompanionSkillInstance.cs [NEW] (개별 컴패니언 스킬 쿨타임/레벨 데이터 - 60줄)
  └── Events/
        └── CompanionEvents.cs [NEW] (마법사 레벨업 보상 동기화 이벤트 - 40줄)

Assets/src/HappyShoot.View/
  ├── Companion/
  │     ├── CompanionView.cs (FSM 이동 및 공격 오케스트레이션 - 420줄 유지)
  │     ├── CompanionSkillExecutor.cs [NEW] (지면강타/휠윈드/글레이브/화살비 발사 및 VFX 전담 - 150줄)
  │     └── CompanionManagerView.cs (이벤트 버스 구독 및 컴패니언 성장 동기화 브릿지 - 140줄)
  └── UI/
        └── LevelUpUiView.cs (보상 선택 시 CompanionRewardSyncEvent 발행 연동 - 430줄 유지)
```

---

## 3. 세부 변경 파일 목록

### 1) [NEW] [CompanionSkillInstance.cs](file:///k:/unityprojects/shoot1/shoot1/Assets/src/HappyShoot.Domain/Entities/CompanionSkillInstance.cs)
- 컴패니언이 보유한 스킬 ID, 레벨(1~5), 쿨다운 타이머를 관리하는 순수 C# 도메인 클래스.

### 2) [MODIFY] [CompanionEntity.cs](file:///k:/unityprojects/shoot1/shoot1/Assets/src/HappyShoot.Domain/Entities/CompanionEntity.cs)
- 스킬 컬렉션(`IReadOnlyList<CompanionSkillInstance> Skills`) 추가.
- `LearnNewSkillRandomly()`, `LevelUpRandomSkill()` 메서드 추가.
- 패시브 1/3 계수 반영 스탯 계산 로직(`GetEffectiveCooldown()`, `CalculateFinalDamage()`) 탑재.

### 3) [NEW] [CompanionEvents.cs](file:///k:/unityprojects/shoot1/shoot1/Assets/src/HappyShoot.Domain/Events/CompanionEvents.cs)
- `CompanionRewardSyncEvent(RewardCategory category, string rewardId)` 도메인 이벤트 정의.

### 4) [NEW] [CompanionSkillExecutor.cs](file:///k:/unityprojects/shoot1/shoot1/Assets/src/HappyShoot.View/Companion/CompanionSkillExecutor.cs)
- `CompanionView`의 500줄 초과를 방지하기 위해, 전사(대검/지면강타/휠윈드) 및 궁수(화살/글레이브/화살비) 스킬 발사 및 이펙트 연출을 전담하는 헬퍼 클래스.

### 5) [MODIFY] [CompanionView.cs](file:///k:/unityprojects/shoot1/shoot1/Assets/src/HappyShoot.View/Companion/CompanionView.cs)
- 하드코딩된 공격 쿨타임을 제거하고, 보유한 스킬들의 쿨다운을 독립적으로 틱(Tick)하며 준비된 스킬을 순차 발동.
- 샌드박스(`SkillConfigRepository`)의 최신 설정값과 100% 동기화.

### 6) [MODIFY] [CompanionManagerView.cs](file:///k:/unityprojects/shoot1/shoot1/Assets/src/HappyShoot.View/Companion/CompanionManagerView.cs)
- `CompanionRewardSyncEvent`를 구독하여 마법사 성장 시 소환된 동료들에게 스킬 부여/레벨업/패시브 동기화 일괄 적용.

### 7) [MODIFY] [LevelUpUiView.cs](file:///k:/unityprojects/shoot1/shoot1/Assets/src/HappyShoot.View/UI/LevelUpUiView.cs)
- `SelectOption` 실행 시 `EventBus.Publish(new CompanionRewardSyncEvent(...))` 1줄 발행 연동.

### 8) [MODIFY] [DevSkillSelectorUiView.cs](file:///k:/unityprojects/shoot1/shoot1/Assets/src/HappyShoot.View/UI/DevSkillSelectorUiView.cs)
- 개발자 콘솔에서 치트로 스킬을 획득/레벨업할 때도 동료 동기화 이벤트 발행.

---

## 4. 검증 계획

### 자동화 테스트 (Automated Tests)
- `CompanionTests.cs`에 신규 테스트 케이스 추가:
  1. 마법사 신규 액티브 획득 시 전사/궁수가 고유 액티브 스킬을 정상 획득하는지 검증.
  2. 마법사 액티브 레벨업 시 동료 스킬이 정상 레벨업(Lv.1 ➔ Lv.2)되는지 검증.
  3. 마법사 패시브 스탯 증가 시 동료에게 1/3 배율만 가산되는지 검증.
  4. 샌드박스 기본 데미지 변경 시 1/3 최종 데미지 산출 검증.
- `wsl bash -c "dotnet test"` 실행으로 모든 테스트 패스 확인.

### 수동 검증 (Manual Verification)
1. **공속/샌드박스 검증**: 유니티 에디터 실행 후 전사의 대검 베기(1.2s)와 궁수의 활 사격(0.8s)이 정상 템포로 동작하는지 확인. 샌드박스 UI에서 쿨다운 조정 시 즉시 체감 확인.
2. **신규 스킬 획득 검증**: 마법사가 레벨업하여 새 액티브를 고를 때, 전사가 지면강타나 휠윈드를 배우고, 궁수가 글레이브나 화살비를 시전하는지 확인.
3. **데미지 검증**: 동료가 가하는 데미지가 마법사 샌드박스 설정 및 패시브의 1/3로 정확히 뜨는지 인게임 데미지 텍스트 확인.
