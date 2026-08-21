# 🔮 비전 폭발(Arcane Explosion) 시각 효과 연동 구현 계획서

1. `ArcaneExplosionExecutedEvent` 이벤트 추가
2. `ArcaneExplosionEffect.cs` 이벤트 발행 및 ILevelableEffect 레벨업 상향
3. `SpriteHelper.cs` 보라색 마법 폭발 충격파 스프라이트 추가
4. `ArcaneExplosionManagerView.cs` [NEW] 16-채널 무할당 보라색 충격파 폭발링 애니메이션 구현 및 GameBootstrap 연동
