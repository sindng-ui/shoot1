# 🧬 패시브 스킬 샌드박스 실시간 튜닝 시스템 구현 계획서

## 1. 개요 및 목표
인게임 내 9종 전체 패시브 스킬의 레벨당 증가 스탯 및 특수 효과 파라미터를 **전투 & 밸런스 샌드박스 UI**에서 실시간으로 슬라이더 조절하고 세이브/로드할 수 있도록 전면 연동합니다.

---

## 2. 튜닝 대상 9종 패시브 스킬 및 파라미터

| 패시브 | 기본 수치 | 튜닝 슬라이더 항목 | 범위 & 단위 |
|---|---|---|---|
| **🧛 흡혈귀의 이빨 (`passive_fang`)** | 공격력 +15% / Lv | 레벨당 공격력 증가율 (%) | `1% ~ 50%` (Step 1%) |
| **🪶 바람의 깃털 (`passive_feather`)** | 이속 +0.6, 투속 +15% / Lv | 레벨당 이동속도 (m/s)<br>레벨당 투사체 속도 (%) | `0.1 ~ 2.0 m/s`<br>`1% ~ 50%` |
| **🔮 마나 룬 (`passive_rune`)** | 쿨감 +10%, 범위 +15% / Lv | 레벨당 쿨타임 감소 (%)<br>레벨당 공격 범위 (%) | `1% ~ 25%`<br>`1% ~ 50%` |
| **🛡️ 강철 갑옷 (`passive_armor`)** | 방어력 +5 / Lv | 레벨당 방어력 증가치 | `1 ~ 25` (Step 1) |
| **💍 황금 반지 (`passive_ring`)** | 자석 흡수 반경 +1.5m / Lv | 레벨당 자석 흡수 반경 (m) | `0.5m ~ 5.0m` (Step 0.1m) |
| **💖 생명의 펜던트 (`passive_heart`)** | 최대체력 +30, 재생 +1.5 / Lv | 레벨당 최대 체력 (HP)<br>레벨당 초당 체력 재생 (HP/s) | `5 ~ 100 HP`<br>`0.2 ~ 10.0 HP/s` |
| **🔥 발화의 불꽃 (`passive_ignition`)** | 공격력 +10%, 화상 7초/10% | 레벨당 공격력 (%)<br>화상 지속시간 (초)<br>화상 틱 피해율 (%) | `1% ~ 50%`<br>`1.0s ~ 15.0s`<br>`1% ~ 30%` |
| **⚡ 과전류의 핵 (`passive_overcharge`)** | 쿨감 +6%, 감전 7초/18% | 레벨당 쿨타임 감소 (%)<br>감전 지속시간 (초)<br>감전 틱 피해율 (%) | `1% ~ 20%`<br>`1.0s ~ 15.0s`<br>`1% ~ 40%` |
| **🎯 치명타의 눈 (`passive_crit`)** | 크리확률 +8%, 크리댐 +5% / Lv | 레벨당 크리티컬 확률 (%)<br>레벨당 크리티컬 데미지 (%) | `1% ~ 25%`<br>`1% ~ 30%` |

---

## 3. 세부 구현 계획

### 1) 데이터 모델 및 직렬화 ([SkillConfigModels.cs](file:///k:/unityprojects/shoot1/shoot1/Assets/src/HappyShoot.Domain/Skills/SkillConfigModels.cs))
- `PassiveConfigData` 클래스를 신규 정의하고 `SkillConfigData.Passives`에 연동.
- JSON 세이브 파일(`skill_configs.json`)에 자동 저장 및 복원.

### 2) 패시브 실시간 라이브 핫리로드 ([SkillLiveApplier.cs](file:///k:/unityprojects/shoot1/shoot1/Assets/src/HappyShoot.View/UI/SkillLiveApplier.cs))
- `ApplyPassivesLive(PlayerEntity player, SkillConfigData config)` 구현:
  플레이어가 이미 획득한 패시브 레벨에 맞춰 `player.Stats`를 실시간 재계산하여, 샌드박스에서 슬라이더를 움직이는 즉시 플레이어 스탯과 공격력/이속/쿨감이 실시간 동기화되도록 연동.

### 3) 샌드박스 UI 탭 및 슬라이더 행 확장
- **[`SkillTuningUiBuilder.cs`](file:///k:/unityprojects/shoot1/shoot1/Assets/src/HappyShoot.View/UI/SkillTuningUiBuilder.cs)**:
  - 6대 카테고리 탭: `[전사]`, `[궁수]`, `[마법사]`, `[패시브]`, `[공통/스탯]`, `[시스템]`
  - `AllSkillDefinitions`에 9종 패시브 등록.
- **[`SkillTuningRowConfigurator.cs`](file:///k:/unityprojects/shoot1/shoot1/Assets/src/HappyShoot.View/UI/SkillTuningRowConfigurator.cs)**:
  - 9종 패시브별 전용 슬라이더 카드 행 및 실시간 라이브 콜백 연결.

### 4) 패시브 레벨업 적용 수식 동기화 ([SkillRegistryHelper.cs](file:///k:/unityprojects/shoot1/shoot1/Assets/src/HappyShoot.View/Bootstrap/SkillRegistryHelper.cs))
- `RegisterAllPassives`에서 하드코딩된 상수 대신 `SkillConfigRepository.Instance.GetConfig().Passives`의 설정값을 참조하도록 연동.

---

## 4. 검증 계획
- 단위 테스트 실행: 전체 124개 도메인 테스트 100% 통과 확인.
- 샌드박스 `[패시브]` 탭 진입 후 9종 패시브 수치를 변경했을 때 즉각 스탯과 인게임 스킬 대미지/속도/효과가 실시간 반영되는지 확인.
