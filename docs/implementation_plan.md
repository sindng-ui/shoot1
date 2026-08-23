# 👾 경험치 증가율 비례 최대 몬스터 수 & 스폰 동적 스케일링 구현 계획서

형님! 강해진 플레이어의 성장에 맞춰 긴장감 넘치는 전투를 유지하기 위해, **플레이어가 레벨업할 때마다 다음 레벨 필요 경험치 증가 비율(Exp Scale Factor)에 1:1로 비례하여 필드 최대 몬스터 수(Max Monsters)와 스폰 빈도(Spawn Rate)가 함께 증가하는 시스템** 구현 계획을 제출합니다!

---

## 🎯 목표 및 핵심 메커니즘

1. **경험치 증가율 비례 몬스터 수 동적 연동**:
   - 1레벨 기본 필요 경험치: $E_1 = \text{CalculateRequiredExp}(1)$
   - 현재 L레벨 필요 경험치: $E_L = \text{CalculateRequiredExp}(L)$
   - **경험치 배율 계수 (Exp Scale)**:
     $$\text{ExpScale}(L) = \frac{E_L}{E_1}$$
   - **최대 몬스터 수 공식**:
     $$\text{CurrentMaxMonsters} = \text{Clamp}\left(\text{RoundToInt}(\text{BaseMaxMonsters}(time) \times \text{ExpScale}(L)), \text{BaseMaxMonsters}, \text{MaxPoolCapacity}\right)$$
     *(예: 샌드박스에서 성장률 1.5배 설정 시, 다음 레벨 필요 경험치가 1.5배 늘어나는 것과 완벽히 동일하게 몹 수 한도도 1.5배씩 계속 누적 확장!)*

2. **스폰 속도(Spawn Interval) 동시 가속**:
   - 몹 수 한도만 늘어나고 스폰이 느리면 필드가 차지 않으므로, 몹 수 스케일에 맞춰 스폰 주기 가속:
     $$\text{CurrentSpawnInterval} = \frac{\text{BaseSpawnInterval}(time)}{\text{Clamp}(\sqrt{\text{ExpScale}(L)}, 1.0f, 3.5f)}$$

3. **대용량 몬스터 풀링 확장 (Zero-Allocation & 60FPS 성능 보장)**:
   - `MonsterSpawnerView` 및 `MonsterSpawner`의 `MaxPoolCapacity`를 **512 ➔ 1,200**으로 대폭 확장.
   - 공간 분할 그리드(`SpatialGrid2D`) 셀 쿼리로 1,200마리 대군단도 프레임 드랍 없이 부드럽게 구동.

4. **전투 & 밸런스 샌드박스 연동**:
   - 샌드박스 [경험치 튜닝 탭]에서 `ExpGrowthFactor` 변경 시 실시간으로 최대 몹 수가 자동 재계산됨.
   - [몬스터 탭]에 `레벨 비례 몹수 스케일링 활성화(ON/OFF)` 및 `최대 몹수 상한선(500~1200)` 슬라이더 추가.

---

## 📂 변경 예정 파일 목록

### 1. 도메인 (Domain Layer)
- [MODIFY] [MonsterSpawner.cs](file:///c:/AntigravityWorkspace/shoot1/Assets/src/HappyShoot.Domain/Entities/MonsterSpawner.cs)
  - 풀 기본 용량 확장 (1,200) 및 메모리 버퍼 최적화.
- [MODIFY] [SkillConfigModels.cs](file:///c:/AntigravityWorkspace/shoot1/Assets/src/HappyShoot.Domain/Skills/SkillConfigModels.cs)
  - `ExpConfig` 또는 `MonsterConfig`에 `EnableLevelExpScaling` 및 `MaxSpawnCapLimit` 필드 추가.

### 2. 뷰 & 시스템 연동 (View Layer)
- [MODIFY] [MonsterSpawnerView.cs](file:///c:/AntigravityWorkspace/shoot1/Assets/src/HappyShoot.View/Monsters/MonsterSpawnerView.cs)
  - `LevelSystem` 바인딩 추가 및 `GetMaxMonsters`, `GetSpawnInterval`에 `ExpScale` 공식 적용.
  - `MaxPoolCapacity`를 1,200으로 확장 및 뷰 풀링 프리웜.
- [MODIFY] [GameBootstrap.cs](file:///c:/AntigravityWorkspace/shoot1/Assets/src/HappyShoot.View/Bootstrap/GameBootstrap.cs)
  - `spawnerView.Initialize(playerView, levelSystem)`으로 레벨 시스템 참조 전달.
- [MODIFY] [SkillTuningRowConfigurator.cs](file:///c:/AntigravityWorkspace/shoot1/Assets/src/HappyShoot.View/UI/SkillTuningRowConfigurator.cs)
  - 샌드박스 경험치/몬스터 탭에 레벨 비례 몹 수 스케일링 튜닝 슬라이더 추가.

---

## 🧪 검증 계획

### 1. 단위 테스트 (Automated Tests)
- `MonsterSpawnerTests.cs` & `LevelSystemTests.cs`: 레벨업 시 필요 경험치 증가량과 몹 수 배율 계산 공식 검증.
- `dotnet test shoot1.sln` 실행 및 통과 확인.

### 2. 수동 검증 (Manual Verification)
- 유니티 실행 후 개발자 모드(`Dev Mode`)에서 [레벨업] 버튼을 눌렀을 때 필드의 최대 몹 수와 스폰 수가 경험치 배율에 맞춰 시원하게 증가하는지 확인.
- 샌드박스에서 경험치 성장률(`ExpGrowthFactor`)을 변경했을 때 몹 스폰 한도가 실시간 반영되는지 확인.
- 500줄 규칙 준수 여부 및 FPS 안정성 점검.

---

형님! 계획서를 확인하시고 **Proceed** 버튼을 눌러주시면 즉시 구현에 착수하겠습니다!
