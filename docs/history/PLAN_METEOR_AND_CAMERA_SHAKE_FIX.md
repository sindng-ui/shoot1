# [구현 계획] 카메라 셰이크 샌드박스 연동 버그 수정 & 메테오 스트라이크 시각효과 개선 및 범위 확장

## 🎯 목표
1. **카메라 셰이크 버그 수정**:
   - Lerp 보간에 의한 진동 뭉개짐 해결 (최종 위치에 직접 셰이크 오프셋 적용)
   - 22종 전체 스킬 ID에 대한 `IsSkillShakeEnabled` 분기 매핑 완벽화
   - 각 스킬별 정확한 `skillId`로 `TriggerShake` 호출 연동
2. **메테오 스트라이크 시각 효과 개선 & 붉은 화면 제거**:
   - 눈 아픈 붉은색 데칼 및 블래스트 제거
   - 세련된 룬 마법진, 운석 화염 꼬리, 착탄 충격파 링, 마그마 파편 비산 연출 구현
3. **메테오 스트라이크 범위 확장**:
   - 폭발 반경 6.0m -> 7.5m 로 확장

## 🛠️ 변경 파일 목록
1. `Assets/src/HappyShoot.View/Camera/CameraFollowView.cs`
2. `Assets/src/HappyShoot.View/Player/PlayerView.cs`
3. `Assets/src/HappyShoot.View/Projectiles/FireballSkillManagerView.cs`
4. `Assets/src/HappyShoot.View/Projectiles/MagicSkillManagerView.cs`
5. `Assets/src/HappyShoot.View/Projectiles/MeteorStrikeManagerView.cs`
6. `Assets/src/HappyShoot.Domain/Skills/Effects/MeteorStrikeEffect.cs`
7. `Assets/src/HappyShoot.Domain/Skills/SkillConfigModels.cs`
8. `APP_MAP.md`
