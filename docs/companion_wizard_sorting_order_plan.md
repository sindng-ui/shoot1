# 마법사 & 동료 스프라이트 렌더링 우선순위(Sorting Order) 개선 계획

## 1. 개요 및 문제 분석
- **현상**: 동료(전사/궁수)와 마법사(플레이어 본체)가 겹쳐 있을 때, 전사가 마법사 앞을 가려 마법사가 보이지 않는 현상 발생.
- **원인 분석**:
  - `PlayerView` (마법사 본체)와 `CompanionView` (전사/궁수 동료) 모두 동일한 Sprite Renderer `sortingOrder = 15` (무기 16)를 사용.
  - Unity 2D 스프라이트 렌더링 파이프라인에서 동일 레이어/동일 오더일 경우 생성 순서나 계층 구조에 따라 나중에 생성된 CompanionView가 PlayerView 위에 겹쳐 렌더링됨.
  - 특히 전사가 근접 공격을 할 때 무기(`sortingOrder = 30`)와 슬래시 이펙트(`sortingOrder = 25`)가 마법사(15~16)를 완전히 덮어버림.

## 2. 해결 방안: 계층형 Sorting Order 재설계
몬스터, 동료, 플레이어, 투사체, UI 간의 명확한 Order in Layer 구간 분리:

| 대상 | 요소 | 기존 sortingOrder | 변경 sortingOrder | 설명 |
| :--- | :--- | :---: | :---: | :--- |
| **그림자** | 공통 2.5D 타원 그림자 | 8 | **8** | 바닥 위, 모든 캐릭터 아래 |
| **몬스터** | 몬스터 그림자 / 몸체 | 9 / 10 | **9 / 10** | 기존 유지 |
| **동료 (Companion)** | 등 뒤 무기 (isBack) | 14 | **11** | 동료 몸체 뒤 |
| | 몸체 (Body) | 15 | **12** | 몬스터(10) 위, 마법사(15~) 아래 |
| | 앞손 무기 | 16 | **13** | 동료 몸체 앞 |
| | 슬래시/공격 무기 및 이펙트 | 25, 30 | **14** | 동료 몸체 앞, 마법사(15~) 아래 |
| **마법사 (Player)** | 등 뒤 지팡이 (isBack) | 14 | **15** | 동료 전체(11~14) 위, 마법사 몸체 뒤 |
| | 대시 잔상 (GhostTrail) | 14 | **15** | 마법사 몸체 바로 뒤 |
| | 몸체 (Body) | 15 | **16** | **동료 전체보다 무조건 앞** |
| | 앞손 지팡이 | 16 | **17** | 마법사 몸체 앞 |
| | 공격 피크 / 슬래시 | 20, 30 | **18** | 마법사 앞, 체력바(20) 아래 |
| **HUD & 이펙트** | 플레이어 체력바 | 20, 21 | **20, 21** | 기존 유지 |
| | 투사체 / 마법 스킬 VFX | 22 ~ 30 | **22 ~ 30** | 기존 유지 |

## 3. 변경 대상 파일
1. `Assets/src/HappyShoot.View/Companion/CompanionView.cs`
   - 기본 몸체 sortingOrder: 15 -> 12
   - 기본 무기 sortingOrder: 16 -> 13
   - 등 뒤 무기 sortingOrder: 14 -> 11
   - 슬래시 이펙트 & 공격 시 무기 sortingOrder: 25/30 -> 14
2. `Assets/src/HappyShoot.View/Player/PlayerView.cs`
   - 기본 몸체 sortingOrder: 15 -> 16
   - 기본 무기 sortingOrder: 16 -> 17
   - 방향 전환 시 등 뒤/앞손 무기: 15 / 17
   - 무기 공격 피크 / 슬래시: 18
3. `Assets/src/HappyShoot.View/Utils/WizardWeaponPlacementHelper.cs`
   - 지팡이 계산 시 `sortingOrder`: `isBack ? 15 : 17`
4. `Assets/tests/HappyShoot.View.Tests/WizardStaffPlacementTests.cs`
   - 변경된 지팡이 레이어(15, 17)에 맞춰 단위 테스트 검증값 업데이트
5. `APP_MAP.md` 및 `docs/app_map/VIEW_MAP.md`
   - 플레이어-동료 간 렌더링 계층 명세 최신화

## 4. 검증 계획
- 단위 테스트 실행: `dotnet test` (또는 NUnit Runner)를 통해 `WizardStaffPlacementTests` 전체 통과 검증
- 코드 검토: 500줄 초과 여부 확인 (PlayerView: 490줄 유지, CompanionView: 456줄 유지)
