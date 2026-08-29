# 🏆 최종 보스(Boss 3) & 3페이즈 몬스터 4종 & '승리자 전용 영구 성장' 시스템 구현 결과

## 1. 개요
형님의 지시에 따라 **"최종 보스 3(사령왕 리치)"**, **"3페이즈 신규 몬스터 4종의 스피디한 15초 단위 순차 전개"**, 그리고 **"오직 3보스 격파 승리자에게만 영구 성장(스킬 트리/상점) 개방"**이라는 핵심 룰 개편을 완벽하게 구현하였습니다.

---

## 2. ⏱️ 3페이즈 4종 몬스터 스피디 전개 (총 60초)
보스 2(Dragon Fiend) 격파 즉시 15초 간격으로 신규 몬스터들이 전장에 긴박감을 불어넣습니다:
1. **0초 (즉시) - 망령 (Wraith)**: 푸른 안광의 반투명 유령. 3.6m/s의 지그재그 고속 돌격으로 틈을 파고듦.
2. **15초 - 사령술사 (Necromancer)**: 흑마법 로브 & 해골 지팡이. 플레이어를 조준하여 유도성 보라빛 저주 영혼탄(Soul Orb) 발사.
3. **30초 - 어보미네이션 (Abomination)**: 썩어가는 녹색/자주색 누더기 거구. 1,500 HP의 압도적인 체력과 넉백 저항.
4. **45초 - 사신 (Death Reaper)**: 은빛 대낫을 든 흑단 사신. 3.8m/s 고속 쇄도 및 60 대미지의 치명적인 일격.
5. **60초 (단 1분!)**: 🔥 **최종 보스 3 (사령왕 리치: Arch-Lich Malakar)** 강림!

---

## 3. 💀 최종 보스 3: 사령왕 리치 (Arch-Lich Malakar) 피날레
- **스펙**: 기본 체력 20,000 HP × 성장 배율, 이동속도 2.4m/s, 접촉 대미지 65.
- **공격 패턴**: 회전 레이저 빔 + 플레이어 발밑 사령 해저드 장판 + 사령술사 저주 영혼탄 복합 공격.
- **격파 피날레**:
  - 필드의 모든 잔몹 즉시 소멸 (`DespawnAll`)
  - [🏆 STAGE CLEAR - VICTORY!] 찬란한 승리 팝업 출현!

---

## 4. 🔒 '오직 3보스 격파 시에만 영구 성장' 룰 개편
- **캐릭터 선택 화면 (시작 전)**:
  - 스킬 트리 버튼 잠금 (`[🔒 3보스 클리어 시 영구성장 개방]`) 처리.
- **사망 (Game Over - 패배)**:
  - 상점 및 스킬트리 진입 버튼 완전 제거!
  - 획득한 골드 및 보석은 영구 저장소에 저장되지 않음 (유실).
  - 안내 문구: `"🔒 영구 성장은 오직 3보스 클리어 시에만 개방됩니다!"`
  - 오직 `[🔄 다시 도전하기]` 버튼만 제공.
- **승리 (VICTORY - 3보스 격파)**:
  - `StageVictoryUiView` 승리 팝업에서만 모든 골드와 보석(루비/에메랄드/자수정)을 영구 저장소에 정산!
  - 황금빛 **[💎 승리자의 특전: 영구 성장 & 스킬 트리 개방]** 버튼을 통해 자유롭게 스킬 트리와 메타 상점을 업그레이드 가능!

---

## 5. 🛡️ 500줄 규칙 준수 모듈화 현황
모든 소스 코드가 500줄 한도를 완벽히 지키고 있습니다:
- [Phase3MonsterSpriteHelper.cs](file:///k:/unityprojects/shoot1/shoot1/Assets/src/HappyShoot.View/Utils/Phase3MonsterSpriteHelper.cs): 314줄
- [StageVictoryUiView.cs](file:///k:/unityprojects/shoot1/shoot1/Assets/src/HappyShoot.View/UI/StageVictoryUiView.cs): 222줄
- [WavePhaseController.cs](file:///k:/unityprojects/shoot1/shoot1/Assets/src/HappyShoot.View/Monsters/WavePhaseController.cs): 225줄
- [MonsterSpawnerView.cs](file:///k:/unityprojects/shoot1/shoot1/Assets/src/HappyShoot.View/Monsters/MonsterSpawnerView.cs): 442줄
- [GameOverResultUiView.cs](file:///k:/unityprojects/shoot1/shoot1/Assets/src/HappyShoot.View/UI/GameOverResultUiView.cs): 292줄
- [CharacterSelectUiView.cs](file:///k:/unityprojects/shoot1/shoot1/Assets/src/HappyShoot.View/UI/CharacterSelectUiView.cs): 415줄
- [EnemyProjectileManagerView.cs](file:///k:/unityprojects/shoot1/shoot1/Assets/src/HappyShoot.View/Projectiles/EnemyProjectileManagerView.cs): 154줄
- [MonsterType.cs](file:///k:/unityprojects/shoot1/shoot1/Assets/src/HappyShoot.Domain/Entities/MonsterType.cs): 155줄
- [MonsterSpawner.cs](file:///k:/unityprojects/shoot1/shoot1/Assets/src/HappyShoot.Domain/Entities/MonsterSpawner.cs): 201줄
- [GameBootstrap.cs](file:///k:/unityprojects/shoot1/shoot1/Assets/src/HappyShoot.View/Bootstrap/GameBootstrap.cs): 386줄
