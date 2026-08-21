# 🎨 게임 내 한글 폰트 전면 적용 및 대형 보상 카드(아이콘 포함) 개편 계획서

## 🔍 변경 요구사항 분석

### 1. 한글 폰트 전면 적용
- `FontHelper.cs`를 신설하여 OS Dynamic Font (`Malgun Gothic`, `맑은 고딕`, `NanumGothic`, `AppleGothic`, `Arial` 순 Fallback)를 안전하게 로드.
- 모든 UI 뷰 컴포넌트(`InGameHudView`, `LevelUpUiView`, `PauseMenuUiView`, `GameOverResultUiView`, `BossHealthBarView`, `MetaShopUiView`, `TreasureChestPopupView`, `EvolutionPopupView`)에서 `FontHelper.GetKoreanFont()` 적용.
- 스킬/패시브 이름 및 설명, HUD 레이블, 버튼 텍스트를 모두 한국어로 일괄 변경.

### 2. 보상 선택 카드 대형화 및 전용 픽셀아트 아이콘 렌더링
- 카드 크기를 **`320 x 460` (대폭 확대)**로 키우고, 컨테이너를 `1080 x 500`으로 확장.
- `RewardIconHelper.cs`를 신설하여 13종 전체 보상(무기 4종, 패시브 6종, 진화 3종)에 대한 **80x80 전용 픽셀아트 아이콘 스프라이트**를 생성 및 카드 중앙에 배치.

---

## 🛠️ 세부 변경 계획 및 파일 구성

### 1. New Helper Components (`Assets/src/HappyShoot.View/Utils/`)
- `FontHelper.cs`: 한글 Dynamic Font 로더
- `RewardIconHelper.cs`: 13종 리워드별 80x80 고해상도 픽셀아트 아이콘 생성기

### 2. UI Views Update (`Assets/src/HappyShoot.View/UI/`)
- `LevelUpUiView.cs`: 카드 크기 320x460 대형화 및 아이콘 렌더링, 한국어 적용
- `InGameHudView.cs`, `PauseMenuUiView.cs`, `GameOverResultUiView.cs`, `BossHealthBarView.cs`, `EvolutionPopupView.cs`, `MetaShopUiView.cs`, `TreasureChestPopupView.cs`: 한글 폰트 적용

### 3. Bootstrap & Domain Skill Rewards (`GameBootstrap.cs`)
- 모든 스킬 및 패시브 등록 시 한국어 타이틀과 상세 설명으로 등록
