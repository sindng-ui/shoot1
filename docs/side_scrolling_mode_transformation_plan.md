# [구현 계획서] 횡스크롤 차원 모드 완전 변신 (몬스터 횡스크롤 레인 + 배경 완전 차단 + 점프 조작)

## 1. 개요
- **목표**: 3회차 보스3 클리어 후 진입하는 횡스크롤 차원 모드가 "캐릭터만 횡스크롤인 어색한 상태"에서 벗어나, **배경, 몬스터, 조작감까지 100% 횡스크롤 전용 아케이드 게임으로 완벽 변신**하도록 개선합니다.
- **주요 문제점**:
  1. 기존 탑다운 격자 바닥 타일맵이 그대로 비쳐서 던전 위에 파란 발판만 얹어둔 것처럼 보임.
  2. 기존 탑다운 몬스터 스포너가 계속 돌아가거나 필드에 남아있던 박쥐/고블린이 360도 자유 탑다운 이동으로 마법사를 향해 대각선으로 쫓아옴.
  3. 횡스크롤 모드인데 수직 점프 조작이 없어 조작감이 굳어있음.

---

## 2. 세부 구현 계획

### A. 배경 완전 변신 (SideScrollBackgroundView & BackgroundManager)
1. **기존 탑다운 바닥 완전 은폐**:
   - 횡스크롤 진입 시 `BackgroundManager.Instance.gameObject.SetActive(false);`로 탑다운 격자 타일맵을 완전히 끕니다.
   - 횡스크롤 모드 종료 시 다시 활성화 복원.
2. **100% 불투명 차원 우주 백드롭 (Full-Screen Dimension Backdrop)**:
   - `SortingOrder = -60`의 풀스크린 솔리드 쿼드/스프라이트를 카메라에 부착하여 탑다운 흔적을 100% 원천 차단.
   - 짙은 네온 바이올렛/미드나이트 블랙 그라데이션, 반짝이는 별무리 펄스 연출.
3. **단단하고 매끄러운 횡스크롤 지면 & 지반 (Infinite Continuous Runway)**:
   - 지면 높이(Y = -1.8f) 바로 아래(Y = -2.2f)에 끊김 없이 연결되는 네온 룬 하이웨이 레일.
   - 레일 하단부를 꽉 채우는 차원 기단(Foundation Abyss)으로 완벽한 플랫폼 지면 완성.
4. **3중 패럴랙스 (Parallax Depth)**:
   - 원경(성운/은하): 0.15x 속도로 아득하게 이동.
   - 중경(고대 룬 모놀리스 오벨리스크): 0.45x 속도로 입체감 있게 흘러감.

---

### B. 몬스터 횡스크롤화 (MonsterEntity & SideScrollMonsterSpawner)
1. **기존 탑다운 몬스터 완전 정리 & 스폰 억제**:
   - 횡스크롤 모드 진입 즉시 기존 메인 스포너의 모든 활성 몬스터 일제 디스폰(`DespawnAll`).
   - 메인 스포너의 탑다운 스폰 루프 일시정지 (`IsSpawningSuppressed = true`).
2. **`MonsterEntity`에 횡스크롤 전용 이동 로직 도입**:
   - `IsSideScrollMode`, `SideScrollBaseY`, `SideScrollWaveAmplitude`, `SideScrollWaveSpeed` 프로퍼티 추가.
   - 횡스크롤 모드 활성화 시:
     - **수평 방향(X축)**: 우측 스폰 지점에서 좌측으로 일관되게 플레이어 전열을 향해 돌진 (`newX = Position.X - speed * dt`).
     - **수직 방향(Y축)**:
       - **지상형 (슬라임, 스켈레톤, 골렘)**: 지면 레일(Y = -1.8f)에 정확히 밀착하여 쿵쿵 돌진.
       - **공중형 (차원 박쥐, 파이어 임프)**: 공중 고도(Y = -0.2f)를 기준으로 부드러운 Sine 파동을 그리며 물결 비행.
     - **방향 플립**: 항상 좌측(플레이어 방향)을 노려보도록 스프라이트 플립 동기화.
3. **플레이어 투사체 & 동료 공격과의 100% 연동**:
   - 도메인 `MonsterEntity` 및 `SpatialGrid2D`에 정상 등록되므로, 마법사의 모든 스킬 투사체(관통 화살, 화염구, 번개, 회전 칼날)가 횡스크롤 몬스터들에게 통쾌하게 적중!
   - 타격 데미지 텍스트, 치명타, 사망 시 젬/골드 폭발 드랍 정상 연동.
4. **300m 차원 핵(Dimensional Void Core) 보스전 유지 및 강화**:
   - 300m 도달 시 우측에서 거대한 네온 핵 보스가 나타나고, 격파 시 보스 처치 연출 후 최종 승리 팝업으로 이어짐.

---

### C. 플레이어 조작감 혁신: 점프(Jump) & 대시(Dash) 콤보
1. **횡스크롤 전용 점프 기능 추가 (`PlayerInputHandler.cs`)**:
   - `W` 키 / `Up` 방향키 / 게임패드 위쪽 방향 입력 시 **상승 도약(점프)**!
   - 점프 상승 속도 8.5f, 중력 -22f 가속도 적용.
   - 지면(Y = -1.8f)에 닿으면 완벽 착지.
   - 점프 중에도 좌우 A/D 이동 가능하여 날아오는 공중 박쥐나 지상 슬라임을 점프로 뛰어넘거나 회피 가능!
2. **순간이동 대시 (Space 키)**:
   - 수평 대시로 전방 돌파 손맛 유지.

---

## 3. 파일별 변경 계획 및 줄 수 준수

1. `Assets/src/HappyShoot.Domain/Entities/MonsterEntity.cs` (현재 367줄 -> 약 395줄, 500줄 이하 엄수)
   - `IsSideScrollMode`, `SideScrollBaseY`, `SideScrollWaveAmplitude`, `SideScrollWaveSpeed` 추가.
   - `UpdateAI`에 횡스크롤 전용 1D 전진 및 파동 비행 로직 분기.

2. `Assets/src/HappyShoot.View/SideScroll/SideScrollModeController.cs` (현재 243줄 -> 약 265줄, 500줄 이하 엄수)
   - 횡스크롤 진입 시 `BackgroundManager.Instance.gameObject.SetActive(false)` 처리.
   - 메인 스포너 디스폰 및 `IsSpawningSuppressed = true` 호출.
   - 모드 종료 시 배경 및 스포너 정상 복원.

3. `Assets/src/HappyShoot.View/SideScroll/SideScrollBackgroundView.cs` (현재 196줄 -> 약 235줄, 500줄 이하 엄수)
   - 완전 불투명(Alpha 1.0)의 풀스크린 우주/차원 백드롭 쿼드 추가 (SortingOrder -60).
   - 바닥 레일 아래를 메우는 단단한 지반(Foundation Abyss) 메쉬/스프라이트 보강.

4. `Assets/src/HappyShoot.View/SideScroll/SideScrollMonsterSpawner.cs` (현재 327줄 -> 약 345줄, 500줄 이하 엄수)
   - 지상 돌진 몬스터(슬라임/골렘)와 공중 파동 몬스터(박쥐/임프)를 횡스크롤 파라미터와 함께 스폰.
   - 플레이어 좌측 화면 밖으로 벗어난 몬스터 안전 디스폰.

5. `Assets/src/HappyShoot.View/Player/PlayerInputHandler.cs` (현재 120줄 -> 약 155줄, 500줄 이하 엄수)
   - 횡스크롤 모드일 때 W/Up 키 수직 점프 물리(점프 속도 + 중력 가속도) 구현.

---

## 4. 검증 계획
- 유니티 컴파일 오류 0건 검증.
- `[🚀 횡스크롤 즉시 진입]` 치트 버튼 클릭 후:
  1. 기존 탑다운 격자 바닥이 100% 사라지고 신비로운 네온 우주 횡스크롤 배경으로 전환되는지 확인.
  2. 몬스터(박쥐, 슬라임)가 오른쪽 화면 밖에서 나와 지상 레일 또는 공중 물결 비행으로 왼쪽을 향해 일관되게 전진하는지 확인.
  3. W/Up 키로 점프하여 몬스터를 뛰어넘거나 피할 수 있는지 확인.
  4. 마법사와 동료들의 공격에 몬스터가 정상 피격 및 처치되는지 확인.
  5. 300m 도달 시 차원 핵 보스가 등장하고 승리로 이어지는지 확인.
