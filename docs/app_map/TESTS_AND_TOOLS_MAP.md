# 🧪 HappyShoot Tests & Tools Map (`TESTS_AND_TOOLS_MAP`)

[🔙 메인 APP_MAP으로 돌아가기](../../APP_MAP.md)

> **경로**: `Assets/tests/`, `Assets/Editor/`, `.github/workflows/`  
> **특징**: 총 140개 이상의 100% 무결점 NUnit 단위 테스트 스위트, 원클릭 크로스플랫폼 에디터 빌드 자동화 및 GitHub Actions CI/CD 파이프라인

---

## 🏛️ 테스트 및 인프라 구조 요약

- **Domain Tests (`Assets/tests/HappyShoot.Domain.Tests`)**: 순수 C# 도메인 모델, 계산 공식, 스킬/패시브, 상태이상, 룬 인스크립션 단위 테스트
- **View Tests (`Assets/tests/HappyShoot.View.Tests`)**: 지팡이 피벗/파지 위치, 스프라이트 포맷 및 규격 무결성 테스트
- **Editor Automation (`Assets/Editor`)**: Unity 에디터 상단 메뉴 원클릭 Windows / Android (APK/AAB) 빌드
- **CI/CD Automation (`.github/workflows`)**: GitHub Actions 기반 원격 자동 빌드, 아티팩트 압축 및 릴리스 배포

---

## 📂 파일 및 세부 명세

### 1. NUnit 단위 테스트 스위트 (140+ Tests, 100% ALL PASS)
| 테스트 스위트 파일 | 대상 모듈 | 주요 검증 내용 |
| :--- | :--- | :--- |
| `RuneSystemTests.cs` | `RuneManager`, `CompositeSkill` | 12종 룬 등록, 지갑 보석 차감 해금, 레벨업 강화 수치 스케일링, 스킬 슬롯 장착/해제, CompositeSkill 실시간 룬 주입 및 쿨다운 단축/무료시전 검증 (5개 테스트) |
| `WizardStaffPlacementTests.cs`| `WizardWeaponPlacementHelper` | 마법사 8방향+정면/후면 지팡이 오른손 1:1 스냅, 각도, flipX, 소팅오더, 캐스팅 펄스 고도화 및 중심 고정 검증 (11개 테스트) |
| `WarriorSkillsTests.cs` | 전사 스킬군 | 지면 강타 도메인 반경 검증, 휠윈드 360도 전방위 4방향 타격, 휠윈드 레벨업 대미지/반경 스케일링, 블러드 이터 150도 전방 부채꼴 적중 및 라이프스틸 회복 |
| `WizardSkillsTests.cs` | 마법사 스킬군 | 화염구 스플래시 판정 및 데미지, 서리 폭발 360도 전방위 적 피격, 연쇄 번개 4회 전이 타격, 마법사 팩토리 스탯, 타 클래스 스킬 배제 |
| `GreatswordSlashTests.cs` | `GreatswordSlashEffect` | 전방 150도 궤적 적 피격, 궤적 반대편 적 무피격, 사거리 밖 적 무피격, `PlayerSlashExecutedEvent` 및 사운드 이벤트 발행 |
| `OrbitingBladesTests.cs` | `OrbitingBladesEffect` | 오비탈 궤도 위치 계산 및 회전 충돌 다중 타격 |
| `SkillEvolutionTests.cs` | `SkillEvolutionManager` | 9대 진화 레시피 합성, 진화 카드 우선순위 추천, 궁극기 진화 후 기본 스킬 선택지 완전 제외 |
| `PassiveItemsTests.cs` | 패시브 9종 | `passive_crit`(치명타의 눈) 레벨업 시 크리티컬 확률 +8% 및 크리티컬 배율 +5% 누적 증가 등 |
| `CriticalStrikeTests.cs` | 치명타 시스템 | 기본 크리 10% 검증, 100% 확정 크리티컬 피해량 배율(2.5x) 연산, `MonsterDamagedEvent.IsCritical` 플래그 발행 |
| `StatusEffectTests.cs` | 상태이상 시스템 | 오한 40% 감속 및 만료 시 정상 복구, 7초 화염 DoT 틱 누적, 7초 감전 DoT 틱 누적, 오한 사망 시 `MonsterShatteredEvent` 발행, 메테오 스트라이크 화상 DoT |
| `LevelSystemTests.cs` | `LevelSystem`, 경험치 | 레벨업 경험치 스케일링 및 경험치 증가분 대비 몹 체력 배율(`MobHpScalingRatio`) 연산 검증 |
| `MonsterVarietyTests.cs` | 몬스터 아키타입 | 4종 아키타입 스탯, 해골 원거리 카이팅 AI, 보스 스폰/피격/사망 이벤트 |
| `TreasureChestTests.cs` | 보물상자 | 상자 스폰, 접근 오픈 및 보상 지급, 보스 사망 시 상자 자동 드랍 |
| `GameSessionTests.cs` | `GameSessionEntity` | 세션 생명주기, 시간 틱, 킬 수/골드 누적, 일시정지, 게임오버/승리 전이 (13개 테스트) |
| `MetaShopTests.cs` | `MetaShopManager` | 골드 추가, 업그레이드 레벨별 구매/차감, 최대 레벨 초과 구매 방지, 100% 환불 골드 계산 |
| `MetaSaveDataTests.cs` | 메타 세이브 데이터 | 업그레이드 데이터 직렬화 및 `MetaUpgradeApplier` 스탯 반영 공식 |
| `AudioEventsTests.cs` | 사운드 이벤트 | 사운드 및 BGM 도메인 이벤트 발행, 수신, 페이로드 정합성 |
| 기타 핵심 단위 테스트 | Core Systems | `PlayerEntityTests`, `MonsterEntityTests`, `MonsterSpawnerTests`, `CharacterClassTests`, `SkillCompositionTests`, `SpatialGridTests`, `ExpGemTests`, `ProjectileTests`, `WaveTimelineTests`, `DamageTextTests`, `EventBusTests`, `TimeProviderTests` |

---

### 2. Editor Build Automation (`Assets/Editor`)
| 파일명 | 주요 클래스 / 메서드 | 기능 및 설명 |
| :--- | :--- | :--- |
| `BuildScript.cs` | `BuildScript` | **원클릭 에디터 상단 메뉴 & 헤드리스 배치모드 자동화 빌드 스크립트**<br>• `HappyShoot > Build > Build Windows 64-bit`<br>• `HappyShoot > Build > Build Android (APK)`<br>• `HappyShoot > Build > Build Android (Google Play AAB)`<br>• 공통 `ExecuteBuild()` 파이프라인, SampleScene 자동 수집, BuildReport 검증, 에디터 완료/실패 다이얼로그 팝업 안내, CI/CD 종료코드 반환 (196줄) |

---

### 3. CI/CD Workflows (`.github/workflows`)
| 파일명 | 워크플로우 명 | 기능 및 설명 |
| :--- | :--- | :--- |
| `.github/workflows/build.yml` | `CI Workflow` | **GitHub Actions Windows Standalone (.exe) 자동 빌드 파이프라인**: Game-CI 액션을 통해 커밋 푸시/PR 시 자동 빌드, 무결성 검증, Zip 아티팩트 업로드 및 Git Release 자동 게시 |
| `.github/workflows/activation.yml` | `Activation Workflow` | **Game-CI v2 공식 Unity 활성화 요청 파일(.alf) 자동 생성**: 수동 라이선스 파일 발급을 위한 간소화 워크플로우 |
| `docs/GITHUB_ACTIONS_SETUP.md` | `CI Setup Guide` | Unity 개인 무료 라이선스 활성화 및 GitHub Secrets(`UNITY_LICENSE`) 등록 단계별 매뉴얼 |

---

[🔙 메인 APP_MAP으로 돌아가기](../../APP_MAP.md)
