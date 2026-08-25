# 🎨 [Phase 1] 하단 3단 집중형 메인 HUD 전면 개편 완료 보고서

## 1. 구현 개요
형님께서 제시해주신 캡처 레퍼런스(Soulstone Survivors / 하이엔드 쿼터뷰 뱀서라이크 스타일)를 바탕으로,
게임 화면의 핵심 정보들을 하단 중앙에 집중 배치하는 **3단 레이어 메인 HUD 시스템**을 전면 구축 완료했습니다.

---

## 2. 세부 구현 내역

### 1) Layer 1 (최하단): 10칸 분할 골드 보더 EXP 프로그레스 바 ([HudSpriteHelper.cs](file:///k:/unityprojects/shoot1/shoot1/Assets/src/HappyShoot.View/Utils/HudSpriteHelper.cs))
- 화면 최하단 전체를 가로지르는 10칸(Segments Divider) 메탈 프레임 베이스.
- 경험치 획득 시 화려한 골드 게이지가 10% 단위 눈금에 맞춰 부드럽게 차오름.
- 좌측 끝: 다이아몬드/방패 모양의 황금빛 레벨 배지 (`17`).
- 중앙: `EXP 1,250 / 2,000 (62.5%)` 정보 텍스트.

### 2) Layer 2 (중앙 중단): 6칸 스킬 슬롯 + 360° 시계방향 쿨타임 마스크 ([InGameHudBuilder.cs](file:///k:/unityprojects/shoot1/shoot1/Assets/src/HappyShoot.View/UI/InGameHudBuilder.cs))
- 6개의 골드 프레임 스킬 슬롯 (`48x48px`) + 좌측 대시(`Space 👟`) 슬롯.
- **시계 방향 360° 쿨타임 회전 오버레이 (Radial Fill Clockwise)**:
  스킬 발동 시 스킬 아이콘 위에서 어두운 마스크가 시계 방향으로 자연스럽게 회전하며 잔여 쿨타임을 직관적으로 표시.
- 각 슬롯 하단: `Lv. 1`, `Lv. 2` 등 스킬 레벨 뱃지.

### 3) Layer 3 (중앙 상단): 투구 엠블럼 대형 체력바
- 중앙에 멋진 전사 투구/혼드 문양 엠블럼 장식.
- 엠블럼 좌우로 펼쳐지는 와이드 루비 레드 체력 게이지 (`480px` 너비).
- 중앙 선명한 체력 수치 텍스트 (`100 / 100`).

### 4) 상단 미니멀 정보창
- 상단 중앙: `00:00` 타이머.
- 상단 우측: `💀 킬 카운터` & `💰 골드 카운터`.

---

### 5) 스킬별 고유 아이콘 분리 ([RangerRewardIconHelper.cs](file:///k:/unityprojects/shoot1/shoot1/Assets/src/HappyShoot.View/Utils/RangerRewardIconHelper.cs), [WarriorRewardIconHelper.cs](file:///k:/unityprojects/shoot1/shoot1/Assets/src/HappyShoot.View/Utils/WarriorRewardIconHelper.cs))
- **관통 화살 (`bow`)**: 날렵한 황금빛 관통 활 & 화살 픽셀아트.
- **화살비 (`arrow_rain`)**: **폭풍 구름에서 대각선 사선으로 지면에 사정없이 쏟아지는 에메랄드/골드 화살 비 & 푸른 궤적** 전용 픽셀아트 적용.
- **풍인 (`glaive`)**: 3날 회전 바람 투척검.
- **휠윈드 (`whirlwind`)**: 360도 3중 강철 회오리 칼날.

### 8) 액티브 스킬 슬롯 투사체 개수 뱃지 표시 ([InGameHudBuilder.cs](file:///k:/unityprojects/shoot1/shoot1/Assets/src/HappyShoot.View/UI/InGameHudBuilder.cs), [InGameHudView.cs](file:///k:/unityprojects/shoot1/shoot1/Assets/src/HappyShoot.View/UI/InGameHudView.cs))
- 스킬창의 각 액티브 스킬 슬롯 **우상단(Top-Right)** 에 선명한 네온 시안/화이트 뱃지로 현재 투사체/발사체 개수(예: 화염구 2개 -> `2`, 3개 -> `3`, 관통화살 5개 -> `5` 등)를 실시간 동기화하여 표시.
- 투사체가 1개이거나 투사체 개념이 없는 스킬(대검베기, 지면강타 등)은 뱃지를 숨겨 깔끔함을 유지하고, 레벨업/패시브로 투사체가 2개 이상이 되는 순간 직관적으로 숫자가 표시됩니다.

### 9) Phase 2: 화면 좌측 패시브 리스트 HUD & 네온 에임 타겟 링 ([InGameHudBuilder.cs](file:///k:/unityprojects/shoot1/shoot1/Assets/src/HappyShoot.View/UI/InGameHudBuilder.cs), [InGameHudView.cs](file:///k:/unityprojects/shoot1/shoot1/Assets/src/HappyShoot.View/UI/InGameHudView.cs), [AimReticleView.cs](file:///k:/unityprojects/shoot1/shoot1/Assets/src/HappyShoot.View/Cameras/AimReticleView.cs))
- **머리 위 오버헤드 패시브 제거 및 화면 좌측 HUD로 전면 이전**:
  - 플레이어 머리 위 버프를 깔끔하게 제거하여 시야를 100% 확보.
  - **화면 좌측(Left)** 에 9종 패시브 슬롯 리스트를 배치하여 보유한 패시브 아이콘 + 우하단 레벨 뱃지 + **실시간 누적 수치(예: `+15% ATK`, `+24% SPD`, `+30% RNG`, `+10 ARM`, `+20% EXP`, `+40 HP`, `+16% CRT` 등)** 를 한눈에 알아보기 쉽게 표시.
- **네온 라임-그린 에임 타겟 링 (Soulstone Survivors 스타일)**:
  - 4방향 다이아몬드 돌기 십자선 과녁 링이 마우스 2D 월드 좌표를 부드럽게 추종하며, 은은한 펄스 및 좌클릭 피드백 반응을 제공합니다.

---

## 3. 검증 결과
- **단위 테스트**: 124개 도메인 단위 테스트 전체 100% 통과 (**124/124 ALL PASS**).
- **모듈화**: 모든 파일 500줄 이하 엄수.
- **문서화**: [`APP_MAP.md`](file:///k:/unityprojects/shoot1/shoot1/APP_MAP.md) 최신화 완료.
