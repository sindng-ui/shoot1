# 🏹 스켈레톤 원거리 뼈다귀 투사체 구현 & 🪓 오비탈 블레이드 뷰 연동 계획서

## 🔍 원인 정밀 분석

### 1. 3번째 몹 (스켈레톤)의 보이지 않는 공격 및 원거리 피격 버그
- **원인**: `MonsterSpawner.cs`에서 스켈레톤의 쿨타임(2초)이 찰 때마다 아무런 투사체 생성 없이 `player.TakeDamage()`를 호출하여 **즉시 히트스캔으로 플레이어 체력을 깎고 있었음**.
- **해결 방안**:
  - 스켈레톤의 원거리 공격을 **시각적 뼈다귀 투사체(Enemy Projectile)** 시스템으로 전환.
  - 플레이어가 날아오는 뼈다귀 투사체를 눈으로 보고 무빙으로 피할 수 있도록 개선.
  - `SpriteHelper.cs`에 24x10 크기의 뼈다귀 화살 스프라이트(`GetOrCreateBoneSprite`) 추가.

### 2. 오비탈 블레이드(Orbiting Blades) 보상 습득 시 미표시 버그
- **원인**: `OrbitingBladeView.cs` 컴포넌트가 정의되어 있으나, `PlayerView`에 부착되거나 도메인 스킬 습득(`orbital`) 시 활성화해 주는 뷰 바인딩 코드가 누락되어 있었음.
- **해결 방안**:
  - `PlayerView.cs`에 `OrbitingBladeView`를 자식 오브젝트로 내장.
  - 플레이어가 `orbital` 스킬을 획득하면 `OrbitingBladeView`가 즉시 켜지며, 플레이어 주위를 회전하는 2~4자루의 검이 선명하게 나타나도록 연동.
