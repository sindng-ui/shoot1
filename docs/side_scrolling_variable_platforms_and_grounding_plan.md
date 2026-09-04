# [구현 계획서] 횡스크롤 입체 징검다리 (오르락내리락 높낮이 발판) & 100% 완벽 착지 물리

## 1. 개요 및 문제 분석
- **문제점 ("천천히 가라앉는다")**:
  - `SideScrollBackgroundView`의 발판 그래픽 위치와 `SideScrollPlatformManager`의 가상 발판 계산 위치가 어긋나 있었음.
  - 게다가 `PlayerInputHandler`는 Y를 -1.8f로 강제하고, `SideScrollPlatformManager`는 틈새로 판단하여 아래로 떨어뜨리려고 하여 서로 충돌, 마법사가 발판을 뚫고 천천히 가라앉는 버그 발생.
- **개선 목표**:
  1. **완벽한 착지 (1:1 Single Source of Truth)**: 발판의 시각 렌더링과 물리 판정을 `SideScrollPlatformManager`로 100% 단일화하여 절대로 가라앉지 않고 발판 위에 딱 안착하도록 수정.
  2. **오르락내리락 다양한 높낮이 발판**:
     - 기본 발판 (`SurfaceY = -1.8f`)
     - 중간 높이 발판 (`SurfaceY = -0.9f`)
     - 높은 공중 발판 (`SurfaceY = 0.0f`)
     - 계단처럼 오르락내리락 배치하여 점프로 높은 발판을 정복하고 뛰어내리는 플랫포머 손맛 극대화!
  3. **2회 추락 탈락 룰 안정화**:
     - 발판 밖으로 떨어져 Y <= -4.5f에 도달하면 1회차는 직전 발판으로 부활(남은 목숨 1개), 2회차는 탈락.
  4. **동료들의 발판 높이 적응**:
     - 동료들도 자기가 딛고 있는 발판 높이에 정확히 발을 붙이고 마법사를 따라 오르락내리락 이동.

---

## 2. 세부 구현 계획

### A. 징검다리 플랫폼 매니저 혁신 (`SideScrollPlatformManager.cs`)
1. **시각 + 물리 통합 플랫폼 (Visual + Collision Unified)**:
   - `SideScrollBackgroundView`의 중복 발판 레이어를 제거하고, `SideScrollPlatformManager`가 실제 발판 타일 오브젝트들을 직접 스폰 및 관리.
   - 각 발판:
     - 중심 X: `i * 5.5f`
     - 폭: 3.8m (틈새 1.7m: 점프/대시로 건너뛰기 최적화)
     - 높이 패턴:
       - `i % 4 == 0`: 기본 지면 발판 (`Y = -1.8f`)
       - `i % 4 == 1`: 살짝 높은 발판 (`Y = -1.0f`)
       - `i % 4 == 2`: 높은 공중 발판 (`Y = -0.2f`)
       - `i % 4 == 3`: 중간 발판 (`Y = -1.0f`)
     - 발판의 상판에는 네온 룬 엣지가 빛나고, 하단은 짙은 차원 지반이 든든하게 받쳐줌.
2. **지면 높이 쿼리 API 제공**:
   - `bool TryGetPlatformSurface(float x, float currentY, out float surfaceY)`:
     - 플레이어(또는 동료)의 X좌표 아래에 발판이 있고, 발이 발판 상판 높이보다 위에 있거나 닿아 있으면 해당 `surfaceY` 반환!
     - 아래에 발판이 없으면 `false` 반환 -> 중력에 의해 낙하!

---

### B. 플레이어 물리 및 착지 처리 (`PlayerInputHandler.cs`)
1. **발판 안착 물리**:
   - 공중에 있을 때 중력(`_jumpVelocity += Gravity * dt`) 적용.
   - 떨어지면서 아래에 있는 발판의 `surfaceY`에 닿으면 즉시 **착지 완료**! (`_isGrounded = true`, `_jumpVelocity = 0f`, `currentY = surfaceY`).
   - 발판 밖 낭떠러지로 걸어가면 지지대가 사라져 자연스럽게 아래 심연으로 낙하!
2. **점프 (`W`/`Up`)**:
   - 현재 딛고 있는 발판 높이(`surfaceY`)에서 위로 힘차게 도약(`JumpSpeed = 9.5f`).
   - 점프하여 더 높은 발판(`Y = -1.0f` or `-0.2f`) 위로 올라탈 수 있음!
3. **심연 낙하 및 부활 (2 Lives Rule)**:
   - 심연으로 떨어져 `currentY <= -4.8f`에 도달하면 `SideScrollPlatformManager.OnChasmFall()` 호출.
   - 1회차: 직전 안전 발판의 `surfaceY` 위로 순간이동 부활! (❤️💔)
   - 2회차: 탈락 안내 및 원래 세계로 귀환.

---

### C. 동료들의 발판 서있기 (`CompanionView.cs`)
- 동료들도 `PlatformManager.TryGetPlatformSurface`를 통해 현재 서 있는 발판의 높이에 발을 붙임.
- 마법사가 높은 발판으로 건너가면 가볍게 도약하여 함께 이동.

---

## 3. 500줄 이하 엄수 계획
- `SideScrollPlatformManager.cs`: 약 180줄 (안전)
- `SideScrollBackgroundView.cs`: 약 200줄 (중복 발판 제거로 슬림화)
- `PlayerInputHandler.cs`: 약 160줄 (안전)
- `CompanionView.cs`: 약 485줄 (안전)

---

## 4. 검증 계획
1. **컴파일 검증**: 오류 0건 및 전 파일 500줄 이하 엄수 확인.
2. **착지 검증**: 마법사가 발판 위에 섰을 때 가라앉지 않고 발판 상판에 딱 서 있는지 확인.
3. **높낮이 점프 검증**:
   - 낮은 발판(-1.8f)에서 중간 발판(-1.0f), 높은 발판(-0.2f)으로 점프하여 올라갈 수 있는지 확인.
   - 높은 발판에서 낮은 발판으로 자연스럽게 뛰어내릴 수 있는지 확인.
4. **2회 추락 룰 검증**:
   - 낭떠러지로 떨어졌을 때 직전 발판으로 부활하는지 확인.
   - 2회 떨어지면 탈락 처리되는지 확인.
