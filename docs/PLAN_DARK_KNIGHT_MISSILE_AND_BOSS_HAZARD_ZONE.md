# [구현 계획서] 흑기사(DarkKnight) 원거리 투사체 공격 & 보스 대형 광역 장판 공격 시스템

## 1. 목적
1. 1페이즈 이후 후반부(2페이즈)에 원거리 공격이 없어 단조로워지는 현상을 보완하기 위해 **흑기사(DarkKnight)에게 보라색 암흑 마법 검기(Dark Slash Wave)** 원거리 투사체 공격을 추가합니다.
2. 보스(Boss 1 Goblin King & Boss 2 Dragon Fiend) 전투에 레이저 외에 **적당히 큰(직경 ~5.6m) 전조형 지옥불/마그마 광역 장판 공격(Hazard AoE Zone)** 을 도입하여 긴장감 넘치는 회피 플레이를 유도합니다.

## 2. 세부 설계
- `MonsterType.DarkKnight`: `isRanged: true, preferredDistance: 4.8f, attackInterval: 2.5f`
- `EnemyProjectileManagerView`: 흑기사 전용 암흑 검기 풀링 및 렌더링 (`SpawnDarkSlashProjectile`)
- `BossHazardZoneManagerView`: 보스 광역 장판 0-할당 풀링 매니저 (1.2s 전조 링 $\rightarrow$ 2.0s 마그마 폭발 지속 피해)
- `MonsterSpawnerView`: 흑기사 발사체 및 보스 장판 라이프사이클 연동
- `MonsterTuningConfig` & 샌드박스 UI: 흑기사/보스 신규 파라미터 슬라이더 추가
