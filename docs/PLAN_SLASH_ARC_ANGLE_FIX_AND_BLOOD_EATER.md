# 대검 베기 부채꼴 각도(ArcAngle) 버그 수정 및 블러드 이터 각도 옵션 추가 계획서

## 1. 개요 및 문제 분석
샌드박스에서 대검 베기(Greatsword Slash)의 부채꼴 각도를 180도 초과(예: 200도~360도)로 설정해도 게임 내 피격 판정 및 비주얼 스윙 각도에 반영되지 않는 문제가 발생했습니다. 또한, 진화 스킬인 **블러드 이터(Blood Eater)**에도 동일한 부채꼴 각도 튜닝 옵션 추가가 필요합니다.

### 원인 분석
1. **도메인 피격 판정 조기 탈출 버그**:
   - `GreatswordSlashEffect.cs` 및 `BloodEaterEffect.cs`에서 `if (dot <= 0f) continue;` 코드가 존재하여, 각도가 180도(+-90도)를 초과하는 적은 `minDot` 판정 전에 무조건 타격 대상에서 제외됨.
2. **비주얼 스윙 각도 하드코딩 (150도 고정)**:
   - `PlayerView.cs`에서 스윙 애니메이션 및 초기 각도가 `-75f ~ +75f` (150도)로 하드코딩되어 있어, 도메인 이벤트로 전달된 `evt.ArcAngleDegrees`가 반영되지 않고 항상 150도로만 칼을 휘두름.
3. **블러드 이터(Blood Eater) 설정 및 샌드박스 옵션 부재**:
   - `BloodEaterConfig`에 `ArcAngle` 속성 누락.
   - `SkillTuningRowConfigurator.cs` 및 `SkillLiveApplier.cs`에 블러드 이터 각도 슬라이더 및 동기화 누락.

---

## 2. 해결 및 구현 계획

### 1) 도메인 피격 판정 로직 수정
- `GreatswordSlashEffect.cs` & `BloodEaterEffect.cs`:
  - `if (dot <= 0f) continue;` 제거.
  - `dot >= minDot` 단일 조건으로 판정 (`minDot = cos(ArcAngleDegrees * 0.5f)`).
  - 이를 통해 30도부터 360도 전방위까지 설정한 부채꼴 각도 내의 모든 적이 정확하게 피격 판정됨.

### 2) 비주얼 스윙 각도 동적 연동 (`PlayerView.cs`)
- `_slashHalfArc` 필드 도입:
  - `OnPlayerSlashExecuted` 및 `OnBloodEaterExecuted` 수신 시 `_slashHalfArc = evt.ArcAngleDegrees * 0.5f;` 저장.
  - 스윙 진행도에 따른 칼 및 검기 회전 각도를 `-_slashHalfArc` ~ `+_slashHalfArc`로 부드럽게 스윙.
  - 초기 스윙 시작 각도도 `_slashBaseAngle - _slashHalfArc`로 정확히 정렬.

### 3) 블러드 이터 부채꼴 각도 옵션 전면 지원
- `SkillConfigModels.cs`: `BloodEaterConfig`에 `public float ArcAngle = 150f;` 추가.
- `SkillRegistryHelper.cs`: 블러드 이터 인스턴스 생성 시 `cfg.BloodEater.ArcAngle` 전달.
- `SkillLiveApplier.cs`: `BloodEaterEffect.ArcAngleDegrees` <-> `config.BloodEater.ArcAngle` 실시간 라이브 동기화 연동.
- `SkillTuningRowConfigurator.cs`: `blood_eater` 튜닝 탭에 `📐 부채꼴 각도 (ArcAngle: 30°~360°)` 슬라이더 추가.

### 4) 단위 테스트 및 검증
- `GreatswordSlashTests.cs` 및 `WarriorSkillsTests.cs`에 180도 초과(220도, 360도) 각도 피격 단위 테스트 추가/검증.
- 샌드박스에서 대검 베기 및 블러드 이터의 각도를 240도 등으로 조절 시 칼 스윙 궤적과 피격 범위가 정확히 동작하는지 검증.
