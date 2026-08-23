# 🎯 크리티컬 시스템 구현, 신규 패시브 추가, 연출 강화 및 테스트모드 크리티컬 튜닝 계획서

형님, 요청해주신 **테스트 모드(전투 샌드박스)에서의 캐릭터 크리티컬 수치(확률 / 배율) 실시간 튜닝 지원**까지 완벽하게 포함하여 계획서를 업데이트했습니다!

---

## 💡 주요 구현 항목 요약

1. **캐릭터 기본 크리티컬 확률 10% (Ranger 20%) 지정 및 실시간 롤링 시스템 구축**
   - 모든 클래스 기본 크리티컬 확률 `0.10f` (10%), 기본 배율 `1.50f` (150%) 적용
   - 궁수(Ranger) 특화: 크리티컬 확률 `0.20f` (20%), 배율 `1.75f` (175%) 적용
   - `PlayerEntity` 및 `SkillContext`에 무할당 `RollDamage(float rawDamage)` 크리티컬 계산 엔진 탑재

2. **신규 패시브 스킬 추가: `치명타의 눈 (passive_crit)`**
   - 효과: **크리티컬 확률 +8% & 크리티컬 데미지 +5% 증가** (레벨당 누적, 최대 Lv.5 시 확률 +40%, 데미지 +25%)
   - `GameBootstrap`, `SkillRewardManager`, `DevSkillSelectorUiView` 연동
   - `RewardIconHelper`에 80x80 고해상도 황금빛 치명타 조준경 픽셀아트 아이콘 구현

3. **🧪 테스트 모드(전투 & 밸런스 샌드박스) 크리티컬/스탯 튜닝 탭 신설**
   - 샌드박스 상단 탭에 **`🎯치명/스탯` 탭** 추가
   - **🎯 크리티컬 확률 (Crit Chance)**: `0% ~ 100%` (0.01 단위 실시간 조절)
   - **💥 크리티컬 데미지 배율 (Crit Damage Multiplier)**: `1.0x ~ 5.0x` (0.05 단위 실시간 조절)
   - **⚔️ 기본 공격력 배율 (Attack Power)**: `0.5x ~ 5.0x`
   - **🛡️ 방어력 & 🏃 이동속도 & ⏱️ 쿨타임 감소율** 실시간 조절 지원

4. **크리티컬 전용 시각 이펙트(VFX) 구현 (`CriticalHitVfxManagerView`)**
   - 무할당 32개 뷰 풀링 기반의 초경량 고성능 VFX 시스템
   - 크리티컬 적중 위치에 번쩍이는 황금빛 십자 섬광(Cross Flash) + 8방향 비산 스타 버스트 스파크 연출

5. **몬스터 피격 시 흔들림(Shake / Squash & Stretch) 차별화 강화**
   - 일반 피격: 가벼운 젤리 탄성(Jolt 0.06s) 및 화이트 플래시
   - 크리티컬 피격: **일반 공격 대비 2배 이상의 강렬한 찌그러짐/팽창(Squash 0.45, Stretch -0.35)**, **무작위 틸트 회전(Tilt +-18도)**, **황금빛 번쩍임 플래시(Flash 0.14s)** 및 반동 셰이크 연출

6. **크리티컬 데미지 숫자 플로팅 텍스트 대폭 강화**
   - 일반 데미지: 흰색(24pt), 부드러운 상승
   - 크리티컬 데미지: **44pt 초대형 볼드 폰트**, **선명한 네온 골드 텍스트(`CRIT {dmg}` 또는 `{dmg}!`)**, **스폰 즉시 1.45배 튀어올랐다가 수축하는 바운스 팝(Pop Animation)**

---

## 🚀 제안드리는 추가 아이디어

> [!TIP]
> 1. **치명타 전용 타격 SFX (Crisp Crit Sound)**: 치명타 발생 시 일반 둔탁한 타격음과 함께 맑고 경쾌한 고음의 '챙!' 치명타 사운드를 믹싱하여 손맛을 극대화합니다.
> 2. **미세 역경직 (Micro Hit-Stop)**: 크리티컬 적중 시 `HitStopManager`를 통해 0.04초의 찰나의 순간 역경직을 부여하여 타격 쾌감을 완성합니다.

---

## 📂 Proposed Changes

Grouped by layer & component:

### 1. Pure C# Domain Layer (`HappyShoot.Domain`)

#### [MODIFY] [CharacterStats.cs](file:///c:/AntigravityWorkspace/shoot1/Assets/src/HappyShoot.Domain/Entities/CharacterStats.cs)
- `CharacterStats.Default`의 기본 `critChance`를 `0.05f` -> `0.10f` (10%)로 상향

#### [MODIFY] [PlayerClassFactory.cs](file:///c:/AntigravityWorkspace/shoot1/Assets/src/HappyShoot.Domain/Entities/PlayerClassFactory.cs)
- 전사/마법사 기본 크리티컬 확률 `0.10f`, 궁수 `0.20f` (20%) 및 배율 `1.75f`로 설정

#### [MODIFY] [PlayerEntity.cs](file:///c:/AntigravityWorkspace/shoot1/Assets/src/HappyShoot.Domain/Entities/PlayerEntity.cs)
- `RollDamage(float rawDamage)` 크리티컬 계산 엔진 추가

#### [MODIFY] [ISkillComponents.cs](file:///c:/AntigravityWorkspace/shoot1/Assets/src/HappyShoot.Domain/Skills/ISkillComponents.cs)
- `SkillContext`에 `RollDamage(float rawDamage)` 헬퍼 메서드 추가

#### [MODIFY] [MonsterEvents.cs](file:///c:/AntigravityWorkspace/shoot1/Assets/src/HappyShoot.Domain/Events/MonsterEvents.cs)
- `MonsterDamagedEvent`에 `public readonly bool IsCritical;` 필드 추가 (기존 생성자 하위 호환 유지)

#### [MODIFY] [MonsterEntity.cs](file:///c:/AntigravityWorkspace/shoot1/Assets/src/HappyShoot.Domain/Entities/MonsterEntity.cs)
- `TakeDamage(float damage, bool isCritical = false)` 시 `MonsterDamagedEvent`에 `isCritical` 전송

#### [MODIFY] [All Skill Effects & Projectiles](file:///c:/AntigravityWorkspace/shoot1/Assets/src/HappyShoot.Domain/Skills/Effects/)
- `GreatswordSlashEffect.cs`, `BloodEaterEffect.cs`, `WhirlwindEffect.cs`, `GroundStompEffect.cs`, `FireballEffect.cs`, `FrostNovaEffect.cs`, `ChainLightningEffect.cs`, `MeteorStrikeEffect.cs`, `OrbitingBladesEffect.cs`, `PiercingArrowEffect.cs`, `StormArrowEffect.cs`, `ProjectileEntity.cs`
- 피해 적용 시 `context.RollDamage()` 또는 투사체 자체 크리티컬 롤링을 통해 `monster.TakeDamage(dmg, isCrit)` 호출

#### [MODIFY] [DamageTextManager.cs](file:///c:/AntigravityWorkspace/shoot1/Assets/src/HappyShoot.Domain/UI/DamageTextManager.cs)
- `OnMonsterDamaged`에서 `evt.IsCritical` 플래그를 그대로 수신하여 정확한 크리티컬 텍스트 생성

---

### 2. Unity Presentation Layer (`HappyShoot.View`)

#### [NEW] [CriticalHitVfxManagerView.cs](file:///c:/AntigravityWorkspace/shoot1/Assets/src/HappyShoot.View/Effects/CriticalHitVfxManagerView.cs)
- 무할당 32개 풀링 기반의 황금빛 십자 섬광 + 8방향 비산 스타 버스트 크리티컬 이펙트 뷰 매니저

#### [MODIFY] [MonsterView.cs](file:///c:/AntigravityWorkspace/shoot1/Assets/src/HappyShoot.View/Monsters/MonsterView.cs)
- `OnHitFeedback(bool isCritical = false)`: 크리티컬 시 강력한 Squash & Stretch, 각도 셰이크, 황금 플래시

#### [MODIFY] [MonsterSpawnerView.cs](file:///c:/AntigravityWorkspace/shoot1/Assets/src/HappyShoot.View/Monsters/MonsterSpawnerView.cs)
- `OnMonsterDamaged` 수신 시 `view.OnHitFeedback(evt.IsCritical)` 전달

#### [MODIFY] [DamageTextView.cs](file:///c:/AntigravityWorkspace/shoot1/Assets/src/HappyShoot.View/UI/DamageTextView.cs)
- 크리티컬 시 44pt 대형 볼드 폰트, 황금 네온 컬러, `!` 느낌표 및 바운스 팝 애니메이션

#### [MODIFY] [SkillTuningUiBuilder.cs](file:///c:/AntigravityWorkspace/shoot1/Assets/src/HappyShoot.View/UI/SkillTuningUiBuilder.cs)
- 테스트 모드(전투 샌드박스) 상단 탭에 `🎯치명/스탯 (crit_tuning)` 탭 추가

#### [MODIFY] [SkillTuningRowConfigurator.cs](file:///c:/AntigravityWorkspace/shoot1/Assets/src/HappyShoot.View/UI/SkillTuningRowConfigurator.cs)
- `crit_tuning` 선택 시 플레이어의 `CritChance(0~100%)`, `CritDamageMultiplier(1.0x~5.0x)`, `AttackPower`, `MoveSpeed` 등 슬라이더 바인딩

#### [MODIFY] [RewardIconHelper.cs](file:///c:/AntigravityWorkspace/shoot1/Assets/src/HappyShoot.View/Utils/RewardIconHelper.cs)
- `DrawCritEyeIcon` 신규 패시브 80x80 황금 조준경 픽셀아트 아이콘 생성

#### [MODIFY] [GameBootstrap.cs](file:///c:/AntigravityWorkspace/shoot1/Assets/src/HappyShoot.View/Bootstrap/GameBootstrap.cs)
- `RegisterAllPassives`에 `passive_crit` (치명타의 눈) 등록, `CriticalHitVfxManagerView` 부트스트랩 생성 및 바인딩

#### [MODIFY] [DevSkillSelectorUiView.cs](file:///c:/AntigravityWorkspace/shoot1/Assets/src/HappyShoot.View/UI/DevSkillSelectorUiView.cs)
- 개발자 도구 패시브 목록에 `passive_crit` 추가

---

### 3. Architecture & Documentation Layer

#### [MODIFY] [APP_MAP.md](file:///c:/AntigravityWorkspace/shoot1/APP_MAP.md)
- 크리티컬 시스템, 신규 패시브, 크리티컬 VFX 매니저, 테스트 모드 크리티컬 튜닝, 피격 연출 등 신규/수정 인터페이스 100% 반영

---

## 🧪 Verification Plan

### Automated Tests
1. `dotnet test HappyShoot.Domain.Tests.csproj` (또는 NUnit 테스트 스위트)
2. `PassiveItemsTests.cs`: `passive_crit` 레벨별 스탯 증가 (CritChance +8%, CritDamageMultiplier +5%) 검증 테스트 추가
3. `CriticalDamageTests.cs`: 기본 크리티컬 확률(10%/20%), `RollDamage` 연산 및 `MonsterDamagedEvent`의 `IsCritical` 이벤트 페이로드 검증 단위 테스트 신설

### Manual Verification
1. 인게임 전투 샌드박스(`SkillTuningUiView`)에서 `🎯치명/스탯` 탭을 열고 크리티컬 확률을 100%로 설정하여 모든 타격이 크리티컬로 터지는지 검증
2. 크리티컬 확률 0%로 설정하여 크리티컬이 전혀 발생하지 않는지 검증
3. 몬스터 타격 시 일반 타격(흰색 숫자)과 크리티컬 타격(대형 황금 숫자 + 십자 스타 버스트 VFX + 강렬한 몬스터 흔들림) 정상 발동 확인
4. 레벨업 보상에서 `치명타의 눈` 패시브 선택 시 정상 누적 확인
