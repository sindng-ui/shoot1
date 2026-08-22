using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using HappyShoot.Domain.Entities;
using HappyShoot.Domain.Events;
using HappyShoot.Domain.Leveling;
using HappyShoot.Domain.Session;
using HappyShoot.Domain.Skills;
using HappyShoot.Domain.Skills.Effects;
using HappyShoot.Domain.Skills.Evolution;
using HappyShoot.Domain.Skills.Targeters;
using HappyShoot.Domain.Skills.Triggers;
using HappyShoot.View.Cameras;
using HappyShoot.View.Config;
using HappyShoot.View.Gems;
using HappyShoot.View.Monsters;
using HappyShoot.View.Player;
using HappyShoot.View.Projectiles;
using HappyShoot.View.Timeline;
using HappyShoot.View.UI;
using HappyShoot.View.Utils;

namespace HappyShoot.View.Bootstrap
{
    /// <summary>
    /// Master one-click bootstrapper that configures and launches the entire 2D Survivors-like game loop.
    /// Works automatically on play even in completely blank scenes!
    /// </summary>
    public class GameBootstrap : MonoBehaviour
    {
        [Header("Starting Class")]
        [SerializeField] private CharacterClassType _selectedClass = CharacterClassType.Warrior;

        private GameSessionEntity _gameSession;

        public GameSessionEntity GameSession => _gameSession;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoBootIfEmpty()
        {
            // Register sceneLoaded callback so restarting or reloading scenes always re-triggers bootstrapping!
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;

            EnsureBootstrapped();
        }

        private static void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
        {
            EnsureBootstrapped();
        }

        private static void EnsureBootstrapped()
        {
            // Upgrade legacy EventSystem if present in scene
            FixLegacyEventSystem();

            if (FindFirstObjectByType<GameBootstrap>() == null && FindFirstObjectByType<PlayerView>() == null)
            {
                var bootGo = new GameObject("___AUTO_GAME_BOOTSTRAP___");
                bootGo.AddComponent<GameBootstrap>();
            }
        }

        private static void FixLegacyEventSystem()
        {
            var eventSystem = FindFirstObjectByType<EventSystem>();
            if (eventSystem == null)
            {
                var esGo = new GameObject("EventSystem");
                esGo.AddComponent<EventSystem>();
                esGo.AddComponent<InputSystemUIInputModule>();
            }
            else
            {
                var legacyModule = eventSystem.GetComponent<StandaloneInputModule>();
                if (legacyModule != null)
                {
                    DestroyImmediate(legacyModule);
                }
                if (eventSystem.GetComponent<InputSystemUIInputModule>() == null)
                {
                    eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
                }
            }
        }

        private void Start()
        {
            Debug.Log("[GameBootstrap] Initializing HappyShoot 2D Game Loop...");

            // 0. Initialize Game Settings Storage
            HappyShoot.Domain.Settings.GameSettings.InitializeStorage(new Settings.UnityPlayerPrefsSettingsStorage());

            // 1. Ensure 2D Orthographic Camera
            var mainCam = UnityEngine.Camera.main;
            if (mainCam == null)
            {
                var camGo = new GameObject("Main Camera");
                mainCam = camGo.AddComponent<UnityEngine.Camera>();
                mainCam.tag = "MainCamera";
                camGo.transform.position = new Vector3(0f, 0f, -10f);
            }
            mainCam.orthographic = true;
            mainCam.orthographicSize = 9.0f; // Wide, expansive 16:9 field of view
            mainCam.backgroundColor = new Color(0.12f, 0.14f, 0.18f, 1.0f); // Sleek dark slate background

            // 2. Create Player GameObject
            var playerGo = new GameObject("Player");
            var playerSr = playerGo.AddComponent<SpriteRenderer>();
            playerSr.sprite = SpriteHelper.GetOrCreateCircleSprite();
            playerSr.color = Color.cyan;
            playerGo.transform.localScale = Vector3.one * 0.9f;

            var playerView = playerGo.AddComponent<PlayerView>();
            playerGo.AddComponent<PlayerInputHandler>();
            Debug.Log($"[GameBootstrap] Spawned Hero Class: {_selectedClass}");

            var camFollow = mainCam.GetComponent<CameraFollowView>() ?? mainCam.gameObject.AddComponent<CameraFollowView>();
            camFollow.SetTarget(playerGo.transform);

            // Overhead Health Bar
            var overheadHpGo = new GameObject("PlayerOverheadHealth");
            var overheadHpView = overheadHpGo.AddComponent<PlayerHealthBarView>();
            overheadHpView.Initialize(playerView);

            // Hit-Stop Juice Manager
            var hitStopGo = new GameObject("HitStopManager");
            hitStopGo.AddComponent<Utils.HitStopManager>();

            // 3. Create Managers
            var spawnerGo = new GameObject("MonsterSpawner");
            var spawnerView = spawnerGo.AddComponent<MonsterSpawnerView>();
            spawnerView.Initialize(playerView);

            var projGo = new GameObject("ProjectileManager");
            var projManagerView = projGo.AddComponent<ProjectileManagerView>();
            projManagerView.Initialize(playerView.EventBus);

            var enemyProjGo = new GameObject("EnemyProjectileManager");
            var enemyProjManagerView = enemyProjGo.AddComponent<Projectiles.EnemyProjectileManagerView>();
            enemyProjManagerView.Initialize(playerView);
            spawnerView.SetEnemyProjectileManager(enemyProjManagerView);

            var groundStompGo = new GameObject("GroundStompManager");
            var groundStompView = groundStompGo.AddComponent<Projectiles.GroundStompManagerView>();
            groundStompView.Initialize(playerView.EventBus);

            var arrowRainGo = new GameObject("ArrowRainManager");
            var arrowRainView = arrowRainGo.AddComponent<Projectiles.ArrowRainManagerView>();
            arrowRainView.Initialize(playerView.EventBus, spawnerView);

            var magicSkillGo = new GameObject("MagicSkillManager");
            var magicSkillView = magicSkillGo.AddComponent<Projectiles.MagicSkillManagerView>();
            magicSkillView.Initialize(playerView.EventBus);

            var meteorStrikeGo = new GameObject("MeteorStrikeManager");
            var meteorStrikeView = meteorStrikeGo.AddComponent<Projectiles.MeteorStrikeManagerView>();
            meteorStrikeView.Initialize(playerView.EventBus);

            var bloodEaterGo = new GameObject("BloodEaterManager");
            var bloodEaterView = bloodEaterGo.AddComponent<Projectiles.BloodEaterManagerView>();
            bloodEaterView.Initialize(playerView.EventBus, playerView.transform);

            var stormBowGo = new GameObject("StormBowManager");
            var stormBowView = stormBowGo.AddComponent<Projectiles.StormBowManagerView>();
            stormBowView.Initialize(playerView.EventBus, spawnerView);

            var windGlaiveGo = new GameObject("WindGlaiveManager");
            var windGlaiveView = windGlaiveGo.AddComponent<Projectiles.WindGlaiveManagerView>();
            windGlaiveView.Initialize(playerView, playerView.EventBus, spawnerView);

            var gemGo = new GameObject("GemManager");
            var gemManagerView = gemGo.AddComponent<GemManagerView>();
            gemManagerView.Initialize(playerView.EventBus, playerView);

            var damageTextGo = new GameObject("DamageTextManager");
            var damageTextView = damageTextGo.AddComponent<DamageTextManagerView>();
            damageTextView.Initialize(playerView.EventBus);

            var soundGo = new GameObject("SoundManager");
            var soundView = soundGo.AddComponent<Audio.SoundManagerView>();
            soundView.Initialize(playerView.EventBus);

            playerView.SetExternalSystems(spawnerView, projManagerView);

            // 4. Setup Timeline
            var timelineGo = new GameObject("WaveTimeline");
            timelineGo.AddComponent<WaveTimelineView>();

            // 5. Setup Evolution Manager & Reward Manager
            var evolutionManager = new SkillEvolutionManager(playerView.EventBus);
            RegisterAllEvolutions(evolutionManager);

            var rewardManager = new SkillRewardManager(evolutionManager);
            rewardManager.SkillLevelHook = (skill, lv) =>
            {
                SkillConfigRepository.Instance.ApplyConfigToSkillLevel(skill, lv);
            };
            RegisterAllSkills(rewardManager);
            RegisterAllPassives(rewardManager);
            RewardIconHelper.PreloadIcons();

            var levelUiGo = new GameObject("LevelUpUI");
            var levelUiView = levelUiGo.AddComponent<LevelUpUiView>();

            var expCfg = SkillConfigRepository.Instance.GetConfig().Exp;
            var levelSystem = new LevelSystem(playerView.EventBus, 1, expCfg);
            gemManagerView.DomainManager.Config = expCfg;
            levelUiView.Initialize(playerView, levelSystem, rewardManager);

            // Connect gem magnet collection to level system & audio feedback
            gemManagerView.DomainManager.OnExpCollected += (exp) =>
            {
                levelSystem.AddExp(exp);
                playerView.EventBus?.Publish(new PlaySoundEvent(SoundEffectType.GemCollect));
            };

            var evoPopupGo = new GameObject("EvolutionPopupUI");
            var evoPopupView = evoPopupGo.AddComponent<EvolutionPopupView>();
            evoPopupView.Initialize(playerView.EventBus);

            // 6. Setup Game Session, Meta Shop & InGame HUD / Pause / GameOver UI
            var metaStorage = new Shop.JsonPlayerPrefsStorage();
            var metaShopManager = new HappyShoot.Domain.Meta.MetaShopManager(metaStorage);

            // Apply permanent upgrades to player starting stats
            playerView.Entity.Stats = HappyShoot.Domain.Meta.MetaUpgradeApplier.ApplyUpgrades(playerView.Entity.Stats, metaShopManager.SaveData);

            var metaShopGo = new GameObject("MetaShopUI");
            var metaShopView = metaShopGo.AddComponent<Shop.MetaShopUiView>();
            metaShopView.Initialize(metaShopManager);

            _gameSession = new GameSessionEntity(playerView.EventBus);
            _gameSession.StartGame();

            var settingsGo = new GameObject("SettingsDialogUI");
            var settingsDialogView = settingsGo.AddComponent<SettingsDialogUiView>();
            settingsDialogView.Initialize();

            var hudGo = new GameObject("InGameHUD");
            var hudView = hudGo.AddComponent<InGameHudView>();
            hudView.SetSettingsDialog(settingsDialogView);
            hudView.Initialize(playerView, levelSystem, _gameSession);

            var bossBarGo = new GameObject("BossHealthBarUI");
            var bossBarView = bossBarGo.AddComponent<BossHealthBarView>();
            bossBarView.Initialize(playerView.EventBus);

            var chestManagerGo = new GameObject("TreasureChestManager");
            var chestManagerView = chestManagerGo.AddComponent<Chests.TreasureChestManagerView>();
            chestManagerView.Initialize(playerView, rewardManager);

            var chestPopupGo = new GameObject("TreasureChestPopupUI");
            var chestPopupView = chestPopupGo.AddComponent<Chests.TreasureChestPopupView>();
            chestPopupView.Initialize(playerView.EventBus);

            var pauseGo = new GameObject("PauseMenuUI");
            var pauseView = pauseGo.AddComponent<PauseMenuUiView>();
            pauseView.SetSettingsDialog(settingsDialogView);
            pauseView.Initialize(_gameSession, playerView.EventBus);

            var gameOverGo = new GameObject("GameOverUI");
            var gameOverView = gameOverGo.AddComponent<GameOverResultUiView>();
            gameOverView.Initialize(_gameSession, playerView.EventBus, metaShopManager, metaShopView);

            // 6. Character Select Screen (Warrior vs Ranger)
            // 7. Developer Skill Selector & Cheat Console UI
            var devConsoleGo = new GameObject("DevSkillSelectorUI");
            var devConsoleView = devConsoleGo.AddComponent<DevSkillSelectorUiView>();
            devConsoleView.Initialize(playerView, rewardManager, levelSystem, _gameSession, spawnerView);
            devConsoleView.Hide(); // Hidden until Dev Mode is enabled

            // 7-2. Skill Tuning Sandbox UI
            var skillTuningGo = new GameObject("SkillTuningUI");
            var skillTuningView = skillTuningGo.AddComponent<SkillTuningUiView>();
            skillTuningView.Initialize(playerView, rewardManager, spawnerView, levelSystem, gemManagerView.DomainManager);
            skillTuningView.Hide();

            // 8. Character Select Screen (Warrior vs Ranger vs Wizard & Dev Mode & Skill Test Mode)
            var charSelectGo = new GameObject("CharacterSelectUI");
            var charSelectView = charSelectGo.AddComponent<CharacterSelectUiView>();
            charSelectView.SetSettingsDialog(settingsDialogView);
            charSelectView.Initialize((selectedClass, isDevMode, isSkillTestMode) =>
            {
                _selectedClass = selectedClass;
                playerView.SetClassType(selectedClass);
                foreach (var s in playerView.Entity.Skills)
                {
                    SkillConfigRepository.Instance.ApplyConfigToSkillLevel(s, s.Level);
                }
                Debug.Log($"[GameBootstrap] Hero Selected & Ready: {selectedClass} (DevMode: {isDevMode}, SkillTest: {isSkillTestMode})");

                if (isDevMode)
                {
                    devConsoleView.Show();
                }

                if (isSkillTestMode)
                {
                    spawnerView.IsSpawningSuppressed = true;
                    spawnerView.SpawnTrainingDummies(playerView.Entity.Position, 5);
                    skillTuningView.Show();
                }
            });

            Debug.Log("[GameBootstrap] Initialization Complete! Press WASD or Arrow Keys to move and survive!");
        }

        private void Update()
        {
            if (_gameSession != null)
            {
                _gameSession.Update(Time.deltaTime);
            }
        }

        private void RegisterAllSkills(SkillRewardManager rewardManager)
        {
            // Warrior Exclusive Skills
            rewardManager.RegisterSkill("slash", "대검 베기", "전방 150도 궤적의 적들을 시원하게 베어버리는 근접 광역 물리 공격",
                () => {
                    var cfg = SkillConfigRepository.Instance.GetConfig();
                    return new CompositeSkill("slash", "대검 베기", new CooldownTrigger(cfg.Slash.Cooldown), new ClosestEnemyTargeter(), new GreatswordSlashEffect(cfg.Slash.Damage, cfg.Slash.Radius, cfg.Slash.ArcAngle), range: cfg.Slash.Radius + 0.3f);
                },
                new[] { CharacterClassType.Warrior });

            rewardManager.RegisterSkill("ground_stomp", "지면 강타", "발로 지면을 강하게 구르고 묵직한 지진 충격파를 일으켜 근처 적들을 타격 (레벨업 시 범위 및 파편 수 증가)",
                () => {
                    var cfg = SkillConfigRepository.Instance.GetConfig();
                    return new CompositeSkill("ground_stomp", "지면 강타", new CooldownTrigger(cfg.GroundStomp.Cooldown), new ClosestEnemyTargeter(), new GroundStompEffect(cfg.GroundStomp.Damage, cfg.GroundStomp.Radius), range: cfg.GroundStomp.Radius);
                },
                new[] { CharacterClassType.Warrior });

            rewardManager.RegisterSkill("whirlwind", "휠윈드", "플레이어 주변 360도 전방위로 회전 검기 충격파를 날려 접근하는 적들을 일제 타격 (레벨업 시 범위 대폭 확장)",
                () => {
                    var cfg = SkillConfigRepository.Instance.GetConfig();
                    return new CompositeSkill("whirlwind", "휠윈드", new CooldownTrigger(cfg.Whirlwind.Cooldown), new ClosestEnemyTargeter(), new WhirlwindEffect(cfg.Whirlwind.Damage, cfg.Whirlwind.Radius), range: cfg.Whirlwind.Radius + 0.3f);
                },
                new[] { CharacterClassType.Warrior });

            // Ranger Exclusive Skills
            rewardManager.RegisterSkill("bow", "관통 화살", "가장 가까운 적을 향해 고속으로 날아가 화면 끝까지 적들을 무제한 꿰뚫는 원거리 관통 사격 (레벨업 시 화살 수 추가)",
                () => {
                    var cfg = SkillConfigRepository.Instance.GetConfig();
                    return new CompositeSkill("bow", "관통 화살", new CooldownTrigger(cfg.Bow.Cooldown), new ClosestEnemyTargeter(), new PiercingArrowEffect(cfg.Bow.Damage, cfg.Bow.Speed, cfg.Bow.ArrowCount, 999, cfg.Bow.SpreadAngle));
                },
                new[] { CharacterClassType.Ranger });

            rewardManager.RegisterSkill("glaive", "칼바람 글레이브", "전방으로 회전하는 풍인을 던져 적들을 관통하고 되돌아오며 2중 타격하는 사냥꾼의 부메랑 무기 (레벨업 시 글레이브 수 증가)",
                () => {
                    var cfg = SkillConfigRepository.Instance.GetConfig();
                    return new CompositeSkill("glaive", "칼바람 글레이브", new CooldownTrigger(cfg.Glaive.Cooldown), new ClosestEnemyTargeter(), new WindGlaiveEffect(cfg.Glaive.Damage, cfg.Glaive.Distance, cfg.Glaive.Speed, cfg.Glaive.GlaiveCount), range: cfg.Glaive.Distance);
                },
                new[] { CharacterClassType.Ranger });

            rewardManager.RegisterSkill("arrow_rain", "화살 비", "적 군집 상공에서 수십 발의 화살이 쏟아져 지속적인 광역 물리 피해 (레벨업 시 지속시간 및 화살 폭격 수 3배 증가)",
                () => {
                    var cfg = SkillConfigRepository.Instance.GetConfig();
                    return new CompositeSkill("arrow_rain", "화살 비", new CooldownTrigger(cfg.ArrowRain.Cooldown), new ClosestEnemyTargeter(), new ArrowRainEffect(cfg.ArrowRain.Damage, cfg.ArrowRain.Radius, cfg.ArrowRain.Duration, cfg.ArrowRain.ArrowCount), range: 9.0f);
                },
                new[] { CharacterClassType.Ranger });

            // Wizard Exclusive Skills
            rewardManager.RegisterSkill("fireball", "화염구", "대상 적을 향해 날아가 충돌 시 광역 폭발 마법 피해 (레벨업 시 최대 3발 동시 발사 & 거대 화염 폭발)",
                () => {
                    var cfg = SkillConfigRepository.Instance.GetConfig();
                    return new CompositeSkill("fireball", "화염구", new CooldownTrigger(cfg.Fireball.Cooldown), new ClosestEnemyTargeter(), new FireballEffect(cfg.Fireball.Damage, cfg.Fireball.Radius, cfg.Fireball.Speed, cfg.Fireball.FireballCount), range: 9.0f);
                },
                new[] { CharacterClassType.Wizard });

            rewardManager.RegisterSkill("frost_nova", "서리 폭발", "플레이어 주변으로 차가운 냉기 파동을 방출하여 적들을 일제 타격 (레벨업 시 5.2m 화면 전체 빙결 & 오한 지속시간 대폭 증가)",
                () => {
                    var cfg = SkillConfigRepository.Instance.GetConfig();
                    return new CompositeSkill("frost_nova", "서리 폭발", new CooldownTrigger(cfg.FrostNova.Cooldown), new ClosestEnemyTargeter(), new FrostNovaEffect(cfg.FrostNova.Damage, cfg.FrostNova.Radius, cfg.FrostNova.ChillDuration), range: cfg.FrostNova.Radius);
                },
                new[] { CharacterClassType.Wizard });

            rewardManager.RegisterSkill("chain_lightning", "연쇄 번개", "가장 가까운 적을 감전시킨 뒤 주변 적들에게 연속 전이 (레벨업 시 최대 8회 전이 & 사거리 증가)",
                () => {
                    var cfg = SkillConfigRepository.Instance.GetConfig();
                    return new CompositeSkill("chain_lightning", "연쇄 번개", new CooldownTrigger(cfg.ChainLightning.Cooldown), new ClosestEnemyTargeter(), new ChainLightningEffect(cfg.ChainLightning.Damage, cfg.ChainLightning.ChainCount, cfg.ChainLightning.JumpRadius), range: 7.0f);
                },
                new[] { CharacterClassType.Wizard });

            // Shared Skills
            rewardManager.RegisterSkill("orbital", "오비탈 블레이드", "플레이어 주위를 원형으로 고속 회전하며 접근하는 적들을 갈아버리는 보호형 무기",
                () => {
                    var cfg = SkillConfigRepository.Instance.GetConfig();
                    return new CompositeSkill("orbital", "오비탈 블레이드", new CooldownTrigger(cfg.Orbital.Cooldown), new SelfTargeter(), new OrbitingBladesEffect(cfg.Orbital.Damage, cfg.Orbital.Radius, cfg.Orbital.RotationSpeed, cfg.Orbital.BladeCount));
                });
        }

        private void RegisterAllPassives(SkillRewardManager rewardManager)
        {
            rewardManager.RegisterPassive("passive_fang", "흡혈귀의 이빨", "공격력 +15% 증가 (대검 진화 재료)", 5, (p, lv) =>
            {
                var s = p.Stats;
                p.Stats = new CharacterStats(s.MaxHealth, s.HealthRegen, s.MoveSpeed, 1.0f + 0.15f * lv, s.Armor, s.CritChance, s.CritDamageMultiplier, s.CooldownReduction, s.AreaMultiplier, s.ProjectileSpeedMultiplier, s.ExtraProjectiles, s.PickupRadius);
            });

            rewardManager.RegisterPassive("passive_feather", "바람의 깃털", "이동속도 +12% & 투사체 속도 +15% 증가 (활 진화 재료)", 5, (p, lv) =>
            {
                var s = p.Stats;
                p.Stats = new CharacterStats(s.MaxHealth, s.HealthRegen, 5.0f * (1.0f + 0.12f * lv), s.AttackPowerMultiplier, s.Armor, s.CritChance, s.CritDamageMultiplier, s.CooldownReduction, s.AreaMultiplier, 1.0f + 0.15f * lv, s.ExtraProjectiles, s.PickupRadius);
            });

            rewardManager.RegisterPassive("passive_rune", "마나 룬", "쿨타임 감소 -10% & 공격 범위 +15% 증가 (폭발 진화 재료)", 5, (p, lv) =>
            {
                var s = p.Stats;
                p.Stats = new CharacterStats(s.MaxHealth, s.HealthRegen, s.MoveSpeed, s.AttackPowerMultiplier, s.Armor, s.CritChance, s.CritDamageMultiplier, 0.10f * lv, 1.0f + 0.15f * lv, s.ProjectileSpeedMultiplier, s.ExtraProjectiles, s.PickupRadius);
            });

            rewardManager.RegisterPassive("passive_armor", "강철 갑옷", "방어력 +5 증가 (받는 대미지 경감)", 5, (p, lv) =>
            {
                var s = p.Stats;
                p.Stats = new CharacterStats(s.MaxHealth, s.HealthRegen, s.MoveSpeed, s.AttackPowerMultiplier, 5f * lv, s.CritChance, s.CritDamageMultiplier, s.CooldownReduction, s.AreaMultiplier, s.ProjectileSpeedMultiplier, s.ExtraProjectiles, s.PickupRadius);
            });

            rewardManager.RegisterPassive("passive_ring", "황금 반지", "경험치 및 아이템 자석 흡수 반경 +1.5m 증가", 5, (p, lv) =>
            {
                var s = p.Stats;
                p.Stats = new CharacterStats(s.MaxHealth, s.HealthRegen, s.MoveSpeed, s.AttackPowerMultiplier, s.Armor, s.CritChance, s.CritDamageMultiplier, s.CooldownReduction, s.AreaMultiplier, s.ProjectileSpeedMultiplier, s.ExtraProjectiles, 2.0f + 1.5f * lv);
            });

            rewardManager.RegisterPassive("passive_heart", "생명의 펜던트", "최대 체력 +30 & 초당 체력 재생 +1.5 HP/s 증가", 5, (p, lv) =>
            {
                var s = p.Stats;
                p.Stats = new CharacterStats(100f + 30f * lv, 1.5f * lv, s.MoveSpeed, s.AttackPowerMultiplier, s.Armor, s.CritChance, s.CritDamageMultiplier, s.CooldownReduction, s.AreaMultiplier, s.ProjectileSpeedMultiplier, s.ExtraProjectiles, s.PickupRadius);
            });

            rewardManager.RegisterPassive("passive_ignition", "발화의 불꽃", "화염 마법 공격 시 적을 7초간 불태우며 공격력 +10% 증가", 5, (p, lv) =>
            {
                var s = p.Stats;
                p.Stats = new CharacterStats(s.MaxHealth, s.HealthRegen, s.MoveSpeed, s.AttackPowerMultiplier * (1.0f + 0.10f * lv), s.Armor, s.CritChance, s.CritDamageMultiplier, s.CooldownReduction, s.AreaMultiplier, s.ProjectileSpeedMultiplier, s.ExtraProjectiles, s.PickupRadius);
            });

            rewardManager.RegisterPassive("passive_overcharge", "과전류의 핵", "전기 마법 공격 시 적을 7초간 감전시키며 쿨타임 -6% 추가 감소", 5, (p, lv) =>
            {
                var s = p.Stats;
                p.Stats = new CharacterStats(s.MaxHealth, s.HealthRegen, s.MoveSpeed, s.AttackPowerMultiplier, s.Armor, s.CritChance, s.CritDamageMultiplier, s.CooldownReduction + 0.06f * lv, s.AreaMultiplier, s.ProjectileSpeedMultiplier, s.ExtraProjectiles, s.PickupRadius);
            });
        }

        private void RegisterAllEvolutions(SkillEvolutionManager evolutionManager)
        {
            evolutionManager.RegisterRecipe(new SkillEvolutionRecipe(
                "slash", "passive_fang", "blood_eater", "블러드 이터",
                () => new CompositeSkill("blood_eater", "블러드 이터", new CooldownTrigger(0.85f), new ClosestEnemyTargeter(), new BloodEaterEffect(85f, 4.8f, 2.0f))));

            evolutionManager.RegisterRecipe(new SkillEvolutionRecipe(
                "bow", "passive_feather", "storm_bow", "폭풍의 활",
                () => new CompositeSkill("storm_bow", "폭풍의 활", new CooldownTrigger(1.6f), new ClosestEnemyTargeter(), new StormArrowEffect(65f, 45f, 1.6f, 20f, 5, 36f))));

            evolutionManager.RegisterRecipe(new SkillEvolutionRecipe(
                "fireball", "passive_rune", "meteor_strike", "메테오 스트라이크",
                () => new CompositeSkill("meteor_strike", "메테오 스트라이크", new CooldownTrigger(1.2f), new ClosestEnemyTargeter(), new MeteorStrikeEffect(120f, 3.0f))));
        }
    }
}
