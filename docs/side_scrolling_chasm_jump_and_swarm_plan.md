# [구현 계획서] 횡스크롤 징검다리 점프 낙하 액션 (2회 기회) + 적 대량 떼스폰 + 동료 독립 보행

## 1. 개요 및 요구사항
- **요구사항 1**: "지금 보면 띄엄 띄엄 징검다리같은 배경이 보이는데 이걸 점프해서 넘어 가도록 하자."
- **요구사항 2**: "떨어지면 한번 더 기회를 주고 두번떨어지면 횡스크롤 게임 탈락이야."
- **요구사항 3**: "몹들도 훨씬더 많이 앞쪽에서 몰려 나와야함. 재미요소!"
- **요구사항 4**: "동료들이 너무 고정되서 마법사랑 동시에 움직이는데 얘들도 별개로 마법사 범위 근처에서 적당히 걸어다닌다. 절대로 마법사에 고정되서 끌려오지 않고 걸어다닌다."

---

## 2. 세부 시스템 설계

### A. 징검다리 플랫폼 & 2회 추락 탈락 시스템 (`SideScrollPlatformManager.cs`)
1. **징검다리 플랫폼 배치 (Stepping Platforms)**:
   - 각 발판 길이: 3.8m, 발판 간 틈새(Chasm Gap): 1.7m (점프 `W`/`Up` 또는 대시 `Space`로 시원하게 뛰어넘는 황금 비율).
   - 발판 높이: `Y = -1.8f`. 발판 아래는 아득한 차원 심연.
2. **낙하 판정 (Pit Fall Detection)**:
   - 마법사의 현재 X좌표가 발판 위에 올라서 있지 않은 상태에서 Y <= -1.8f로 내려오면 지지대가 없어 **심연으로 추락**!
   - 추락 가속도와 함께 화면이 쑥 꺼지며 셰이크 & 추락 사운드.
3. **목숨 2회 룰 (2 Lives Rule)**:
   - 상단 HUD에 `[차원 생명력: ❤️ ❤️]` 표시.
   - **1회차 추락 (기회 제공)**:
     - 마법사가 차원 균열로 떨어지면, 화면이 순간 번쩍이며 직전 안전 발판의 중앙으로 되감기(Rewind) 부활!
     - HUD: `[차원 생명력: ❤️ 💔] (⚠️ 1회 더 추락 시 탈락!)`
   - **2회차 추락 (최종 탈락)**:
     - "💀 차원의 심연으로 빨려 들어갔습니다!"
     - 횡스크롤 모드 종료 및 탈락 처리 -> 원래 현실 세계로 귀환하여 세션 결산.

---

### B. 동료(전사 & 궁수)의 살아 숨쉬는 독립 보행 AI (`CompanionView.cs`)
1. **마법사 고정 바인딩 완전 해제**:
   - 마법사가 움직일 때마다 동시에 자석처럼 끌려오던 코드를 완전히 제거.
2. **발판 위 자율 보행 및 독립 전투**:
   - 마법사 주변 발판 위에서 **자기 주변의 몬스터를 향해 스스로 걸어가서 공격**!
     - 전사: 몬스터를 향해 지면을 걸어가서 대검 휘두르기.
     - 궁수: 몬스터와 사거리를 유지하며 관통 화살 사격.
   - 마법사가 다음 징검다리로 건너가서 거리가 4.5m 이상 멀어질 때만:
     - "마법사님 기다려요!" 하고 가볍게 점프 도약하여 마법사가 있는 발판으로 안착!
   - 걸을 때 상하로 통통 튀는 **보빙(Bobbing) 애니메이션**을 적용하여 살아있는 원정대 느낌 100%!

---

### C. 적 대량 떼스폰 & 횡스크롤 전용 재미요소 (`SideScrollMonsterSpawner.cs`)
1. **적 물량 대폭발 (Massive Waves)**:
   - 스폰 간격 0.5~0.7초로 단축.
   - **지상 대군단**: 슬라임 5~8마리 줄지어 돌진 + 거대 골렘 1~2마리 샌드백.
   - **공중 편대**: 차원 박쥐 4~6마리가 상/중/하 다층 고도에서 Sine 파동을 그리며 쇄도.
   - 마법사의 쿨다운 50% 폭주 마법과 관통 화살, 대검 베기가 쏟아지며 수십 마리가 팝콘처럼 팡팡 터져나가는 통쾌한 무쌍 쾌감!
2. **차원 불안정 폭발 수정 (Unstable Void Crystal)**:
   - 발판 길목에 불안정하게 깜빡이는 붉은 수정 스폰.
   - 공격 시 `BOOM!!` 거대한 연쇄 폭발로 반경 6m 내 모든 적을 일격에 날려버림!
3. **차원 초가속 링 (Speed Dash Ring)**:
   - 통과 시 3.5초간 초고속 질주 + 푸른 잔상 트레일 + 마주치는 모든 적을 튕겨내며 100 데미지 로드킬(Roadkill) 쾌감!

---

## 3. 500줄 이하 엄수 모듈화 구조

| 파일 경로 | 예상 라인 수 | 주요 역할 |
|---|---|---|
| [SideScrollPlatformManager.cs](file:///k:/unityprojects/shoot1/shoot1/Assets/src/HappyShoot.View/SideScroll/SideScrollPlatformManager.cs) [NEW] | ~160줄 | 징검다리 발판 좌표 관리, 낙하 감지, 2회 목숨(Lives) 판정 및 리스폰/탈락 콜백 |
| [UnstableVoidCrystalView.cs](file:///k:/unityprojects/shoot1/shoot1/Assets/src/HappyShoot.View/SideScroll/UnstableVoidCrystalView.cs) [NEW] | ~110줄 | 피격 시 반경 6m 내 모든 적을 날려버리는 연쇄 폭발 수정 |
| [SpeedBoostRingView.cs](file:///k:/unityprojects/shoot1/shoot1/Assets/src/HappyShoot.View/SideScroll/SpeedBoostRingView.cs) [NEW] | ~90줄 | 통과 시 3.5초간 초고속 질주 + 100뎀 로드킬 충격파 |
| [DimensionalVoidCoreView.cs](file:///k:/unityprojects/shoot1/shoot1/Assets/src/HappyShoot.View/SideScroll/DimensionalVoidCoreView.cs) [NEW] | ~130줄 | 300m 차원 핵 보스 뷰 및 격파 연출 |
| [SideScrollMonsterSpawner.cs](file:///k:/unityprojects/shoot1/shoot1/Assets/src/HappyShoot.View/SideScroll/SideScrollMonsterSpawner.cs) | ~230줄 (슬림화) | 대량 몬스터 떼스폰 스케줄링 및 수정/링/보석 이벤트 트리거 |
| [SideScrollModeController.cs](file:///k:/unityprojects/shoot1/shoot1/Assets/src/HappyShoot.View/SideScroll/SideScrollModeController.cs) | ~260줄 | 징검다리 매니저 연동, 생명력 HUD(❤️ ❤️) 갱신, 2회 추락 시 탈락 처리 |
| [CompanionView.cs](file:///k:/unityprojects/shoot1/shoot1/Assets/src/HappyShoot.View/Companion/CompanionView.cs) | ~480줄 | 횡스크롤 독립 보행 AI, 적 타겟 추적, 징검다리 도약 추종, 보빙 애니메이션 |

---

## 4. 검증 계획
1. **컴파일 검증**: 오류 0건 및 전 파일 500줄 이하 엄수 확인.
2. **징검다리 점프 검증**: 발판 위에서는 안전하게 걷고, 틈새 구역으로 이동 시 심연으로 떨어지는지 확인.
3. **2회 목숨 룰 검증**:
   - 1회 떨어지면 HUD 하트가 1개 깎이며 직전 발판으로 부활하는지 확인.
   - 2회 떨어지면 탈락 안내와 함께 원래 세계로 정상 귀환하는지 확인.
4. **동료 독립 보행 검증**: 마법사가 움직여도 동료가 묶여 끌려오지 않고 제자리에서 몬스터를 향해 걸어가며 싸우는지, 발판 사이를 도약하는지 확인.
5. **적 대량 스폰 & 폭발 수정 검증**: 오른쪽 화면에서 적 수십 마리가 쏟아져 나오고, 폭발 수정을 치면 시원하게 연쇄 폭발하는지 확인.
