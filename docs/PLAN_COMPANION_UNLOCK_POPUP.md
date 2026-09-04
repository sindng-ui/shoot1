# 🛡️🏹 보스 처치 시 신규 동료 영입 전용 팝업 연출 ('짜잔!') 구현 계획

최종 보스인 **사령왕 리치(Arch-Lich Malakar, Boss 3)**를 처치하고 1회차/2회차를 클리어했을 때, 단순 텍스트 한 줄로 안내되던 기존 방식을 개선하여, **선명하고 큰 동료 도트 일러스트와 함께 팡파레를 울리며 "짜잔!" 하고 등장하는 전용 영입 축하 팝업(`CompanionUnlockPopupView`)**을 구현합니다.

---

## 📋 사용자 검토 및 확인 요청 (User Review Required)

> [!IMPORTANT]
> **연출 흐름 및 UI 전환 방식**:
> 1. 보스 3(리치 킹) 격파 시 즉시 게임이 일시정지(`Time.timeScale = 0`)되며 신규 동료 해금 여부를 판별합니다.
> 2. **1회차 클리어 시** ➔ **[전사(Warrior) 영입 팝업]**이 먼저 화면 중앙에 웅장한 팡파레 SFX와 함께 등장합니다.
> 3. **2회차 클리어 시** ➔ **[궁수(Ranger) 영입 팝업]**이 먼저 화면 중앙에 웅장한 팡파레 SFX와 함께 등장합니다.
> 4. 유저가 팝업 하단의 **`[ ⚔️ 원정대에 합류시키기 ]`** 버튼을 누르면, 팝업이 닫히며 기존 **스테이지 승리 정산 창(`StageVictoryUiView`)**으로 매끄럽게 연결됩니다.
> 5. **3회차 이상 클리어 시**에는 신규 해금 동료가 없으므로 기존대로 스테이지 승리 정산 창이 바로 뜹니다.

> [!TIP]
> **성능 및 리소스 최적화**:
> - 불필요한 GPU 부하를 유발하는 `Blur` 효과는 완전히 배제하고, 깔끔한 딥 네이비 반투명 배경 및 클래스별 시그니처 테두리(전사: 앰버 골드, 궁수: 에메랄드 그린)를 적용합니다.
> - 스프라이트는 기존에 검증된 고품질 도트 엔진인 [`HeroSpriteHelper`](file:///k:/unityprojects/shoot1/shoot1/Assets/src/HappyShoot.View/Utils/HeroSpriteHelper.cs)를 100% 재사용하여 메모리 할당을 최소화합니다.
> - 파일 분리 원칙에 따라 200줄 내외의 단일 책임 뷰 컴포넌트로 분리 생성하여 파일당 500줄 제한 규칙을 엄격히 준수합니다.

---

## 🏛️ 제안하는 변경 사항 (Proposed Changes)

### 🎮 Presentation Layer (Unity View)

#### [NEW] [CompanionUnlockPopupView.cs](file:///k:/unityprojects/shoot1/shoot1/Assets/src/HappyShoot.View/UI/CompanionUnlockPopupView.cs)
- **위치**: `Assets/src/HappyShoot.View/UI/CompanionUnlockPopupView.cs`
- **역할**: 보스 처치로 동료가 최초 해금될 때 화면 전체를 덮으며 나타나는 축하 모달 팝업
- **주요 기능**:
  - `Show(CompanionType type, Action onClosed)`: 동료 타입에 맞춰 팝업 동적 구성
  - **대형 아바타 렌더링**: `HeroSpriteHelper.GetHeroSprite(classType, Front, 32)` 기반 140x140 픽셀 선명한 스프라이트 및 프레임
  - **타이틀 & 칭호**:
    - 🛡️ 전사: `"🎉 신규 동료 영입! [호위 전사 (Warrior)]"` / `"대검의 수호자"`
    - 🏹 궁수: `"🎉 신규 동료 영입! [지원 궁수 (Ranger)]"` / `"바람의 명사수"`
  - **전투 역할 & 시너지 설명**:
    - 전사: *"마법사를 근접 호위하며 대검으로 전방의 몬스터를 격파합니다!"*
    - 궁수: *"마법사의 후방에서 관통 화살로 사각지대를 정밀 타격합니다!"*
    - 공통: `"(본체의 공격력 및 레벨업 보너스가 1/3 비율로 동료에게 실시간 연동됩니다)"`
  - **사운드**: `PlaySoundEvent(SoundEffectType.WeaponEvolve, 1.0f)` 팡파레 효과음 재생
  - **닫기 액션**: `[ ⚔️ 마법 원정대 합류 완료! ]` 버튼 클릭 시 `onClosed` 호출 및 UI 언마운트/비활성화

---

#### [MODIFY] [StageVictoryUiView.cs](file:///k:/unityprojects/shoot1/shoot1/Assets/src/HappyShoot.View/UI/StageVictoryUiView.cs)
- `CompanionUnlockPopupView` 참조 보유 및 `Initialize()` 시 주입/생성
- `ShowVictoryPopup()`에서 클리어 횟수 판별:
  - `clears == 1` ➔ `_unlockPopup.Show(CompanionType.Warrior, () => ShowMainVictoryDialog())`
  - `clears == 2` ➔ `_unlockPopup.Show(CompanionType.Ranger, () => ShowMainVictoryDialog())`
  - `clears >= 3` ➔ 즉시 `ShowMainVictoryDialog()`
- 승리 정산 창 내부의 `_companionRewardText` 안내 문구도 더욱 가독성 높고 깔끔하게 정돈

---

#### [MODIFY] [GameBootstrap.cs](file:///k:/unityprojects/shoot1/shoot1/Assets/src/HappyShoot.View/Bootstrap/GameBootstrap.cs)
- `StageVictoryUI` 생성 시 `CompanionUnlockPopupView` 컴포넌트를 함께 등록하고 바인딩

---

### 🗺️ 문서 업데이트 (Documentation)

#### [MODIFY] [APP_MAP.md](file:///k:/unityprojects/shoot1/shoot1/APP_MAP.md) 및 [VIEW_MAP.md](file:///k:/unityprojects/shoot1/shoot1/docs/app_map/VIEW_MAP.md)
- Presentation Layer 다이어그램 및 UI 컴포넌트 목록에 `CompanionUnlockPopupView` 명세 추가 및 링크 연결

---

## 🧪 검증 계획 (Verification Plan)

### 1. 자동화 단위 테스트 검증
- NUnit 도메인 테스트 스위트 확인:
  - `CompanionTests.cs` (클리어 카운트별 해금 로직 정합성)
  - `GameSessionTests.cs` (세션 승리 전이 정합성)

### 2. 코드 및 컴파일 검증
- 리눅스 WSL 커맨드로 C# 문법, 네임스페이스 및 참조 무결성 검증
- 500줄 초과 여부 확인 (`wc -l`)

### 3. 수동 검증 가이드
- 개발자 치트 콘솔(`DevSkillSelectorUiView`) 또는 게임 플레이를 통해 보스 3을 처치했을 때:
  1. 1회차 클리어 시 전사 획득 팝업이 대형 이미지와 함께 뜨는지 확인
  2. 버튼 클릭 시 스테이지 승리 화면으로 이어지는지 확인
  3. 로비 및 다음 세션에서 전사가 정상 소환되어 호위하는지 확인
  4. 2회차 클리어 시 궁수 획득 팝업이 뜨는지 확인
