# 🌐 HappyShoot Domain Layer Map (`DOMAIN_MAP`)

[🔙 메인 APP_MAP으로 돌아가기](../../APP_MAP.md)

> **경로**: `Assets/src/HappyShoot.Domain`  
> **특징**: Unity 엔진 의존성 0 (`noEngineReferences: true`), 순수 C# 기반의 고속 시뮬레이션 및 100% 독립 단위 테스트 보장

---

## 🏛️ 도메인 계층 구조 요약

- **Entities**: 플레이어, AI 동료, 몬스터, 보스, 발사체, 보석 등 순수 게임 월드 데이터 및 비즈니스 모델
- **Events**: 계층 간 결합도를 제로화하는 경량 불변 이벤트 (`EventBus` 발행)
- **Skills & Passives**: 3영웅 무기 스킬, 9대 스킬 진화 합성, 9종 패시브 계산
- **Progression & Skill Tree**: 360° 대마법사 비전 성좌, 3종 보석 및 골드 경제 시스템
- **Magic Forge**: 12종 룬 인스크립션, 제로할당 수정자(`RuneModifiers`), 영구 저장
- **Spatial & Pool**: 0-GC 공간 분할 해시 그리드(`SpatialGrid2D`) 및 제네릭 무할당 풀러

---

## 📂 파일 및 클래스 세부 명세

### 1. Entities & Characters
| 카테고리 | 파일명 | 주요 클래스 / 인터페이스 | 설명 |
| :--- | :--- | :--- | :--- |
| **Player** | `PlayerEntity.cs` | `PlayerEntity`, `ISpatialEntity` | 플레이어 순수 도메인 엔티티. 이동, 스탯/패시브 보유, `AttackPowerMultiplier` 배율과 `SkillContext.BaseDamage` 실시간 동기화, 피격 히트박스 반경(0.38f), 크리티컬 롤러(`RollDamage`) 지원 |
| **Stats** | `CharacterStats.cs` | `CharacterStats` | 기본 크리티컬 확률 10%(0.10f), 치명타 배율(1.5x), 이동속도, 공격력, 방어력, 쿨다운 감소 등 캐릭터 종합 스탯 모델 |
| **Factory** | `PlayerClassFactory.cs` | `PlayerClassFactory`, `CharacterClassType` | 마법사 생성 시 `startSkillId`(화염구/서리폭발/연쇄번개)에 따른 시작 스킬 분기 및 영웅별 기본 크리율(전사/마법사 10%, 궁수 20%) 설정 |
| **Companions** | `CompanionEntity.cs` | `CompanionEntity`, `CompanionType` | AI 동료 순수 도메인 엔티티. 마법사 성장 동기화, 클래스 고유 스킬 풀(전사 3종, 궁수 3종), 패시브 1/3 효과 적용, 샌드박스 연동 최종 1/3 데미지 산출 |
| | `CompanionSkillInstance.cs` | `CompanionSkillInstance` | 컴패니언 개별 스킬 인스턴스 (Lv.1~5 레벨 및 독립 쿨다운 타이머 관리) |
| **Monsters** | `MonsterEntity.cs` | `MonsterEntity` | 몬스터 순수 도메인 엔티티. 이동 AI, 피격(`TakeDamage`), 상태이상(오한, 화상, 감전) 타이머, 횡스크롤 차원 모드 전용 수평 전진 및 파동 비행(`IsSideScrollMode`, `SideScrollBaseY`, `SideScrollWaveAmplitude`) 지원 |
| | `MonsterType.cs` | `MonsterType`, `MonsterDefinition` | 7종 일반 몬스터(Slime, Bat, Skeleton, Golem, FireImp, ToxicSpider, DarkKnight) + 4종 Phase 3 몬스터(망령, 사령술사, 어보미네이션, 사신) + 3대 보스(골렘 킹, 대악마, 사령왕 리치) 정의 |
| | `MonsterSpawner.cs` | `MonsterSpawner` | 도메인 몬스터 스포너. 1,280 오브젝트 풀링 및 SpatialGrid2D 공간분할 쿼리 |
| **Gems** | `GemStoneEntity.cs` | `GemStoneEntity` | 보석 3종(루비/에메랄드/아메시스트) 필드 드랍 엔티티 (0-GC 풀링, 자석 흡수) |
| | `GemManager.cs` | `GemManager` | 경험치 보석 + 영구 성장 보석 통합 풀링 관리자 (일반몹 1% 드랍, 보스 확정 5개 드랍) |
| **Projectiles** | `ProjectileEntity.cs` | `ProjectileEntity` | 투사체 관통 적중 및 미니 AoE 폭발 시 개별 크리티컬 롤링 판정 지원 |
| | `ProjectileManager.cs` | `ProjectileManager` | 투사체 생성 및 발사자의 `CritChance`, `CritDamageMultiplier` 주입 관리 |
| **Chests** | `TreasureChestEntity.cs` | `TreasureChestEntity` | 필드에 스폰된 황금 보물상자 도메인 엔티티 |
| | `TreasureChestManager.cs`| `TreasureChestManager` | 보물상자 스폰 및 개봉 비즈니스 로직 |

---

### 2. Events (`EventBus` 기반 발행/구독)
| 파일명 | 주요 이벤트 정의 | 설명 |
| :--- | :--- | :--- |
| `CompanionEvents.cs` | `CompanionRewardSyncEvent` | 마법사 보상(액티브 획득, 레벨업, 패시브) 선택 시 동료 성장 실시간 동기화 |
| `GemStoneEvents.cs` | `GemStoneDroppedEvent`, `GemStoneCollectedEvent` | 보석 3종 필드 드랍 및 자석 수집 알림 도메인 이벤트 |
| `AudioEvents.cs` | `PlaySoundEvent`, `PlayBgmEvent`, `StopBgmEvent`, `SoundEffectType` | 16종 SFX 및 BGM 재생 요청 도메인 이벤트 |
| `MagicEvents.cs` | `FrostNovaExecutedEvent`, `ChainLightningExecutedEvent`, `FireballExplodedEvent`, `MeteorStrikeExecutedEvent`, `MonsterShatteredEvent` | 마법사 마법 발동 및 폭발/빙결 파쇄 도메인 이벤트 집합 |
| `PlayerEvents.cs` | `PlayerDamagedEvent`, `PlayerMovedEvent`, `PlayerSlashExecutedEvent` | 플레이어 이동, 피격 및 칼 휘두르기 궤적/각도/사거리 실행 이벤트 |
| `MonsterEvents.cs` | `MonsterDamagedEvent`, `DamageType` (Default, Arrow, WindGlaive, StellarRain, Fireball, BurnDot, ShockDot, Ice, Lightning), `MonsterSpawnedEvent`, `MonsterDiedEvent` | 몬스터 피격/스폰/사망 이벤트 및 속성별 대미지 타입 열거형 |
| `DamageTextEntity.cs` | `DamageTextEntity` | Pure C# 부유 대미지 숫자 엔티티 (데미지 수치, 크리티컬 여부, DamageType 속성, 수명, 알파 페이드) |
| `DamageTextManager.cs` | `DamageTextManager` | 0-Alloc 오브젝트 풀링 기반 부유 대미지 텍스트 스폰, 위치 오프셋, 수명 수명주기 관리자 |
| `BossEvents.cs` | `BossSpawnedEvent`, `BossHealthUpdatedEvent`, `BossDiedEvent` | 보스 스폰/체력 갱신/처치 이벤트 |
| `ChestEvents.cs` | `TreasureChestSpawnedEvent`, `TreasureChestOpenedEvent` | 보물상자 스폰 및 보상 지급 이벤트 |
| `EvolutionEvents.cs`| `SkillEvolvedEvent` | 9대 스킬 진화 발생 알림 이벤트 |
| `LevelEvents.cs` | `PlayerLevelUpEvent`, `ExpGainedEvent` | 경험치 획득 및 레벨업 이벤트 |
| `SessionEvents.cs` | `GameStateChangedEvent`, `SurvivalTimeUpdatedEvent`, `KillCountUpdatedEvent`, `GoldGainedEvent` | 게임 상태 및 타이머/통계 이벤트 |

---

### 3. Skills, Passives & Evolutions
| 카테고리 | 파일명 | 클래스 / 인터페이스 | 설명 |
| :--- | :--- | :--- | :--- |
| **System** | `CompositeSkill.cs` | `CompositeSkill` | 룬 모디파이어(`RuneModifiers`)와 연동된 스킬 조합 엔진 (쿨다운, 공격력, 관통, 크리티컬) |
| | `SkillRewardManager.cs` | `SkillRewardManager`, `SkillRewardOption`, `PassiveDefinition` | 클래스별 3종 무기, 공통 오비탈, 9종 패시브, 진화 스킬 3지선다 롤링 및 추천 엔진 |
| | `SkillEvolutionManager.cs` | `SkillEvolutionManager` | Lv.5 무기 + 필수 패시브 결합 시 진화 조건 검증 및 스킬 교체 |
| | `SkillEvolutionRecipe.cs` | `SkillEvolutionRecipe` | 9대 스킬 진화 레시피 불변 정의 |
| | `SkillRegistryHelper.cs` | `SkillRegistryHelper` | 스킬, 패시브, 9대 진화 레시피 등록 전담 모듈 |
| **Wizard** | `FireballEffect.cs` | `FireballEffect` | [마법사] 초고속 혜성 화염구 투사체 발사 및 착탄 순간 1회 단일 폭발 + 대미지 동기화 |
| | `FrostNovaEffect.cs` | `FrostNovaEffect` | [마법사] 360도 전방위 냉기 파동 및 빙결/오한 상태이상 부여 |
| | `ChainLightningEffect.cs`| `ChainLightningEffect` | [마법사] 4마리 적 순차 연쇄 전이 전기 타격 및 감전 DoT |
| | `MeteorStrikeEffect.cs` | `MeteorStrikeEffect` | [마법사 궁극기] 초광역 메테오 낙하 (공격력 220, 반경 7.5m, 7초 화상 DoT) |
| | `GigastormLightningEffect.cs` | `GigastormLightningEffect` | [마법사 진화 2] 연쇄번개 + 과전류의 핵 $\rightarrow$ 10마리 순차 전이 + 35% 플라즈마 방전 폭발 + 100% 감전 |
| | `BlizzardNovaEffect.cs` | `BlizzardNovaEffect` | [마법사 진화 3] 서리폭발 + 생명의 펜던트 $\rightarrow$ 2중 팽창 서리 파동 + 8방향 고드름 파편 |
| **Warrior** | `GreatswordSlashEffect.cs` | `GreatswordSlashEffect` | [전사] 전방 30°~360° 부채꼴 궤적 판정 및 크리티컬 대미지 롤링 연동 |
| | `WhirlwindEffect.cs` | `WhirlwindEffect` | [전사] 360도 전방위 회전 검기 연속 크리티컬 롤링 연동 |
| | `GroundStompEffect.cs` | `GroundStompEffect` | [전사] 디아블로4 야만용사 [지각변동(Upheaval)] 스타일 방향성 연쇄 지진 충격파 |
| | `BloodEaterEffect.cs` | `BloodEaterEffect` | [전사 진화 1] 대검 + 뱀파이어 이빨 $\rightarrow$ 흡혈 대검 베기 (부채꼴 각도 커스텀 튜닝 연동) |
| | `TempestWhirlwindEffect.cs` | `TempestWhirlwindEffect` | [전사 진화 2] 휠윈드 + 바람의 깃털 $\rightarrow$ 2연속 초고속 사이클론 + 4방향 칼바람 참격 |
| | `EarthshakerEffect.cs` | `EarthshakerEffect` | [전사 진화 3] 지면강타 + 강철 갑옷 $\rightarrow$ 중심 마그마 크레이터 + 십자 4방향 3단 지진 균열 |
| **Ranger** | `PiercingArrowEffect.cs` | `PiercingArrowEffect` | [궁수] 무제한 관통 사격, 투사체별 개별 크리티컬 롤링 및 활시위 사운드 연동 |
| | `WindGlaiveEffect.cs` | `WindGlaiveEffect` | [궁수] 회전 풍인 관통 및 복귀 2중 타격 크리티컬 롤링 연동 |
| | `ArrowRainEffect.cs` | `ArrowRainEffect` | [궁수] 20~60발 집중 화살 착탄 즉시 1:1 대미지 동기화 |
| | `StormArrowEffect.cs` | `StormArrowEffect` | [궁수 진화 1] 활 + 깃털 $\rightarrow$ 폭풍 충격파 및 초고속 관통 폭풍살 |
| | `PhantomGlaiveEffect.cs` | `PhantomGlaiveEffect` | [궁수 진화 2] 글레이브 + 치명타의 눈 $\rightarrow$ 메인 글레이브 + 2개 나선형 환영 부메랑 2중 타격 |
| | `StellarRainEffect.cs` | `StellarRainEffect` | [궁수 진화 3] 화살비 + 황금 반지 $\rightarrow$ 2배 밀도 황금 유성 화살비 + 스타더스트 폭발 |
| **Common** | `OrbitingBladesEffect.cs`| `OrbitingBladesEffect` | [공통] 플레이어 주위를 공전하는 수호의 칼날 물리 판정 및 크리티컬 연동 |

---

### 4. Magic Forge & Runes (`Assets/src/HappyShoot.Domain/Forge`)
| 파일명 | 주요 클래스 / 모델 | 설명 |
| :--- | :--- | :--- |
| `RuneModifiers.cs` | `RuneModifiers` | 룬이 스킬에 부여하는 수치 변조 제로할당 구조체 (피해/쿨다운/범위 배율, 투사체/관통 증가, 흡혈, 연쇄, 처치폭발, 무료시전, 공명) |
| `RuneDefinition.cs` | `RuneDefinition`, `RuneGrade` | 12종 룬 불변 정의 모델 (등급, 해금비용, 기본수정자, 레벨당 스케일링, 주 보석 유형, 레벨업 비용 연산) |
| `RuneRegistry.cs` | `RuneRegistry` | 12종 룬(일반 4종, 희귀 4종, 전설 4종) 등록소 |
| `RuneManager.cs` | `RuneManager` | 룬 인스크립션 비즈니스 로직 매니저 (해금/강화/스킬슬롯 장착/해제 및 스킬별 최종 수정자 연산) |
| `ForgeSaveData.cs` | `ForgeSaveData`, `SerializableStringDict` | 마법 대장간 영구 저장 데이터 모델 (룬 레벨, 스킬 슬롯 바인딩, 결정체 레벨/장착, 재련 해금) |

---

### 5. Progression & Skill Tree (`Assets/src/HappyShoot.Domain/Progression`)
| 파일명 | 주요 클래스 / 모델 | 설명 |
| :--- | :--- | :--- |
| `GemType.cs` | `GemType`, `BranchType`, `GemTypeExtensions` | 영구 성장 재화 보석 3종(Ruby=0, Emerald=1, Amethyst=2) 및 3원소 분기(화염/빙결/전격) 열거형 |
| `SkillTreeNodeDef.cs` | `SkillTreeNodeDef`, `NodeEffectType` | GoldCost 필드(150G~1,500G) 및 18개 마법사 성좌 노드 불변 데이터 모델 |
| `SkillTreeSaveData.cs` | `SkillTreeSaveData`, `SerializableDict` | 성좌 스킬 트리 세이브 데이터 (클리어 횟수, 골드, 루비/에메랄드/아메시스트 지갑) |
| `PlayerProgressionFlags.cs` | `PlayerProgressionFlags` | 0-GC 전투 시스템 참조용 속성 특화 효과 플래그 구조체 |
| `SkillTreeManager.cs` | `SkillTreeManager`, `ISkillTreeStorage` | 골드(Gold) 기반 노드 해금 및 50% 골드 환불 각성 리셋 매니저 |
| `SkillTreeRegistry.cs` | `SkillTreeRegistry` | 마법사 Only 전용 18개 노드 단독 등록 및 골드 비용 밸런싱 |
| `SkillTreeApplier.cs` | `SkillTreeApplier` | 해금된 성좌 노드를 CharacterStats 및 PlayerProgressionFlags로 변환 |

---

### 6. Meta Shop & Systems
| 파일명 | 주요 클래스 | 설명 |
| :--- | :--- | :--- |
| `MetaShopManager.cs` | `MetaShopManager` | (구 시스템 호환) 8종 영구 강화 구매 및 100% 무료 환불 관리자 |
| `MetaUpgradeDefinition.cs` | `MetaUpgradeDefinition`, `MetaUpgradeSaveData` | 8종 영구 강화 항목 정의 및 세이브 데이터 모델 |
| `MetaUpgradeApplier.cs` | `MetaUpgradeApplier` | 세이브 데이터를 읽어 플레이어 시작 스탯에 영구 증강 적용 |
| `SpatialGrid2D.cs` | `SpatialGrid2D<T>` | 2D 공간 분할 해시 그리드 (대규모 몬스터/보석/투사체 0-GC 고속 충돌 쿼리) |
| `ObjectPool.cs` | `ObjectPool<T>` | 제네릭 무할당 메모리 풀러 |
| `SkillConfigModels.cs` | `ExpConfig`, `SkillConfigData` | 경험치 및 몬스터 스탯 동적 스케일링 설정 직렬화 모델 |

---

[🔙 메인 APP_MAP으로 돌아가기](../../APP_MAP.md)
- [DashConfigData.cs](file:///k:/unityprojects/shoot1/shoot1/Assets/src/HappyShoot.Domain/Entities/DashConfigData.cs): 플레이어 대시 스킬 설정 데이터 모델 (쿨타임, 이동거리, 지속시간, 감속곡선 계수 등)
