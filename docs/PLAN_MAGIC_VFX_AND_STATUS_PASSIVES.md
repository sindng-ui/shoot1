# ⚡ 마법사 비주얼 대격변 & 상태이상 패시브 2종 구현 계획서

## 1. 개요 및 목표
1. **메테오 스트라이크 (Meteor Strike) 화면 연출 구현**:
   - 상공에서 거대한 불타는 운석 낙하 ➡️ 지면 강타 폭발 ➡️ 화면 진동 + 화염 파편 비산 + 용암 충격파 링 연출
2. **서리 폭발 (Frost Nova) 얼음 마법 고도화 & 오한 & 빙결 파괴**:
   - 화려한 방사형 서리 결정 파동 연출
   - **오한 (Chill / Slow)**: 피격 몬스터 3.5초간 이동속도 40% 감속 + 몬스터 푸른빛 틴팅
   - **얼음 깨짐 (Ice Shatter on Death)**: 오한 상태 또는 서리 폭발로 사망 시 '챙그랑!' 얼음 조각이 사방으로 산산조각 깨지는 Zero-Allocation 파편 연출
3. **체인 라이트닝 (Chain Lightning) 진짜 번개 연출**:
   - 지그재그 굴절 아크(Zigzag Electric Bolt) + 번쩍이는 번개 방전 스파크 연출
4. **상태이상 패시브 스킬 2종 추가 (7초간 지속 피해)**:
   - 🔥 **발화의 불꽃 (Ignition Spark)**: 불 스킬(화염구, 메테오) 적중 시 7초간 화염 지속 틱 데미지 (Burn DoT)
   - ⚡ **과전류의 핵 (Overcharge Core)**: 전기 스킬(체인 라이트닝) 적중 시 7초간 감전 지속 틱 데미지 (Shock DoT)

---

## 2. 세부 설계

### 2.1 도메인 레이어 (HappyShoot.Domain)
- `StatusEffect`: 몬스터에 부착되는 상태이상 구조 (Chill, Burn DoT, Shock DoT)
- `MonsterEntity.cs`: `ApplyChill`, `ApplyBurn`, `ApplyShock`, DoT 틱 업데이트, 감속 이동 로직, 사망 시 `MonsterShatteredEvent` 발행
- `MagicEvents.cs`: `MeteorStrikeExecutedEvent`, `MonsterShatteredEvent`, `StatusEffectAppliedEvent` 추가
- `MeteorStrikeEffect.cs`: `MeteorStrikeExecutedEvent` 발행 및 발화 패시브 상태이상 연동
- `FireballEffect.cs`, `FrostNovaEffect.cs`, `ChainLightningEffect.cs`: 상태이상 및 오한 연동
- `SkillRewardManager.cs` & `GameBootstrap.cs`: 2종 신규 패시브 (`passive_ignition`, `passive_overcharge`) 등록

### 2.2 뷰 레이어 (HappyShoot.View)
- `MeteorStrikeManagerView.cs` [NEW]: 운석 낙하 궤적 애니메이션, 대폭발, 화면 진동, 잔상 연출 (500줄 초과 방지 분리)
- `MagicSkillManagerView.cs`: 지그재그 번개 아크 렌더링, 서리 결정 파동, 얼음 파괴 샤터 풀링
- `WizardSpriteHelper.cs`: 메테오 운석, 서리 스파이크, 얼음 파편, 번개 스파크 스프라이트 추가
- `MonsterView.cs`: 오한 상태 시 푸른색 틴팅 및 피격 플래시 복구

### 2.3 테스트 레이어 (HappyShoot.Domain.Tests)
- `StatusEffectTests.cs` [NEW]: 오한 감속, 화염 7초 DoT 틱, 감전 7초 DoT 틱, 메테오 이벤트 발행 등 단위 테스트
