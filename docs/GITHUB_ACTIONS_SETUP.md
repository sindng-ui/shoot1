# 🔑 GitHub Actions Unity 자동 빌드 설정 가이드

형님! 깃허브 액션(Linux 러너)에서 유니티 빌드를 실행하려면 **Linux 러너용 1회성 라이선스 활성화(`.ulf`)**가 필요합니다.

---

## 📌 1회성 라이선스 발급 및 등록 순서 (3단계)

### 1단계: 깃허브에서 `.alf` 파일 추출
1. 깃허브 리포지토리의 **[Actions]** 탭으로 이동합니다.
2. 좌측 목록에서 **`🔑 Acquire Unity Activation File (.alf)`** 워크플로우를 클릭합니다.
3. 우측의 **[Run workflow]** 버튼을 누르고 실행합니다.
4. 약 15초 후 작업이 완료되면, 실행 결과 페이지 하단의 **Artifacts**에서 **`manual-activation-file`**을 다운로드하여 압축을 풉니다 (예: `Unity_v6000.3.22f1.alf`).

---

### 2단계: 유니티 공식 사이트에서 무료 활성화 (.ulf 받기)
1. 웹 브라우저에서 [https://license.unity3d.com/manual](https://license.unity3d.com/manual) 로 접속하여 로그인합니다.
2. 1단계에서 얻은 **`.alf` 파일**을 업로드하고 **[Next]**를 누릅니다.
3. **"Unity Personal Edition"** (개인/무료)을 선택하고 **[Next]**를 누릅니다.
4. 생성된 **`.ulf` 파일 (라이선스 파일)**을 다운로드합니다.

---

### 3단계: 깃허브 Secret에 등록
1. 다운로드받은 **`.ulf` 파일**을 **메모장**으로 열어서 텍스트 전체를 복사합니다 (`Ctrl + A` → `Ctrl + C`).
2. 깃허브 리포지토리 **Settings** → **Secrets and variables** → **Actions** 로 이동합니다.
3. **[New repository secret]** 버튼을 누릅니다:
   - **Name**: `UNITY_LICENSE`
   - **Secret**: 복사한 텍스트 전체 붙여넣기 (`Ctrl + V`)
4. **[Add secret]** 버튼을 누르면 끝!

---

## 🎮 빌드 실행 및 `HappyShoot.exe` 받기
- 이제 **[Actions]** 탭 → **`🚀 Build HappyShoot (Windows Standalone)`** → **[Run workflow]**를 누르시면 정상적으로 빌드되어 **`HappyShoot-Windows-x64.zip` (`.exe` 포함)** 아티팩트가 생성됩니다! 🚀
