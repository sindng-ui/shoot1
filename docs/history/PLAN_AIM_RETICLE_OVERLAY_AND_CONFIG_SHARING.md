# [구현 계획서] 샌드박스 메뉴 조준선(Aim) 최상위 렌더링 & Git 샌드박스 설정 공유 보장

## 1. 목적
1. 샌드박스 메뉴(UI)보다 조준선(Aim Reticle)이 아래에 깔려 가려지는 현상을 해결하여, 어떤 UI 위에서도 마우스 조준선이 최상단에 선명하게 보이도록 개선합니다.
2. 샌드박스 튜닝 설정 파일(`skill_configs.json`)이 GitHub에 정상 푸시되고 다른 PC에서 `git pull` 시 100% 동일하게 공유/적용되도록 Unity `Resources` 로드 체계 및 다중 폴더 저장/동기화 시스템을 구축합니다.

## 2. 세부 작업 내역
1. `Assets/src/HappyShoot.View/Cameras/AimReticleView.cs`:
   - 월드 SpriteRenderer 대신 최상위 ScreenSpaceOverlay Canvas (`sortingOrder = 32760`) + `Image` UI 구조로 전환.
   - `raycastTarget = false` 적용으로 샌드박스 조작 완벽 지원.
2. `Assets/Resources/Config/skill_configs.json`:
   - Unity 공식 번들 리소스 경로에 설정 파일 배치 및 Git 버전 관리.
3. `Assets/src/HappyShoot.View/Config/SkillConfigRepository.cs`:
   - `Load()` 시 Git 원본 파일 및 `Resources.Load<TextAsset>` 최우선 로드.
   - `Save()` 시 `Resources/Config` 및 `Config` 양쪽 동시 저장 + `AssetDatabase.Refresh()`.
4. 회귀 테스트 및 APP_MAP.md 업데이트.
