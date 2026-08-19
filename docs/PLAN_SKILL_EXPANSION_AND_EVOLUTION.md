# ⚔️ Phase 3: 무기/액티브 스킬 & 패시브 6종 확장 및 진화(Evolution) 시스템 구현 계획서

## 1. 개요
- **목표**: 4번째 액티브 무기(회전 오비탈 블레이드) 추가, 6종 패시브 아이템 풀 구축, 무기 8레벨+패시브 결합 시 진화(Evolution) 3종 완성 및 레벨업/보물상자 UI 연동.
- **아키텍처**: Pure C# Domain 레이어에서 완벽히 시뮬레이션 및 테스트하고, Unity Presentation 레이어에서 화려한 카드 팝업 및 진화 배너 연출을 바인딩.

---

## 2. 세부 구현 내역

### 1) 신규 무기 & 효과
- **`OrbitingBladesEffect.cs`**: 플레이어 주변 반경 $R$에서 일정한 각속도로 회전하는 칼날/성수.
  - SpatialGrid를 통해 플레이어 주변의 몬스터들과 충돌 검사하여 지속 대미지 부여.

### 2) 6종 패시브 아이템 풀 체계화
`PlayerEntity`에 패시브 슬롯 및 레벨 추적 시스템 구축:
1. `passive_fang` (뱀파이어 이빨): 공격력 +15%, 타격 시 체력 흡수(흡혈)
2. `passive_feather` (바람의 깃털): 이동속도 +12%, 투사체 속도 +15%
3. `passive_rune` (마나 룬): 쿨다운 감소 +10%, 스킬 공격 범위 +15%
4. `passive_armor` (강철 갑옷): 방어력 +5 (피해 경감 공식 적용)
5. `passive_ring` (황금 반지): 자석 흡수 반경 +1.5m, 골드 획득 보너스
6. `passive_heart` (심장 펜던트): 최대 체력 +30, 초당 체력 재생 +1.5 HP/s

### 3) 3종 스킬 진화(Evolution) 완성
1. **블러드 이터 (Blood Eater)**: 대검 (8Lv) + 뱀파이어 이빨 (1Lv+) -> 거대 진홍빛 검기 + 타격 시 체력 흡수
2. **스톰 보우 (Storm Bow)**: 활 (8Lv) + 바람의 깃털 (1Lv+) -> 8방향 관통 폭풍 화살 난사
3. **메테오 스트라이크 (Meteor Strike)**: 마법구 (8Lv) + 마나 룬 (1Lv+) -> 전장을 뒤흔드는 거대 운석 소환

### 4) 레벨업 & 보물상자 UI 연동
- `SkillRewardManager`: 진화 조건 충족 시 진화 카드를 최우선 후보로 추천
- `LevelUpUiView` & `EvolutionPopupView`: 진화 획득 시 화려한 팡파르 배너 표시

---

## 3. 단위 테스트 계획
- `OrbitingBladesTests.cs`: 회전 궤적 및 다중 충돌 판정 테스트
- `PassiveItemsTests.cs`: 6종 패시브 획득 및 스탯 누적/적용 테스트
- `SkillEvolutionTests.cs`: 8레벨+패시브 결합 시 진화 트리거 및 스킬 교체 검증
