# 🗺️ HappyShoot Application Map (APP_MAP)

> **프로젝트 개요**: Pure C# Domain Layer와 Unity Presentation Layer가 완벽히 분리된 고성능 2D 탑다운 Survivors-like 액션 슈팅 게임

---

## 🏛️ 아키텍처 구조 (Clean Architecture)

```mermaid
graph TD
    subgraph Unity Presentation [HappyShoot.View]
        GB[GameBootstrap] --> BGM[BackgroundManager & 3x3 Infinite Grid]
        GB --> HUD[InGameHudView]
        GB --> BB[BossHealthBarView]
        GB --> TCM[TreasureChestManagerView]
        GB --> TCP[TreasureChestPopupView]
        GB --> OBV[OrbitingBladeView]
        GB --> MSU[MetaShopUiView]
        GB --> PHB[PlayerHealthBarView]
        GB --> PM[PauseMenuUiView]
        GB --> GOR[GameOverResultUiView]
        GB --> SV[SoundManagerView & 16-Ch Pool]
        GB --> PAH[ProceduralAudioHelper]
        GB --> PV[PlayerView]
        GB --> MV[MonsterSpawnerView]
        GB --> PJV[ProjectileManagerView]
        GB --> MSMV[MagicSkillManagerView]
        GB --> MSV[MeteorStrikeManagerView]
        GB --> GV[GemManagerView]
        GB --> DTV[DamageTextManagerView]
        GB --> LV[LevelUpUiView]
        GB --> WV[WaveTimelineView]
        GB --> EV[EvolutionPopupView]
        GB --> STU[SkillTuningUiView]
        GB --> STB[SkillTuningUiBuilder]
        GB --> STR[SkillTuningRowConfigurator]
        GB --> STF[SkillTuningSliderFactory]
        GB --> SLA[SkillLiveApplier]
        GB --> STM[SkillTuningMemoryCache]
        GB --> DSU[DevSkillSelectorUiView]
    end

    subgraph Event & Decoupling
        EB[EventBus]
    end

    subgraph Pure C# Domain [HappyShoot.Domain]
        GSE[GameSessionEntity]
        PE[PlayerEntity & Passives]
        ME[MonsterEntity & Status Effects]
        MS[MonsterSpawner]
        TCE[TreasureChestEntity]
        TCMgr[TreasureChestManager]
        MSM[MetaShopManager]
        MSD[MetaUpgradeSaveData]
        MUA[MetaUpgradeApplier]
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

## 📂 모듈 및 파일 맵

### 1. 🌐 Pure C# Domain Layer (`Assets/src/HappyShoot.Domain`)
*Unity 엔진 의존성 0 (noEngineReferences: true), 독립적인 고속 시뮬레이션 및 테스트 가능*

| 카테고리 | 파일명 | 주요 클래스/인터페이스 | 설명 |
| :--- | :--- | :--- | :--- |
| **Config** | `.gitignore [UPDATED]` | - | Unity 표준 제외 규칙, Standalone 빌드 출력 폴더(`/Zxe/`, `/build/`), .NET 빌드 아티팩트(`**/bin/`, `**/obj/`), IDE/OS 캐시 파일 포괄적 무시 설정 |
| **Entities** | `PlayerEntity.cs [UPDATED]` | `PlayerEntity` | 플레이어 순수 C# 엔티티 (스탯, 패시브, **`AttackPowerMultiplier` 공격력 배율 `SkillContext.BaseDamage` 실시간 완벽 동기화**, 스킬 틱/실행 관리) |
| **Events** | `GemStoneEvents.cs [NEW]` | `GemStoneDroppedEvent`, `GemStoneCollectedEvent` | 보석(루비/에메랄드/아메시스트) 필드 드랍 및 자석 수집 알림 도메인 이벤트 |
| | `AudioEvents.cs` | `SoundEffectType`, `PlaySoundEvent`, `PlayBgmEvent`, `StopBgmEvent` | 14종 SFX 및 BGM 재생 요청 도메인 이벤트 집합 |
| | `MagicEvents.cs [UPDATED]` | `FrostNovaExecutedEvent`, `ChainLightningExecutedEvent`, `FireballExplodedEvent`, `MeteorStrikeExecutedEvent`, `MonsterShatteredEvent` | 마법사 서리 폭발, 연쇄 번개, 화염구 폭발, 메테오 낙하, 빙결 파괴 도메인 이벤트 집합 |
| | `BossEvents.cs` | `BossSpawnedEvent`, `BossHealthUpdatedEvent`, `BossDiedEvent` | 보스 스폰/체력 변경/사망 이벤트 집합 |
| | `ChestEvents.cs` | `TreasureChestSpawnedEvent`, `TreasureChestOpenedEvent` | 보물상자 스폰 및 개봉 이벤트 집합 |
| | `EvolutionEvents.cs` | `SkillEvolvedEvent` | 스킬 진화 발생 이벤트 |
| | `PlayerEvents.cs` | `PlayerDamagedEvent`, `PlayerMovedEvent`, `PlayerSlashExecutedEvent` | 플레이어 관련 이벤트 및 칼 휘두르기 궤적/각도/사거리 실행 이벤트 |
| | `MonsterEvents.cs [UPDATED]` | `MonsterDamagedEvent (IsCritical 포함)`, `MonsterSpawnedEvent`, `MonsterDiedEvent` 등 | 몬스터 관련 이벤트 집합 (크리티컬 적중 여부 플래그 지원) |
| | `LevelEvents.cs` | `PlayerLevelUpEvent`, `ExpGainedEvent` | 경험치 및 레벨업 이벤트 |
| | `SessionEvents.cs` | `GameStateChangedEvent`, `SurvivalTimeUpdatedEvent`, `KillCountUpdatedEvent`, `GoldGainedEvent` | 세션 및 상태 전이 관련 도메인 이벤트 집합 |
| | `EventBus.cs` | `EventBus` | 제네릭 타입 기반의 고성능 도메인 이벤트 버스 |
| **Gems** | `GemStoneEntity.cs [NEW]` | `GemStoneEntity` | 보석 3종(루비/에메랄드/아메시스트) 필드 드랍 엔티티 (0-GC 풀링, 자석 흡수) |
| | `GemManager.cs [UPDATED]` | `GemManager` | 경험치 보석 + 영구 성장 보석 통합 풀링 관리자 (일반몹 1% 드랍, 보스 확정 5개 드랍) |
| **Progression** | `GemType.cs [NEW]` | `GemType`, `BranchType`, `GemTypeExtensions` | 보석 3종 및 속성 분기 3종(화염/빙결/전기) 정의 및 유틸리티 |
| | `SkillTreeNodeDef.cs [NEW]` | `SkillTreeNodeDef`, `NodeEffectType` | 54개 노드 정의 및 불변 데이터 모델 |
| | `SkillTreeSaveData.cs [NEW]` | `SkillTreeSaveData`, `SerializableDict` | 보석 지갑, 노드 레벨, 각성 분기 직렬화 세이브 데이터 |
| | `PlayerProgressionFlags.cs [NEW]` | `PlayerProgressionFlags` | 0-GC 전투 시스템 참조용 속성 특화 효과 플래그 구조체 |
| | `SkillTreeManager.cs [NEW]` | `SkillTreeManager`, `ISkillTreeStorage` | 보석 스킬 트리 핵심 매니저 (해금, 2:1 교환, 배타적 각성, 50% 환불 리셋) |
| | `SkillTreeRegistry.cs [NEW]` | `SkillTreeRegistry` | 3대 클래스 × 18노드 (총 54노드) 정적 팩토리 레지스트리 |
| | `SkillTreeApplier.cs [NEW]` | `SkillTreeApplier` | 해금 노드들을 CharacterStats 및 PlayerProgressionFlags로 변환 |
| **Meta** | `MetaShopManager.cs` | `MetaShopManager` | (구 시스템 호환 유지) 영구 강화 구매/100% 무료 환불 관리자 |
| | `MetaUpgradeDefinition.cs` | `MetaUpgradeDefinition`, `MetaUpgradeSaveData` | 8종 영구 강화 항목 정의 및 세이브 데이터 구조체 |
| | `MetaUpgradeApplier.cs` | `MetaUpgradeApplier` | 세이브 데이터를 읽어 플레이어 시작 스탯에 영구 증강 적용 |
| | `ISaveStorage.cs` | `ISaveStorage` | 영구 저장소 로컬/클라우드 입출력 추상화 인터페이스 |
| **Skills & Passives** | `SkillRewardManager.cs [UPDATED]` | `SkillRewardManager`, `SkillRewardOption`, `PassiveDefinition` | 클래스별 전용 무기 3종, 공통 오비탈 무기, 9종 패시브(**`passive_crit: 치명타의 눈` 포함**), 진화 스킬 롤링 및 추천 관리자 |
| | `OrbitingBladesEffect.cs [UPDATED]` | `OrbitingBladesEffect` | [전 클래스 공통] 개별 회전 칼날 물리 위치 판정 및 크리티컬 대미지 롤링 연동 |
| | `ChainLightningEffect.cs [UPDATED]` | `ChainLightningEffect` | [마법사 전용] 연쇄 번개 도메인 로직 및 크리티컬 대미지 롤링 연동 |
| | `FireballEffect.cs [UPDATED]` | `FireballEffect` | [마법사 전용] 비행 혜성 투사체 발사 및 착탄 순간 1회 단일 폭발 + 대미지 동기화 |
| | `FrostNovaEffect.cs [UPDATED]` | `FrostNovaEffect` | [마법사 전용] 360도 전방위 냉기 파동 및 크리티컬 대미지 롤링 연동 |
| | `ArrowRainEffect.cs [UPDATED]` | `ArrowRainEffect` | [궁수 전용] 화살 착탄 즉시 1:1 대미지 동기화 및 크리티컬 롤링 연동 |
| | `WindGlaiveEffect.cs [UPDATED]` | `WindGlaiveEffect` | [궁수 전용] 회전 풍인 관통 및 복귀 2중 타격 크리티컬 롤링 연동 |
| | `PiercingArrowEffect.cs [UPDATED]` | `PiercingArrowEffect` | [궁수 전용] 화면 끝까지 무제한 관통 사격, 투사체별 개별 크리티컬 롤링 및 **발사 시마다 시원한 BowShoot 활시위/공기 가르기 사운드 이벤트 100% 발행 연동** |
| | `GreatswordSlashEffect.cs [UPDATED]` | `GreatswordSlashEffect` | [전사 전용] **전방 부채꼴 궤적 판정(30°~360° 전방위 각도 완벽 지원) 및 크리티컬 대미지 롤링 연동** |
| | `WhirlwindEffect.cs [UPDATED]` | `WhirlwindEffect` | [전사 전용] 360도 전방위 회전 검기 연속 크리티컬 롤링 연동 |
| | `GroundStompEffect.cs [UPDATED]` | `GroundStompEffect` | [전사 전용] **디아블로4 야만용사 [지각변동(Upheaval)] 스타일 방향성 연쇄 지진 충격파 (LV1~2: 1줄기, LV3~4: 2줄기 부채꼴, LV5: 3줄기 광역 지각변동, 사거리/반경/줄기수 튜닝 연동)** |
| | `BloodEaterEffect.cs [UPDATED]` | `BloodEaterEffect` | [전사 진화 1] **대검 + 뱀파이어 이빨 -> 흡혈 대검 베기 (부채꼴 각도 30°~360° 커스텀 튜닝 및 라이브 동기화 지원)** |
| | `TempestWhirlwindEffect.cs [NEW]` | `TempestWhirlwindEffect` | [전사 진화 2] 휠윈드 + 바람의 깃털 -> 2연속 초고속 사이클론 + 4방향 칼바람 참격 파동 |
| | `EarthshakerEffect.cs [NEW]` | `EarthshakerEffect` | [전사 진화 3] 지면강타 + 강철 갑옷 -> 중심 마그마 크레이터 + 십자 4방향 3단 지진 균열 충격파 |
| | `StormArrowEffect.cs` | `StormArrowEffect` | [궁수 진화 1] 활 + 깃털 -> 폭풍 충격파 및 투사체 |
| | `PhantomGlaiveEffect.cs [NEW]` | `PhantomGlaiveEffect` | [궁수 진화 2] 글레이브 + 치명타의 눈 -> 메인 글레이브 + 2개 나선형 환영 부메랑 2중 관통 타격 |
| | `StellarRainEffect.cs [NEW]` | `StellarRainEffect` | [궁수 진화 3] 화살비 + 황금 반지 -> 2배 밀도 황금빛 유성 화살비 + 착탄 스타더스트 폭발 |
| | `MeteorStrikeEffect.cs [UPDATED]` | `MeteorStrikeEffect` | [마법사 궁극기] 초광역 하늘 유성 낙하 (기본 공격력 220, **확장 폭발 반경 7.5m**, 화상 DoT 7초) |
| | `GigastormLightningEffect.cs [UPDATED]` | `GigastormLightningEffect` | [마법사 진화 2] 연쇄번개 + 과전류의 핵 -> 10마리 순차 전이 + 각 노드 35% 플라즈마 방전 스플래시 폭발 + 100% 감전 |
| | `BlizzardNovaEffect.cs [NEW]` | `BlizzardNovaEffect` | [마법사 진화 3] 서리폭발 + 생명의 펜던트 -> 2중 팽창 서리 파동 + 8방향 관통 고드름 빙하 파편 |
| | `SkillRegistryHelper.cs [NEW]` | `SkillRegistryHelper` | 스킬, 패시브, 9대 스킬 진화 레시피 등록 전담 모듈 (GameBootstrap 슬림화) |
| | `SkillEvolutionManager.cs` | `SkillEvolutionManager` | Lv.5 무기 + 패시브 결합 시 진화 조건 검증 및 스킬 교체 (`SkillEvolvedEvent` 발행) |
| | `SkillEvolutionRecipe.cs` | `SkillEvolutionRecipe` | 9대 스킬 진화 레시피 정의 |
| **Entities** | `PlayerEntity.cs [UPDATED]` | `PlayerEntity`, `ISpatialEntity` | 플레이어 도메인 로직, `RollDamage(float rawDamage)` 크리티컬 롤러 제공 |
| | `CharacterStats.cs [UPDATED]` | `CharacterStats` | **기본 크리티컬 확률 10% (0.10f)**, 치명타 피해량(1.5x), 이동속도, 공격력, 방어력, 쿨감 등 종합 스탯 |
| | `PlayerClassFactory.cs [UPDATED]` | `PlayerClassFactory`, `CharacterClassType` | 전사/마법사 기본 크리 10%, 궁수 기본 크리 20% 및 전용 크리 배율(1.75x) 팩토리 |
| | `MonsterEntity.cs [UPDATED]` | `MonsterEntity` | 몬스터 도메인 로직, `TakeDamage(float damage, bool isCritical = false)` 및 원거리 AI 타이머 지원 |
| | `MonsterType.cs [UPDATED]`| `MonsterType`, `MonsterDefinition` | **흑기사(DarkKnight) 원거리 암흑 검기 공격 속성(`isRanged: true, preferredDistance: 4.8f, attackInterval: 2.5f`)** 및 7종 일반 몬스터/보스 아키타입 정의 |
| **Editor / CI/CD** | `BuildScript.cs [NEW]` | `BuildScript` | **GitHub Actions CI/CD 배치모드 헤드리스 빌드 자동화 스크립트** (`BuildWindows()`, SampleScene 자동 수집, BuildReport 검증 및 종료코드 반환) (90줄) |
| | `.github/workflows/build.yml [NEW]` | `CI Workflow` | **GitHub Actions Windows Standalone (.exe) 자동 빌드 & Zip 아티팩트 업로드 및 Release 게시 플로우** |
| | `.github/workflows/activation.yml [NEW]` | `Activation Workflow` | **Game-CI v2 공식 Unity 활성화 요청 파일(.alf) 자동 생성 워크플로우** |
| | `docs/GITHUB_ACTIONS_SETUP.md [NEW]` | `CI Setup Guide` | **Unity 활성화 및 라이선스(`UNITY_LICENSE`) 등록 가이드** |
| | `MonsterSpawnerView.cs [UPDATED]` | `MonsterSpawnerView` | 몬스터 스폰 & 페이즈 관리, **도망 방향 120도 부채꼴 스폰 억제(90% 후방/측면 스폰 & 도주로 확보)**, **레벨업 경험치 순증가분의 30% 비례 최대 몹 수 완만 스케일링**, **경험치 증가분 대비 몹 체력 제곱근(Square Root) 완만 감쇠 공식(`1.0 + sqrt(expIncrease) * ratio`) 및 최대 상한선(`MobHpMaxCapMultiplier`) 클램핑 실시간 동적 체력 스케일링(`GetExpGrowthHpScale()`)** 뷰 매니저 |
| | `MonsterSpawner.cs [UPDATED]` | `MonsterSpawner` | 도메인 몬스터 스포너 (1,280 오브젝트 풀링, SpatialGrid2D 공간분할 쿼리) |
| | `ProjectileEntity.cs [UPDATED]` | `ProjectileEntity` | 투사체 관통 적중 및 미니 AoE 폭발 시 개별 크리티컬 롤링 판정 지원 |
| | `ProjectileManager.cs [UPDATED]` | `ProjectileManager` | 발사 시 발사자의 `CritChance` 및 `CritDamageMultiplier`를 투사체에 전달 |
| **UI/Text** | `DamageTextEntity.cs` | `DamageTextEntity` | 대미지 및 크리티컬 여부 플래그 저장 |
| | `DamageTextManager.cs [UPDATED]` | `DamageTextManager` | `MonsterDamagedEvent.IsCritical` 플래그를 수신하여 크리티컬 텍스트 엔티티 생성 |
| **Spatial & Pool**| `SpatialGrid2D.cs`, `ObjectPool.cs` | `SpatialGrid2D<T>`, `ObjectPool<T>` | 공간 분할 해시 그리드 및 제네릭 무할당 풀러 |
| **Skills & Config**| `SkillConfigModels.cs [UPDATED]` | `ExpConfig` | **`MobHpScalingRatio`(경험치 비례 몹 체력 감쇠율, 기본 10%) 및 `MobHpMaxCapMultiplier`(몹 최대 체력 배율 상한선 1.0x~10.0x, 기본 5.0x)** 직렬화 모델 |

---

### 2. 🎮 Unity Presentation Layer (`Assets/src/HappyShoot.View`)

| 카테고리 | 파일명 | 주요 컴포넌트 | 설명 |
| :--- | :--- | :--- | :--- |
| **Background** | `BackgroundManager.cs [NEW]` | `BackgroundManager` | **무한 배경 타일링 시스템: 3x3 그리드(총 72m x 72m) 기반으로 카메라를 추적하여 부드러운 0-GC 랩어라운드(Wrap-around) 순환 이동 및 4종 텍스처 바리에이션 동적 갱신 (156줄)** |
| | `BackgroundTileView.cs [NEW]` | `BackgroundTileView` | **개별 배경 타일 렌더러 컴포넌트 (`sortingOrder = -100` 최하단 고정, 24m x 24m, 56줄)** |
| | `BackgroundSpriteHelper.cs [NEW]` | `BackgroundSpriteHelper` | **고대 던전 석판 4종(기본, 크랙, 룬, 이끼) 프로시저럴 픽셀아트 텍스처 및 스프라이트 생성기 (277줄)** |
| | `BackgroundAmbientDustView.cs [NEW]` | `BackgroundAmbientDustView` | **전장 깊이감(Depth)과 공기감을 부여하는 28개 초경량 앰비언트 부유 먼지/마법 불씨 입자 무할당 순환 시스템 (`sortingOrder = -50`, 125줄)** |
| **Effects** | `SlashHitVfxManagerView.cs [NEW]` | `SlashHitVfxManagerView` | **⚔️ 슬래시 타격 이펙트 매니저: 대검 베기 및 스킬로 적 피격 시 몬스터 위치에 날카로운 대각선 슬래시 컷 스파크(Diagonal Slash Spark & Glint, sortingOrder = 32)를 0.10초 동안 재생하여 극상의 베는 손맛/타격감(Juice) 제공 (32개 무할당 풀링)** |
| | `CriticalHitVfxManagerView.cs [UPDATED]` | `CriticalHitVfxManagerView` | **황금빛 십자 섬광 + 8방향 스타버스트 크리티컬 스파크 VFX(sortingOrder = 35) 및 샌드박스 설정 연동 크리티컬 역경직(Hit-Stop) 트리거** |
| | `HitStopManager.cs [UPDATED]` | `HitStopManager` | **GC Zero-Allocation Update 루프 타이머 기반 초경량 타격 역경직 매니저 (코루틴/가비지 압박 제거, 찰진 20% 슬로우모션 최적화)** |
| **Player** | `PlayerView.cs [UPDATED]` | `PlayerView` | 클래스별 외형 3단 분기, **플레이어 몸체(sortingOrder = 15) 및 무기(평상시 16, 스윙 시 30 동적 승격)**, **대검 베기 & 블러드 이터 부채꼴 각도(`ArcAngleDegrees`)에 연동된 동적 스윙 궤적(-halfArc ~ +halfArc) 및 100% 선명 회전(sortingOrder = 30)** |
| | `MonsterSpawnerView.cs [UPDATED]` | `MonsterSpawnerView` | `MonsterDamagedEvent.IsCritical`을 `MonsterView.OnHitFeedback(evt.IsCritical)`로 전달, **보스 1/2 처치 후 페이즈별 웨이브 진행(`_phaseCtrl.Update`) 및 원거리 몬스터(흑기사/스켈레톤) 투사체 발사 제어 (402줄)** |
| | `WavePhaseController.cs [UPDATED]` | `WavePhaseController` | **보스 1 처치 후 시간 경과(30s, 70s, 110s)에 따른 Phase 2 다채로운 몬스터 생태계(화염임프/독거미/흑기사/골렘/스켈레톤 조합) 순차 전개 및 Boss 2 스폰 제어 (124줄)** |
| **Projectiles** | `ProjectileView.cs [UPDATED]` | `ProjectileManagerView`, `ProjectileView` | 관통 화살(Piercing Arrow, sortingOrder = 24), 황금빛 앰버 골드 일관 유지 (128개 사전 생성 Prewarm 및 0-Allocation 풀링) |
| | `OrbitingBladeView.cs [UPDATED]` | `OrbitingBladeView` | 공통 수호의 검(sortingOrder = 22)으로 몬스터(10) 및 플레이어(15) 상단에서 선명하게 회전 |
| | `WhirlwindManagerView.cs [UPDATED]` | `WhirlwindManagerView` | 전사 휠윈드 회전 검날(sortingOrder = 28) 및 바람 스파크(sortingOrder = 29)로 몬스터 상단에서 사이클론 폭풍 선명 연출 |
| | `GroundStompManagerView.cs [UPDATED]` | `GroundStompManagerView` | **💥 전사 지각변동(Upheaval) 뷰 매니저: 0.030초 초고속 간격으로 전방을 향해 쐐기형 V자 지진 충격파(Wedge Shockwave Crest, sortingOrder = 6)가 두두두두! 뻗어나가며 좌우 지반 바위 슬래브(Earth Chunks, sortingOrder = 5)가 들썩이는 역동적 지진 연출 (365줄)** |
| | `UpheavalSpriteHelper.cs [NEW]` | `UpheavalSpriteHelper` | **전사 지각변동 전용 쐐기형 지진파 아크, 파쇄 바위 슬래브, 암석 가시 픽셀아트 생성기 (179줄)** |
| | `MagicSkillManagerView.cs [UPDATED]` | `MagicSkillManagerView` | 서리폭발(sortingOrder = 26), 빙하샤드(27), **기가스톰 체인라이트닝 1~3갈래 다중 번개 줄기(StreamCount) 및 체인 플라즈마 빔(sortingOrder = 28, 29)** |

---

### 2. 🎮 Unity Presentation Layer (`Assets/src/HappyShoot.View`)

| 카테고리 | 파일명 | 주요 컴포넌트 | 설명 |
| :--- | :--- | :--- | :--- |
| **Audio** | `ProceduralAudioHelper.cs [UPDATED]` | `ProceduralAudioHelper` | **초극상 찰진 슬랩 스냅+미트 펀치 타격음**, **관통화살 Whoosh 사운드**, **🦶 전사 지면강타(지각변동) 전용 단단한 지면 쿵 + 대지 진동 럼블 + 바삭한 암석 파쇄 크런치 스냅**, 16종 프로시저럴 SFX 및 BGM 생성기 |
| | `ProceduralSkillAudioHelper.cs [NEW]` | `ProceduralSkillAudioHelper` | **🏹 고유 스킬 및 도트뎀 전용 사운드 합성기**: **관통화살/폭풍의 활(예리한 촥! 쏜살 관통 스냅)**, **칼바람 글레이브(회전 바람/금속 슬래시)**, **스텔라 레인(영롱한 별빛 크리스탈 챠링)**, **인페르노 화염구(화르륵 콰앙 마그마 폭발)**, **화염 도트뎀(치이익- 바스락 불꽃)**, **전기 도트뎀(찌릿- 플라즈마 스파크)** |
| | `SoundManagerView.cs [UPDATED]` | `SoundManagerView` | **32채널 풀링, DamageType 기반 스킬별 힛트음 분기 라우팅, 화염/전기 도트뎀 프레임당 1회 지능형 스로틀링(Throttling)으로 대규모 몹 전투 시 '드드드듣ㄱ' 소음 근절 및 찰진 오디오 쾌감 보장**, BGM 루프 재생 |
| **Background** | `BackgroundManager.cs [NEW]` | `BackgroundManager` | **무한 지형 3x3 타일링 매니저** (카메라 추적 기반 24x24m 타일 재배치, 0-GC 풀링) |
| | `BackgroundSpriteHelper.cs [UPDATED]` | `BackgroundSpriteHelper` | **🏛️ 2.5D 아이소메트릭 다이아몬드(Isometric Diamond) 석판 타일 생성기** (45° 마름모 투영, 상단 하이라이트 림 + 하단 3D 두께 음영 Bevel, 4종 석판 변형) |
| | `BackgroundTileView.cs [NEW]` | `BackgroundTileView` | 개별 배경 타일 렌더러 (SpriteRenderer sortingOrder = -100) |
| | `BackgroundAmbientDustView.cs [NEW]` | `BackgroundAmbientDustView` | 던전 바닥 은은한 공기 먼지/부유 입자 파티클 시각화 |
| **Projectiles & Spells** | `MeteorStrikeManagerView.cs [UPDATED]` | `MeteorStrikeManagerView` | **메테오 스트라이크 비주얼 대격변: 주변 붉은 번짐 절반 이하(`radius * 0.85f`) 축소, 황금빛 광선 림, 착탄 노바 섬광(Nova Flash), 지면 마그마 크레이터 룬, 중력 가속 혜성 꼬리 연출** |
| **Bootstrap** | `GameBootstrap.cs [UPDATED]` | `GameBootstrap` | 마스터 부트스트랩 (카메라, 3영웅, 사운드 매니저, 마법 스킬 매니저, 메테오 매니저, **보석 스킬 트리 매니저 & UI, GemStoneManagerView, HUD 보석 카운터**, 세션, UI 일괄 생성 및 연동) |
| **Gems** | `GemStoneView.cs [NEW]` | `GemStoneView`, `GemStoneManagerView` | 필드에 스폰된 보석 3종(루비/에메랄드/아메시스트) 렌더링, 펄스 애니메이션, 0-GC 풀링 |
| **Skill Tree** | `SkillTreeUiView.cs [UPDATED]` | `SkillTreeUiView` | **🌌 360도 방사형 원형 성좌(Radial Constellation) 스킬 트리 메인 화면** (중앙 기원 허브, 3대 클래스 120° 섹터 동시 렌더링, 54개 노드 인터랙션, 우측 상세 인스펙터 패널, ESC/뒤로가기 지원) |
| | `SkillTreeExchangePopupView.cs [NEW]` | `SkillTreeExchangePopupView` | **💎 2:1 보석 교환소 모달 팝업** (루비/에메랄드/아메시스트 상호 6방향 변환 다이얼로그) |
| | `SkillTreeBackgroundHelper.cs [NEW]` | `SkillTreeBackgroundHelper` | **🌌 512x512 고대 천구 석판 다이얼 및 4중 동심원 룬 궤도 홈 프로시저럴 텍스처 생성기** |
| | `SkillTreeNodeView.cs [NEW]` | `SkillTreeNodeView` | 개별 스킬 트리 노드 UI 버튼 및 상태 뷰어 (원형 룬 뱃지, 속성 아이콘, 비용/레벨, 해금 콜백) |
| | `SkillTreeLayoutHelper.cs [UPDATED]` | `SkillTreeLayoutHelper` | **중앙 엠블럼 기준 120도(Warrior 90°, Ranger 330°, Wizard 210°) 극좌표(r, theta) 방사형 배치 및 120° 섹터 구분선(Ray) 렌더링** |
| | `SkillTreeSpriteHelper.cs [UPDATED]` | `SkillTreeSpriteHelper` | **원형 룬 젬 뱃지 4종(해금/가능/잠김/차단)**, 중앙 황금 룬 허브 텍스처, 보석 3종, 속성 아이콘(🔥❄️⚡) 프로시저럴 픽셀아트 생성기 |
| | `JsonSkillTreeStorage.cs [NEW]` | `JsonSkillTreeStorage` | Unity PlayerPrefs JSON 기반 스킬 트리 세이브 데이터 영구 저장소 |
| **Shop** | `MetaShopUiView.cs` | `MetaShopUiView`, `JsonPlayerPrefsStorage` | (구 시스템) 8종 영구 강화 카드 목록 상점 UI |
| | `GameOverResultUiView.cs [UPDATED]` | `GameOverResultUiView` | 플레이어 사망 시 골드 + **💎 3종 보석 영구 저장소 자동 적립**, 런 통계창, **[💎 스킬 트리 (영구 성장)]** 및 [PLAY AGAIN] 연동 |
| **UI** | `InGameGemCounterHudView.cs [NEW]` | `InGameGemCounterHudView` | 인게임 상단에 실시간으로 이번 런에서 획득한 보석 3종(🔴 루비, 🟢 에메랄드, 🟣 아메시스트) 수집 카운터 표시 |
| **Skills & Evolutions** | `OrbitingBladeView.cs` | `OrbitingBladeView` | 플레이어 주위를 원형 회전하는 공통 오비탈 칼날 시각화 (실시간 생성 및 칼날 수/반경 동기화) |
| | `EvolutionPopupView.cs` | `EvolutionPopupView` | 스킬 진화 성공 시 상단에 등장하는 축하 배너 팝업 및 자동 생성 |
| | `LevelUpUiView.cs [UPDATED]` | `LevelUpUiView` | **레벨업 시 320x460 대형 카드 및 80x80 픽셀아트 아이콘 3지선다 보상 선택 UI (Unity New Input System 연동: Q/W/E 위 1, 2, 3 숫자키 및 마우스 클릭 즉시 선택 지원)** |
| **Boss & Chests** | `BossHealthBarView.cs` | `BossHealthBarView` | 화면 상단 슬림 보스 HP 바(1920x1080 반응형 앵커), 타이머와 분리된 상단 배치 |
| | `TreasureChestView.cs` | `TreasureChestView` | 필드에 스폰된 황금 보물상자 렌더링 및 펄스 반짝임 애니메이션 |
| | `TreasureChestManagerView.cs`| `TreasureChestManagerView` | 도메인 보물상자 매니저 업데이트 및 뷰 풀링 (상자 오픈/이벤트 종료 시 즉시 필드 디스폰) |
| | `TreasureChestPopupView.cs [UPDATED]` | `TreasureChestPopupView` | 상자 획득 시 1~3개 스킬 보상 및 골드 획득 연출 팝업 (Space/Enter/1/2/3 키보드 즉시 확인) |
| **UI** | `SettingsDialogUiView.cs` | `SettingsDialogUiView` | 3개 탭 종합 환경 설정 모달 다이얼로그 (자동/수동조준, 볼륨, UI스케일) |
| | `CharacterSelectUiView.cs [UPDATED]` | `CharacterSelectUiView` | **첫 시작 영웅 선택 화면** (전사/궁수/마법사 3영웅 카드, 🛠️ 개발자모드, 🧪 샌드박스, ⚙️ 환경설정, **🚪 게임 종료 버튼**) |
| | `DevSkillSelectorUiView.cs [UPDATED]` | `DevSkillSelectorUiView` | [개발자 모드] 인게임 실시간 스킬(10종)/진화/패시브 원클릭 장착 및 **우클릭 즉시 Lv.0 해제/제거**, 치트(무적, 레벨업, 전멸, 배속 등) UI |
| | `SkillTuningUiView.cs [UPDATED]` | `SkillTuningUiView` | **🧪 전투 & 밸런스 샌드박스 (Combat Sandbox)** - 실시간 10종 스킬 + 9종 진화 스킬 + **🧬 9종 패시브 스킬 튜닝**, **💎 경험치 & 레벨업 시스템 튜닝**, **👾 7종 몬스터 + 보스 스탯**, **🎯 치명타 확률/배율 및 플레이어 코어 스탯 실시간 조절 및 프로젝트 내부 JSON 파일(`Assets/Config/skill_configs.json`) 영구 저장/GitHub 동기화 지원** |
| | `SkillTuningUiBuilder.cs [UPDATED]` | `SkillTuningUiBuilder` | 샌드박스 모드 UI 요소 생성 전담 헬퍼 (**6대 대분류 카테고리 탭: 전사/궁수/마법사/패시브/공통/시스템**, 500줄 규칙 준수 모듈화) |
| | `SkillTuningPassiveConfigurator.cs [NEW]` | `SkillTuningPassiveConfigurator` | **🧬 9종 패시브 스킬 샌드박스 슬라이더 행 생성 및 실시간 핫리로드 연동 전담 헬퍼 (흡혈귀의 이빨, 바람의 깃털, 마나 룬, 강철 갑옷, 황금 반지, 생명의 펜던트, 발화의 불꽃, 과전류의 핵, 치명타의 눈)** |
| | `SkillLiveApplier.cs [UPDATED]` | `SkillLiveApplier` | 스킬 수치 실시간 핫리로드 및 **`ApplyPassivesLive` 플레이어 보유 패시브 레벨 비례 `PlayerEntity.Stats` 실시간 재계산/동기화** |
| | `SkillConfigRepository.cs [UPDATED]` | `SkillConfigRepository` | **📁 샌드박스 설정 파일 멀티 PC/Git 동기화 저장소** - `Assets/Resources/Config/skill_configs.json` 및 `Assets/Config/skill_configs.json` 이중 저장 & `Resources.Load<TextAsset>` 1순위 로드로 GitHub pull 시 다른 PC 및 빌드에서도 100% 최신 설정 공유 보장 (에디터 자동 AssetDatabase.Refresh 및 Fallback 지원) |
| | `skill_configs.json [UPDATED]` | `Assets/Resources/Config/skill_configs.json` | **깃허브 형상관리 및 Unity Resources 공식 연동 샌드박스 튜닝 설정 파일** (전체 스킬/진화/패시브 9종/몬스터/경험치/크리티컬 커스텀 수치 보존 및 타 PC 즉각 공유) |
| | `MonsterTuningConfig.cs [UPDATED]` | `MonsterTuningConfigData`, `DarkKnightStatConfig [NEW]`, `BossStatConfig [UPDATED]` | **흑기사 투사체 속도/대미지 (`DarkKnightStatConfig`) 및 보스 광역 장판 주기/대미지/반경 (`BossStatConfig`) 설정 데이터 모델** |
| | `SkillTuningSliderFactory.cs [UPDATED]` | `SkillTuningSliderFactory` | **🎨 샌드박스 슬라이더 UI 팩토리: 슬라이더 + [-]/[+] 스텝 버튼 + 키보드 숫자 직접 입력창(`InputField`) 완벽 결합 및 100% 양방향 실시간 동기화** (271줄, 500줄 규칙 준수) |
| | `SkillTuningMemoryCache.cs [NEW]` | `SkillTuningMemoryCache` | 스킬 테스트 모드에서 L1~L5 레벨 간 이동 시 각 레벨별로 튜닝한 수치(공격력, 쿨다운, 반경 등)를 메모리에 완벽 보존/복원하는 세션 캐시 관리자 |
| **Boss & Attacks** | `BossHazardZoneManagerView.cs [NEW]` | `BossHazardZoneManagerView` | **💥 보스 대형 광역 장판 0-할당 풀링 매니저: 1.2s 붉은색 전조 경고 링 $\rightarrow$ 2.0s 마그마 지옥불 폭발 지속 피해 장판(직경 5.6m) $\rightarrow$ 소멸 (226줄)** |
| | `EnemyProjectileManagerView.cs [UPDATED]` | `EnemyProjectileManagerView` | **해골 뼈 화살 + 흑기사 보라색 암흑 마법 검기(`SpawnDarkSlashProjectile`) 0-할당 풀링 매니저 (153줄)** |
| | `EnemyAttackSpriteHelper.cs [NEW]` | `EnemyAttackSpriteHelper` | **보라색 암흑 검기, 보스 전조 경고 링, 보스 마그마 장판 프로시저럴 픽셀아트 생성기 (180줄)** |
| **UI** | `InGameHudView.cs [UPDATED]` | `InGameHudView` | **메인 HUD 매니저: 하단 3단 EXP/스킬/HP 바 + 좌측 9종 패시브 슬롯 리스트 & 실시간 수치(ATK/SPD/RNG/ARM/EXP/HP/CRT 등) 표시 (442줄)** |
| | `InGameHudBuilder.cs [NEW]` | `InGameHudBuilder` | 절차적 메인 HUD UI 팩토리 빌더 (하단 3단 및 좌측 패시브 슬롯 리스트 포함, 440줄) |
| | `HudSpriteHelper.cs [NEW]` | `HudSpriteHelper` | 10칸 분할 EXP 프레임, 다이아몬드 레벨 뱃지, 골드 스킬 슬롯 보더, 투구 엠블럼, 체력바 프레임 프로시저럴 비주얼 생성기 (236줄) |
| | `PlayerHealthBarView.cs` | `PlayerHealthBarView` | 플레이어 머리 위를 따라다니는 초경량 오버헤드 미니 체력바 (SpriteRenderer 기반 무할당) |
| **Visual Feedback** | `AimReticleView.cs [UPDATED]` | `AimReticleView` | **최상위 Canvas Overlay (`ScreenSpaceOverlay`, sortingOrder: 32760) 기반 조준선**: 샌드박스 메뉴/HUD/팝업 등 모든 UI 위에서 가림 없이 100% 최상단 선명 렌더링, `raycastTarget = false`로 메뉴 조작 방해 제로, 마우스 4초 유휴 시 부드러운 페이드아웃, 클릭 펄스(1.35x), 브리딩 펄스(1.0~1.08x) (182줄) |
| | `ReticleSpriteHelper.cs [NEW]` | `ReticleSpriteHelper` | 48x48 네온 라임-그린 십자선 과녁 링 프로시저럴 픽셀아트 생성기 (104줄) |
| | `PlayerInputHandler.cs` | `PlayerInputHandler` | New Input System 기반 이동 입력 수신 및 도메인 전달 |
| | `PauseMenuUiView.cs` | `PauseMenuUiView` | ESC 일시정지 다이얼로그 (계속하기, ⚙️ 환경 설정, 다시 시작, 게임 종료) |
| | `DamageTextView.cs` | `DamageTextManagerView`, `DamageTextView` | 이벤트 기반 무할당 32개 풀링 (GameSettings.ShowDamageText 옵션 적용) |
| **Player** | `PlayerView.cs [UPDATED]` | `PlayerView` | **지능형 시선 제어: (1) 마우스 조작 시 Aim 9방향 실시간 조준, (2) 마우스 유휴(4s) 시 Aim 숨김 및 즉시 기본 정면(Front) 샷 복귀, (3) 키보드(WASD) 이동 시 누른 방향(9방향) 질주 및 멈추면 즉시 정면 복귀, (4) 마우스 움직임 시 즉시 Aim 복원**, 오늘 시작 시의 100% 원본 치비 픽셀아트, 2.5D Blob Shadow, Brotato 젤리 물리 모션 (495줄) |
| | `HeroSpriteHelper.cs [NEW]` | `HeroSpriteHelper` | **오늘 시작 시의 100% 원본 32x32 귀여운 치비 3영웅(전사 둥근투구/빨간망토, 궁수 그린후드/화살통, 마법사 롭/고깔모자) 9방향 스프라이트 및 원본 브로드소드/리커브활/크리스탈스태프 연동 (368줄)** |
| | `PlayerInputHandler.cs` | `PlayerInputHandler` | New Input System 기반 이동 입력 수신 및 도메인 전달 |
| **Monsters** | `MonsterView.cs [UPDATED]` | `MonsterView` | **7종 몬스터(Slime, Bat, Skeleton, Golem, FireImp, ToxicSpider, DarkKnight) + 2종 보스(Golem King, Laser Archdemon) 고퀄리티 픽셀아트 때깔/명암/발광 코어 강화**, 2.5D Blob Shadow 타원 그림자, 타입별 젤리 물리 모션 (316줄) |
| | `MonsterDeathFxManagerView.cs [NEW]` | `MonsterDeathFxManagerView` | **몬스터 속성별(암석 파편/형광 독즙/화염 불씨/영혼 가루/골드 룬) 처치 미니 파티클 무할당 풀링(64개) 뷰 매니저 (154줄)** |
| | `MonsterSpriteHelper.cs [UPDATED]` | `MonsterSpriteHelper` | **7종 일반 몬스터 + 보스 2종 고해상도 셀 셰이딩/글레어/발광 코어/룬 픽셀아트 프로시저럴 생성기 (367줄)** |
| | `MonsterSpawnerView.cs [UPDATED]` | `MonsterSpawnerView` | **Phase 1→보스1→Phase 2(FireImp→ToxicSpider→DarkKnight 순차 합류)→보스2 웨이브 진화 시스템**, `BossLaserBeamManagerView` 라이프사이클 연동, 512개 풀링 |
| | `WavePhaseController.cs [NEW]` | `WavePhaseController` | **보스 격퇴 후 웨이브 페이즈 진화 컨트롤러** (Phase1/Phase2Wave1~3/Boss2Spawned 5단계, 페이즈별 몬스터 아키타입 롤 테이블 분리 관리) |
| **Projectiles** | `ProjectileView.cs [UPDATED]` | `ProjectileManagerView`, `ProjectileView` | **관통 화살(Piercing Arrow) 비주얼 대폭 개편: 32x10 날렵하고 얄쌍한 다이아몬드 화살촉 + 슬림 화살대 + V자 화살깃 신규 픽셀아트 적용, 1발~5발 레벨업 시에도 변색 없이 처음의 찬란한 황금빛 앰버 골드 일관 유지** (128개 사전 생성 Prewarm 및 0-Allocation 풀링) |
| | `SkillSpriteHelper.cs [UPDATED]` | `SkillSpriteHelper` | **`GetOrCreatePiercingArrowSprite` (32x10 날렵한 관통 화살)**, `GetOrCreateStormArrowSprite` (28x12 폭풍 화살), `GetOrCreateWindGlaiveSprite` (32x32 풍인), `GetOrCreateStormBlastSprite` 픽셀아트 생성기 |
| | `EnemyProjectileManagerView.cs` | `EnemyProjectileManagerView` | Struct 배열 0-Allocation 풀링(64개) 적용, 날렵한 뼈다귀 투사체 매니저 |
| | `GroundStompManagerView.cs [UPDATED]` | `GroundStompManagerView` | 전사 지면 강타 발동 시 **128x128 대지 파쇄 크레이터 & 실제 도메인 반경 100% 일치 림 경계선**, 12개 포물선 암석 파편 & 엠버 비산, 화면 진동 연동 뷰 매니저 |
| | `WhirlwindManagerView.cs [NEW]` | `WhirlwindManagerView` | 전사 휠윈드 발동 시 **128x128 360도 강철 소용돌이 3중 칼날 & 1080 deg/s 초고속 사이클론 스핀**, 바람 잔상 및 8개 윈드 스파크 비산 뷰 매니저 (16개 무할당 풀링) |
| | `ArrowRainManagerView.cs [UPDATED]` | `ArrowRainManagerView` | 궁수 화살비 & 스텔라 레인 발동 시 20~60발의 화살이 지면에 꽂히는 첫 프레임에 몬스터에게 1:1 즉시 대미지를 가하는 착탄 동기화 뷰 매니저 (**바닥 원형 인디케이터/무늬 0.6초 후 페이드아웃 및 깔끔한 조기 제거 적용**) |
| | `RewardIconHelper.cs [UPDATED]` | `RewardIconHelper` | 인게임 보상 및 스킬 슬롯용 프로시저럴 픽셀아트 아이콘 마스터 캐시 및 디스패처 (198줄) |
| | `RangerRewardIconHelper.cs [NEW]` | `RangerRewardIconHelper` | **궁수 스킬 전용 아이콘 생성기 (관통화살, 화살비 타겟팅 마법진+집중일제사격, 풍인 3날 부메랑, 스텔라레인 등 고유 픽셀아트)** |
| | `WarriorRewardIconHelper.cs [NEW]` | `WarriorRewardIconHelper` | **전사 스킬 전용 아이콘 생성기 (대검베기, 휠윈드 3중 회오리날, 지면강타 크레이터, 블러드이터 등)** |
| | `PassiveRewardIconHelper.cs [NEW]` | `PassiveRewardIconHelper` | **패시브 9종 전용 아이콘 생성기 (흡혈귀의 이빨, 바람의 깃털, 마나 룬, 강철 갑옷, 황금 반지, 생명의 펜던트, 치명타의 눈 등)** |
| | `MagicSkillManagerView.cs [UPDATED]` | `MagicSkillManagerView` | 서리폭발/빙하샤드 및 **기가스톰 초고전압 중뇌격 플라즈마 기둥(0.72m) & 일반 체인라이트닝 썬더볼트(0.34m)** 뷰 매니저 |
| | `MeteorStrikeManagerView.cs [UPDATED]`| `MeteorStrikeManagerView`| **인페르노 화염구 뷰 매니저: 거대 헬파이어 화염구 3발 발사 + 1회 관통(1차 충돌 폭발 + 관통 비행 + 2차 폭발) + 7초 화상 DoT & 마그마 네뷸라 이펙트** |
| | `WizardSkillSpriteHelper.cs [UPDATED]`| `WizardSkillSpriteHelper`| 마법사 스킬 전용 프로시저럴 스프라이트 생성기 (화염구 혜성/폭발구/불씨, **32x16 고전압 플라즈마 번개 빔**, 16x16 십자 전기 스파크) |
| | `MagicSkillVisualModels.cs [NEW]` | `MagicSkillVisualModels` | 서리 파동/번개 볼트/빙하 파편/전기 스파크 풀링 인스턴스 데이터 모델 (500줄 규칙 준수 모듈화) |
| | `FireballSkillManagerView.cs [NEW]` | `FireballSkillManagerView` | [마법사 기본 스킬] **초고속 혜성 화염구 투사체(Comet Fireball, 18m/s) 비행 & 불씨 잔상**, **128x128 다단 플라즈마 화염 폭발 성운**, **8방향 방사형 불티(Embers) 비산 및 카메라 셰이크** 뷰 매니저 (266줄, 0-Allocation 풀링) |
| | `LevelUpUiView.cs [UPDATED]` | `LevelUpUiView` | **레벨업 3지선다 키보드 숫자키 1, 2, 3(키패드 1,2,3 포함) 즉시 선택 단축키 지원 & 버튼 뱃지 표시** |
| | `TreasureChestPopupView.cs [UPDATED]` | `TreasureChestPopupView` | **보물 상자 개봉 시 640x580 대형 다이얼로그, 80x80 스킬 아이콘 + 골드 타이틀 + 대형 한글 설명 카드 패널 렌더링, [스페이스/엔터/클릭] 수령 지원** |
| **Projectiles** | `MeteorStrikeManagerView.cs [UPDATED]` | `MeteorStrikeManagerView` | **[마법사 궁극기 메테오 스트라이크 개편] 눈 아픈 붉은 화면 제거, 은은한 룬 마법진, 운석 화염 꼬리(Flame Trail), 황금-주황빛 충격파 링 및 12개 마그마 파편 비산 연출** |
| | `WindGlaiveManagerView.cs [UPDATED]` | `WindGlaiveManagerView` | **[궁수 기본/진화 글레이브] 윈드 글레이브 & 팬텀 글레이브 뷰 매니저 (0.5~3.0배 칼날 스케일링, 1~7개 대칭 부채꼴 환영 비산, 왕복 2타 판정 동기화)** |
| | `FireballSkillManagerView.cs [UPDATED]` | `FireballSkillManagerView` | 화염구 발사 및 폭발 뷰 매니저 (정확한 `fireball` 카메라 셰이크 연동) |
| | `MagicSkillManagerView.cs [UPDATED]` | `MagicSkillManagerView` | 서리폭발/빙하샤드/기가스톰/체인라이트닝 뷰 매니저 (스킬별 정확한 고유 카메라 셰이크 연동) |
| **Camera** | `CameraFollowView.cs [UPDATED]` | `CameraFollowView` | **스킬별(0~100%) 및 마스터(0~100%) 카메라 셰이크 강도 배율 지원, 다중 스킬 셰이크 중첩 시 최대값 우선(Max Clamping) 및 절대 한계선(0.38m) 캡핑으로 눈 피로/어지러움 완벽 차단** |
| **Utils** | `RewardIconHelper.cs [UPDATED]` | `RewardIconHelper` | 18종 무기/패시브/진화 전용 80x80 고해상도 픽셀아트 아이콘 생성기 |
| | `SpriteHelper.cs [UPDATED]` | `SpriteHelper` | **2.5D 고해상도(48x24) 카툰 타원 블롭 섀도우(자연스러운 58% 불투명도 및 소프트 페이드아웃 감쇠) 및 공용 픽셀아트 스프라이트 생성기 (446줄)** |
| **Bootstrap** | `GameBootstrap.cs [UPDATED]` | `GameBootstrap` | 마스터 부트스트랩 (카메라, 3영웅, 사운드 매니저, 마법 스킬 매니저, 메테오 매니저, 영구 상점, 세션, UI 일괄 생성 및 연동) |

---

### 3. 🧪 Tests Layer (`Assets/tests/HappyShoot.Domain.Tests`)
*총 124개 NUnit 단위 테스트 스위트 (100% ALL PASS)*
- `WarriorSkillsTests.cs [UPDATED]`: 지면 강타 도메인 반경 검증, 휠윈드 360도 전방위 4방향 타격 검증, 휠윈드 레벨업 시 대미지/반경 스케일링, 블러드 이터 150도 전방 부채꼴 적중 및 플레이어 라이프스틸 회복 검증 등
- `LevelSystemTests.cs [UPDATED]`: 레벨업 경험치 스케일링 및 **경험치 증가분 대비 몹 체력 배율(`MobHpScalingRatio`) 연산 검증**
- `StatusEffectTests.cs [UPDATED]`: 메테오 스트라이크 대미지 및 화상(Burn) DoT 적용 검증 등
- `CriticalStrikeTests.cs`: 기본 크리 10% 검증, 100% 확정 크리티컬 피해량 배율(2.5x) 연산, 몬스터 피격 시 `MonsterDamagedEvent.IsCritical` 플래그 발행 등
- `PassiveItemsTests.cs`: `passive_crit` (치명타의 눈) 레벨업 시 크리티컬 확률 +8% 및 크리티컬 배율 +5% 누적 증가 검증 포함 3개 테스트
- `SkillEvolutionTests.cs`: 3대 진화 레시피 합성, 진화 카드 우선순위 추천, 궁극기로 진화 후 보상 선택지에서 기본 스킬 완전 제외 검증 5개 테스트
- `StatusEffectTests.cs`: 오한 40% 감속 검증, 오한 만료 시 정상 속도 복구, 7초 화염 DoT 틱 누적, 7초 감전 DoT 틱 누적, 오한 사망 시 `MonsterShatteredEvent` 발행, 메테오 스트라이크 이벤트 발행 및 화염 DoT 검증 6개 테스트
- `WizardSkillsTests.cs`: 화염구 스플래시 판정 및 데미지, 서리 폭발 360도 전방위 적 피격, 연쇄 번개 4회 전이 타격, 마법사 팩토리 스탯, 레벨업 보상 롤링 및 타 클래스 스킬 배제 5개 테스트
- `GreatswordSlashTests.cs`: 전방 150도 궤적 적 피격, 궤적 반대편 적 무피격, 사거리 밖 적 무피격, `PlayerSlashExecutedEvent` 및 사운드 이벤트 발행 4개 테스트
- `AudioEventsTests.cs`: 사운드 및 BGM 도메인 이벤트 발행, 수신, 페이로드 정합성 3개 테스트
- `MetaShopTests.cs`: 골드 추가, 업그레이드 레벨별 구매/골드 차감, 최대 레벨 초과 구매 방지, 100% 환불 골드 계산 5개 테스트
- `MetaSaveDataTests.cs`: 업그레이드 데이터 직렬화 및 `MetaUpgradeApplier` 스탯 반영 공식 1개 테스트
- `OrbitingBladesTests.cs`: 오비탈 궤도 위치 계산 및 충돌 다중 타격 2개 테스트
- `MonsterVarietyTests.cs`: 4종 아키타입 스탯, 해골 원거리 카이팅 AI, 보스 스폰/피격/사망 이벤트 4개 테스트
- `TreasureChestTests.cs`: 상자 스폰, 접근 오픈 및 보상 지급, 보스 사망 시 상자 자동 드랍 3개 테스트
- `GameSessionTests.cs`: 세션 생명주기, 시간 틱, 킬 수/골드 누적, 일시정지, 게임오버/승리 13개 테스트
- `PlayerEntityTests`, `MonsterEntityTests`, `MonsterSpawnerTests`, `CharacterClassTests`
- `SkillCompositionTests`, `SpatialGridTests`, `LevelSystemTests`
- `SkillRewardTests`, `ExpGemTests`, `ProjectileTests`, `WaveTimelineTests`
- `DamageTextTests`, `EventBusTests`, `TimeProviderTests`
