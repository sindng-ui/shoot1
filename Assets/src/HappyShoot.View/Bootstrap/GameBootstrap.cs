using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using HappyShoot.Domain.Entities;
using HappyShoot.Domain.Events;
using HappyShoot.Domain.Leveling;
using HappyShoot.Domain.Progression;
using HappyShoot.Domain.Session;
using HappyShoot.Domain.Skills;
using HappyShoot.Domain.Skills.Effects;
using HappyShoot.Domain.Skills.Evolution;
using HappyShoot.Domain.Skills.Targeters;
using HappyShoot.Domain.Skills.Triggers;
using HappyShoot.View.Background;
using HappyShoot.View.Cameras;
using HappyShoot.View.Config;
using HappyShoot.View.Effects;
using HappyShoot.View.Gems;
using HappyShoot.View.Monsters;
using HappyShoot.View.Player;
using HappyShoot.View.Projectiles;
using HappyShoot.View.SkillTree;
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

            // 1.1. Create Infinite Background Tiling Manager
            var bgGo = new GameObject("BackgroundManager");
            var bgManager = bgGo.AddComponent<BackgroundManager>();
            bgManager.Initialize(mainCam);

            // 2. Create Player GameObject
            var playerGo = new GameObject("Player");
            var playerSr = playerGo.AddComponent<SpriteRenderer>();
            playerSr.enabled = false; // Disabled: PlayerView renders cute chibi hero in child BodyVisual
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

            var whirlwindGo = new GameObject("WhirlwindManager");
            var whirlwindView = whirlwindGo.AddComponent<Projectiles.WhirlwindManagerView>();
            whirlwindView.Initialize(playerView.EventBus);

            var arrowRainGo = new GameObject("ArrowRainManager");
            var arrowRainView = arrowRainGo.AddComponent<Projectiles.ArrowRainManagerView>();
            arrowRainView.Initialize(playerView.EventBus, spawnerView, playerView);

            var magicSkillGo = new GameObject("MagicSkillManager");
            var magicSkillView = magicSkillGo.AddComponent<Projectiles.MagicSkillManagerView>();
            magicSkillView.Initialize(playerView.EventBus);

            var fireballSkillGo = new GameObject("FireballSkillManager");
            var fireballSkillView = fireballSkillGo.AddComponent<Projectiles.FireballSkillManagerView>();
            fireballSkillView.Initialize(playerView.EventBus, spawnerView, playerView);

            var meteorStrikeGo = new GameObject("MeteorStrikeManager");
            var meteorStrikeView = meteorStrikeGo.AddComponent<Projectiles.MeteorStrikeManagerView>();
            meteorStrikeView.Initialize(playerView.EventBus, spawnerView, playerView);

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

            var gemStoneGo = new GameObject("GemStoneManager");
            var gemStoneManagerView = gemStoneGo.AddComponent<Gems.GemStoneManagerView>();
            gemStoneManagerView.Initialize(gemManagerView.DomainManager);

            var damageTextGo = new GameObject("DamageTextManager");
            var damageTextView = damageTextGo.AddComponent<DamageTextManagerView>();
            damageTextView.Initialize(playerView.EventBus);

            var slashVfxGo = new GameObject("SlashHitVfxManager");
            var slashVfxView = slashVfxGo.AddComponent<Effects.SlashHitVfxManagerView>();
            slashVfxView.Initialize(playerView.EventBus);

            var critVfxGo = new GameObject("CriticalHitVfxManager");
            var critVfxView = critVfxGo.AddComponent<Effects.CriticalHitVfxManagerView>();
            critVfxView.Initialize(playerView.EventBus);

            var soundGo = new GameObject("SoundManager");
            var soundView = soundGo.AddComponent<Audio.SoundManagerView>();
            soundView.Initialize(playerView.EventBus);

            var reticleGo = new GameObject("AimReticle");
            var reticleView = reticleGo.AddComponent<Cameras.AimReticleView>();
            reticleView.Initialize();

            var deathFxGo = new GameObject("MonsterDeathFxManager");
            var deathFxView = deathFxGo.AddComponent<Monsters.MonsterDeathFxManagerView>();
            deathFxView.Initialize(playerView.EventBus);

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
            spawnerView.SetLevelSystem(levelSystem);
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

            // 6. Setup Game Session, Meta Skill Tree & InGame HUD / Pause / GameOver UI
            var skillTreeStorage = new JsonSkillTreeStorage();
            var skillTreeManager = new SkillTreeManager(skillTreeStorage);
            SkillTreeRegistry.RegisterAll(skillTreeManager);

            // Apply permanent skill tree upgrades to player starting stats
            playerView.Entity.Stats = SkillTreeApplier.ApplyStats(playerView.Entity.Stats, skillTreeManager, _selectedClass);
            playerView.Entity.ProgressionFlags = SkillTreeApplier.BuildFlags(skillTreeManager, _selectedClass);

            // Apply saved custom sandbox stats if available
            var skillConfig = SkillConfigRepository.Instance.GetConfig();
            if (skillConfig?.CritStat != null && skillConfig.CritStat.IsCustom)
            {
                var s = playerView.Entity.Stats;
                var c = skillConfig.CritStat;
                playerView.Entity.Stats = new CharacterStats(s.MaxHealth, s.HealthRegen, c.MoveSpeed, c.AttackPowerMultiplier, c.Armor, c.CritChance, c.CritDamageMultiplier, c.CooldownReduction, s.AreaMultiplier, s.ProjectileSpeedMultiplier, s.ExtraProjectiles, s.PickupRadius);
            }

            var skillTreeGo = new GameObject("SkillTreeUI");
            var skillTreeUiView = skillTreeGo.AddComponent<SkillTreeUiView>();
            skillTreeUiView.Initialize(skillTreeManager);

            _gameSession = new GameSessionEntity(playerView.EventBus);
            _gameSession.StartGame();

            var settingsGo = new GameObject("SettingsDialogUI");
            var settingsDialogView = settingsGo.AddComponent<SettingsDialogUiView>();
            settingsDialogView.Initialize();

            var hudGo = new GameObject("InGameHUD");
            var hudView = hudGo.AddComponent<InGameHudView>();
            hudView.SetSettingsDialog(settingsDialogView);
            hudView.Initialize(playerView, levelSystem, _gameSession);

            var gemCounterGo = new GameObject("GemCounterHUD");
            var gemCounterView = gemCounterGo.AddComponent<InGameGemCounterHudView>();
            gemCounterView.Initialize(playerView.EventBus, hudGo.transform);

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
            gameOverView.Initialize(_gameSession, playerView.EventBus, null, null, skillTreeManager, skillTreeUiView, gemCounterView);

            // 6.5. Exclusive Stage Victory UI (Unlocks Skill Tree & Meta Growth ONLY on Boss 3 defeat)
            var victoryGo = new GameObject("StageVictoryUI");
            var victoryView = victoryGo.AddComponent<UI.StageVictoryUiView>();
            victoryView.Initialize(_gameSession, null, skillTreeManager, skillTreeUiView, gemCounterView, hudGo.transform);
            spawnerView.SetVictoryUiView(victoryView);

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
            charSelectView.SetSkillTreeUiView(skillTreeUiView);
            charSelectView.Initialize((selectedClass, isDevMode, isSkillTestMode, startPhase) =>
            {
                _selectedClass = selectedClass;
                playerView.SetClassType(selectedClass);

                // Re-apply class-specific skill tree progression stats and elemental flags
                playerView.Entity.Stats = HappyShoot.Domain.Progression.SkillTreeApplier.ApplyStats(playerView.Entity.Stats, skillTreeManager, selectedClass);
                playerView.Entity.ProgressionFlags = HappyShoot.Domain.Progression.SkillTreeApplier.BuildFlags(skillTreeManager, selectedClass);
                foreach (var s in playerView.Entity.Skills)
                {
                    SkillConfigRepository.Instance.ApplyConfigToSkillLevel(s, s.Level);
                }
                Debug.Log($"[GameBootstrap] Hero Selected & Ready: {selectedClass} (DevMode: {isDevMode}, SkillTest: {isSkillTestMode}, StartPhase: {startPhase})");

                if (isDevMode)
                {
                    devConsoleView.Show();
                }

                if (startPhase > 1)
                {
                    Debug.Log($"[GameBootstrap] Starting directly in Phase {startPhase} via Dev Mode!");
                    spawnerView.JumpToPhase(startPhase);
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

        private void RegisterAllSkills(SkillRewardManager rewardManager) => SkillRegistryHelper.RegisterAllSkills(rewardManager);

        private void RegisterAllPassives(SkillRewardManager rewardManager) => SkillRegistryHelper.RegisterAllPassives(rewardManager);

        private void RegisterAllEvolutions(SkillEvolutionManager evolutionManager) => SkillRegistryHelper.RegisterAllEvolutions(evolutionManager);
    }
}
