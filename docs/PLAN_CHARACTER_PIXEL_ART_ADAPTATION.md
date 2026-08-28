# 캐릭터 스프라이트 도트화(Pixel Art) 변환 계획서

## 1. 개요
- **배경**: 인게임 몬스터(28x28 ~ 54x54 도트) 및 타일셋 등은 클래식 픽셀 아트로 제작되어 있으나, 최근 적용된 캐릭터 3종(Warrior, Ranger, Wizard)은 350x450 고해상도 벡터/일러스트 스타일이라 혼자만 너무 매끄럽고 튀는 이질감이 발생함.
- **목표**: 캐릭터 3종의 이미지 자체를 다른 오브젝트들과 조화로운 "적절한 크기의 도트(Pixel Art)" 형태로 변환하여 게임 전반의 도트 비주얼 완성도를 극대화.
- **핵심 원칙**: 
  1. "너무 큰 도트 말고 적절하게": 얼굴 이목구비나 디테일이 뭉개지지 않으면서 도트 감성이 완벽하게 살아나는 밸런스 유지.
  2. 기존 캔버스 규격(350x450)과 피벗, PPU를 100% 유지하여 지팡이 위치, 투구/무기 정렬, 그림자 바닥 기준점과의 호환성 보장.

## 2. 변환 대상 (총 15개 스프라이트 + 3개 시트)
- **Warrior (5방향)**: `warrior_front.png`, `warrior_front_diagonal.png`, `warrior_side.png`, `warrior_back_diagonal.png`, `warrior_back.png`, `warrior.png`
- **Ranger (5방향)**: `ranger_front.png`, `ranger_front_diagonal.png`, `ranger_side.png`, `ranger_back_diagonal.png`, `ranger_back.png`, `ranger.png`
- **Wizard (5방향)**: `wizard_front.png`, `wizard_front_diagonal.png`, `wizard_side.png`, `wizard_back_diagonal.png`, `wizard_back.png`, `wizard.png`

## 3. 세부 구현 내용
1. **도트화 프로세싱 스크립트 구축 (`scratch/apply_dot_characters.js`)**:
   - 투명 배경 마스크 및 외곽선 보존
   - 3x3 (또는 4x4) 도트 그리드 클러스터링 및 셀 음영 최적화
   - 지저분한 먼지 픽셀(Islands) 완벽 제거
2. **`CustomHeroSpriteLoader.cs` 필터 모드 조정**:
   - `FilterMode.Bilinear` -> `FilterMode.Point` 적용으로 줌인/줌아웃 및 회전 시에도 픽셀이 뭉개지지 않고 선명한 도트 표현 유지
3. **단위 테스트 검증**:
   - `WizardStaffPlacementTests.cs` 등 기존 위치 계산 및 뷰 로직 100% 통과 확인
4. **APP_MAP.md 업데이트**:
   - 캐릭터 스프라이트 도트화 및 렌더링 필터 변경 사항 기록
