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
using HappyShoot.View.Effects;
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

            // 6. Setup Game Session, Meta Shop & InGame HUD / Pause / GameOver UI
            var metaStorage = new Shop.JsonPlayerPrefsStorage();
            var metaShopManager = new HappyShoot.Domain.Meta.MetaShopManager(metaStorage);

            // Apply permanent upgrades to player starting stats
            playerView.Entity.Stats = HappyShoot.Domain.Meta.MetaUpgradeApplier.ApplyUpgrades(playerView.Entity.Stats, metaShopManager.SaveData);

            // Apply saved custom sandbox stats if available
            var skillConfig = SkillConfigRepository.Instance.GetConfig();
            if (skillConfig?.CritStat != null && skillConfig.CritStat.IsCustom)
            {
                var s = playerView.Entity.Stats;
                var c = skillConfig.CritStat;
                playerView.Entity.Stats = new CharacterStats(s.MaxHealth, s.HealthRegen, c.MoveSpeed, c.AttackPowerMultiplier, c.Armor, c.CritChance, c.CritDamageMultiplier, c.CooldownReduction, s.AreaMultiplier, s.ProjectileSpeedMultiplier, s.ExtraProjectiles, s.PickupRadius);
            }

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

        private void RegisterAllSkills(SkillRewardManager rewardManager) => SkillRegistryHelper.RegisterAllSkills(rewardManager);

        private void RegisterAllPassives(SkillRewardManager rewardManager) => SkillRegistryHelper.RegisterAllPassives(rewardManager);

        private void RegisterAllEvolutions(SkillEvolutionManager evolutionManager) => SkillRegistryHelper.RegisterAllEvolutions(evolutionManager);
    }
}
