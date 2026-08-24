# [구현 계획] 스킬별 카메라 셰이크 0~100% 세분화 & 다중 중첩 눈 피로 방지(Max Clamping) 시스템

형님께서 질문해주신 **"1) 스킬별 셰이크 0~100% 세분화"** 및 **"2) 여러 스킬 셰이크 중첩 시 눈 피로/어지러움 방지 아키텍처"**에 대한 세부 구현 계획서입니다.

---

## 🎯 다중 스킬 셰이크 중첩 해결 방안 (Eye Comfort & Juice)

여러 스킬(메테오, 지면강타, 화살비, 휠윈드 등)이 동시에 터질 때 진동을 무작정 가산(Add)하면 화면이 요동치며 심한 눈 피로와 멀미를 유발합니다. 이를 해결하기 위해 다음과 같은 **3중 댐핑 & 캡핑 아키텍처**를 적용합니다:

1. **Max Clamping (최대값 우선 병합)**:
   - 새로운 셰이크 요청 시 단순 덧셈이 아닌 `_shakeIntensity = Mathf.Max(_shakeIntensity, newIntensity)`를 적용하여 **동시에 여러 스킬이 터져도 가장 강한 스킬의 묵직한 진동 1개만 깔끔하게 유지**.
2. **Global Shake Hard Ceiling (절대 상한선 캡핑)**:
   - 어떤 극한의 상황에서도 화면 흔들림 반경이 `0.38m`를 초과하지 못하도록 하드 캡핑하여 화면 이탈/어지러움 원천 차단.
3. **Decay Priority (지속시간 부드러운 병합)**:
   - `_shakeTimer = Mathf.Max(_shakeTimer, newDuration)`으로 갱신하여 자잘하게 덜덜거리는 고주파 노이즈를 묵직하고 자연스러운 1회의 펀치감으로 압축.
4. **[시스템] 마스터 카메라 셰이크 슬라이더 (0~100%)**:
   - [시스템] 탭에서 전체 스킬 셰이크를 한 번에 비율로 줄이거나(예: 50%), 원클릭으로 0% 완전 무진동 모드로 변경 가능.

---

## 🛠️ 세부 변경 내용

### 1. 도메인 설정 모델 (`SkillConfigModels.cs`)
- 각 스킬별 `EnableCameraShake (bool)` $\rightarrow$ **`CameraShakeScale (float: 0 ~ 100%)`**로 업그레이드:
  - `Slash`, `GroundStomp`, `Whirlwind`, `Bow`, `Glaive`, `ArrowRain`, `Fireball`, `FrostNova`, `ChainLightning`, `Orbital` 및 9종 진화 궁극기 전체.
- `ExpConfig`([시스템] 탭):
  - **`public float MasterCameraShakeScale = 100f;`** (0~100%) 추가.

---

### 2. 카메라 진동 제어 (`CameraFollowView.cs`)
- `TriggerShake(skillId, baseDuration, baseIntensity)` 호출 시:
  ```csharp
  float skillScale = GetSkillShakeScale(skillId); // 0~100%
  float masterScale = GetMasterShakeScale();       // 0~100%
  float finalIntensity = baseIntensity * (skillScale / 100f) * (masterScale / 100f);

  if (finalIntensity <= 0.001f) return;

  // Max Clamping: 중첩 시 덧셈이 아닌 최대 강도 선택 + 하드 캡(0.38f)
  _shakeIntensity = Mathf.Min(0.38f, Mathf.Max(_shakeIntensity, finalIntensity));
  _shakeTimer = Mathf.Max(_shakeTimer, baseDuration);
  ```

---

### 3. 샌드박스 UI (`SkillTuningRowConfigurator.cs`)
- 기존 0/1 토글 대신 **`📳 카메라 셰이크 강도 (%: 0~100%)`** 정밀 슬라이더 배치.
- [시스템] 탭에 **`📳 마스터 카메라 셰이크 강도 (%: 0~100%)`** 슬라이더 추가.
- 500줄 규칙 준수를 위해 슬라이더 생성 로직을 최적화하여 450줄 수준으로 슬림하게 유지.

---

## 🧪 검증 계획

### Automated Tests
- 도메인 단위 테스트 124개 전체 실행 및 통과 검증 (`TestHost.exe`)

### Manual Verification
1. **개별 스킬 셰이크 0~100% 조절**:
   - 샌드박스에서 대검 베기 셰이크를 20%, 메테오를 100%로 설정 시 각각 설정한 세기만큼 자연스럽게 흔들리는지 확인.
   - 0% 설정 시 해당 스킬은 전혀 흔들리지 않는지 확인.
2. **다중 스킬 중첩 테스트**:
   - 메테오 + 지면강타 + 휠윈드가 동시에 터져도 화면이 찢어지듯 떨리지 않고 최대 강도로 묵직하고 안정감 있게 1회 연출되는지 확인.
3. **[시스템] 마스터 셰이크 조절**:
   - 마스터 셰이크를 50%로 내리면 모든 스킬의 진동이 절반으로 줄어들고, 0%로 하면 전체 스킬 진동이 꺼지는지 확인.
