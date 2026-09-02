# 안드로이드 모바일 가로 모드 & 플로팅 터치 조이스틱 복원 계획

## 1. 개요
브랜치 분기(`bef368c` vs `ce295ce`)로 인해 최신 main에 반영되지 못했던 **모바일 가로 모드(Landscape)** 및 **플로팅 가상 조이스틱 터치 이동 시스템**을 현재 최신 브랜치(대장간/동행자 유지)에 무손실 통합 복원합니다.

## 2. 세부 변경 계획
1. **신규 컴포넌트 복원 (`bef368c` 커밋 기반)**:
   - `Assets/src/HappyShoot.View/UI/TouchJoystickView.cs` (플로팅 가상 조이스틱)
   - `Assets/src/HappyShoot.View/UI/TouchJoystickSpriteHelper.cs` (픽셀아트 링/노브 텍스처)
   - `Assets/src/HappyShoot.View/UI/MobilePauseButtonView.cs` (모바일 우상단 [⏸] 일시정지 버튼)
   - `Assets/src/HappyShoot.View/UI/PlayerDamageVignetteView.cs` (피격 비네트)
   - `Assets/src/HappyShoot.View/Player/PlayerHitFeedbackView.cs` (플레이어 피격 피드백)
2. **입력 핸들러 수정 (`PlayerInputHandler.cs`)**:
   - `SetTouchJoystick()` 바인딩 및 가상 조이스틱 입력 벡터 수신
   - 키보드/게임패드/터치 하이브리드 지원
3. **부트스트랩 초기화 결합 (`GameBootstrap.cs`)**:
   - `Screen.sleepTimeout = SleepTimeout.NeverSleep;`
   - `Application.targetFrameRate = 60;`
   - HUD 캔버스에 가상 조이스틱 및 모바일 일시정지 버튼 인스턴스 생성 및 연결 (대장간/동행자 기능 100% 보존)
4. **가로 모드 고정 (`ProjectSettings/ProjectSettings.asset`)**:
   - `defaultScreenOrientation: 3` (Landscape Left)
   - 세로 회전 비활성화 (`allowedAutorotateToPortrait: 0`, `allowedAutorotateToPortraitUpsideDown: 0`)
   - 가로 좌/우 회전 허용 (`allowedAutorotateToLandscapeRight: 1`, `allowedAutorotateToLandscapeLeft: 1`)
5. **APP_MAP.md 갱신**

## 3. 검증 계획
- Unity 에디터 컴파일 에러 0건 확인
- PlayerSettings 화면 방향 값 검증
- 500줄 초과 여부 확인
