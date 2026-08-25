# 🩸 흡혈귀의 이빨 패시브 공격력 미반영 버그 수정 완료 보고서

## 1. 개요 및 해결된 문제
- **문제 현상**: 전사 캐릭터로 게임 진행 중 '흡혈귀의 이빨'(`passive_fang`) 패시브를 획득하여 공격력(+15%)이 증가했음에도, 대검 베기 등의 스킬 피해량이 38로 동일하게 유지되어 공격력 증가 효과가 스킬에 전혀 반영되지 않던 문제를 해결했습니다.
- **원인 분석**:
  1. `PlayerEntity.cs`의 `SkillContext` 생성자에서 `BaseDamage = 10f`로 고정되어 있었고, `PlayerEntity.Update()` 매 틱마다 `_skillContext.BaseDamage`를 `Stats.AttackPowerMultiplier`와 연동하여 업데이트하지 않았습니다.
  2. 따라서 플레이어 스탯(`Stats.AttackPowerMultiplier`)이 패시브나 메타 업그레이드로 올라가도 모든 스킬의 유효 피해량 계산식(`BaseDamage * (context.BaseDamage / 10f)`)에 1도 전달되지 않는 상태였습니다.

---

## 2. 주요 수정 내역

### 1) PlayerEntity 공격력 배율 실시간 동기화 ([PlayerEntity.cs](file:///k:/unityprojects/shoot1/shoot1/Assets/src/HappyShoot.Domain/Entities/PlayerEntity.cs))
- 생성자 및 `Update()` 루프에서:
  ```csharp
  _skillContext.BaseDamage = 10f * Stats.AttackPowerMultiplier;
  ```
  를 적용하여, 기본 직업 공격력 배율(전사 1.1x = 38.5 dmg), 흡혈귀의 이빨 패시브(+15% = 43.75 dmg), 메타 상점 공격력 영구 강화, 샌드박스 공격력 배율 조절이 **모든 액티브 스킬 및 진화 궁극기에 즉시 실시간으로 100% 반영**되도록 수정했습니다.

### 2) 패시브 레벨업 증가량 수식 정리 ([SkillRegistryHelper.cs](file:///k:/unityprojects/shoot1/shoot1/Assets/src/HappyShoot.View/Bootstrap/SkillRegistryHelper.cs))
- 레벨업 1회당 정해진 증가량(예: `s.AttackPowerMultiplier + 0.15f`)이 깔끔하게 누적되도록 패시브 콜백을 정리했습니다.

---

## 3. 검증 결과

### 🧪 단위 테스트 검증
- `PassiveItemsTests.cs`에 `ApplyPassiveFang_IncreasesSlashSkillDamage_OnPlayer` 테스트를 추가하여,
  전사 기본 대미지(38.5) -> 흡혈귀의 이빨 획득 후 대미지(43.75)로 정상 증가함을 완벽히 검증했습니다.
- **총 124개 도메인 단위 테스트 전체 100% 통과 (124/124 ALL PASS)** 완료!

### 🗺️ 문서 동기화
- [`APP_MAP.md`](file:///k:/unityprojects/shoot1/shoot1/APP_MAP.md)에 도메인 `PlayerEntity` 변경 사항 최신화 완료.
