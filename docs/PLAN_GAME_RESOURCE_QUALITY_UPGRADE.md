# 🎨 HappyShoot 게임 전체 리소스 퀄리티 업그레이드 단계적 로드맵

> **작성일자**: 2026-09-03  
> **상태**: 계획 수립 완료 (형님 승인 대기 중)  
> **목표**: 캐릭터에 선행 적용된 고화질 이미지 에셋 파이프라인을 몬스터, 배경, 스킬 VFX, 아이템, UI 등 게임 전체 리소스 영역으로 확장하여 최상급 다크 판타지 Survivors-like 비주얼 완성

---

## 1. 개요 및 배경

- **현황 분석**:
  - 캐릭터 3종(Warrior, Ranger, Wizard)은 350x450 고해상도 기반의 정교한 일러스트/도트 스프라이트와 CustomHeroSpriteLoader 시스템이 구축되어 훌륭한 퀄리티를 보여줌.
  - 반면, 게임 내 몬스터(28x28 픽셀), 바닥 타일(절차적 수식 드로잉), 스킬 투사체 및 슬래시 궤적, 보석 및 보물상자, 전투 HUD 프레임 등은 여전히 구형 절차적 SetPixels 코드로 생성되어 **캐릭터와 주변 환경 간 시각적 격차(Visual Discrepancy)**가 발생함.
- **핵심 목표**:
  - 게임 내 모든 시각 리소스의 해상도와 디테일을 단계별로 끌어올려 **일관된 프리미엄 다크 판타지 비주얼** 확립.
  - 이미지가 없어도 100% 정상 작동하는 **2중 안전 폴백(Fallback) 아키텍처** 유지로 개발/빌드 안정성 확보.

---

## 2. 핵심 아키텍처 원칙 (Core Architectural Rules)

1. **100% 무중단 2중 폴백 (Zero-Regression Fallback)**:
   - 에셋 로더가 디스크/Resources에서 고화질 이미지를 우선 탐색하되, 파일이 없으면 기존의 절차적 픽셀 아트 생성 메서드로 즉시 안전 폴백.
   - 단 하나의 에셋 파일만 추가되어도 해당 개체만 즉시 고화질로 적용되며 빌드가 절대 깨지지 않음.
2. **500줄 제한 및 단일 책임 분리 (Modular Sub-Loaders)**:
   - 무거운 단일 클래스를 지양하고, 각 도메인별 전용 로더(CustomMonsterSpriteLoader, CustomBackgroundSpriteLoader, CustomVfxSpriteLoader, CustomItemSpriteLoader, CustomUiSpriteLoader)로 300줄 이내 분할.
3. **선명한 엣지 & 필터 일관성 (Point Filter & True Pixel Density)**:
   - FilterMode.Point 및 명시적 PPU(Pixels Per Unit) / Pivot 설정을 통해 회전이나 줌 상황에서도 번짐 없는 선명한 비주얼 유지.
4. **모바일 & 성능 최적화 (Zero-Alloc & No Blur)**:
   - 딕셔너리 기반 텍스처 메모이제이션으로 런타임 0-Alloc 유지.
   - 모바일 성능 저하를 유발하는 UI Blur 효과 완전 배제.

---

## 3. 5단계 상세 로드맵 (5-Step Phase Roadmap)

### 📌 [Phase 1] 몬스터 & 보스 고화질 에셋화 (Monsters & Bosses) - 🔥 최우선순위
- **작업 내용**:
  - CustomMonsterSpriteLoader.cs 구축 (Resources 우선 탐색 + 절차적 Fallback)
  - **1차 (일반 몬스터 4종)**: 슬라임(Slime), 박쥐(Bat), 스켈레톤(Skeleton), 파이어임프(FireImp)
  - **2차 (엘리트 & 특수몹 3종)**: 독거미(ToxicSpider), 다크나이트(DarkKnight), 골렘(Golem)
  - **3차 (3대 보스)**: 사령왕 리치(Lich King), 대형 골렘 보스, 화염 지옥 군주
- **표준 규격**:
  - 일반 몬스터: 128x128 ~ 256x256 캔버스 (PPU 160f, Pivot 바닥 발 위치 0.5, 0.25)
  - 보스 몬스터: 256x256 ~ 512x512 캔버스 (PPU 200f)
- **피드백 연동**: 피격 시 순백 플래시(_flashDamageColor) 효과와 투명 알파 채널 100% 호환 보장.

---

### 📌 [Phase 2] 환경 & 무한 배경 타일셋 고화질화 (Environment & Tiles)
- **작업 내용**:
  - CustomBackgroundSpriteLoader.cs 구축
  - 바닥 타일 4종 고화질 심리스(Seamless) 텍스처 제작 및 적용:
    1. 클래식 던전 석판 (Classic Stone Tile)
    2. 균열 석판 (Cracked Runic Stone Tile)
    3. 고대 비전 룬 석판 (Arcane Inscribed Tile)
    4. 이끼 낀 고대 석판 (Mossy Forgotten Tile)
- **표준 규격**:
  - 512x512 Seamless 반복 텍스처 (PPU 21.3f로 24x24m 월드 그리드와 1:1 완벽 정합)
  - 3x3 무한 스크롤 렌더러 드로우콜 0 오버헤드 유지.

---

### 📌 [Phase 3] 무기 & 스킬 발사체 / 전투 VFX 고화질화 (Weapons & Skill VFX)
- **작업 내용**:
  - CustomVfxSpriteLoader.cs 구축
  - **영웅 무기 3종**:
    - 워리어 강철 대검(Greatsword) - 손잡이 피벗 정밀 정렬
    - 레인저 요정의 장궁(Elven Longbow)
    - 위저드 비전 오브 지팡이(Arcane Orb Staff)
  - **스킬 전투 VFX**:
    - 근접 슬래시 궤적(Slash Arc) 및 피의 참격(Blood Slash) 고해상도 아크 텍스처
    - 관통 화살(Piercing Arrow), 바람 칼날(Wind Glaive)
    - 마법 발사체: 화염구(Fireball), 서리 폭발 링(Frost Nova), 전격 이펙트(Chain Lightning), 메테오 운석(Meteor)
- **표준 규격**: 128x128 ~ 256x256 해상도, Additive/Alpha 블렌딩 지원.

---

### 📌 [Phase 4] 아이템 & 수집품 / 보물상자 고화질화 (Loot & Collectibles)
- **작업 내용**:
  - CustomItemSpriteLoader.cs 구축
  - **경험치 보석 3종**: 찬란한 블루/그린/레드 젬스톤 (보석 컷팅 디테일 및 하이라이트)
  - **재화 4종**: 반짝이는 골드 코인, 루비/사파이어/토파즈 룬스톤
  - **보물상자**: 일반 판타지 상자(닫힘/열림), 황금 보물상자(닫힘/열림)
  - **대장간 룬 12종**: 각 속성별 고유 양각 룬 엠블럼 아이콘
- **표준 규격**: 64x64 ~ 128x128 해상도.

---

### 📌 [Phase 5] UI/HUD & 프레임 / 모바일 조이스틱 고화질화 (Modern UI/HUD)
- **작업 내용**:
  - CustomUiSpriteLoader.cs 구축
  - **전투 HUD**: One UI 스타일 체력바/경험치바 프레임, 스킬 슬롯 골드/메탈 테두리
  - **레벨업 팝업**: 3지선다 스킬 선택 카드(황금빛 양각 프레임 및 배경 텍스처)
  - **스킬트리 360° 다이얼**: 비전 성좌 휠(Arcane Constellation Wheel) 텍스처
  - **모바일 조이스틱**: 매끄러운 림(Rim) & 센터 노브(Knob) 질감
- **표준 규격**: 9-Slice 스프라이트 규격 준수 (해상도 무관 자동 스케일링).

---

## 4. 형님께 드리는 추가 아이디어 제안 (Creative Proposals)

1. **인게임 리소스 스타일 실시간 토글 (Retro Pixel ↔ High-Res Modern)**:
   - 설정(Settings) 또는 일시정지 메뉴에 [비주얼 스타일: 클래식 도트 / 고화질 리마스터] 원클릭 토글 버튼을 제공하여, 전후 비교 시각적 쾌감과 사용자 취향 선택권 제공.
2. **절차적 2.5D 바운스/호흡 쥬스(Juice) 결합**:
   - 고화질 몬스터 스프라이트가 스폰되거나 이동할 때, 단일 스프라이트여도 코드로 부드럽게 눌리고 늘어나는(Squash & Stretch) 젤리/호흡 바운스 애니메이션을 결합하여 생동감 200% 증폭.
3. **에셋 파이프라인 자동화 툴킷**:
   - 생성된 고화질 이미지를 즉시 규격에 맞게 크롭, 알파 정리, Resources 폴더 배치까지 원클릭으로 처리하는 헬퍼 스크립트 구축.

---

## 5. 단계별 검증 계획

- **빌드 및 호환성 검증**:
  - dotnet test (또는 NUnit) 140+ 단위 테스트 100% ALL PASS 유지
  - Unity 에디터 재생 모드 시 콘솔 에러 0건 및 60FPS 버터 프레임 유지
  - 에셋 파일이 누락된 상태에서도 절차적 폴백으로 정상 구동 확인
