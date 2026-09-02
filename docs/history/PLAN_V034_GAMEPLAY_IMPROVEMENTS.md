# [종합 구현 계획] 카메라 셰이크 수정, 메테오 개편, 숫자키 1/2/3 보상 선택 & 보스 상자 가독성 강화

## 🎯 목표
1. **카메라 셰이크 버그 수정**: Lerp 이후 직접 진동 오프셋 가산, 22종 전체 스킬 ID 매핑, 개별 스킬 TriggerShake 연동
2. **메테오 스트라이크 개편**: 눈 아픈 붉은 화면 제거, 범위 6.0m -> 7.5m 확장, 운석 화염 꼬리 + 충격파 링 + 마그마 파편 비산 연출
3. **레벨업 3지선다 숫자키 1/2/3 선택**: 키보드 1, 2, 3 및 키패드 1, 2, 3 누르면 즉시 선택
4. **보스 상자 보상 설명 가독성 강화**: 다이얼로그 640x580 확장, 80x80 아이콘 + 골드 타이틀 + 대형 설명 카드 패널 렌더링, 스페이스/엔터 수령 지원

## 🛠️ 변경 파일 목록
1. `Assets/src/HappyShoot.View/Camera/CameraFollowView.cs`
2. `Assets/src/HappyShoot.View/Player/PlayerView.cs`
3. `Assets/src/HappyShoot.View/Projectiles/FireballSkillManagerView.cs`
4. `Assets/src/HappyShoot.View/Projectiles/MagicSkillManagerView.cs`
5. `Assets/src/HappyShoot.View/Projectiles/MeteorStrikeManagerView.cs`
6. `Assets/src/HappyShoot.Domain/Skills/Effects/MeteorStrikeEffect.cs`
7. `Assets/src/HappyShoot.Domain/Skills/SkillConfigModels.cs`
8. `Assets/src/HappyShoot.View/UI/LevelUpUiView.cs`
9. `Assets/src/HappyShoot.View/Chests/TreasureChestPopupView.cs`
10. `APP_MAP.md`
