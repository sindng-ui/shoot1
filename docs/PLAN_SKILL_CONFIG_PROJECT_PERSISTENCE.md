# 🛠️ 샌드박스 튜닝 설정 파일 프로젝트 내부화 및 Git 연동 계획서

형님, 지금까지 샌드박스(전투/스킬/몬스터/경험치/크리티컬 등)에서 실시간으로 튜닝하고 저장한 설정 파일이 OS 임시 데이터 폴더(`Application.persistentDataPath` - AppData)에 저장되고 있어서, GitHub에 커밋되지 않고 다른 PC에서 작업할 때 설정이 이어지지 않던 문제를 깔끔하게 해결하겠습니다!

---

## 🎯 목표
1. **기존 튜닝 데이터 프로젝트 내부 이전**: 현재 PC의 AppData에 저장되어 있는 최신 샌드박스 튜닝 JSON 데이터를 프로젝트 내부 디렉터리(`Assets/Config/skill_configs.json`)로 가져와 Git 추적 대상에 포함합니다.
2. **저장 및 로드 경로 리팩토링**: `SkillConfigRepository.cs`가 1순위로 `Assets/Config/skill_configs.json` (프로젝트 내부)을 읽고 쓰도록 수정합니다.
3. **크로스 플랫폼/멀티 PC 호환성 보장**: 다른 PC에서 `git pull`을 받았을 때도 별도의 설정 없이 프로젝트 내부의 최신 튜닝 수치가 즉시 로드되고, 인게임 샌드박스에서 [Save Config] 버튼을 누르면 프로젝트 내부 파일이 즉각 갱신되도록 구성합니다.

---

## 🔍 변경 사항 상세

### 1. 📂 샌드박스 설정 파일 배치 (`Assets/Config/skill_configs.json`)
- 현재 AppData에 저장되어 있던 사용자의 튜닝 파일(Fireball, Chain Lightning, Blood Eater, Gigastorm, Monster 스탯, EXP 설정 등)을 그대로 가져와 `Assets/Config/skill_configs.json`에 배치합니다.
- Unity 에디터 및 프로젝트 어디서든 즉시 확인 가능하며 Git으로 버전 관리됩니다.

### 2. 💻 `SkillConfigRepository.cs` 로직 업데이트
- **경로 우선순위 (`GetConfigFilePath()`)**:
  1. `Path.Combine(Application.dataPath, "Config/skill_configs.json")` (Unity 에디터 및 프로젝트 실행 시 최우선)
  2. Fallback: Standalone 빌드 환경 고려하여 `Path.Combine(Directory.GetCurrentDirectory(), "Assets/Config/skill_configs.json")` 또는 `Path.Combine(Application.persistentDataPath, "skill_configs.json")`
- **로드(`Load()`)**:
  - `Assets/Config/skill_configs.json`이 존재하면 즉시 로드.
  - 없을 경우 `persistentDataPath` 또는 기본값 생성 후 프로젝트 경로에 저장.
- **저장(`Save()`)**:
  - 샌드박스 UI에서 Save 시 `Assets/Config/skill_configs.json`에 직접 기록.
  - 디렉터리 자동 생성 보장 (`Directory.CreateDirectory`).
- **코드 무결성**: 500줄 이하 유지 (현재 274줄 -> 약 280줄).

### 3. 🗺️ 문서 및 맵 업데이트
- `APP_MAP.md`에 `Assets/Config/skill_configs.json` 및 `SkillConfigRepository` 경로 변경 내용 반영.
- `docs/PLAN_SKILL_CONFIG_PROJECT_PERSISTENCE.md` 생성.

---

## 📂 Proposed Changes

### Configuration & Persistence

#### [NEW] [skill_configs.json](file:///c:/AntigravityWorkspace/shoot1/Assets/Config/skill_configs.json)
- 기존 AppData에서 추출한 형님의 실제 샌드박스 튜닝 데이터 JSON 파일.

#### [MODIFY] [SkillConfigRepository.cs](file:///c:/AntigravityWorkspace/shoot1/Assets/src/HappyShoot.View/Config/SkillConfigRepository.cs)
- `GetConfigFilePath()` 경로를 `Assets/Config/skill_configs.json` 우선으로 변경.
- 프로젝트 내 파일 저장 및 로드 로직 최적화.

#### [MODIFY] [APP_MAP.md](file:///c:/AntigravityWorkspace/shoot1/APP_MAP.md)
- 샌드박스 설정 파일의 프로젝트 내부화 및 Git 관리 구조 업데이트.

---

## 🧪 Verification Plan

### Automated Tests
- 유닛 테스트 실행: `dotnet test`로 Domain 및 View 로직 전체 통과 검증.

### Manual Verification
- `Assets/Config/skill_configs.json` 파일 생성 및 내용(스킬별 커스텀 수치) 무결성 확인.
- `SkillConfigRepository`에서 해당 파일을 올바르게 감지하고 읽는지 검증.
- `git status`를 통해 형님의 튜닝 설정 파일이 정상적으로 Git 추적 대상에 잡히는지 확인.
