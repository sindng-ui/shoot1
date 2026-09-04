# 🗺️ HappyShoot Application Map (APP_MAP)

> **프로젝트 개요**: Pure C# Domain Layer와 Unity Presentation Layer가 완벽히 분리된 고성능 2D 탑다운 Survivors-like 액션 슈팅 게임

---

## 🏛️ 아키텍처 구조 (Clean Architecture)

```mermaid
graph TD
    subgraph Unity Presentation [HappyShoot.View]
        GB[GameBootstrap] --> BGM[BackgroundManager & 3x3 Infinite Grid]
        GB --> HUD[InGameHudView]
        HUD --> PDV[PlayerDamageVignetteView]
        GB --> BB[BossHealthBarView]
        GB --> TCM[TreasureChestManagerView]
        GB --> TCP[TreasureChestPopupView]
        GB --> OBV[OrbitingBladeView]
        GB --> MSU[MetaShopUiView]
        GB --> PHB[PlayerHealthBarView]
        GB --> PM[PauseMenuUiView]
        GB --> GOR[GameOverResultUiView]
        GB --> SVU[StageVictoryUiView]
        GB --> CUP[CompanionUnlockPopupView]
        SVU --> CUP
        GB --> SV[SoundManagerView & 32-Ch Pool]
        GB --> PAH[ProceduralAudioHelper]
        GB --> PV[PlayerView]
        PV --> PHF[PlayerHitFeedbackView]
        PV --> PDC[PlayerDashController & GhostTrail]
        PV --> DCI[PlayerDashChargeIndicatorView]
        HUD --> MDTZ[MobileDashTouchZoneView]
        GB --> MV[MonsterSpawnerView]
        MV --> SVU
        GB --> PJV[ProjectileManagerView]
        GB --> MSMV[MagicSkillManagerView]
        GB --> MSV[MeteorStrikeManagerView]
        GB --> GV[GemManagerView]
        GB --> DTV[DamageTextManagerView]
        GB --> LV[LevelUpUiView]
        GB --> WV[WaveTimelineView]
        GB --> EV[EvolutionPopupView]
        GB --> STU[SkillTuningUiView]
        GB --> DSU[DevSkillSelectorUiView]
        GB --> CSU[CharacterSelectUiView]
        CSU --> SSS[StartSkillSelectorView]
        CSU --> CSP[CompanionSelectPreviewHelper]
        CSU --> MFV[MagicForgeUiView (Modal)]
        MFV --> RTV[RuneInscriptionTabView]
        GB --> CM[CompanionManagerView]
        CM --> CV[CompanionView (Warrior & Ranger)]
        GB --> STV[SkillTreeUiView (360° Arcane Dial)]
        GB --> IGC[InGameGemCounterHudView (Gold & 3-Gems)]
        GB --> TJV[TouchJoystickView (Mobile Floating)]
    end

    subgraph Event & Decoupling
        EB[EventBus]
    end

    subgraph Pure C# Domain [HappyShoot.Domain]
        GSE[GameSessionEntity]
        PE[PlayerEntity & Passives]
        CE[CompanionEntity (1/3 Scaling)]
        ME[MonsterEntity & Status Effects]
        MS[MonsterSpawner]
        TCE[TreasureChestEntity]
        TCMgr[TreasureChestManager]
        MSM[MetaShopManager]
        STMgr[SkillTreeManager]
        RNM[RuneManager & 12 RuneDefs]
        RNMOD[RuneModifiers (Zero-Alloc Struct)]
        FSD[ForgeSaveData]
        SG[SpatialGrid2D]
        CS[CompositeSkill]
        LS[LevelSystem]
        SRM[SkillRewardManager]
        SEM[SkillEvolutionManager]
        WTM[WaveTimelineManager]
        SCR[SkillConfigRepository]
    end

    Unity Presentation -->|Observe Events / Bind| Pure C# Domain
    Pure C# Domain -->|Publish Events| EB
    EB -->|Notify| Unity Presentation
```

---

## 📂 대분류 서브 문서 맵 (Sub Maps)

`APP_MAP.md`의 세부 파일 및 컴포넌트 명세는 아래 3대 핵심 서브 문서로 모듈화되어 관리됩니다. 세부 항목 조회를 원하실 경우 각 링크를 클릭해 주십시오.

| 레이어 | 서브 문서 링크 | 주요 관리 내용 |
| :--- | :--- | :--- |
| **🌐 Domain Layer** | [**`docs/app_map/DOMAIN_MAP.md`**](file:///k:/unityprojects/shoot1/shoot1/docs/app_map/DOMAIN_MAP.md) | • **엔티티 & 스탯**: `PlayerEntity`, `CompanionEntity`, `MonsterEntity`, `GemStoneEntity`<br>• **도메인 이벤트**: `EventBus`, 사운드/마법/피격/레벨/세션 이벤트<br>• **스킬 & 진화**: 3영웅 스킬, 9대 진화 레시피, 9종 패시브, `CompositeSkill`<br>• **마법 대장간**: 12종 룬 정의, 제로할당 수정자(`RuneModifiers`), 영구 저장<br>• **성장 & 스킬트리**: 360° 비전 성좌, 3종 보석 지갑, 골드 환불 시스템 |
| **🎮 Presentation Layer** | [**`docs/app_map/VIEW_MAP.md`**](file:///k:/unityprojects/shoot1/shoot1/docs/app_map/VIEW_MAP.md) | • **부트스트랩**: `GameBootstrap` 단일 라이프사이클 총괄<br>• **플레이어 & 3인 원정대**: 마법사 지팡이 결합, AI 동료 호위, 피격 쥬스(Juice)<br>• **스킬 & 투사체 VFX**: 화염구/서리폭발/연쇄번개/메테오/지각변동 0-할당 풀링<br>• **몬스터 & 3대 보스**: 7종 일반몹, 언데드 군단, 사령왕 리치 3대 패턴<br>• **UI/HUD & 대장간**: 메인 HUD, 360° 성좌 UI, 대장간 모달, 레벨업 3지선다<br>• **전투 샌드박스**: 6대 카테고리 튜닝, 슬라이더 팩토리, Git 영구 동기화<br>• **오디오 & 배경 & 모바일**: 3x3 무한 지형 타일링, 32채널 SFX, 가상 조이스틱 |
| **🧪 Tests & Pipeline** | [**`docs/app_map/TESTS_AND_TOOLS_MAP.md`**](file:///k:/unityprojects/shoot1/shoot1/docs/app_map/TESTS_AND_TOOLS_MAP.md) | • **단위 테스트**: 140+ NUnit 테스트 스위트 (100% ALL PASS)<br>• **에디터 자동화**: `BuildScript.cs` (Windows / Android APK / AAB 원클릭 빌드)<br>• **CI/CD 자동화**: GitHub Actions 원격 빌드, 릴리스 배포, Unity 라이선스 활성화 |

---

## 📌 문서 유지보수 규칙

> **개발자 작업 규칙**:
> 1. 신규 기능, UI 컴포넌트, 도메인 로직이 추가되거나 변경될 경우 해당 레이어의 서브 문서([`DOMAIN_MAP.md`](file:///k:/unityprojects/shoot1/shoot1/docs/app_map/DOMAIN_MAP.md), [`VIEW_MAP.md`](file:///k:/unityprojects/shoot1/shoot1/docs/app_map/VIEW_MAP.md), [`TESTS_AND_TOOLS_MAP.md`](file:///k:/unityprojects/shoot1/shoot1/docs/app_map/TESTS_AND_TOOLS_MAP.md))에 즉시 반영하고 사용자에게 보고합니다.
> 2. 최상위 아키텍처나 주요 레이어 간 흐름에 변경이 있을 경우 본 `APP_MAP.md`의 Mermaid 다이어그램 및 링크 인덱스를 갱신합니다.
