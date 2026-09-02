# 스킬 가시성 전면 개선 및 대검/근접 타격감 극대화 계획서 (Skill Visibility & Melee Hit Juice Rework)

## 1. 개요 및 문제 분석
적들이 대량으로 몰려올 때 **대검 베기** 등의 스킬을 사용하면 칼과 검기 궤적이 적들에게 완전히 파묻혀 보이지 않는 문제가 발생하고 있습니다. 이로 인해 공격이 실제로 적중하고 있는지, 몬스터들을 시원하게 베고 있는지에 대한 시각적/체감적 타격감(Juice)이 크게 저하됩니다.

### 원인 분석
1. **Sorting Order(렌더링 순서) 역전 현상**:
   - `MonsterView` (적 몸체): `sortingOrder = 10`
   - `PlayerView` (플레이어 몸체): `sortingOrder = 0` (적 밑에 깔림)
   - `PlayerView` (대검/무기): `sortingOrder = 2` (적 밑에 깔림)
   - `PlayerView` (대검 베기 검기 아크): `sortingOrder = 3` (적 밑에 깔림)
   - `WhirlwindManagerView` (휠윈드 회전날): `sortingOrder = 4, 6` (적 밑에 깔림)
   - `ProjectileView` (관통 화살 등): `sortingOrder = 4` (적 밑에 깔림)
   - `OrbitingBladeView` (수호의 검): `sortingOrder = 7` (적 밑에 깔림)
   - `GroundStompManagerView` (지면강타 파편): `sortingOrder = 5` (적 밑에 깔림)
   - `MagicSkillManagerView` (서리폭발/연쇄번개 등): `sortingOrder = 4~9` (적 밑에 깔림)
   - **결과**: 적이 몰려오면 플레이어 캐릭터와 대검, 검기 궤적, 투사체, 회전날 등이 몬스터 몸통 뒤로 렌더링되어 완전히 가려짐.
2. **타격 피드백(Hit Spark & Impact Juice) 부재**:
   - 크리티컬 발생 시에만 크리티컬 스타버스트가 뜨고, 일반 슬래시/대검 베기 적중 시에는 적 위치에 찰지게 그어지는 베기 타격선(Slash Cut VFX)이나 임팩트 파티클이 없어 베는 맛이 부족함.
3. **대검 베기 궤적 비주얼 볼륨감**:
   - 150도 스윙 시 몬스터들 머리 위에서 빛나는 고휘도 림 라이트(Glowing Edge)와 선명한 잔상 궤적의 강조 필요.

---

## 2. 해결 및 개선 계획

### 1) 체계적인 2D Sorting Order 아키텍처 재정립
- **-1 ~ 0**: 바닥 그림자 (`BlobShadow`), 바닥 데칼
- **5**: 바닥 아이템 / 경험치 젬 (`ExpGemView`)
- **9 ~ 10**: 몬스터 그림자(9) 및 몬스터 몸체(10)
- **15 ~ 16**: 플레이어 몸체(15) 및 평상시 무기(16) -> 플레이어가 적 무리 속에 파묻히지 않음
- **22 ~ 28**: 플레이어 스킬/투사체 레이어
  - 수호의 검 (`OrbitingBladeView`): 22
  - 관통 화살 (`ProjectileView`): 24
  - 마법 스킬 (`MagicSkillManagerView` - Frost Nova, Chain Lightning, Ice Shards): 26
  - 지면강타 파편 (`GroundStompManagerView`): 26
  - 휠윈드 폭풍 (`WhirlwindManagerView`): 28
  - 블러드 이터 (`BloodEaterManagerView`): 29
- **30**: 근접 무기 공격 스윙 및 대검 베기 아크 (`PlayerView`의 `_swordSr` 스윙 중 & `_slashVisualSr`) -> **몬스터 머리 위 최상단에서 시원하게 베어 가르는 궤적 노출!**
- **32**: 슬래시 타격 이펙트 (`SlashHitVfxManagerView` - 신규)
- **35**: 크리티컬 스타버스트 (`CriticalHitVfxManagerView`)

### 2) 대검 베기 스프라이트 & 스윙 애니메이션 강화
- `WarriorSkillSpriteHelper.cs`:
  - `GetOrCreateSlashArcSprite`: 외곽 칼날에 초고휘도 백금/골드 림 라이트와 중심부 묵직한 오라를 부여하여 몬스터 떼 위에서 눈부시게 번쩍이는 시각 효과 적용.
  - `GetOrCreateBloodSlashArcSprite`: 블러드 이터 진화 스킬 역시 강렬한 루비/크림슨 블러드 엣지 적용.
- `PlayerView.cs`:
  - 대검 스윙 시 대검과 검기 아크의 sortingOrder를 30으로 설정하여 적들을 가르는 모션이 100% 보이도록 처리.

### 3) 묵직하고 찰진 'Slash Hit Spark VFX' (신규 타격 이펙트) 구축
- `SlashHitVfxManagerView.cs` (신규 생성, 제로 가비지 풀링 32개):
  - 대검 베기 및 근접 스킬 적중 시, 베인 몬스터 중심 위치에 **대각선 슬래시 컷 스파크(Diagonal Slash Spark) & 임팩트 섬광**을 몬스터 상단(sortingOrder 32)에 0.10초 동안 번쩍이게 생성.
  - 타격된 적마다 "촥! 촥!" 시원하게 베이는 시각적 타격감 극대화.
- `MonsterView.cs`:
  - 피격 시 순간적인 타격 스쿼시(Squash & Stretch) 및 탄성 반동을 주어 "잘 패고 있다"는 느낌 강화.

### 4) `GameBootstrap.cs` 및 `APP_MAP.md` 연동
- 신규 `SlashHitVfxManagerView` 인스턴스 자동 초기화 연결.
- `APP_MAP.md`에 새로운 아키텍처 및 렌더링 레이어 정리 반영.

---

## 3. 검증 계획
- 단위 테스트(`dotnet test`) 실행으로 도메인 및 뷰 모델 회귀 검증.
- 몬스터 대량 스폰 시 대검 베기, 휠윈드, 수호의 검, 화살 등의 궤적이 몬스터 상단에 선명하게 보이는지 검증.
