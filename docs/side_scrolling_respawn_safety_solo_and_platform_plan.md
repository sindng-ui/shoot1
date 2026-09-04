# 횡스크롤 차원 모드 게임플레이 3대 편의성 & 밸런스 개선 계획

## 1. 개요 및 요구사항
형님의 요청 사항:
1. **낙하 후 리스폰 안전 위치 보장**: 징검다리에서 떨어져 1차 부활 시, 떨어지던 모서리 경계가 아닌 **발판 정중앙(안전한 장소)**에 나타나도록 개선.
2. **마법사 단독 진행 (동료 제거)**: 횡스크롤 차원 질주 모드에서는 동료(전사/궁수) 없이 오직 **마법사 혼자서** 호쾌하게 질주하고 점프하도록 설정.
3. **징검다리 너비 & 간격(Gap) 황금 밸런스 조정**: 발판 너비를 조금만 줄이고(4.2m -> 3.3m), 발판 사이 떨어져 있는 간격(Gap)을 0.8m에서 1.7m로 넓혀 징검다리 점프 액션의 손맛과 긴장감 강화.

---

## 2. 상세 변경 계획

### [A] 리스폰 안전 위치 & 안정화 (SideScrollPlatformManager / PlayerInputHandler)
- **원인**: 이전에는 플레이어가 발판 모서리 끝자락(낙하 직전 X 좌표)에 있을 때도 그 좌표를 안전 위치로 등록하여, 부활하자마자 다시 낭떠러지로 떨어지는 현상이 발생함.
- **개선**:
  - `TryGetPlatformAtX`에서 현재 밟고 있는 발판의 `CenterX`(발판 중심 좌표)를 반환하도록 개선.
  - 안전 리스폰 좌표(`_lastSafePlatformCenterX`, `_lastSafeSurfaceY`)를 발판 모서리가 아닌 **해당 발판의 정중앙 X좌표**로 고정 등록.
  - 리스폰 시 `_jumpVelocity = 0f`, `_isGrounded = true`로 착지 상태를 즉시 동기화하고, 리스폰 직후 1.2초간 무적(Invulnerability)을 부여하여 몬스터 피격 넉백으로 재추락하는 사고 방지.

### [B] 횡스크롤 모드 마법사 단독 진행 (SideScrollModeController)
- **개선**:
  - `EnterSideScrollMode()` 진입 시 필드의 모든 `CompanionView`를 `gameObject.SetActive(false)`로 즉시 비활성화.
  - 횡스크롤 모드 동안에는 화면에 오직 마법사만 노출되어 점프/대시/차원 질주에 온전히 몰입.
  - `ExitSideScrollMode()`(탑다운 복귀) 시 원래 보유하던 동료를 다시 활성화하여 일반 모드와의 리그레션 방지.

### [C] 징검다리 너비 및 틈새(Chasm Gap) 조정 (SideScrollPlatformManager)
- **현재 파라미터**:
  - `PlatformSpacing = 5.0f`
  - `PlatformWidth = 4.2f` (틈새 간격: `5.0 - 4.2 = 0.8m` - 너무 좁아서 징검다리 느낌이 부족함)
- **조정 파라미터**:
  - `PlatformSpacing = 5.0f`
  - `PlatformWidth = 3.3f` (발판 너비 적당히 축소)
  - 틈새 간격(Chasm Gap) = `5.0 - 3.3 = 1.7m`
  - 마법사의 점프 수평 도달 거리(약 3.2m~3.8m) 및 대시 거리(약 3.5m)에 완벽하게 부합하며, 징검다리를 뛰어넘는 쾌감 극대화.

---

## 3. 수정 대상 파일 및 라인 수 점검
- `Assets/src/HappyShoot.View/SideScroll/SideScrollPlatformManager.cs` (현재 361줄 -> 수정 후 ~380줄, 500줄 이하)
- `Assets/src/HappyShoot.View/SideScroll/SideScrollModeController.cs` (현재 359줄 -> 수정 후 ~370줄, 500줄 이하)
- `Assets/src/HappyShoot.View/Player/PlayerInputHandler.cs` (현재 205줄 -> 수정 후 ~215줄, 500줄 이하)
- `docs/app_map/VIEW_MAP.md` 명세 갱신
