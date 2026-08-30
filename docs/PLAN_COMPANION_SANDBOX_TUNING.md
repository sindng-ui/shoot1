# [구현 계획서] 전투 & 밸런스 샌드박스 컴패니언(AI 동료) 튜닝 설정 시스템

형님! 지시하신 **컴패니언 샌드박스 튜닝 설정 및 추천 파라미터 전면 연동**에 대한 정밀 구현 계획서입니다.  
본 계획서는 샌드박스 UI(`[⚙️ 시스템]` 탭)에 컴패니언 전용 튜닝 화면을 추가하여, 형님이 실시간으로 슬라이더를 조작하며 동료들의 성능, 행동 반경, 전술 AI를 자유롭게 튜닝하고 파일(`skill_configs.json`)에 영구 저장할 수 있도록 구축합니다.

---

## 1. 샌드박스 튜닝 파라미터 8종 상세 명세

| 번호 | 항목명 (UI 라벨) | 조절 범위 | 기본값 | 실시간 게임플레이 영향 |
|---|---|---|---|---|
| 1 | **최종 공격력 보정 (Final Dmg Scale)** | `0% ~ 100%` | **33.3%** | 샌드박스 기본 스킬 데미지에 곱해지는 최종 데미지 배율 |
| 2 | **패시브 효과 보정 (Passive Scale)** | `0% ~ 100%` | **33.3%** | 마법사의 공격력/쿨감/범위 패시브 보너스를 수혜받는 계수 |
| 3 | **주인공 주변 재합류 반경 (Regroup Radius)** | `1.0m ~ 10.0m` | **5.0m** | 마법사가 이 거리 이상 멀어지면 재합류를 시작하는 기준 거리 |
| 4 | **마법사 호위 안착 반경 (Arrival Distance)** | `1.0m ~ 5.0m` | **2.6m** | 재합류 중 마법사 곁 이 거리 이내로 들어오면 멈추고 독립 전투 복귀 |
| 5 | **동료 이동속도 배율 (Speed Multiplier)** | `50% ~ 200%` | **100%** | 마법사 걷는 속도 대비 동료의 이동속도 (돌격 전차 vs 호위) |
| 6 | **전사 몬스터 추적 사거리 (Warrior Engage)** | `1.0m ~ 6.0m` | **3.8m** | 전사가 스스로 인지하고 뛰어가서 칼로 써는 자율 교전 반경 |
| 7 | **궁수 원거리 저격 사거리 (Ranger Sniping)** | `5.0m ~ 18.0m` | **12.0m** | 궁수가 화면 안/밖 몬스터를 조준 사격하는 최대 사거리 |
| 8 | **타겟팅 우선순위 (Targeting Priority)** | `0 ~ 1 (토글)` | **0 (자신 근접)** | 0 = 자신에게 가장 가까운 적(광전사), 1 = 마법사에게 가장 가까운 적(철통 경호원) |

---

## 2. 500줄 규칙 준수 모듈화 설계

기존 튜닝 파일들이 450줄 내외이므로, 파일 비대화를 방지하기 위해 **전담 UI 컨피규레이터 헬퍼**를 분리합니다:

```
Assets/src/HappyShoot.Domain/Skills/
  └── SkillConfigModels.cs (CompanionTuningConfig 클래스 및 SkillConfigData에 필드 추가 - 390줄)

Assets/src/HappyShoot.View/UI/
  ├── SkillTuningCompanionConfigurator.cs [NEW] (컴패니언 8종 슬라이더 UI 생성 및 실시간 바인딩 - 150줄)
  ├── SkillTuningUiBuilder.cs (AllSkillDefinitions에 "companion_tuning" 1줄 등록 - 446줄)
  └── SkillTuningUiView.cs ("companion_tuning" 선택 시 Configurator 호출 - 490줄)

Assets/src/HappyShoot.View/Companion/
  ├── CompanionView.cs (재합류 반경, 안착거리, 이속배율, 교전사거리, 경호 타겟팅 실시간 참조 - 438줄 유지)
  └── CompanionSkillExecutor.cs (최종 데미지 배율 실시간 참조 - 225줄 유지)
```

---

## 3. 세부 구현 단계

### 1) [MODIFY] [SkillConfigModels.cs](file:///k:/unityprojects/shoot1/shoot1/Assets/src/HappyShoot.Domain/Skills/SkillConfigModels.cs)
- `CompanionTuningConfig` 클래스 정의 (기본값 설정 및 `[Serializable]` 지원).
- `SkillConfigData.Companion` 프로퍼티 추가 및 JSON 영구 직렬화 지원.

### 2) [NEW] [SkillTuningCompanionConfigurator.cs](file:///k:/unityprojects/shoot1/shoot1/Assets/src/HappyShoot.View/UI/SkillTuningCompanionConfigurator.cs)
- 샌드박스에서 슬라이더 8종을 동적으로 생성하고 `SkillConfigRepository.Instance.GetConfig().Companion`과 양방향 바인딩.
- 슬라이더 조작 시 인게임 플레이어와 동료들에게 실시간 즉시 반영 (`Pull/Push` 동기화).

### 3) [MODIFY] [SkillTuningUiBuilder.cs](file:///k:/unityprojects/shoot1/shoot1/Assets/src/HappyShoot.View/UI/SkillTuningUiBuilder.cs)
- `AllSkillDefinitions`의 `system` 카테고리에 `("companion_tuning", "👥 AI 동료 튜닝", "system", false)` 1줄 추가.

### 4) [MODIFY] [SkillTuningUiView.cs](file:///k:/unityprojects/shoot1/shoot1/Assets/src/HappyShoot.View/UI/SkillTuningUiView.cs)
- `SelectSkill("companion_tuning")` 시 `SkillTuningCompanionConfigurator.BuildSliders(...)` 호출 연동.

### 5) [MODIFY] [CompanionEntity.cs](file:///k:/unityprojects/shoot1/shoot1/Assets/src/HappyShoot.Domain/Entities/CompanionEntity.cs)
- `CalculateFinalDamage`와 `GetEffectiveAttackPowerMultiplier`에서 샌드박스의 `FinalDamageScale`과 `PassiveScale`을 실시간 참조하도록 연동.

### 6) [MODIFY] [CompanionView.cs](file:///k:/unityprojects/shoot1/shoot1/Assets/src/HappyShoot.View/Companion/CompanionView.cs)
- 재합류 트리거 거리(`RegroupRadius`), 안착 거리(`RegroupArrivalDistance`), 이동속도 배율(`MoveSpeedMultiplier`), 교전 사거리(`WarriorEngageRange`, `RangerSnipingRange`), 타겟팅 우선순위(`PrioritizeProtectWizard`)를 샌드박스에서 실시간 참조하도록 갱신.

---

## 4. 검증 계획

### 자동화 테스트 (Automated Tests)
- `CompanionTests.cs`에 샌드박스 설정 실시간 변경 시 데미지 및 패시브 배율 변화 테스트 추가.

### 수동 검증 (Manual Verification)
1. **샌드박스 UI 오픈**: 인게임에서 `ESC` 또는 샌드박스 아이콘 클릭 ➔ `[⚙️ 시스템]` 탭 클릭 ➔ `[👥 AI 동료 튜닝]` 선택.
2. **슬라이더 조작 및 실시간 체감**:
   - **반경 조절**: 주변 반경을 2m로 줄이면 마법사 조금만 움직여도 즉시 따라오고, 10m로 늘리면 저 멀리서도 꿋꿋이 싸우는지 확인.
   - **공속/데미지 조절**: 최종 공격력 보정을 100%로 올리면 마법사와 동급 데미지가 터지는지 확인.
   - **이속 배율 조절**: 이속 180%로 올리면 마법사보다 빠르게 슝슝 뛰어오는지 확인.
   - **타겟팅 우선순위 조절**: 경호 모드로 전환 시 마법사 곁에 붙은 몹을 먼저 베어버리는지 확인.
3. **파일 저장 및 복원**: `💾 파일에 반영 (Save Config)` 클릭 후 재시작 시 설정값 유지 확인, `🔄 기본값 복원 (Restore)` 시 33%/5m/2.6m로 초기화 확인.
