# 🔑 GitHub Actions Unity 자동 빌드 설정 가이드

형님! 깃허브에서 Unity 프로젝트를 자동으로 빌드하여 `.exe` 파일을 생성하려면 **Unity 라이선스 인증(1회성)**이 필요합니다.

아래 두 가지 방법 중 **가장 편하신 방법 하나**를 선택하여 깃허브 리포지토리에 Secret을 등록해주시면 됩니다!

---

## 📌 방법 A: Unity 계정 정보로 등록 (가장 간편한 방법! ⚡)

Unity Plus/Pro 또는 정식 Serial 키가 있으신 경우 (또는 개인용 무료 라이선스):

1. 내 깃허브 리포지토리 페이지로 이동합니다.
2. **Settings** 탭 → 좌측 메뉴 **Secrets and variables** → **Actions**를 클릭합니다.
3. **New repository secret** 버튼을 눌러 아래 4개의 시크릿을 등록합니다:

| Secret 이름 | 내용 |
| :--- | :--- |
| `UNITY_EMAIL` | 유니티 계정 이메일 주소 |
| `UNITY_PASSWORD` | 유니티 계정 비밀번호 |
| `UNITY_SERIAL` | 유니티 개인용/프로 시리얼 번호 (무료 Personal인 경우 비워두거나 방법 B 사용) |

---

## 📌 방법 B: 무료 Personal 라이선스 `.ulf` 파일 등록 (추천 ⭐)

무료 Unity Personal 사용 시 가장 안정적인 방법입니다:

### 1단계: 활성화 요청 파일(.alf) 생성
리포지토리의 **Actions** 탭에서 Game-CI의 활성화 워크플로우를 실행하거나, 로컬에서 `.alf` 파일을 준비합니다.

### 2단계: 유니티 공식 라이선스 사이트에서 활성화
1. 브라우저에서 [https://license.unity3d.com/manual](https://license.unity3d.com/manual) 로 이동하여 로그인합니다.
2. `.alf` 파일을 업로드하고 **Unity Personal Edition**을 선택합니다.
3. 활성화된 **`.ulf` 파일(라이선스 파일)**을 다운로드합니다.

### 3단계: 깃허브 Secret에 등록
1. 다운로드받은 `.ulf` 파일 내용을 메모장으로 열어 전체 복사합니다.
2. 깃허브 리포지토리 **Settings** → **Secrets and variables** → **Actions** → **New repository secret** 클릭.
3. 이름: `UNITY_LICENSE`
4. 값: 복사한 `.ulf` 텍스트 내용 전체 붙여넣기 후 저장!

---

## 🎮 빌드 실행 및 `.exe` 다운로드 방법

1. 깃허브 리포지토리의 **Actions** 탭으로 이동합니다.
2. 좌측 워크플로우 목록에서 **🚀 Build HappyShoot (Windows Standalone)**를 클릭합니다.
3. 우측 상단의 **[Run workflow]** 버튼을 누르고 브랜치를 `main`으로 선택한 뒤 실행합니다!
4. 빌드가 완료(약 5~10분 소요)되면, 해당 실행 내역 상세 페이지 하단의 **Artifacts** 섹션에서 **`HappyShoot-Windows-x64`** Zip 파일을 클릭하여 바로 다운로드할 수 있습니다!
5. 압축을 풀면 바로 플레이 가능한 **`HappyShoot.exe`** 파일이 들어있습니다! 🎉

---

## 🏷️ 버전 태그로 깃 릴리즈 자동 생성 (옵션)
```bash
git tag v0.3.5
git push origin v0.3.5
```
위와 같이 버전 태그를 푸시하면, 깃허브 **Releases** 페이지에 자동으로 `v0.3.5` 릴리즈가 생성되고 `HappyShoot-Windows-x64.zip` 파일이 자동으로 첨부됩니다! 🚀
