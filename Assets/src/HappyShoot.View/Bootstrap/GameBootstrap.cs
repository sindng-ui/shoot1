using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using HappyShoot.Domain.Entities;
using HappyShoot.Domain.Events;
using HappyShoot.Domain.Leveling;
using HappyShoot.Domain.Skills;
using HappyShoot.Domain.Skills.Effects;
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

            playerView.SetExternalSystems(spawnerView, projManagerView);

            // 4. Setup Timeline
            var timelineGo = new GameObject("WaveTimeline");
            timelineGo.AddComponent<WaveTimelineView>();

            // 5. Setup Level System and Reward Manager
            var rewardManager = new SkillRewardManager();
            RegisterAllSkills(rewardManager);

            var levelUiGo = new GameObject("LevelUpUI");
            var levelUiView = levelUiGo.AddComponent<LevelUpUiView>();

            var levelSystem = new LevelSystem();
            levelUiView.Initialize(levelSystem, rewardManager);

            Debug.Log("[GameBootstrap] Initialization Complete! Press WASD or Arrow Keys to move and survive!");
        }

        private void RegisterAllSkills(SkillRewardManager rewardManager)
        {
            rewardManager.RegisterSkill("slash", "Greatsword Slash", "Melee arc slash hitting all front enemies",
                () => new CompositeSkill("slash", "Greatsword Slash", new CooldownTrigger(1.2f), new ClosestEnemyTargeter(), new GreatswordSlashEffect(35f, 2.5f)));

            rewardManager.RegisterSkill("bow", "Piercing Bow", "Fires piercing arrows at the closest enemy",
                () => new CompositeSkill("bow", "Piercing Bow", new CooldownTrigger(0.8f), new ClosestEnemyTargeter(), new PiercingArrowEffect(22f, 15f, 3)));

            rewardManager.RegisterSkill("explosion", "Arcane Explosion", "Creates magical explosions at target positions",
                () => new CompositeSkill("explosion", "Arcane Explosion", new CooldownTrigger(1.5f), new ClosestEnemyTargeter(), new ArcaneExplosionEffect(40f, 2.0f)));
        }
    }
}
