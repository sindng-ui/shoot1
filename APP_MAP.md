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
| **Events** | `AudioEvents.cs` | `SoundEffectType`, `PlaySoundEvent`, `PlayBgmEvent`, `StopBgmEvent` | 14종 SFX 및 BGM 재생 요청 도메인 이벤트 집합 |
| | `MagicEvents.cs [UPDATED]` | `FrostNovaExecutedEvent`, `ChainLightningExecutedEvent`, `FireballExplodedEvent`, `MeteorStrikeExecutedEvent [NEW]`, `MonsterShatteredEvent [NEW]` | 마법사 서리 폭발, 연쇄 번개, 화염구 폭발, 메테오 낙하, 빙결 파괴 도메인 이벤트 집합 |
| | `BossEvents.cs` | `BossSpawnedEvent`, `BossHealthUpdatedEvent`, `BossDiedEvent` | 보스 스폰/체력 변경/사망 이벤트 집합 |
| | `ChestEvents.cs` | `TreasureChestSpawnedEvent`, `TreasureChestOpenedEvent` | 보물상자 스폰 및 개봉 이벤트 집합 |
| | `EvolutionEvents.cs` | `SkillEvolvedEvent` | 스킬 진화 발생 이벤트 |
| | `PlayerEvents.cs` | `PlayerDamagedEvent`, `PlayerMovedEvent`, `PlayerSlashExecutedEvent` | 플레이어 관련 이벤트 및 칼 휘두르기 궤적/각도/사거리 실행 이벤트 |
| | `MonsterEvents.cs` | `MonsterSpawnedEvent`, `MonsterDiedEvent` 등 | 몬스터 관련 이벤트 집합 |
| | `LevelEvents.cs` | `PlayerLevelUpEvent`, `ExpGainedEvent` | 경험치 및 레벨업 이벤트 |
| | `SessionEvents.cs` | `GameStateChangedEvent`, `SurvivalTimeUpdatedEvent`, `KillCountUpdatedEvent`, `GoldGainedEvent` | 세션 및 상태 전이 관련 도메인 이벤트 집합 |
| | `EventBus.cs` | `EventBus` | 제네릭 타입 기반의 고성능 도메인 이벤트 버스 |
| **Meta** | `MetaShopManager.cs` | `MetaShopManager` | 영구 강화 구매/100% 무료 환불, 골드 적립, 세이브 직렬화 관리자 |
| | `MetaUpgradeDefinition.cs` | `MetaUpgradeDefinition`, `MetaUpgradeSaveData` | 8종 영구 강화 항목 정의 및 세이브 데이터 구조체 |
| | `MetaUpgradeApplier.cs` | `MetaUpgradeApplier` | 세이브 데이터를 읽어 플레이어 시작 스탯에 영구 증강 적용 |
| | `ISaveStorage.cs` | `ISaveStorage` | 영구 저장소 로컬/클라우드 입출력 추상화 인터페이스 |
| **Skills & Passives** | `OrbitingBladesEffect.cs` | `OrbitingBladesEffect` | [전 클래스 공통] 플레이어 주위를 원형 회전하며 충돌 몬스터들에게 지속 물리 대미지를 입히는 오비탈 무기 |
| | `SkillRewardManager.cs [UPDATED]` | `SkillRewardManager`, `SkillRewardOption`, `PassiveDefinition` | 클래스별 전용 무기 3종, 공통 오비탈 무기, 8종 패시브(발화의 불꽃, 과전류의 핵 추가), 진화 스킬 롤링 및 추천 관리자 |
| | `SkillEvolutionManager.cs` | `SkillEvolutionManager` | 8레벨 무기 + 패시브 결합 시 진화 조건 검증 및 스킬 교체 (`SkillEvolvedEvent` 발행) |
| | `SkillEvolutionRecipe.cs` | `SkillEvolutionRecipe` | 대검+이빨->블러드 이터, 활+깃털->스톰 보우, 화염구+룬->메테오 스트라이크 레시피 |
| | `GreatswordSlashEffect.cs` | `GreatswordSlashEffect` | [전사 전용] 전방 150도 부채꼴 궤적 판정 및 `PlayerSlashExecutedEvent` 발행 |
| | `WhirlwindEffect.cs` | `WhirlwindEffect` | [전사 전용] 플레이어 주변 360도 전방위 회전 검기 연속 타격 스킬 |
| | `GroundStompEffect.cs` | `GroundStompEffect` | [전사 전용] 발로 지면을 구르고 지진 충격파로 근접 적 타격 |
| | `PiercingArrowEffect.cs` | `PiercingArrowEffect` | [궁수 전용] 화면 끝까지 무제한 관통하며 날아가는 초고속 화살 발사 |
| | `MultiShotEffect.cs` | `MultiShotEffect` | [궁수 전용] 전방 부채꼴로 다수의 관통 화살을 일제 발사하는 속사 사격 |
| | `ArrowRainEffect.cs` | `ArrowRainEffect` | [궁수 전용] 적 군집 상공에서 2.0초 동안 32발의 화살이 쏟아지는 광역 물리 화살 폭격 |
| | `FireballEffect.cs [UPDATED]` | `FireballEffect` | [마법사 전용] 대상 적을 향해 날아가 충돌 시 반경 내 광역 스플래시 폭발 + 7초 화염 DoT(Burn) 부여 |
| | `FrostNovaEffect.cs [UPDATED]` | `FrostNovaEffect` | [마법사 전용] 플레이어 주변 360도 전방위로 차가운 냉기 파동 방출 + 3.5초간 오한(Chill 40% 감속) 부여 |
| | `ChainLightningEffect.cs [UPDATED]` | `ChainLightningEffect` | [마법사 전용] 가장 가까운 적 감전 후 최대 4마리 연속 번개 전이 + 7초 감전 DoT(Shock) 부여 |
| | `BloodEaterEffect.cs` | `BloodEaterEffect` | [진화] 대검 + 뱀파이어 이빨 -> 흡혈 대검 베기 |
| | `StormArrowEffect.cs` | `StormArrowEffect` | [진화] 활 + 깃털 -> 8방향 연사 폭풍 화살 |
| | `MeteorStrikeEffect.cs [UPDATED]` | `MeteorStrikeEffect` | [진화] 화염구 + 마나 룬 -> 하늘에서 거대한 메테오 낙하 쾅! 광역 대폭발 + 7초 화염 DoT |
| **Entities** | `PlayerEntity.cs` | `PlayerEntity`, `ISpatialEntity` | 플레이어 도메인 로직 (이동, 피격, 체력 재생, 패시브 레벨 관리, 스킬 실행) |
| | `CharacterStats.cs` | `CharacterStats` | 이동속도, 공격력, 방어력, 크리티컬, 쿨감, 범위, 자석 등 종합 스탯 및 대미지 경감 공식 |
| | `MonsterType.cs` | `MonsterType`, `MonsterDefinition` | 슬라임(근접), 박쥐(비행 스웜), 해골(원거리 카이팅), 골렘(헤비 탱커), 보스(대형 레이드) |
| | `MonsterEntity.cs [UPDATED]` | `MonsterEntity` | 몬스터 도메인 로직 (오한 40% 감속 이동, 7초 화염/감전 DoT 틱 데미지, 사망 시 빙결 파괴 이벤트) |
| | `MonsterSpawner.cs` | `MonsterSpawner` | 몬스터 풀링 스폰, 플레이어 충돌 공격 업데이트, 웨이브 스케일링, 보스 소환 |
| | `PlayerClassFactory.cs` | `PlayerClassFactory`, `CharacterClassType` | 전사(Warrior), 궁수(Ranger), 마법사(Wizard) 3영웅 스탯 및 고유 시작 무기 팩토리 |
| **Chests** | `TreasureChestEntity.cs` | `TreasureChestEntity` | 보스 처치 시 드랍되는 황금 보물상자 |
| | `TreasureChestManager.cs` | `TreasureChestManager` | 보물상자 풀링, `BossDiedEvent` 시 자동 생성, 플레이어 충돌 시 오픈 |
| **Session** | `GameSessionEntity.cs` | `GameSessionEntity` | 게임 세션 생명주기, 생존 시간 틱, 킬 수, 획득 골드, 레벨 관리 |
| **Spatial** | `SpatialGrid2D.cs` | `SpatialGrid2D<T>`, `ISpatialGrid2D` | 대규모 엔티티를 위한 2D 공간 분할 해시 그리드 (O(1) 근접/범위 검색) |
| **Gems** | `ExpGemEntity.cs` | `ExpGemEntity` | 경험치 젬 도메인 데이터 (위치, 경험치량, 자석 이동 가속도) |
| | `GemManager.cs` | `GemManager` | 젬 풀링, 플레이어 흡수 반경 체크 및 자동 견인 흡수 |
| **Leveling** | `LevelSystem.cs` | `LevelSystem` | 경험치 누적, 레벨업 계산 곡선, 레벨업 이벤트 |
| **Waves** | `WaveStep.cs` | `WaveStep` | 시간대별 몬스터 타입, 스폰 간격, 스펙 배율 정의 데이터 |
| | `WaveTimelineManager.cs`| `WaveTimelineManager` | 게임 경과 시간 기반 타임라인 스텝 자동 전환 관리 |
| **Projectiles** | `ProjectileEntity.cs` | `ProjectileEntity` | 투사체 이동, 관통 카운트, 수명 주기, 적 충돌 판정 |
| | `ProjectileManager.cs` | `ProjectileManager` | 투사체 풀링 관리 및 공간 그리드 기반 충돌 체크 |
| **UI/Text** | `DamageTextEntity.cs` | `DamageTextEntity` | 대미지/크리티컬 플로팅 텍스트 수명 및 부유 좌표 계산 |
| | `DamageTextManager.cs` | `DamageTextManager` | 텍스트 엔티티 생성 및 풀링 관리 |
| **Settings** | `GameSettings.cs` | `GameSettings` | 조준 모드, 볼륨, 뮤트, UI 스케일, 데미지 텍스트, 전체화면 모델 및 PlayerPrefs 저장소 |
| **Pool** | `ObjectPool.cs`, `IPoolable.cs` | `ObjectPool<T>` | 무할당(Zero-allocation) 도메인 객체 풀러 |

---

### 2. 🎮 Unity Presentation Layer (`Assets/src/HappyShoot.View`)

| 카테고리 | 파일명 | 주요 컴포넌트 | 설명 |
| :--- | :--- | :--- | :--- |
| **Audio** | `ProceduralAudioHelper.cs` | `ProceduralAudioHelper` | 수학적 파형 합성을 통한 14종 SFX 및 아케이드 칩튠 BGM 프로시저럴 클립 생성기 |
| | `SoundManagerView.cs` | `SoundManagerView` | 16채널 AudioSource 풀링, 사운드 디바운싱, BGM 루프 재생, 도메인 이벤트 리액티브 오디오 |
| **Bootstrap** | `GameBootstrap.cs [UPDATED]` | `GameBootstrap` | 마스터 부트스트랩 (카메라, 3영웅, 사운드 매니저, 마법 스킬 매니저, 메테오 매니저, 영구 상점, 세션, UI 일괄 생성 및 연동) |
| **Shop** | `MetaShopUiView.cs` | `MetaShopUiView`, `JsonPlayerPrefsStorage` | 8종 영구 강화 카드 목록, 보유 골드 실시간 표시, 100% 무료 환불 버튼 연동 상점 UI |
| | `GameOverResultUiView.cs` | `GameOverResultUiView` | 플레이어 사망 시 골드 자동 영구 정산, 런 통계창, [POWER UP SHOP] 및 [PLAY AGAIN] 연동 |
| **Skills & Evolutions** | `OrbitingBladeView.cs` | `OrbitingBladeView` | 플레이어 주위를 원형 회전하는 공통 오비탈 칼날 시각화 (실시간 생성 및 칼날 수/반경 동기화) |
| | `EvolutionPopupView.cs` | `EvolutionPopupView` | 스킬 진화 성공 시 상단에 등장하는 축하 배너 팝업 및 자동 생성 |
| | `LevelUpUiView.cs` | `LevelUpUiView` | 레벨업 시 320x460 대형 카드 및 80x80 픽셀아트 아이콘, 한글 폰트 적용 3지선다 보상 선택 UI |
| **Boss & Chests** | `BossHealthBarView.cs` | `BossHealthBarView` | 화면 상단 슬림 보스 HP 바(1920x1080 반응형 앵커), 타이머와 분리된 상단 배치 |
| | `TreasureChestView.cs` | `TreasureChestView` | 필드에 스폰된 황금 보물상자 렌더링 및 펄스 반짝임 애니메이션 |
| | `TreasureChestManagerView.cs`| `TreasureChestManagerView` | 도메인 보물상자 매니저 업데이트 및 뷰 풀링 (상자 오픈/이벤트 종료 시 즉시 필드 디스폰) |
| | `TreasureChestPopupView.cs` | `TreasureChestPopupView` | 상자 획득 시 1~3개 스킬 보상 및 골드 획득 연출 팝업 (한글 폰트 적용) |
| **UI** | `SettingsDialogUiView.cs` | `SettingsDialogUiView` | 3개 탭 종합 환경 설정 모달 다이얼로그 (자동/수동조준, 볼륨, UI스케일) |
| | `CharacterSelectUiView.cs` | `CharacterSelectUiView` | 영웅 선택창 (전사/궁수/마법사 3영웅 카드 가로 배치, 한글 폰트, 스탯/스킬 설명 및 [⚙️ 게임 환경 설정] 버튼) |
| | `InGameHudView.cs` | `InGameHudView` | 1920x1080 반응형 CanvasScaler, 상단 EXP 바, HP/타이머/킬/골드 HUD, 6칸 스킬 인벤토리 |
| | `PlayerHealthBarView.cs` | `PlayerHealthBarView` | 플레이어 머리 위를 따라다니는 초경량 오버헤드 미니 체력바 (SpriteRenderer 기반 무할당) |
| | `PauseMenuUiView.cs` | `PauseMenuUiView` | ESC 일시정지 다이얼로그 (계속하기, ⚙️ 환경 설정, 다시 시작, 게임 종료) |
| | `GameOverResultUiView.cs` | `GameOverResultUiView` | 플레이어 사망 시 골드 정산, [다시 도전하기] 씬 리로드 |
| | `DamageTextView.cs` | `DamageTextManagerView`, `DamageTextView` | 이벤트 기반 무할당 32개 풀링 (GameSettings.ShowDamageText 옵션 적용) |
| **Player** | `PlayerView.cs` | `PlayerView` | 클래스별 외형(전사 은빛갑옷/대검 vs 궁수 그린후드/활 vs 마법사 퍼플로브/지팡이) 3단 분기 렌더링 |
| | `PlayerInputHandler.cs` | `PlayerInputHandler` | New Input System 기반 이동 입력 수신 및 도메인 전달 |
| **Monsters** | `MonsterView.cs [UPDATED]` | `MonsterView` | Transform 캐싱 및 0-Allocation 경량 렌더링, 오한(청록)/화염(주황)/감전(황금) 상태이상 실시간 비주얼 틴팅 |
| | `MonsterSpawnerView.cs` | `MonsterSpawnerView` | 512개 MonsterView 사전 생성 Prewarm (화면 밖 21m 안전 스폰, 박쥐 밸런싱) |
| **Projectiles** | `ProjectileView.cs` | `ProjectileManagerView`, `ProjectileView` | 128개 사전 생성 Prewarm 및 이벤트 기반 스폰, 매 프레임 순회 제거 |
| | `EnemyProjectileManagerView.cs` | `EnemyProjectileManagerView` | Struct 배열 0-Allocation 풀링(64개) 적용, 날렵한 뼈다귀 투사체 매니저 |
| | `GroundStompManagerView.cs` | `GroundStompManagerView` | 전사 지면 강타 발동 시 대지 균열, 8방향 비산하는 암석 파편, 화면 진동 연동 뷰 매니저 |
| | `ArrowRainManagerView.cs` | `ArrowRainManagerView` | 궁수 화살 비 발동 시 2.0초 동안 하늘에서 32발의 화살이 쏟아져 꽂히는 집중 폭격 뷰 매니저 |
| | `MagicSkillManagerView.cs [UPDATED]` | `MagicSkillManagerView` | 서리 폭발 파동 팽창, 지그재그 전격 아크 렌더링, 얼음 사망 시 산산조각 깨짐(Ice Shatter) 파편 풀링 뷰 매니저 |
| | `MeteorStrikeManagerView.cs [NEW]` | `MeteorStrikeManagerView` | 하늘 상공에서 급강하하는 거대한 불타는 운석, 착지 순간 초대형 폭발 구체, 충격파 링, 10개 화염 파편 및 화면 진동 연출 매니저 |
| **Gems** | `ExpGemView.cs` | `GemManagerView`, `ExpGemView` | 512개 ExpGemView 사전 생성 Prewarm 및 이벤트 기반 1회 바인딩으로 대량 흡수 시 무할당 |
| **Timeline** | `WaveTimelineView.cs` | `WaveTimelineView` | 경과 시간 기반 도메인 WaveTimeline 갱신 |
| **Camera** | `CameraFollowView.cs` | `CameraFollowView` | 9.0f 와이드 광시야각, 부드러운 추적 및 지진/메테오/타격 시 화면 진동 효과 |
| **Utils** | `WizardSpriteHelper.cs [UPDATED]` | `WizardSpriteHelper` | 마법사 캐릭터, 지팡이, 화염구, 서리 결정, 지그재그 번개, 메테오 운석, 얼음 샤터 파편 및 패시브 아이콘 2종 프로시저럴 생성기 |
| | `FontHelper.cs` | `FontHelper` | OS 한글 폰트 동적 로더 및 전역 폰트 제공 헬퍼 |
| | `RewardIconHelper.cs [UPDATED]` | `RewardIconHelper` | 18종 무기/패시브/진화 전용 80x80 고해상도 픽셀아트 아이콘 생성기 |
| | `SkillSpriteHelper.cs` | `SkillSpriteHelper` | 대검 검기, 대지 균열 지진, 뼈다귀 투사체 등 스킬 전용 픽셀아트 생성기 |
| | `SpriteHelper.cs` | `SpriteHelper` | 전사/궁수, 검/활, 보석, UI 단색 스프라이트 등 공통 픽셀아트 생성 |
| | `MonsterSpriteHelper.cs` | `MonsterSpriteHelper` | 박쥐, 스켈레톤, 골렘, 보스, 보물상자 전용 프로시저럴 스프라이트 생성 |

---

### 3. 🧪 Tests Layer (`Assets/tests/HappyShoot.Domain.Tests`)
*총 101개 NUnit 단위 테스트 스위트 (100% ALL PASS)*
- `StatusEffectTests.cs [NEW]`: 오한 40% 감속 검증, 오한 만료 시 정상 속도 복구, 7초 화염 DoT 틱 누적, 7초 감전 DoT 틱 누적, 오한 사망 시 `MonsterShatteredEvent` 발행, 메테오 스트라이크 이벤트 발행 및 화염 DoT 검증 6개 테스트
- `WizardSkillsTests.cs`: 화염구 스플래시 판정 및 데미지, 서리 폭발 360도 전방위 적 피격, 연쇄 번개 4회 전이 타격, 마법사 팩토리 스탯, 레벨업 보상 롤링 및 타 클래스 스킬 배제 5개 테스트
- `GreatswordSlashTests.cs`: 전방 150도 궤적 적 피격, 궤적 반대편 적 무피격, 사거리 밖 적 무피격, `PlayerSlashExecutedEvent` 및 사운드 이벤트 발행 4개 테스트
- `AudioEventsTests.cs`: 사운드 및 BGM 도메인 이벤트 발행, 수신, 페이로드 정합성 3개 테스트
- `MetaShopTests.cs`: 골드 추가, 업그레이드 레벨별 구매/골드 차감, 최대 레벨 초과 구매 방지, 100% 환불 골드 계산 5개 테스트
- `MetaSaveDataTests.cs`: 업그레이드 데이터 직렬화 및 `MetaUpgradeApplier` 스탯 반영 공식 1개 테스트
- `OrbitingBladesTests.cs`: 오비탈 궤도 위치 계산 및 충돌 다중 타격 2개 테스트
- `PassiveItemsTests.cs`: 6종 패시브 스탯 누적, 최대 레벨(5Lv) 제외 2개 테스트
- `SkillEvolutionTests.cs`: 3대 진화 레시피 합성, 진화 카드 우선순위 추천 4개 테스트
- `MonsterVarietyTests.cs`: 4종 아키타입 스탯, 해골 원거리 카이팅 AI, 보스 스폰/피격/사망 이벤트 4개 테스트
- `TreasureChestTests.cs`: 상자 스폰, 접근 오픈 및 보상 지급, 보스 사망 시 상자 자동 드랍 3개 테스트
- `GameSessionTests.cs`: 세션 생명주기, 시간 틱, 킬 수/골드 누적, 일시정지, 게임오버/승리 13개 테스트
- `PlayerEntityTests`, `MonsterEntityTests`, `MonsterSpawnerTests`, `CharacterClassTests`
- `SkillCompositionTests`, `SpatialGridTests`, `LevelSystemTests`
- `SkillRewardTests`, `ExpGemTests`, `ProjectileTests`, `WaveTimelineTests`
- `DamageTextTests`, `EventBusTests`, `TimeProviderTests`
