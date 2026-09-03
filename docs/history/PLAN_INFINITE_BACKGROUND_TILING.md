# 🏛️ 무한 배경 타일링 시스템 (BackgroundManager) 및 고품질 다크 판타지 전장 구현 계획

## 1. 개요
플레이어가 광활한 전장을 이동할 때 카메라 영역에 맞춰 배경 타일이 끊김 없이 자연스럽게 반복 이동(Wrap-around)하는 무한 배경 타일링 시스템을 구축합니다.
동시에 기존의 밋밋한 단색 카메라 배경을 탈피하여, 고대 석판 바닥(던전 플래그스톤) 질감, 금 간 석판, 고대 마법 룬 문양, 미세한 이끼 디테일, 그리고 전장에 깊이감을 부여하는 앰비언트 부유 입자(Ambient Floating Motes)까지 갖춘 **"진짜 서바이버즈 게임다운 그럴싸하고 몰입감 넘치는 전장"**을 완성합니다.

---

## 2. 세부 설계 및 아키텍처

### A. 무한 타일링(Wrap-around) 메커니즘 (`BackgroundManager.cs`)
- **그리드 구성**: 3x3 타일 그리드 (총 9개 타일 오브젝트)
- **타일 단위 크기**: 24m x 24m (총 72m x 72m 커버)
  - 카메라 `orthographicSize = 9.0f` 기준 뷰포트 크기: 세로 18m, 16:9 가로 32m, 21:9 울트라와이드 43m.
  - 3x3(72m x 72m) 영역은 격렬한 카메라 쉐이크(지각변동, 메테오 등) 및 21:9 울트라와이드 모니터 환경에서도 화면 끝이 절대 비지 않는 충분한 안전 마진(Safety Margin)을 제공합니다.
- **초경량 무할당 랩어라운드(Wrap-around) 알고리즘**:
  - 카메라 중심 좌표를 기준으로 타일의 상대 오프셋 계산:
    - `diffX = tilePos.x - camPos.x`
    - `diffX > halfGridWidth (36m)` $\rightarrow$ `tilePos.x -= gridWidth (72m)`
    - `diffX < -halfGridWidth (-36m)` $\rightarrow$ `tilePos.x += gridWidth (72m)`
  - Y축도 동일한 방식으로 적용
  - 매 프레임 단 9개의 타일에 대해 float 연산만 수행하므로 **GC Alloc = 0 Bytes, CPU 점유율 0.001ms 미만**의 초극상 성능을 보장합니다.

### B. "그럴싸한" 비주얼 디자인 및 픽셀아트 생성 (`BackgroundSpriteHelper.cs`)
- 단조로운 바둑판 반복감을 없애기 위해 4가지 다채로운 고대 던전 석판 텍스처를 절차적으로 생성(FilterMode.Bilinear, 압축 없는 선명한 픽셀아트):
  1. **Tile A (기본 고대 석판 - Classic Flagstone)**: 사각/직사각형 석판들이 정교하게 맞물린 석조 바닥, 어두운 줄눈(Mortar) 및 석판 테두리 입체 음영(Bevel)
  2. **Tile B (금이 간 석판 - Cracked Flagstone)**: 세월과 격렬한 전투의 흔적으로 거미줄처럼 갈라진 석판 크랙
  3. **Tile C (고대 마법 룬 - Ancient Rune Inscribed)**: 신비로운 고대 기호와 마법 룬이 은은하게 새겨진 석판
  4. **Tile D (풍화 및 이끼 - Weathered & Moss Stone)**: 석판 틈새에 짙은 이끼와 흙먼지가 자연스럽게 낀 고풍스러운 질감
- 9개의 타일에 위 4개 변형을 지그재그/체스판 패턴으로 배치하여 시각적 다양성 극대화.

### C. 전장 깊이감(Depth) 연출: 앰비언트 부유 입자 (`BackgroundAmbientDustView.cs`)
- 전장에 생동감을 불어넣는 초경량 앰비언트 부유 먼지/마법 불씨 입자 시스템.
- 카메라 주변 로컬 영역에서 미세하게 위아래/좌우로 부유하며, 화면 밖으로 나가면 반대편으로 순환.
- 0-Allocation 풀링 및 은은한 반투명(Alpha 0.15~0.25)으로 전투 시인성을 전혀 해치지 않으면서 극상의 분위기를 자아냄.

### D. 렌더링 계층(Sorting Order) 무결성
- `Background Tiles`: `sortingOrder = -100` (최하단)
- `Ambient Dust`: `sortingOrder = -50` (타일 위, 그림자 아래)
- `Player/Monster Blob Shadows`: `sortingOrder = -10`
- `Ground Decals / Hazards`: `sortingOrder = 5`
- `Monsters`: `sortingOrder = 10`
- `Player`: `sortingOrder = 15`
- `Projectiles & Skills`: `sortingOrder = 20 ~ 30`
- $\rightarrow$ 기존 모든 게임플레이 요소와의 렌더링 충돌/가림 현상 완벽 방지!

---

## 3. 모듈 분할 및 500줄 규칙 준수
1. `Assets/src/HappyShoot.View/Background/BackgroundManager.cs` (~180줄)
2. `Assets/src/HappyShoot.View/Background/BackgroundTileView.cs` (~120줄)
3. `Assets/src/HappyShoot.View/Background/BackgroundSpriteHelper.cs` (~220줄)
4. `Assets/src/HappyShoot.View/Background/BackgroundAmbientDustView.cs` (~130줄)
5. `Assets/src/HappyShoot.View/Bootstrap/GameBootstrap.cs` (수정: BackgroundManager 초기화 4줄 추가)

---

## 4. 검증 계획
- WSL Bash 환경에서 Standalone 빌드 또는 컴파일 스크립트 실행을 통한 에러 0 검증.
- 500줄 규칙 준수 검증 (모든 신규 파일이 250줄 이하).
- `APP_MAP.md`에 신규 모듈 및 인터페이스 상세 업데이트.
