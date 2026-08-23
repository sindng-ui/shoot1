# 🎯 크리티컬 시스템 & 신규 패시브 및 샌드박스 튜닝 구현 완료

형님! 요청해주신 **캐릭터 크리티컬 시스템(기본 10% 지정)**, **신규 패시브 '치명타의 눈' (+8% 크리, +5% 대미지)**, **임팩트 넘치는 크리티컬 황금 섬광 VFX & 몬스터 셰이크/대형 데미지 팝 연출**, 그리고 **전투 샌드박스 모드 내 실시간 크리티컬/스탯 튜닝 탭** 구현이 모두 완벽하게 완료되었습니다!

---

## 🚀 주요 구현 내용

### 1. 🎲 도메인 크리티컬 시스템 (Critical Strike Framework)
- **기본 확률 지정**: `CharacterStats.Default`에 `CritChance: 0.10f` (10%), `CritDamageMultiplier: 1.50f` 기본 탑재.
  - 전사/마법사 기본 크리티컬: **10%** (1.5x)
  - 궁수 기본 크리티컬: **20%** (1.75x)
- **`PlayerEntity.RollDamage(float rawDamage)`**:
  - `(float damage, bool isCritical)` 튜플 반환.
  - 내부 의사난수 롤링을 통해 크리티컬 발생 시 `rawDamage * CritDamageMultiplier` 즉시 계산.
- **모든 13종 무기/스킬/투사체 크리티컬 롤링 연동**:
  - 전사(대검 베기, 지면 강타, 휠윈드, 블러드 이터)
  - 궁수(관통 화살 투사체별 개별 롤링, 칼바람 글레이브 왕복 2타, 화살비 착탄, 폭풍 화살 연쇄 충격파)
  - 마법사(화염구 폭발, 프로스트 노바, 체인 라이트닝 전이 4체, 메테오 스트라이크)
  - 공용(오비탈 블레이드 6개 칼날 독립 롤링)
- **`MonsterDamagedEvent.IsCritical` & `MonsterEntity.TakeDamage(damage, isCritical)`**:
  - 피격 도메인 이벤트에 `IsCritical` 플래그를 실어 프레젠테이션 뷰 및 데미지 텍스트로 전달.

---

### 2. 💎 신규 패시브 스킬 '치명타의 눈' (`passive_crit`)
- **아이템명**: `치명타의 눈` (Hawk's Eye)
- **효과**: **크리티컬 확률 +8% & 크리티컬 데미지 +5% 증가** (최대 Lv.5까지 누적 스케일링)
- **인게임 레벨업 3지선다 카드 시스템 연동**: 레벨업 시 3지선다 보상 선택창에 정상 등장.
- **개발자 모드(`DevSkillSelectorUiView`) 연동**: 치트 창에서 좌클릭(+1Lv) / 우클릭(Lv.0 즉시 해제) 지원.
- **80x80 고해상도 픽셀아트 아이콘 (`RewardIconHelper.DrawCritEyeIcon`)**:
  - 황금빛 정밀 조준경 링(Reticle Ring) + 타오르는 네온 레드 동공 + 반짝이는 십자 광원(Crosshair Glint) 구현.

---

### 3. ✨ 크리티컬 비주얼 VFX & 타격감 극대화 연출
- **`CriticalHitVfxManagerView.cs` [NEW]**:
  - 무할당 32개 풀링 기반의 **황금빛 십자 섬광(Cross Beam) + 8방향 비산 스타버스트 스파크(Starburst Spark)** 이펙트.
  - 크리티컬 피격 좌표에서 0.18초 초고속 팝업 스케일 및 페이드아웃.
- **몬스터 피격 피드백 강화 (`MonsterView.cs`)**:
  - 일반 피격: 흰색 플래시, 부드러운 스쿼시 (0.22 / -0.18), 0.06초.
  - **크리티컬 피격**: **선명한 황금빛 플래시(Golden Flash)**, **2배 강력한 Squash & Stretch (0.45 / -0.35)**, **격렬한 좌우 셰이크 틸트 진동(+-16도)** 동시 폭발!
- **대형 플로팅 데미지 텍스트 (`DamageTextView.cs`)**:
  - 일반 데미지: 흰색 24pt
  - **크리티컬 데미지**: **볼드 42pt 대형 폰트**, **네온 골드 컬러**, **강조 느낌표("!") 부착**, **1.45배 튀어올랐다 착지하는 다이내믹 바운스 팝(Pop Animation)**!

---

### 4. 🧪 전투 & 밸런스 샌드박스 실시간 치명타/스탯 튜닝 (`crit_tuning`)
- **신규 탭**: 샌드박스 메인 탭에 **`🎯치명/스탯`** 탭 추가.
- **실시간 슬라이더 6종 제공**:
  1. **🎯 크리티컬 확률 (Crit Chance)**: `0% ~ 100%` (step: 1%)
  2. **💥 크리티컬 데미지 배율 (Crit Multiplier)**: `1.0x ~ 5.0x` (step: 0.05)
  3. **⚔️ 기본 공격력 배율 (Attack Power)**: `0.2x ~ 5.0x` (step: 0.1)
  4. **🏃 이동 속도 (Move Speed)**: `2.0 ~ 12.0` (step: 0.2)
  5. **🛡️ 방어력 (Armor)**: `0 ~ 100` (step: 1)
  6. **⏱️ 쿨타임 감소율 (CDR)**: `0% ~ 75%` (step: 1%)
- 슬라이더를 조작하는 즉시 실시간으로 플레이어 인게임 스탯에 100% 반영되어, 100% 확정 크리티컬이나 500% 데미지 실험을 즉시 플레이하며 테스트할 수 있습니다!

---

## 🧪 단위 테스트 및 검증 결과

- **신규 테스트 파일**: `Assets/tests/HappyShoot.Domain.Tests/Entities/CriticalStrikeTests.cs` (6개 테스트)
- **전체 도메인 테스트**: **110개 단위 테스트 100% ALL PASS** (0 Failed)
- **어셈블리 빌드**: `HappyShoot.Domain.csproj`, `HappyShoot.View.csproj`, `HappyShoot.Domain.Tests.csproj` 모두 컴파일 오류 0개, 경고 0개 통과.
- **문서화**: `APP_MAP.md`에 신규/수정 인터페이스 및 컴포넌트 100% 최신화 완료.
