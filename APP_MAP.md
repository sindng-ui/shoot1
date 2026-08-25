# 🗺️ HappyShoot Application Map (APP_MAP)

> **프로젝트 개요**: Pure C# Domain Layer와 Unity Presentation Layer가 완벽히 분리된 고성능 2D 탑다운 Survivors-like 액션 슈팅 게임

---

## 🏛️ 아키텍처 구조 (Clean Architecture)

```mermaid
graph TD
    subgraph Unity Presentation [HappyShoot.View]
        GB[GameBootstrap] --> HUD[InGameHudView]
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
| **Common** | `AppVersion.cs` | `AppVersion` | 버전 관리 단일 소스 (`Current = "v0.3.0"`, `ReleaseDate`) |
| **Entities** | `PlayerEntity.cs [UPDATED]` | `PlayerEntity` | 플레이어 순수 C# 엔티티 (스탯, 패시브, **`AttackPowerMultiplier` 공격력 배율 `SkillContext.BaseDamage` 실시간 완벽 동기화**, 스킬 틱/실행 관리) |
| **Events** | `AudioEvents.cs` | `SoundEffectType`, `PlaySoundEvent`, `PlayBgmEvent`, `StopBgmEvent` | 14종 SFX 및 BGM 재생 요청 도메인 이벤트 집합 |
| | `MagicEvents.cs [UPDATED]` | `FrostNovaExecutedEvent`, `ChainLightningExecutedEvent`, `FireballExplodedEvent`, `MeteorStrikeExecutedEvent`, `MonsterShatteredEvent` | 마법사 서리 폭발, 연쇄 번개, 화염구 폭발, 메테오 낙하, 빙결 파괴 도메인 이벤트 집합 |
| | `BossEvents.cs` | `BossSpawnedEvent`, `BossHealthUpdatedEvent`, `BossDiedEvent` | 보스 스폰/체력 변경/사망 이벤트 집합 |
| | `ChestEvents.cs` | `TreasureChestSpawnedEvent`, `TreasureChestOpenedEvent` | 보물상자 스폰 및 개봉 이벤트 집합 |
| | `EvolutionEvents.cs` | `SkillEvolvedEvent` | 스킬 진화 발생 이벤트 |
| | `PlayerEvents.cs` | `PlayerDamagedEvent`, `PlayerMovedEvent`, `PlayerSlashExecutedEvent` | 플레이어 관련 이벤트 및 칼 휘두르기 궤적/각도/사거리 실행 이벤트 |
| | `MonsterEvents.cs [UPDATED]` | `MonsterDamagedEvent (IsCritical 포함)`, `MonsterSpawnedEvent`, `MonsterDiedEvent` 등 | 몬스터 관련 이벤트 집합 (크리티컬 적중 여부 플래그 지원) |
| | `LevelEvents.cs` | `PlayerLevelUpEvent`, `ExpGainedEvent` | 경험치 및 레벨업 이벤트 |
| | `SessionEvents.cs` | `GameStateChangedEvent`, `SurvivalTimeUpdatedEvent`, `KillCountUpdatedEvent`, `GoldGainedEvent` | 세션 및 상태 전이 관련 도메인 이벤트 집합 |
| | `EventBus.cs` | `EventBus` | 제네릭 타입 기반의 고성능 도메인 이벤트 버스 |
| **Meta** | `MetaShopManager.cs` | `MetaShopManager` | 영구 강화 구매/100% 무료 환불, 골드 적립, 세이브 직렬화 관리자 |
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
| | `GroundStompEffect.cs [UPDATED]` | `GroundStompEffect` | [전사 전용] 대지 균열 지진파 크리티컬 롤링 연동 |
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
| | `MonsterEntity.cs [UPDATED]` | `MonsterEntity` | 몬스터 도메인 로직, `TakeDamage(float damage, bool isCritical = false)` 지원 |
| | `MonsterType.cs` | `MonsterType`, `MonsterDefinition` | 7종 일반 몬스터 및 보스 아키타입 정의 |
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
| **Effects** | `SlashHitVfxManagerView.cs [NEW]` | `SlashHitVfxManagerView` | **⚔️ 슬래시 타격 이펙트 매니저: 대검 베기 및 스킬로 적 피격 시 몬스터 위치에 날카로운 대각선 슬래시 컷 스파크(Diagonal Slash Spark & Glint, sortingOrder = 32)를 0.10초 동안 재생하여 극상의 베는 손맛/타격감(Juice) 제공 (32개 무할당 풀링)** |
| | `CriticalHitVfxManagerView.cs [UPDATED]` | `CriticalHitVfxManagerView` | **황금빛 십자 섬광 + 8방향 스타버스트 크리티컬 스파크 VFX(sortingOrder = 35) 및 샌드박스 설정 연동 크리티컬 역경직(Hit-Stop) 트리거** |
| | `HitStopManager.cs [UPDATED]` | `HitStopManager` | **GC Zero-Allocation Update 루프 타이머 기반 초경량 타격 역경직 매니저 (코루틴/가비지 압박 제거, 찰진 20% 슬로우모션 최적화)** |
| **Player** | `PlayerView.cs [UPDATED]` | `PlayerView` | 클래스별 외형 3단 분기, **플레이어 몸체(sortingOrder = 15) 및 무기(평상시 16, 스윙 시 30 동적 승격)**, **대검 베기 & 블러드 이터 부채꼴 각도(`ArcAngleDegrees`)에 연동된 동적 스윙 궤적(-halfArc ~ +halfArc) 및 100% 선명 회전(sortingOrder = 30)** |
| **Monsters** | `MonsterView.cs [UPDATED]` | `MonsterView` | **2.5D Blob Shadow 타원 그림자(sortingOrder = 9)**, **몬스터 몸체(sortingOrder = 10)**, **피격 시 탄성 스쿼시(0.28/-0.22) & 타격 진동 반동(Jolt) 대폭 강화로 시원한 피격 손맛 선사** |
| | `MonsterSpawnerView.cs [UPDATED]` | `MonsterSpawnerView` | `MonsterDamagedEvent.IsCritical`을 `MonsterView.OnHitFeedback(evt.IsCritical)`로 전달 |
| **Projectiles** | `ProjectileView.cs [UPDATED]` | `ProjectileManagerView`, `ProjectileView` | 관통 화살(Piercing Arrow, sortingOrder = 24), 황금빛 앰버 골드 일관 유지 (128개 사전 생성 Prewarm 및 0-Allocation 풀링) |
| | `OrbitingBladeView.cs [UPDATED]` | `OrbitingBladeView` | 공통 수호의 검(sortingOrder = 22)으로 몬스터(10) 및 플레이어(15) 상단에서 선명하게 회전 |
| | `WhirlwindManagerView.cs [UPDATED]` | `WhirlwindManagerView` | 전사 휠윈드 회전 검날(sortingOrder = 28) 및 바람 스파크(sortingOrder = 29)로 몬스터 상단에서 사이클론 폭풍 선명 연출 |
| | `GroundStompManagerView.cs [UPDATED]` | `GroundStompManagerView` | 전사 지면 강타 바닥 크레이터(sortingOrder = 2) 및 튀어오르는 암석 파편(sortingOrder = 26) |
| | `MagicSkillManagerView.cs [UPDATED]` | `MagicSkillManagerView` | 서리폭발(sortingOrder = 26), 빙하샤드(27), 기가스톰/체인라이트닝 플라즈마 빔(28, 29) |

---

### 2. 🎮 Unity Presentation Layer (`Assets/src/HappyShoot.View`)

| 카테고리 | 파일명 | 주요 컴포넌트 | 설명 |
| :--- | :--- | :--- | :--- |
| **Audio** | `ProceduralAudioHelper.cs [UPDATED]` | `ProceduralAudioHelper` | **초극상 찰진 슬랩 스냅(1450Hz)+단단한 미트 펀치 럼블(240Hz)+바삭한 육질 크런치 타격음(`GeneratePunchHit`) 신규 합성**, **관통화살(BowShoot) 전용 활시위 릴리즈 + 날카로운 공기 가르기 휘이잉 Whoosh(`GenerateBowArrowWhoosh`) 사운드**, 화염구/서리폭발/연쇄번개 등 15종 프로시저럴 SFX 및 BGM 생성기 |
| | `SoundManagerView.cs [UPDATED]` | `SoundManagerView` | **32채널 풀링, 관통 피격음 볼륨 0.85f~1.0f 대폭 상향, 프레임당 최대 4개 지능형 캡핑(`MaxHitsPerFrame`) & 피치 랜덤 변조(0.90~1.15x)로 100마리 관통 시에도 귀가 안 아프고 찰진 연타 손맛 보장**, BGM 루프 재생, 도메인 이벤트 리액티브 오디오 |
| **Projectiles & Spells** | `MeteorStrikeManagerView.cs [UPDATED]` | `MeteorStrikeManagerView` | **메테오 스트라이크 비주얼 대격변: 주변 붉은 번짐 절반 이하(`radius * 0.85f`) 축소, 황금빛 광선 림, 착탄 노바 섬광(Nova Flash), 지면 마그마 크레이터 룬, 중력 가속 혜성 꼬리 연출** |
| **Bootstrap** | `GameBootstrap.cs [UPDATED]` | `GameBootstrap` | 마스터 부트스트랩 (카메라, 3영웅, 사운드 매니저, 마법 스킬 매니저, 메테오 매니저, 영구 상점, 세션, UI 일괄 생성 및 연동) |
| **Shop** | `MetaShopUiView.cs` | `MetaShopUiView`, `JsonPlayerPrefsStorage` | 8종 영구 강화 카드 목록, 보유 골드 실시간 표시, 100% 무료 환불 버튼 연동 상점 UI |
| | `GameOverResultUiView.cs` | `GameOverResultUiView` | 플레이어 사망 시 골드 자동 영구 정산, 런 통계창, [POWER UP SHOP] 및 [PLAY AGAIN] 연동 |
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
| | `SkillTuningUiView.cs [UPDATED]` | `SkillTuningUiView` | **🧪 전투 & 밸런스 샌드박스 (Combat Sandbox)** - 실시간 10종 스킬 + 3종 궁극기 튜닝, **💎 경험치 & 레벨업 시스템 튜닝**, **👾 7종 몬스터 + 보스 스탯**, **🎯 치명타 확률/배율 및 플레이어 코어 스탯 실시간 조절 및 프로젝트 내부 JSON 파일(`Assets/Config/skill_configs.json`) 영구 저장/GitHub 동기화 지원** |
| | `SkillTuningUiBuilder.cs [UPDATED]` | `SkillTuningUiBuilder` | 샌드박스 모드 UI 요소 생성 전담 헬퍼 (16개 메인 탭 + 몬스터 8종 서브탭, 500줄 규칙 준수 모듈화) |
| | `SkillConfigRepository.cs [UPDATED]` | `SkillConfigRepository` | **📁 샌드박스 설정 파일 프로젝트 내부화** - `Assets/Config/skill_configs.json` 1순위 입출력 및 GitHub 동기화 보장 (AppData/PlayerPrefs 멀티 폴백 및 자동 마이그레이션) |
| | `skill_configs.json [NEW]` | `Assets/Config/skill_configs.json` | **깃허브 형상관리 연동 샌드박스 튜닝 설정 파일** (전체 스킬/진화/몬스터/경험치/크리티컬 커스텀 수치 보존) |
| | `SkillConfigModels.cs [UPDATED]` | `SkillConfigData`, `ExpConfig`, `CritStatConfig` | 전체 스킬/경험치/몬스터 스탯, **경험치 대비 몹 체력 배율(`MobHpScalingRatio`)**, **역경직(`EnableHitStop`, `HitStopDuration`, `HitStopSlowScale`)** 및 **플레이어 치명타/공격력/이속/방어력/쿨감 커스텀 스탯(`CritStatConfig`) 직렬화 모델** |
| | `MonsterTuningConfig.cs [NEW]` | `MonsterTuningConfigData`, `MonsterStatConfig` | 7종 일반 몬스터(슬라임/박쥐/해골/골렘/화염임프/독거미/흑기사) 및 보스 스탯 설정 데이터 모델 |
| | `SkillTuningMemoryCache.cs [NEW]` | `SkillTuningMemoryCache` | 스킬 테스트 모드에서 L1~L5 레벨 간 이동 시 각 레벨별로 튜닝한 수치(공격력, 쿨다운, 반경 등)를 메모리에 완벽 보존/복원하는 세션 캐시 관리자 |
| | `InGameHudView.cs` | `InGameHudView` | 1920x1080 반응형 CanvasScaler, 상단 EXP 바, HP/타이머/킬/골드 HUD, 6칸 스킬 인벤토리 |
| | `PlayerHealthBarView.cs` | `PlayerHealthBarView` | 플레이어 머리 위를 따라다니는 초경량 오버헤드 미니 체력바 (SpriteRenderer 기반 무할당) |
| | `PauseMenuUiView.cs` | `PauseMenuUiView` | ESC 일시정지 다이얼로그 (계속하기, ⚙️ 환경 설정, 다시 시작, 게임 종료) |
| | `GameOverResultUiView.cs` | `GameOverResultUiView` | 플레이어 사망 시 골드 정산, [다시 도전하기] 씬 리로드 |
| | `DamageTextView.cs` | `DamageTextManagerView`, `DamageTextView` | 이벤트 기반 무할당 32개 풀링 (GameSettings.ShowDamageText 옵션 적용) |
| **Player** | `PlayerView.cs [UPDATED]` | `PlayerView` | 클래스별 외형 3단 분기, **2.5D Blob Shadow 타원 그림자**, **Brotato 젤리 물리 모션**, **대검 베기 150도 풀 부채꼴 밀착 스윙 궤적 연동** |
| | `PlayerInputHandler.cs` | `PlayerInputHandler` | New Input System 기반 이동 입력 수신 및 도메인 전달 |
| **Monsters** | `MonsterView.cs [UPDATED]` | `MonsterView` | **2.5D Blob Shadow 타원 그림자**, **7종 몬스터 타입별 젤리 물리 모션** (FireImp 빠른 다트, ToxicSpider 크리피 스커틀, DarkKnight 중장갑 행진), **피격 Flash White** (오비탈 다단히트 HitStop 제거로 120fps 보장) |
| | `MonsterSpawnerView.cs [UPDATED]` | `MonsterSpawnerView` | **Phase 1→보스1→Phase 2(FireImp→ToxicSpider→DarkKnight 순차 합류)→보스2 웨이브 진화 시스템**, `BossLaserBeamManagerView` 라이프사이클 연동, 512개 풀링 |
| | `WavePhaseController.cs [NEW]` | `WavePhaseController` | **보스 격퇴 후 웨이브 페이즈 진화 컨트롤러** (Phase1/Phase2Wave1~3/Boss2Spawned 5단계, 페이즈별 몬스터 아키타입 롤 테이블 분리 관리) |
| **Projectiles** | `ProjectileView.cs [UPDATED]` | `ProjectileManagerView`, `ProjectileView` | **관통 화살(Piercing Arrow) 비주얼 대폭 개편: 32x10 날렵하고 얄쌍한 다이아몬드 화살촉 + 슬림 화살대 + V자 화살깃 신규 픽셀아트 적용, 1발~5발 레벨업 시에도 변색 없이 처음의 찬란한 황금빛 앰버 골드 일관 유지** (128개 사전 생성 Prewarm 및 0-Allocation 풀링) |
| | `SkillSpriteHelper.cs [UPDATED]` | `SkillSpriteHelper` | **`GetOrCreatePiercingArrowSprite` (32x10 날렵한 관통 화살)**, `GetOrCreateStormArrowSprite` (28x12 폭풍 화살), `GetOrCreateWindGlaiveSprite` (32x32 풍인), `GetOrCreateStormBlastSprite` 픽셀아트 생성기 |
| | `EnemyProjectileManagerView.cs` | `EnemyProjectileManagerView` | Struct 배열 0-Allocation 풀링(64개) 적용, 날렵한 뼈다귀 투사체 매니저 |
| | `GroundStompManagerView.cs [UPDATED]` | `GroundStompManagerView` | 전사 지면 강타 발동 시 **128x128 대지 파쇄 크레이터 & 실제 도메인 반경 100% 일치 림 경계선**, 12개 포물선 암석 파편 & 엠버 비산, 화면 진동 연동 뷰 매니저 |
| | `WhirlwindManagerView.cs [NEW]` | `WhirlwindManagerView` | 전사 휠윈드 발동 시 **128x128 360도 강철 소용돌이 3중 칼날 & 1080 deg/s 초고속 사이클론 스핀**, 바람 잔상 및 8개 윈드 스파크 비산 뷰 매니저 (16개 무할당 풀링) |
| | `ArrowRainManagerView.cs [UPDATED]` | `ArrowRainManagerView` | 궁수 화살비 & 스텔라 레인 발동 시 20~60발의 화살이 지면에 꽂히는 첫 프레임에 몬스터에게 1:1 즉시 대미지를 가하는 착탄 동기화 뷰 매니저 (**바닥 원형 인디케이터/무늬 0.6초 후 페이드아웃 및 깔끔한 조기 제거 적용**) |
| | `MagicSkillManagerView.cs [UPDATED]` | `MagicSkillManagerView` | 서리폭발/빙하샤드 및 **기가스톰 초고전압 중뇌격 플라즈마 기둥(0.72m) & 일반 체인라이트닝 썬더볼트(0.34m)** 뷰 매니저 |
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
