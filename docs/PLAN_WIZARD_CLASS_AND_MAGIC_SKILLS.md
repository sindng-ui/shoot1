# 🧙 마법사(Wizard) 클래스 & 전용 액티브 마법 스킬 3종 구현 계획서

## 1. 개요
전사(대검 베기, 지면 강타, 휠윈드)와 궁수(관통 화살, 멀티샷, 화살 비)에 이어, 3번째 직업인 **마법사(Wizard)**를 완벽히 구현합니다.
- 공통 스킬: `오비탈 블레이드(Orbital Blades)` (모든 직업 공용)
- 마법사 전용 액티브 스킬 3종:
  1. `화염구(Fireball)`: 대상 적을 향해 날아가 충돌 시 반경 내 광역 폭발 마법 피해를 입히는 시작 무기
  2. `서리 폭발(Frost Nova)`: 플레이어 주변 360도 전방위에 즉발 냉기 파동을 방출하여 광역 타격
  3. `연쇄 번개(Chain Lightning)`: 가장 가까운 적을 시작으로 주변의 적들에게 최대 4회 전이되며 감전 타격
- 진화 스킬: `화염구(Fireball)` + `마나 룬(Mana Rune)` ➡️ `메테오 스트라이크(Meteor Strike)` 진화 레시피 완성

---

## 2. 세부 설계

### 2.1 도메인 레이어 (Pure C#)
- `FireballEffect.cs`: 투사체 발사 및 폭발 스플래시 판정 로직
- `FrostNovaEffect.cs`: 공간 그리드 기반 주변 360도 반경 내 적 일제 타격 및 `FrostNovaExecutedEvent` 발행
- `ChainLightningEffect.cs`: 공간 그리드 기반 타겟 추적 후 주변 적 탐색하여 연쇄 타격 및 `ChainLightningExecutedEvent` 발행
- `MagicEvents.cs`: 마법 스킬 발동 및 시각 효과 연동용 도메인 이벤트
- `PlayerClassFactory.cs`: 마법사 기본 스탯 (쿨타임 감소 15%, 범위 +20%, 공격력 +25%) 및 시작 스킬을 `Fireball`로 변경
- `SkillRewardManager.cs`: 마법사 전용 스킬 필터링 및 레벨업 롤링

### 2.2 뷰 레이어 (Unity Presentation)
- `WizardSpriteHelper.cs`: 500줄 초과 방지를 위한 마법사 보라색 로브/고깔모자, 비전 지팡이(Arcane Staff) 및 마법 스킬 투사체/이펙트 스프라이트 생성기
- `MagicSkillManagerView.cs`: 서리 폭발 파동 팽창/페이드 연출, 연쇄 번개 전격 빔 연결 연출, 화염구 폭발 연출을 담당하는 경량 뷰 매니저
- `CharacterSelectUiView.cs`: 전사/궁수/마법사 3영웅 카드 선택 화면으로 확장
- `PlayerView.cs`: 마법사 선택 시 로브 외형 및 지팡이 렌더링
- `GameBootstrap.cs`: 마법사 스킬 및 뷰 시스템 부트스트랩 연동

### 2.3 테스트 레이어 (NUnit Tests)
- `WizardSkillsTests.cs`: 화염구, 서리 폭발, 연쇄 번개, 마법사 스탯 및 레벨업 보상 롤링 등 단위 테스트 작성

---
