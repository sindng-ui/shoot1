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
            mainCam.orthographicSize = 6.0f;
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

            // 3. Create Managers
            var spawnerGo = new GameObject("MonsterSpawner");
            var spawnerView = spawnerGo.AddComponent<MonsterSpawnerView>();
            spawnerView.Initialize(playerView);

            var projGo = new GameObject("ProjectileManager");
            var projManagerView = projGo.AddComponent<ProjectileManagerView>();

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
            RegisterAllSkills(rewardManager);
            RegisterAllPassives(rewardManager);

            var levelUiGo = new GameObject("LevelUpUI");
            var levelUiView = levelUiGo.AddComponent<LevelUpUiView>();

            var levelSystem = new LevelSystem();
            levelUiView.Initialize(levelSystem, rewardManager);

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

            var hudGo = new GameObject("InGameHUD");
            var hudView = hudGo.AddComponent<InGameHudView>();
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
            pauseView.Initialize(_gameSession, playerView.EventBus);

            var gameOverGo = new GameObject("GameOverUI");
            var gameOverView = gameOverGo.AddComponent<GameOverResultUiView>();
            gameOverView.Initialize(_gameSession, playerView.EventBus, metaShopManager, metaShopView);

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
            rewardManager.RegisterSkill("slash", "Greatsword Slash", "Melee arc slash hitting all front enemies",
                () => new CompositeSkill("slash", "Greatsword Slash", new CooldownTrigger(1.2f), new ClosestEnemyTargeter(), new GreatswordSlashEffect(35f, 2.5f)));

            rewardManager.RegisterSkill("bow", "Piercing Bow", "Fires piercing arrows at the closest enemy",
                () => new CompositeSkill("bow", "Piercing Bow", new CooldownTrigger(0.8f), new ClosestEnemyTargeter(), new PiercingArrowEffect(22f, 15f, 3)));

            rewardManager.RegisterSkill("explosion", "Arcane Explosion", "Creates magical explosions at target positions",
                () => new CompositeSkill("explosion", "Arcane Explosion", new CooldownTrigger(1.5f), new ClosestEnemyTargeter(), new ArcaneExplosionEffect(40f, 2.0f)));

            rewardManager.RegisterSkill("orbital", "Orbiting Blades", "Spinning blades revolving around the player",
                () => new CompositeSkill("orbital", "Orbiting Blades", new CooldownTrigger(0.3f), new ClosestEnemyTargeter(), new OrbitingBladesEffect(25f, 2.0f, 4.0f, 2)));
        }

        private void RegisterAllPassives(SkillRewardManager rewardManager)
        {
            rewardManager.RegisterPassive("passive_fang", "Vampire Fang", "+15% Damage & Blood Leech", 5, (p, lv) =>
            {
                var s = p.Stats;
                p.Stats = new CharacterStats(s.MaxHealth, s.HealthRegen, s.MoveSpeed, 1.0f + 0.15f * lv, s.Armor, s.CritChance, s.CritDamageMultiplier, s.CooldownReduction, s.AreaMultiplier, s.ProjectileSpeedMultiplier, s.ExtraProjectiles, s.PickupRadius);
            });

            rewardManager.RegisterPassive("passive_feather", "Wind Feather", "+12% Move & Projectile Speed", 5, (p, lv) =>
            {
                var s = p.Stats;
                p.Stats = new CharacterStats(s.MaxHealth, s.HealthRegen, 5.0f * (1.0f + 0.12f * lv), s.AttackPowerMultiplier, s.Armor, s.CritChance, s.CritDamageMultiplier, s.CooldownReduction, s.AreaMultiplier, 1.0f + 0.15f * lv, s.ExtraProjectiles, s.PickupRadius);
            });

            rewardManager.RegisterPassive("passive_rune", "Mana Rune", "+10% Cooldown Reduction & +15% Area", 5, (p, lv) =>
            {
                var s = p.Stats;
                p.Stats = new CharacterStats(s.MaxHealth, s.HealthRegen, s.MoveSpeed, s.AttackPowerMultiplier, s.Armor, s.CritChance, s.CritDamageMultiplier, 0.10f * lv, 1.0f + 0.15f * lv, s.ProjectileSpeedMultiplier, s.ExtraProjectiles, s.PickupRadius);
            });

            rewardManager.RegisterPassive("passive_armor", "Iron Armor", "+5 Armor (Damage Mitigation)", 5, (p, lv) =>
            {
                var s = p.Stats;
                p.Stats = new CharacterStats(s.MaxHealth, s.HealthRegen, s.MoveSpeed, s.AttackPowerMultiplier, 5f * lv, s.CritChance, s.CritDamageMultiplier, s.CooldownReduction, s.AreaMultiplier, s.ProjectileSpeedMultiplier, s.ExtraProjectiles, s.PickupRadius);
            });

            rewardManager.RegisterPassive("passive_ring", "Golden Ring", "+1.5m Pickup Magnet Radius", 5, (p, lv) =>
            {
                var s = p.Stats;
                p.Stats = new CharacterStats(s.MaxHealth, s.HealthRegen, s.MoveSpeed, s.AttackPowerMultiplier, s.Armor, s.CritChance, s.CritDamageMultiplier, s.CooldownReduction, s.AreaMultiplier, s.ProjectileSpeedMultiplier, s.ExtraProjectiles, 2.0f + 1.5f * lv);
            });

            rewardManager.RegisterPassive("passive_heart", "Heart Pendant", "+30 Max Health & +1.5 HP/s Regen", 5, (p, lv) =>
            {
                var s = p.Stats;
                p.Stats = new CharacterStats(100f + 30f * lv, 1.5f * lv, s.MoveSpeed, s.AttackPowerMultiplier, s.Armor, s.CritChance, s.CritDamageMultiplier, s.CooldownReduction, s.AreaMultiplier, s.ProjectileSpeedMultiplier, s.ExtraProjectiles, s.PickupRadius);
            });
        }

        private void RegisterAllEvolutions(SkillEvolutionManager evolutionManager)
        {
            evolutionManager.RegisterRecipe(new SkillEvolutionRecipe(
                "slash", "passive_fang", "blood_eater", "Blood Eater",
                () => new CompositeSkill("blood_eater", "Blood Eater", new CooldownTrigger(0.9f), new ClosestEnemyTargeter(), new BloodEaterEffect(55f, 3.5f, 0.08f))));

            evolutionManager.RegisterRecipe(new SkillEvolutionRecipe(
                "bow", "passive_feather", "storm_bow", "Storm Bow",
                () => new CompositeSkill("storm_bow", "Storm Bow", new CooldownTrigger(0.6f), new ClosestEnemyTargeter(), new StormArrowEffect(30f, 18f, 8))));

            evolutionManager.RegisterRecipe(new SkillEvolutionRecipe(
                "explosion", "passive_rune", "meteor_strike", "Meteor Strike",
                () => new CompositeSkill("meteor_strike", "Meteor Strike", new CooldownTrigger(1.2f), new ClosestEnemyTargeter(), new MeteorStrikeEffect(90f, 4.0f))));
        }
    }
}
