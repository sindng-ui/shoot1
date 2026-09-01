# 💎 인게임 보석 표시 UI 개선 계획 (위치 이동 및 보석 아이콘+숫자 비주얼화)

## 📌 배경 및 목적
- **현상**: 인게임 상단 중앙의 타이머 바로 밑(`Y = -62f`)에 보석 카운터 텍스트 바(`113 G | 1 0 0`)가 밋밋하게 배치되어 있어 몬스터/플레이어 시야를 가리고 시각적으로 단조로움.
- **목적**: 
  1. 보석 카운터 UI의 위치를 **시간(타이머) 왼쪽 공간**으로 이동하여 시야를 쾌적하게 확보.
  2. 단순 텍스트 숫자가 아닌, **영롱한 색상의 픽셀아트 보석 모양(루비/에메랄드/자수정) + 숫자 텍스트 슬롯 형태**로 고급스럽고 직관적인 비주얼 HUD로 전면 리뉴얼.

---

## 🛠️ 세부 작업 내용

### 1. `InGameGemCounterHudView.cs` 전면 리뉴얼
- **위치 조정**:
  - `Anchor`: `(0.5f, 1f)` (상단 중앙 기준)
  - `Pivot`: `(1f, 0.5f)` (우측 중앙 기준)
  - `AnchoredPosition`: `(-100f, -24f)` (타이머 좌측 여백 20px, 타이머와 동일 높이)
  - `Size`: `(230f, 34f)`
- **비주얼 구성**:
  - **다크 글래스모피즘 캡슐 바**: 반투명 흑청색 배경 (`Color(0.08f, 0.10f, 0.15f, 0.85f)`) + 슬림 골드 아웃라인
  - **보석 슬롯 3종**:
    1. **루비 (Ruby)**: `SkillTreeSpriteHelper.GetOrCreateRubySprite(32)` 아이콘 + `#FFB8C2` 수량 텍스트
    2. **에메랄드 (Emerald)**: `SkillTreeSpriteHelper.GetOrCreateEmeraldSprite(32)` 아이콘 + `#B8FFD2` 수량 텍스트
    3. **아메시스트 (Amethyst)**: `SkillTreeSpriteHelper.GetOrCreateAmethystSprite(32)` 아이콘 + `#E4B8FF` 수량 텍스트
  - **루팅 연출 (Punch Scale)**:
    - 보석을 먹었을 때 해당 보석 아이콘이 1.25배로 팝업 후 원래 크기로 복귀하는 경량 트윈 효과
- **호환성 유지**:
  - `StageVictoryUiView`, `GameOverResultUiView`에서 읽는 `RunRubyCount`, `RunEmeraldCount`, `RunAmethystCount`, `RunGoldCount` 및 `ResetRun()` 인터페이스 완벽 유지.

---

## 🗺️ 문서 및 아키텍처 업데이트
- `APP_MAP.md`에 `InGameGemCounterHudView` 비주얼 및 위치 변경 사항 최신화.
- 500줄 초과 여부 확인 및 무결성 검증.
