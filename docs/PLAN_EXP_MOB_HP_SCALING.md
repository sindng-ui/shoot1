# [구현 계획] 샌드박스 몹 체력 스케일링 & 역경직(Hit-Stop) 조절 기능 추가

## 🎯 목표
1. **경험치 대비 몹 체력 스케일링**:
   - 샌드박스 시스템(EXP) 탭에 '경험치 증가분 대비 몹 젠비율' 아래 '경험치 증가분 대비 몹 체력 (%: 0~100%)' 슬라이더 추가
   - 레벨업 시 필요 경험치 증가량에 비례하여 몬스터/보스 체력 스케일링 적용
2. **역경직(Hit-Stop) 조절 기능**:
   - 샌드박스에 역경직 시간(Hit-Stop Duration) 및 슬로우 강도(Slow Scale) 조절 슬라이더 추가
   - 크리티컬 타격 시 설정된 역경직 수치로 HitStopManager 연동
3. **파일 저장/복원 연동**:
   - "파일에 반영" 시 완벽 영구 저장 및 복원

## 🛠️ 변경 파일 목록
1. `Assets/src/HappyShoot.Domain/Skills/SkillConfigModels.cs`: `MobHpScalingRatio`, `HitStopDuration`, `HitStopSlowScale`, `EnableHitStop` 추가
2. `Assets/src/HappyShoot.View/UI/SkillTuningRowConfigurator.cs`: 몹 체력 및 역경직 슬라이더 행 추가 (500줄 이하 유지)
3. `Assets/src/HappyShoot.View/Monsters/MonsterSpawnerView.cs`: `GetExpGrowthHpScale()` 추가 및 스폰 시 체력 배율 적용
4. `Assets/src/HappyShoot.View/Effects/CriticalHitVfxManagerView.cs`: 크리티컬 히트 시 HitStopManager 연동
5. `Assets/tests/HappyShoot.Domain.Tests/Leveling/LevelSystemTests.cs`: 체력 스케일링 테스트 추가
6. `APP_MAP.md`: 변경점 업데이트
