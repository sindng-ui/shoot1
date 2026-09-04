# [계획서] 횡스크롤 차원 모드: 골드 러시(골드 파밍 & 3지선다 차단) 및 초고속 잔상 이펙트

형님! 횡스크롤 모드에서 온전히 질주와 파밍의 재미를 만끽하실 수 있도록, **경험치 구슬 및 3지선다 팝업 방해를 차단하고 황금 코인이 쏟아지는 '골드 러시' 시스템**과 **부스트 링 획득 시 마법사 뒤로 찬란하게 펼쳐지는 '초고속 고스트 잔상'** 연출을 준비했습니다.

---

## 🎯 핵심 구현 목표

1. **경험치 구슬 차단 & 골드 드랍 전환 (골드 러시)**
   - 횡스크롤 모드에서는 몬스터 처치 시 경험치 구슬 대신 **빛나는 황금 코인(Gold Coin)**이 쏟아져 나옵니다.
   - 플레이어가 접근하면 자석처럼 빨려들어와 즉시 골드가 누적됩니다.
   - 경험치가 오르지 않으므로 게임 템포를 끊는 **3지선다 레벨업 팝업이 일절 발생하지 않습니다**.
   - 하늘에서 떨어지는 보석 소나기(`FallingGemShowerView`)도 고화질 황금 코인 스프라이트(`CustomResourceSpriteLoader.TryGetGoldCoinSprite()`)로 변환되어 대량의 골드를 시원하게 쓸어담을 수 있습니다.

2. **스피드 부스트 링 획득 시 초고속 고스트 잔상 (Afterimage Ghost Trail)**
   - 스피드 부스트 링 통과 시 3.5초간 마법사 뒤로 푸른빛/비전 에메랄드빛의 **고스트 트레일 잔상**이 0.05초 간격으로 연속 생성되어 환상적인 스피드감을 선사합니다.
   - 기존 검증된 `PlayerDashGhostTrail` 시스템을 활용해 0-할당 풀링으로 성능 저하 없이 구현합니다.

3. **코드 규격(500줄 규칙) 준수를 위한 MonsterSpawnerView 슬림화 리팩토링**
   - 현재 509줄인 `MonsterSpawnerView.cs`에서 더미 스폰 로직(`SpawnTrainingDummies`, `SpawnBatDummies`)을 전용 헬퍼 `MonsterTrainingDummyHelper.cs`로 분리하여 480줄 이하로 경량화합니다.

---

## 🛠️ 변경 예정 상세 내역

### 1. Presentation & Domain Layer

#### [NEW] [SideScrollGoldCoinView.cs](file:///k:/unityprojects/shoot1/shoot1/Assets/src/HappyShoot.View/SideScroll/SideScrollGoldCoinView.cs)
- 횡스크롤 모드 전용 필드 골드 코인 뷰.
- 몬스터 사망 위치에서 통통 튀며 등장(Bounce VFX), 플레이어가 다가가면 자석 흡수되어 `GameSessionEntity.AddGold(...)` 호출 및 코인 획득 사운드 재생.

#### [MODIFY] [GemManager.cs](file:///k:/unityprojects/shoot1/shoot1/Assets/src/HappyShoot.Domain/Gems/GemManager.cs)
- `IsSideScrollMode` 플래그 추가.
- 횡스크롤 모드 활성화 시 경험치 구슬(`SpawnGem`) 생성을 건너뛰어 레벨업 및 3지선다 유발을 원천 방지.

#### [MODIFY] [DimensionalVoidCoreView.cs](file:///k:/unityprojects/shoot1/shoot1/Assets/src/HappyShoot.View/SideScroll/DimensionalVoidCoreView.cs)
- `FallingGemShowerView`를 황금 코인 스프라이트와 골드 획득 로직(`AddGold(15)`)으로 업그레이드.

#### [MODIFY] [SpeedBoostRingView.cs](file:///k:/unityprojects/shoot1/shoot1/Assets/src/HappyShoot.View/SideScroll/SpeedBoostRingView.cs)
- `SideScrollHyperSpeedBuff` 내부에 잔상 스폰 루프 추가.
- 부스트 활성화 동안 마법사의 현재 스프라이트, 방향, 회전 각도를 복제한 비전 고스트 트레일을 연속 방출.

#### [MODIFY] [SideScrollModeController.cs](file:///k:/unityprojects/shoot1/shoot1/Assets/src/HappyShoot.View/SideScroll/SideScrollModeController.cs)
- 횡스크롤 진입 시 `GemManager.IsSideScrollMode = true` 설정 및 복귀 시 원복.

#### [NEW] [MonsterTrainingDummyHelper.cs](file:///k:/unityprojects/shoot1/shoot1/Assets/src/HappyShoot.View/Monsters/MonsterTrainingDummyHelper.cs)
- `MonsterSpawnerView`에서 더미 스폰 로직을 분리하여 500줄 초과 해소.

#### [MODIFY] [MonsterSpawnerView.cs](file:///k:/unityprojects/shoot1/shoot1/Assets/src/HappyShoot.View/Monsters/MonsterSpawnerView.cs)
- 더미 생성 위임 처리로 509줄 -> 480줄대로 다이어트.

---

## 🧪 검증 계획

1. **골드 드랍 및 3지선다 억제 검증**:
   - 횡스크롤 모드 진입 후 슬라임/골렘/박쥐 처치 시 경험치 구슬 대신 황금 코인이 정상 스폰되는지 확인.
   - 골드 획득 시 HUD의 골드 수치가 정상 증가하고, 레벨업 3지선다 팝업이 뜨지 않는지 확인.
2. **스피드 링 고스트 잔상 검증**:
   - 가속 링 통과 시 마법사 뒤로 멋진 잔상이 3.5초간 따라오는지 확인.
3. **코드 줄 수 및 아키텍처 검증**:
   - 모든 수정/신규 파일 500줄 미만 준수 여부 확인.
   - `APP_MAP.md` 및 서브 문서 업데이트.
