# 🎮 HappyShoot Presentation View Layer Map (`VIEW_MAP`)

[🔙 메인 APP_MAP으로 돌아가기](../../APP_MAP.md)

> **경로**: `Assets/src/HappyShoot.View`  
> **특징**: Unity Engine Presentation 계층. 도메인 이벤트를 구독(Observe)하여 화면 렌더링, 픽셀아트 생성, 사운드 합성, 파티클 VFX, 반응형 UI 및 모바일 터치를 전담

---

## 🏛️ 뷰 계층 구조 요약

- **Bootstrap**: 전체 단일 진입점 (`GameBootstrap.cs`)
- **Player & Companions**: 마법사 시선/지팡이 스냅, 3인 원정대 호위 AI, 피격 쥬스(Juice)
- **Monsters & Bosses**: 7종 일반몹 + 4종 Phase3 언데드 + 3대 보스(골렘/대악마/사령왕 리치)
- **Projectiles, Skills & VFX**: 3영웅 고유 스킬 및 9대 진화 VFX, 0-GC 풀링, 타격 역경직
- **UI, HUD & Menus**: 인게임 3단 메인 HUD, 마법사 메인 메뉴, 360° 대마법사 성좌 UI, 대장간 모달
- **Combat Sandbox**: 6대 카테고리 밸런스 샌드박스, 핫리로드, Git 영구 동기화
- **Audio, Background & Mobile**: 프로시저럴 사운드, 3x3 무한 지형 타일링, 모바일 가상 조이스틱

---

## 📂 파일 및 컴포넌트 세부 명세

### 1. Bootstrap & Lifecycle
| 파일명 | 주요 컴포넌트 | 설명 |
| :--- | :--- | :--- |
| `GameBootstrap.cs` | `GameBootstrap` | 마스터 부트스트랩. 마법사 Only 모드 단일 진입점, 카메라, 사운드, 메테오, 보석 스킬트리, AI 동료 매니저, 마법 대장간 룬 주입 및 UI 라이프사이클 총괄 |

---

### 2. Player, Companions & Control
| 카테고리 | 파일명 | 주요 컴포넌트 / 헬퍼 | 설명 |
| :--- | :--- | :--- | :--- |
| **Player** | `PlayerView.cs` | `PlayerView` | 플레이어 외형 3단 분기, 스마트 하이브리드 시선 질주/조준, 대검 스윙 동적 궤적(-halfArc ~ +halfArc), 동료(11~14) 상위 레이어링(몸체 sortingOrder=16, 무기 15/17, 공격 피크 18), 피격 위임 분리 |
| | `PlayerHitFeedbackView.cs` | `PlayerHitFeedbackView` | 💥 피격 피드백(Juice): 화이트$\rightarrow$크림슨 레드 2단계 플래시, 스쿼시&바운스 찌그러짐, 펀치 미세 카메라 셰이크, 0-GC 2.5D 도트 피격 스파크 |
| | `PlayerDamageVignetteView.cs` | `PlayerDamageVignetteView` | 🩸 외곽 피격 비네트 UI: 피격 시 붉은 펄스 페이드아웃, HP 30% 이하 시 심장 박동 경고, Blur 배제 0-GC 절차적 Radial Gradient 1 DrawCall |
| | `PlayerHealthBarView.cs` | `PlayerHealthBarView` | 머리 위 오버헤드 미니 체력바 (SpriteRenderer 기반 0-할당, sortingOrder 20/21) |
| | `PlayerInputHandler.cs` | `PlayerInputHandler` | New Input System 기반 WASD/터치 이동 입력 수신 및 도메인 전달 |
| | `WizardWeaponPlacementHelper.cs` | `WizardWeaponPlacementHelper` | 🧙‍♂️ 마법사 8방향+정면/후면 지팡이 오른손 1:1 결합 스냅, 캐스팅 펄스 리프트, flipX 및 소팅오더(등 뒤 15 / 앞손 17) 완전 제어 |
| **Companions** | `CompanionView.cs` | `CompanionView` | AI 동료 시각화 및 전투 AI: 마법사 하위/몬스터 상위 레이어링(몸체 sortingOrder=12, 무기 11/13), 샌드박스 쿨다운/데미지 실시간 연동, 다중 스킬 순차 발동, 6m 정속 재합류, 젤리 보빙, 횡스크롤 독립 보행 및 동적 발판 높이 추적 |
| | `CompanionSlashEffect.cs` | `CompanionSlashEffect` | ⚔️ 전사 동료 대검 스윙 회전 및 절차적 슬래시 궤적 호(SlashArc) 비주얼 이펙트 전담 컴포넌트 |
| | `CompanionManagerView.cs` | `CompanionManagerView` | AI 동료 생명주기 관리: CompanionRewardSyncEvent 구독, 마법사 레벨업 시 동료 스킬 해금/레벨업/패시브 자동 동기화, 클리어 회차 기반 스폰 |
| | `CompanionSkillExecutor.cs` | `CompanionSkillExecutor` | 동료 스킬 실행 및 VFX 전담: 글레이브, 화살비, 지면강타, 휠윈드 전용 비주얼 연동 |
| | `CompanionSelectPreviewHelper.cs` | `CompanionSelectPreviewHelper` | 메인 메뉴 3인 원정대 프리뷰: 마법사 좌우 호위 전사/궁수 카드 렌더링, 미해금 시 실루엣 + 락 뱃지 |
| **Sprites** | `HeroSpriteHelper.cs` | `HeroSpriteHelper` | 고화질 9방향 스프라이트 우선 로드 및 32x32 치비 3영웅 절차적 픽셀아트 폴백 |
| | `CustomHeroSpriteLoader.cs` | `CustomHeroSpriteLoader` | 고해상도 커스텀 영웅 스프라이트 4단계 안전 로더 (전사 PPU 520f, 궁수 PPU 400f, 마법사 PPU 450f, `FilterMode.Point`) |

---

### 3. Monsters & Bosses
| 카테고리 | 파일명 | 컴포넌트 / 헬퍼 | 설명 |
| :--- | :--- | :--- | :--- |
| **Monsters** | `MonsterView.cs` | `MonsterView` | 7종 일반몹 + 2종 보스 고퀄리티 픽셀아트/명암/발광 코어, 2.5D Blob Shadow 타원 그림자, 젤리 물리 모션 |
| | `MonsterSpawnerView.cs` | `MonsterSpawnerView` | 몬스터 스폰 & 페이즈 관리, 도망 방향 120° 스폰 억제(후방/측면 90% 스폰), 몹 수 완만 스케일링, 경험치 증가분 대비 몹 체력 제곱근 감쇠 공식 및 상한선 클램핑, Phase 점프 |
| | `MonsterDeathFxManagerView.cs` | `MonsterDeathFxManagerView` | 암석 파편/형광 독즙/화염 불씨/영혼 가루/골드 룬 처치 미니 파티클 무할당 풀링(64개) |
| | `MonsterSpriteHelper.cs` | `MonsterSpriteHelper` | 7종 일반 몬스터 + 보스 2종 고해상도 셀 셰이딩/글레어/발광 코어/룬 픽셀아트 생성기 |
| | `CustomMonsterSpriteLoader.cs` | `CustomMonsterSpriteLoader` | 8종 몬스터(슬라임/박쥐/스켈레톤/임프/거미/다크나이트/골렘/리치왕) 고해상도 커스텀 스프라이트 4단계 안전 로더 (타입별 최적 PPU 600~750f, 피벗 자동 보정, FilterMode.Point, 100% 절차적 Fallback) |
| **Phases & Bosses** | `WavePhaseController.cs` | `WavePhaseController` | 보스 격퇴 후 웨이브 페이즈 진화 컨트롤러 (Phase1/Phase2Wave1~3/Boss2Spawned/Phase3) |
| | `WaveTimelineView.cs` | `WaveTimelineView` | 웨이브 진행 타임라인 시각화 |
| | `BossHealthBarView.cs` | `BossHealthBarView` | 화면 상단 슬림 보스 HP 바 (1920x1080 반응형 앵커) |
| | `BossHazardZoneManagerView.cs`| `BossHazardZoneManagerView` | 💥 보스 광역 장판 0-할당 풀링: 1.2s 전조 경고 링 $\rightarrow$ 2.0s 마그마 지옥불 장판(직경 5.6m) |
| | `ArchLichPatternController.cs` | `ArchLichPatternController` | 💀 최종 보스 3(사령왕 리치) 전용 3대 맹공 패턴: 8방향 나선 회전 영혼 탄막, 3연속 쐐기 암흑 참격파, 언데드 군단 소환 |
| | `Phase3MonsterSpriteHelper.cs` | `Phase3MonsterSpriteHelper` | 망령, 사령술사, 어보미네이션, 사신, 사령왕 리치, 저주 영혼탄 프로시저럴 스프라이트 생성기 |

---

### 4. Side-Scrolling Dimension Trial (New!)
| 카테고리 | 파일명 | 컴포넌트 / 헬퍼 | 설명 |
| :--- | :--- | :--- | :--- |
| **Dimension Mode** | `DimensionPortalView.cs` | `DimensionPortalView` | 🌀 3회차(궁수 해금) 보스3 격파 시 출현하는 시공간 균열 차원 포탈: 보라/시안 성운 회전 펄스, 근접 플레이어 흡수 스핀 & 화이트 플래시 전환 |
| | `SideScrollModeController.cs` | `SideScrollModeController` | 🚀 횡스크롤 모드 총괄 마스터: 카메라 Y축 고정, 수평 좌우 1D 이동, 300m 돌파 거리 HUD 게이지, 마법 오버드라이브(-50% CDR) 및 최종 승리 연계 |
| | `SideScrollBackgroundView.cs` | `SideScrollBackgroundView` | 🌌 3중 패럴랙스 차원 회랑 배경: 원경 네온 성운(0.15x), 중경 고대 룬 모놀리스(0.40x), 근경 빛나는 바닥 레일 무한 루프 |
| | `SideScrollMonsterSpawner.cs` | `SideScrollMonsterSpawner` | 👾 횡스크롤 우측 웨이브 스포너: 차원 슬라임/임프/가고일, 가속 링(Speed Ring), 보석 폭풍(Gem Storm), 300m 거대 차원 핵(Void Core) 보스전 |

---

### 4. Projectiles, Skills & Visual Effects
| 카테고리 | 파일명 | 컴포넌트 / 헬퍼 | 설명 |
| :--- | :--- | :--- | :--- |
| **Juice & VFX**| `SlashHitVfxManagerView.cs` | `SlashHitVfxManagerView` | ⚔️ 슬래시 타격 이펙트: 피격 시 대각선 슬래시 컷 스파크(0.10s) 32개 무할당 풀링 |
| | `CriticalHitVfxManagerView.cs`| `CriticalHitVfxManagerView` | 황금빛 십자 섬광 + 8방향 스타버스트 크리티컬 스파크 VFX 및 역경직 트리거 |
| | `HitStopManager.cs` | `HitStopManager` | 0-Allocation Update 루프 타이머 기반 초경량 타격 역경직 매니저 (찰진 20% 슬로우모션) |
| **Wizard Spells**| `FireballSkillManagerView.cs` | `FireballSkillManagerView` | [마법사] 초고속 혜성 화염구 비행 & 불씨 잔상, 128x128 다단 플라즈마 화염 폭발, 방사형 불티 비산 |
| | `MagicSkillManagerView.cs` | `MagicSkillManagerView` | [마법사] 서리폭발/빙하샤드, 기가스톰 1~3갈래 다중 번개 줄기 및 체인 플라즈마 빔 |
| | `MeteorStrikeManagerView.cs` | `MeteorStrikeManagerView` | [마법사 궁극기] 메테오 스트라이크: 마법진, 운석 화염 꼬리, 황금-주황 충격파 링, 12개 마그마 파편 비산 |
| | `WizardSkillSpriteHelper.cs` | `WizardSkillSpriteHelper` | 화염구 혜성/폭발구/불씨, 32x16 고전압 플라즈마 번개 빔, 16x16 십자 전기 스파크 생성기 |
| | `MagicSkillVisualModels.cs` | `MagicSkillVisualModels` | 서리 파동/번개 볼트/빙하 파편 풀링 인스턴스 데이터 모델 |
| **Warrior Skills**| `WhirlwindManagerView.cs` | `WhirlwindManagerView` | [전사] 128x128 360도 강철 소용돌이 3중 칼날 & 1080 deg/s 초고속 사이클론 스핀 |
| | `GroundStompManagerView.cs` | `GroundStompManagerView` | [전사] 💥 지각변동(Upheaval): 0.030s 간격 쐐기형 V자 지진파가 뻗어나가며 좌우 바위 슬래브가 들썩이는 연출 |
| | `UpheavalSpriteHelper.cs` | `UpheavalSpriteHelper` | 쐐기형 지진파 아크, 파쇄 바위 슬래브, 암석 가시 픽셀아트 생성기 |
| **Ranger Skills**| `ProjectileView.cs` | `ProjectileManagerView`, `ProjectileView` | [궁수] 관통 화살(Piercing Arrow): 32x10 날렵한 다이아몬드 화살촉 + 황금빛 앰버 골드 일관 유지 (128개 Prewarm) |
| | `WindGlaiveManagerView.cs` | `WindGlaiveManagerView` | [궁수] 윈드 글레이브 & 팬텀 글레이브 (부채꼴 환영 비산, 왕복 2타 판정, ReturnTarget 동적 바인딩) |
| | `ArrowRainManagerView.cs` | `ArrowRainManagerView` | [궁수] 화살비 & 스텔라 레인 20~60발 낙하 폭격 및 바닥 원형 인디케이터 페이드아웃 |
| | `StormBowManagerView.cs` | `StormBowManagerView` | [궁수 진화] 폭풍의 활 초고속 관통 폭풍살 및 충격파 뷰 매니저 |
| **Enemy & Common**| `EnemyProjectileManagerView.cs`| `EnemyProjectileManagerView`| 해골 뼈 화살 + 흑기사 보라색 암흑 마법 검기 0-할당 풀링 (64개) |
| | `OrbitingBladeView.cs` | `OrbitingBladeView` | 공통 수호의 검 회전 시각화 (칼날 수/반경 실시간 동기화) |
| | `EnemyAttackSpriteHelper.cs` | `EnemyAttackSpriteHelper` | 보라색 암흑 검기, 보스 전조 경고 링, 보스 마그마 장판 픽셀아트 생성기 |
| | `SkillSpriteHelper.cs` | `SkillSpriteHelper` | 관통화살, 폭풍화살, 풍인, 스톰블래스트 픽셀아트 생성기 |

---

### 5. UI, HUD & Navigation
| 카테고리 | 파일명 | 컴포넌트 / 헬퍼 | 설명 |
| :--- | :--- | :--- | :--- |
| **Main Menu** | `CharacterSelectUiView.cs` | `CharacterSelectUiView` | 🧙‍♂️ 마법사 메인 메뉴: 마법사 단독 프리뷰 + 3인 원정대 동료 프리뷰, 시작 마법 선택기, 게임 시작/대장간/설정 버튼 |
| | `StartSkillSelectorView.cs` | `StartSkillSelectorView` | 🔮 시작 기본 마법 3종(화염구/서리폭발/연쇄번개) 선택기 (선택 테두리 하이라이트, 스킬 설명 실시간 갱신, PlayerPrefs 기억) |
| **In-Game HUD** | `InGameHudView.cs` | `InGameHudView` | 메인 HUD 매니저: 하단 3단 EXP/스킬/HP 바 + 좌측 9종 패시브 슬롯 리스트 & 실시간 수치(ATK/SPD/RNG/ARM/EXP/HP/CRT 등) |
| | `InGameHudBuilder.cs` | `InGameHudBuilder` | 절차적 메인 HUD UI 팩토리 빌더 |
| | `InGameGemCounterHudView.cs` | `InGameGemCounterHudView` | 상단 좌측 5종 통합 리소스 HUD (보석 3종, 골드, 킬수) 및 1.40x 펀치 루팅 애니메이션 |
| | `InGameSlotBuilder.cs` | `InGameSlotBuilder` | 스킬 슬롯, 대시 슬롯, 패시브 슬롯 생성 및 360° 쿨다운 마스크 팩토리 |
| | `HudSpriteHelper.cs` | `HudSpriteHelper` | 10칸 분할 EXP 프레임, 다이아몬드 레벨 뱃지, 골드 스킬 보더, 투구 엠블럼 생성기 |
| | `AimReticleView.cs` | `AimReticleView` | 최상위 Canvas Overlay 기반 조준선: 마우스 능동 조작 감지, 유휴 시 페이드아웃, 클릭 펄스 |
| | `ReticleSpriteHelper.cs` | `ReticleSpriteHelper` | 네온 라임-그린 십자선 과녁 링 픽셀아트 생성기 |
| **Modals & Popups**| `LevelUpUiView.cs` | `LevelUpUiView` | 대형 카드 & 픽셀아트 아이콘 3지선다 보상 선택 UI (숫자키 1, 2, 3 및 마우스 클릭 즉시 선택) |
| | `TreasureChestView.cs` | `TreasureChestView` | 황금 보물상자 필드 렌더링 및 펄스 반짝임 |
| | `TreasureChestManagerView.cs`| `TreasureChestManagerView`| 보물상자 매니저 업데이트 및 뷰 풀링 |
| | `TreasureChestPopupView.cs` | `TreasureChestPopupView` | 보물상자 개봉 시 대형 카드 다이얼로그 (Space/Enter/1/2/3 키보드 수령) |
| | `EvolutionPopupView.cs` | `EvolutionPopupView` | 스킬 진화 성공 시 상단 등장 축하 배너 팝업 |
| | `DamageTextView.cs` | `DamageTextManagerView`, `DamageTextView` | 이벤트 기반 무할당 32개 풀링 대미지 텍스트 |
| | `PauseMenuUiView.cs` | `PauseMenuUiView` | ESC 일시정지 다이얼로그 (계속하기, 환경설정, 다시시작, 종료) |
| | `SettingsDialogUiView.cs` | `SettingsDialogUiView` | 3개 탭 환경설정 다이얼로그 (자동/수동조준, 볼륨, UI스케일) |
| | `GameOverResultUiView.cs` | `GameOverResultUiView` | 사망(Game Over) 시 영구 상점 차단, 재도전 및 3보스 클리어 룰 안내 |
| | `StageVictoryUiView.cs` | `StageVictoryUiView` | 🏆 최종 스테이지 승리 전용 UI (Canvas Overlay sortingOrder 120), 영구 성장 & 스킬 트리 개방 |
| | `CompanionUnlockPopupView.cs` | `CompanionUnlockPopupView` | 🎉 1·2회차 보스 격파 시 신규 동료(전사/궁수) 영입 축하 전용 '짜잔!' 모달 팝업 (대형 도트 아바타 & 팡파레) |
| **Icons & Sprites**| `RewardIconHelper.cs` | `RewardIconHelper` | 인게임 보상 및 스킬 슬롯용 프로시저럴 픽셀아트 아이콘 마스터 캐시 및 디스패처 |
| | `WarriorRewardIconHelper.cs` | `WarriorRewardIconHelper` | 전사 스킬 전용 아이콘 생성기 (대검베기, 휠윈드, 지면강타, 블러드이터 등) |
| | `RangerRewardIconHelper.cs` | `RangerRewardIconHelper` | 궁수 스킬 전용 아이콘 생성기 (관통화살, 화살비, 풍인, 스텔라레인 등) |
| | `PassiveRewardIconHelper.cs` | `PassiveRewardIconHelper` | 패시브 9종 전용 아이콘 생성기 (이빨, 깃털, 마나룬, 갑옷, 황금반지, 치명타눈 등) |
| | `SpriteHelper.cs` | `SpriteHelper` | 2.5D 카툰 타원 블롭 섀도우 및 공용 스프라이트 생성기 |

---

### 6. Progression (Skill Tree) & Magic Forge UI
| 카테고리 | 파일명 | 컴포넌트 / 헬퍼 | 설명 |
| :--- | :--- | :--- | :--- |
| **Skill Tree** | `SkillTreeUiView.cs` | `SkillTreeUiView` | 🌌 대마법사 비전 성좌 메인 화면: 마법사 Only 단일 360° 원형 성좌(18노드), 골드 재화 UI, 50% 환불 각성 리셋 |
| | `SkillTreeNodeView.cs` | `SkillTreeNodeView` | 개별 성좌 노드 UI 버튼: 원형 룬 뱃지, 속성 아이콘, 필요 골드 표기, 상태 뷰어 |
| | `SkillTreeLayoutHelper.cs` | `SkillTreeLayoutHelper` | 360° 대칭 비전 성좌(화염 90°, 빙결 210°, 전격 330°) 극좌표계 배치 및 120° 디바이더 레이저 |
| | `SkillTreeBackgroundHelper.cs`| `SkillTreeBackgroundHelper`| 512x512 고대 천구 석판 다이얼 및 4중 동심원 룬 궤도 홈 프로시저럴 텍스처 생성기 |
| | `SkillTreeSpriteHelper.cs` | `SkillTreeSpriteHelper` | 원형 룬 젬 뱃지 4종(해금/가능/잠김/차단), 속성 아이콘(🔥❄️⚡) 생성기 |
| | `SkillTreeExchangePopupView.cs`| `SkillTreeExchangePopupView`| 💎 2:1 보석 교환소 모달 팝업 (루비/에메랄드/자수정 상호 6방향 변환) |
| | `GemSpriteHelper.cs` | `GemSpriteHelper` | 찬란한 브릴리언트/스텝 컷 3종 보석 프로시저럴 픽셀아트 생성기 |
| | `JsonSkillTreeStorage.cs` | `JsonSkillTreeStorage` | Unity PlayerPrefs JSON 기반 스킬 트리 세이브 데이터 저장소 |
| **Magic Forge** | `MagicForgeUiView.cs` | `MagicForgeUiView` | ⚒️ 마법 대장간 메인 팝업 UI (980x620), 3탭 컨테이너(룬 각인소/마법 결정체/스킬 재련), 보유 재화 지갑 HUD |
| | `RuneInscriptionTabView.cs` | `RuneInscriptionTabView` | 룬 각인소 탭: 3대 스킬 슬롯 장착/해제(✕), 12종 룬 카탈로그, 실시간 변조 프리뷰, 해금 및 Lv.∞ 무한 강화 |
| | `ForgeSpriteHelper.cs` | `ForgeSpriteHelper` | 룬/결정체/빈 슬롯 전용 프로시저럴 픽셀아트 아이콘 및 원형 프레임 생성기 |
| | `JsonForgeStorage.cs` | `JsonForgeStorage` | 마법 대장간 전용 영구 저장소 (`HappyShoot_ForgeSave_v1`) |
| **Legacy Shop** | `MetaShopUiView.cs` | `MetaShopUiView` | (구 시스템) 8종 영구 강화 카드 목록 상점 UI |

---

### 7. Combat Sandbox & Tuning
| 파일명 | 주요 컴포넌트 / 헬퍼 | 설명 |
| :--- | :--- | :--- |
| `SkillTuningUiView.cs` | `SkillTuningUiView` | 🧪 전투 & 밸런스 샌드박스: 실시간 10종 스킬 + 9종 진화 + 9종 패시브 + 경험치/레벨업 + 몬스터 스탯 + 치명타 + AI 동료 8종 조절 및 JSON 영구 저장 |
| `SkillTuningUiBuilder.cs` | `SkillTuningUiBuilder` | 샌드박스 UI 빌더: 6대 대분류 카테고리 탭(전사/궁수/마법사/패시브/공통/시스템) 및 동료 튜닝 연동 |
| `SkillTuningCompanionConfigurator.cs`| `SkillTuningCompanionConfigurator`| 👥 AI 동료 8종 슬라이더(최종 공격력, 패시브 보정, 주변 반경, 안착거리, 이속배율, 전사/궁수 사거리, 경호 타겟팅) 생성 헬퍼 |
| `SkillTuningPassiveConfigurator.cs` | `SkillTuningPassiveConfigurator` | 🧬 9종 패시브 스킬 샌드박스 슬라이더 행 생성 및 실시간 핫리로드 연동 헬퍼 |
| `SkillTuningSliderFactory.cs` | `SkillTuningSliderFactory` | 슬라이더 + [-]/[+] 버튼 + 키보드 숫자 직접 입력창(`InputField`) 완벽 결합 양방향 동기화 팩토리 |
| `SkillLiveApplier.cs` | `SkillLiveApplier` | 스킬 수치 실시간 핫리로드 및 플레이어 보유 패시브 레벨 비례 스탯 실시간 동기화 |
| `SkillTuningMemoryCache.cs` | `SkillTuningMemoryCache` | L1~L5 레벨 간 이동 시 각 레벨별 커스텀 튜닝 수치를 메모리에 보존/복원하는 세션 캐시 |
| `SkillConfigRepository.cs` | `SkillConfigRepository` | 📁 샌드박스 설정 파일 멀티 PC/Git 동기화 저장소 (`Assets/Resources/Config/skill_configs.json` 및 `Assets/Config/skill_configs.json` 이중 저장) |
| `DevSkillSelectorUiView.cs` | `DevSkillSelectorUiView` | 🛠️ 인게임 개발자 콘솔: 모든 액티브/진화/패시브 무기 실시간 레벨링, AI 동료 실시간 소환/해제 |
| `DevCheatButtonHelper.cs` | `DevCheatButtonHelper` | 💎 개발자 치트 모듈: 보석 3종(+10), 전체보석(+50), 무적, 풀피, 몬스터전멸, 배속, 골드(+1000), 페이즈점프 |

---

### 8. Audio, Background & Mobile Touch
| 카테고리 | 파일명 | 컴포넌트 / 헬퍼 | 설명 |
| :--- | :--- | :--- | :--- |
| **Audio** | `SoundManagerView.cs` | `SoundManagerView` | 32채널 풀링, 스킬별 히트음 분기 라우팅, 도트뎀 프레임당 1회 지능형 스로틀링(Throttling)으로 대규모 교전 시 소음 방지 |
| | `ProceduralAudioHelper.cs` | `ProceduralAudioHelper` | 초극상 슬랩 스냅+미트 펀치 타격음, 관통화살 Whoosh, 전사 지진 럼블, 16종 SFX 및 BGM 생성기 |
| | `ProceduralSkillAudioHelper.cs` | `ProceduralSkillAudioHelper` | 고유 스킬 및 도트뎀 사운드 합성기 (스텔라 레인 크리스탈, 인페르노 폭발, 화염/전기 도트음) |
| **Background** | `BackgroundManager.cs` | `BackgroundManager` | 무한 배경 타일링: 3x3 그리드(72m x 72m) 카메라 추적 0-GC 랩어라운드 및 4종 텍스처 갱신 |
| | `BackgroundTileView.cs` | `BackgroundTileView` | 개별 배경 타일 렌더러 (`sortingOrder = -100`, 24m x 24m) |
| | `BackgroundSpriteHelper.cs` | `BackgroundSpriteHelper` | 🏛️ 2.5D 아이소메트릭 다이아몬드 고대 던전 석판 4종 프로시저럴 픽셀아트 생성기 |
| | `BackgroundAmbientDustView.cs` | `BackgroundAmbientDustView` | 전장 깊이감을 부여하는 28개 초경량 앰비언트 부유 먼지/마법 불씨 입자 순환 (`sortingOrder = -50`) |
| | `CameraFollowView.cs` | `CameraFollowView` | 스킬별/마스터 카메라 셰이크 강도 조절, 다중 셰이크 최대값 클램핑 및 절대 한계선(0.38m) 캡핑 |
| **Mobile** | `TouchJoystickView.cs` | `TouchJoystickView` | 📱 모바일 순수 플로팅 가상 터치 조이스틱: 손 뗄 시 100% 은닉, 터치 시 동적 생성, PC 마우스 간섭 차단 |
| | `TouchJoystickSpriteHelper.cs` | `TouchJoystickSpriteHelper` | 조이스틱 베이스 링(160x160) & 노브(64x64) 절차적 생성 헬퍼 |
| | `MobilePauseButtonView.cs` | `MobilePauseButtonView` | 모바일 우측 상단 인게임 터치 전용 일시정지 [⏸] 버튼 |

### 9. Side-Scrolling Dimension Corridor Mode (3rd Clear Secret Mode)
| 파일명 | 주요 컴포넌트 / 헬퍼 | 설명 |
| :--- | :--- | :--- |
| `DimensionPortalView.cs` | `DimensionPortalView` | 🌀 3회차 보스3(사령왕 리치) 격파 시 스폰되는 차원 포탈. 2중 역회전 펄스 링 & 마법사 흡수 트윈 연출 |
| `SideScrollModeController.cs`| `SideScrollModeController` | 🚀 횡스크롤 모드 마스터 컨트롤러: 징검다리 낙하/2회 목숨(❤️❤️) 연동, 마법사 단독 질주(동료 일시 은닉), 탑다운 배경/스포너 100% 은폐, 300m 게이지 HUD, 탈락/승리 연계 |
| `SideScrollPlatformManager.cs`| `SideScrollPlatformManager`| 🌉 입체 징검다리 플랫폼(3.3m 발판, 1.7m 틈새 심연, -1.8f~+0.6f 5단 높낮이) 및 낙하 감지: 1회 추락 시 발판 정중앙 안전 장소 리스폰(❤️💔), 2회 추락 시 탈락 룰 전담 관리자 |
| `SideScrollBackgroundView.cs`| `SideScrollBackgroundView` | 🌌 횡스크롤 전용 100% 불투명 솔리드 우주 백드롭(-60), 3중 패럴랙스(성운 0.15x, 부유 모놀리스 0.45x, 솔리드 차원 그리드) |
| `SideScrollMonsterSpawner.cs`| `SideScrollMonsterSpawner`| 👾 횡스크롤 대량 몬스터 떼스폰: 0.5초마다 지상 슬라임 5~8마리 팩 + 골렘 및 공중 박쥐 편대 쇄도 스케줄러 |
| `UnstableVoidCrystalView.cs` | `UnstableVoidCrystalView` | 💥 차원 불안정 폭발 수정: 타격 시 반경 6m 내 모든 적을 일망타진하는 연쇄 폭발 기믹 |
| `SpeedBoostRingView.cs` | `SpeedBoostRingView` | ⚡ 피버 초가속 링: 통과 시 3.5초간 이동속도 1.7배 상승 + 적 100뎀 로드킬 충격파 버프 |
| `DimensionalVoidCoreView.cs` | `DimensionalVoidCoreView` | 🔮 300m 도달 시 출현하는 거대 차원 핵 보스 뷰: 도망치지 않는 고정형 아레나 보스, 도메인 MonsterEntity 연동으로 모든 스킬/동료 자동 타겟팅 피격, 머리 위 전용 체력바, 근접 전투 피해, 보석/골드 대폭발 격파 연출 |
| `PlayerInputHandler.cs` | `PlayerInputHandler` | 🕹️ 횡스크롤 점프(W/Up/Dpad Up 속도 9.0f, 중력 -26f, 바닥 착지) & 수평 순간이동 대시(Space) 지원 & 발판 중심 안전 리스폰 좌표 등록 |
| `CompanionView.cs` | `CompanionView` | 👥 횡스크롤 모드 진입 시 마법사 단독 질주를 위해 자동 은닉, 일반 탑다운 복귀 시 정상 복구 |

---

[🔙 메인 APP_MAP으로 돌아가기](../../APP_MAP.md)

- [BossSpriteHelper.cs](file:///k:/unityprojects/shoot1/shoot1/Assets/src/HappyShoot.View/Utils/BossSpriteHelper.cs): Phase 1~3 삼대 보스(Boss1 마그마 로드, Boss2 거미/비룡, Boss3 리치킹) 스프라이트 로딩 전담 헬퍼
- [CustomResourceSpriteLoader.cs](file:///k:/unityprojects/shoot1/shoot1/Assets/src/HappyShoot.View/Utils/CustomResourceSpriteLoader.cs): 보석류(루비/에메랄드/자수정), 경험치 구슬(Exp1/Exp2), 황금 코인 고화질 스프라이트 로딩 전담 헬퍼
- [PlayerDashController.cs](file:///k:/unityprojects/shoot1/shoot1/Assets/src/HappyShoot.View/Player/PlayerDashController.cs): 플레이어 대시 물리(Ease-Out 감속 곡선), 쿨타임 제어, 고스트 트레일 잔상 스폰 전담 컨트롤러
- [PlayerDashGhostTrail.cs](file:///k:/unityprojects/shoot1/shoot1/Assets/src/HappyShoot.View/Player/PlayerDashGhostTrail.cs): 대시 이동 시 생성되는 반투명 고스트 트레일 잔상 페이드아웃 뷰 컴포넌트
- [PlayerDashChargeIndicatorView.cs](file:///k:/unityprojects/shoot1/shoot1/Assets/src/HappyShoot.View/Player/PlayerDashChargeIndicatorView.cs): 캐릭터 머리 위 대시 충전 점(살짝 푸른빛 하얀 점 1~3개) 부유 인디케이터
- [MobileDashTouchZoneView.cs](file:///k:/unityprojects/shoot1/shoot1/Assets/src/HappyShoot.View/UI/MobileDashTouchZoneView.cs): 모바일 화면 우측 45% 전역 터치/탭 시 대시 발동 터치존

---

## 🌀 8. 횡스크롤 차원 모드 시스템 (HappyShoot.View.SideScroll)

- [SideScrollModeController.cs](file:///k:/unityprojects/shoot1/shoot1/Assets/src/HappyShoot.View/SideScroll/SideScrollModeController.cs): 3페이즈 클리어 포탈 진입 시 활성화되는 횡스크롤 차원 모드 마스터 컨트롤러. 마법사 단독 진행(동료 은닉), 300m 질주, 카메라 Y축 고정 및 뷰포트 확대, 카메라 우측 편향(+4.5m 오프셋)을 통한 플레이어 좌측 35% 배치 & 우측 전방 시야 65% 확보, 차원의 핵 보스전, 탈락/승리 트랜지션 총괄.
- [SideScrollPlatformManager.cs](file:///k:/unityprojects/shoot1/shoot1/Assets/src/HappyShoot.View/SideScroll/SideScrollPlatformManager.cs): 5단계 가변 높이(-1.8f~+0.6f) 플랫폼, 3.3m 발판 너비 & 1.7m 틈새 심연, 넉넉한 착지 스윕 판정, 1회 추락 시 발판 정중앙 안전 장소 리스폰(❤️💔), 2회 추락 탈락 규칙, 심연(-10.5f) 완전 추락 소멸 연출 총괄.
- [SideScrollMonsterSpawner.cs](file:///k:/unityprojects/shoot1/shoot1/Assets/src/HappyShoot.View/SideScroll/SideScrollMonsterSpawner.cs): 화면 우측 화면 밖(20m+ 전방)에서 대규모 몬스터 군단(슬라임 러셔, 골렘 선봉, 박쥐 파동), 불안정한 공허 수정, 가속 링, 보석 소나기를 스폰하고 뷰를 즉시 동기화. 300m 보스전 시 고정형 MonsterEntity와 DimensionalVoidCoreView를 결합하여 자동 타겟팅 연동.
- [DimensionPortalView.cs](file:///k:/unityprojects/shoot1/shoot1/Assets/src/HappyShoot.View/SideScroll/DimensionPortalView.cs): 사령왕 리치 처치 시 바닥에 등장하는 차원 균열 포탈 뷰. 플레이어 진입 감지 및 횡스크롤 모드 전환 트리거.
- [SideScrollBackgroundView.cs](file:///k:/unityprojects/shoot1/shoot1/Assets/src/HappyShoot.View/SideScroll/SideScrollBackgroundView.cs): 성운, 심연 산맥, 시공간 왜곡 별무리로 구성된 3중 패럴랙스 횡스크롤 배경 뷰.
- [UnstableVoidCrystalView.cs](file:///k:/unityprojects/shoot1/shoot1/Assets/src/HappyShoot.View/SideScroll/UnstableVoidCrystalView.cs): 타격 시 0.6초 후 대폭발(반경 5.5m, 350 데미지)을 일으켜 몬스터 무리를 일망타진하는 전략 오브젝트.
- [SpeedBoostRingView.cs](file:///k:/unityprojects/shoot1/shoot1/Assets/src/HappyShoot.View/SideScroll/SpeedBoostRingView.cs): 통과 시 3.5초간 이속 +70% 폭풍 질주, 로드킬 충격파, 0.05초 간격의 찬란한 비전 고스트 트레일 잔상(Afterimage Ghost Trail) 연출.
- [SideScrollGoldCoinView.cs](file:///k:/unityprojects/shoot1/shoot1/Assets/src/HappyShoot.View/SideScroll/SideScrollGoldCoinView.cs): 횡스크롤 모드 전용 필드 골드 코인 뷰. 몬스터 사망 시 통통 튀며 스폰, 플레이어 자석 흡수, 경험치 3지선다 팝업 방해 없이 대량의 골드 누적.
- [FallingGemShowerView.cs](file:///k:/unityprojects/shoot1/shoot1/Assets/src/HappyShoot.View/SideScroll/FallingGemShowerView.cs): 차원 질주 중 하늘에서 비처럼 쏟아지는 고화질 황금 코인 소나기 뷰 (접촉 시 회당 +15G 획득).
- [DimensionalVoidCoreView.cs](file:///k:/unityprojects/shoot1/shoot1/Assets/src/HappyShoot.View/SideScroll/DimensionalVoidCoreView.cs): 300m 도달 시 등장하는 차원의 핵 최종 보스. 고정형 배치로 도망 방지, MonsterEntity 연동으로 플레이어/동료 스킬 자동 타겟팅, 머리 위 전용 체력바, 근접 타격 판정, 파괴 시 최종 승리 트리거.
- [MonsterTrainingDummyHelper.cs](file:///k:/unityprojects/shoot1/shoot1/Assets/src/HappyShoot.View/Monsters/MonsterTrainingDummyHelper.cs): 테스트/샌드박스용 훈련 허수아비 및 박쥐 스폰 전담 분리 헬퍼.


