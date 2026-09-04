using System;
using UnityEngine;
using UnityEngine.UI;
using HappyShoot.Domain.Session;
using HappyShoot.View.Background;
using HappyShoot.View.Cameras;
using HappyShoot.View.Companion;
using HappyShoot.View.Monsters;
using HappyShoot.View.Player;
using HappyShoot.View.UI;
using HappyShoot.View.Utils;

namespace HappyShoot.View.SideScroll
{
    /// <summary>
    /// Master controller for the Side-Scrolling Dimension Corridor mode.
    /// Manages transition, horizontal camera tracking, 300m distance progress HUD,
    /// Overdrive buff, and connects Void Core defeat to final Stage Victory.
    /// Strictly modular and under 500 lines.
    /// </summary>
    public class SideScrollModeController : MonoBehaviour
    {
        public static SideScrollModeController Instance { get; private set; }

        private PlayerView _playerView;
        private CompanionManagerView _companionManager;
        private MonsterSpawnerView _mainSpawner;
        private StageVictoryUiView _victoryUiView;
        private CameraFollowView _cameraFollow;

        private SideScrollBackgroundView _bgView;
        private SideScrollMonsterSpawner _sideSpawner;
        private SideScrollPlatformManager _platformManager;
        private GameSessionEntity _gameSession;
        private HappyShoot.Domain.Gems.GemManager _gemManager;

        private bool _isActive;
        private int _currentLives = 2;
        private float _startX;
        private float _currentDistance;
        private const float TargetDistance = 300f;

        // UI elements
        private GameObject _hudGo;
        private Text _distanceText;
        private Image _progressBarFill;

        public GameSessionEntity GameSession => _gameSession;
        public bool IsActive => _isActive;

        private void Awake()
        {
            Instance = this;
        }

        public void Initialize(
            PlayerView playerView,
            CompanionManagerView companionManager,
            MonsterSpawnerView mainSpawner,
            StageVictoryUiView victoryUiView,
            CameraFollowView cameraFollow,
            GameSessionEntity gameSession = null,
            HappyShoot.Domain.Gems.GemManager gemManager = null)
        {
            _playerView = playerView;
            _companionManager = companionManager;
            _mainSpawner = mainSpawner;
            _victoryUiView = victoryUiView;
            _cameraFollow = cameraFollow;
            _gameSession = gameSession;
            _gemManager = gemManager;
        }

        public void EnterSideScrollMode()
        {
            if (_isActive || _playerView == null) return;
            _isActive = true;

            Debug.Log("[SideScrollModeController] 🌀 Entering Side-Scrolling Dimension Mode!");

            // 1. Completely hide top-down dungeon background tiles
            if (BackgroundManager.Instance != null)
            {
                BackgroundManager.Instance.gameObject.SetActive(false);
            }

            // 2. Suppress main top-down spawner and clear existing field monsters
            if (_mainSpawner != null)
            {
                _mainSpawner.IsSpawningSuppressed = true;
                _mainSpawner.DomainSpawner?.DespawnAll();
                var activeMonsterViews = FindObjectsByType<MonsterView>(FindObjectsSortMode.None);
                for (int m = 0; m < activeMonsterViews.Length; m++)
                {
                    activeMonsterViews[m].gameObject.SetActive(false);
                }
            }

            // 3. Warp player and companions to origin line (X = 0, Y = -1.8f)
            _startX = 0f;
            _currentDistance = 0f;
            _playerView.Entity.SetPosition(new Domain.Spatial.Vector2D(0f, -1.8f));
            _playerView.transform.position = new Vector3(0f, -1.8f, 0f);

            // 2. Configure PlayerInputHandler for horizontal-only movement
            var inputHandler = _playerView.GetComponent<PlayerInputHandler>();
            if (inputHandler != null)
            {
                inputHandler.IsSideScrollMode = true;
                inputHandler.SideScrollFixedY = -1.8f;
            }

            // 3. Hide Companions so only the Wizard tackles the Side-Scrolling Dimension Corridor!
            var companions = FindObjectsByType<CompanionView>(FindObjectsSortMode.None);
            for (int i = 0; i < companions.Length; i++)
            {
                companions[i].gameObject.SetActive(false);
            }

            // 4. Lock Camera Y, adjust wide view, and bias camera to the right so player stays on the left ~35% of screen
            if (_cameraFollow != null)
            {
                _cameraFollow.LockYAxis = true;
                _cameraFollow.LockedY = -0.5f;
                _cameraFollow.TargetOrthoSize = 7.5f;
                _cameraFollow.OffsetX = 4.5f;
            }

            // 5. Spawn Parallax Dimension Background
            var bgGo = new GameObject("SideScrollBackground");
            _bgView = bgGo.AddComponent<SideScrollBackgroundView>();
            _bgView.Initialize(UnityEngine.Camera.main.transform);

            // 6. Spawn SideScroll Monster & Boss Spawner
            var spawnerGo = new GameObject("SideScrollMonsterSpawner");
            _sideSpawner = spawnerGo.AddComponent<SideScrollMonsterSpawner>();
            _sideSpawner.Initialize(_playerView, _mainSpawner, OnVoidCoreDefeated);

            // 6-1. Initialize Platform Manager with 2-lives chasm fall rule
            _currentLives = 2;
            _platformManager = gameObject.GetComponent<SideScrollPlatformManager>();
            if (_platformManager == null) _platformManager = gameObject.AddComponent<SideScrollPlatformManager>();
            _platformManager.Initialize(_playerView, OnLifeChanged, OnChasmEliminated);

            // 7. Arcane Overdrive: Boost player attack tempo and cooldown
            if (_playerView.Entity != null)
            {
                var s = _playerView.Entity.Stats;
                _playerView.Entity.Stats = new Domain.Entities.CharacterStats(
                    s.MaxHealth, s.HealthRegen, s.MoveSpeed * 1.15f, s.AttackPowerMultiplier * 1.35f,
                    s.Armor, s.CritChance + 0.15f, s.CritDamageMultiplier, 0.50f, // 50% CDR
                    s.AreaMultiplier * 1.2f, s.ProjectileSpeedMultiplier * 1.3f, s.ExtraProjectiles + 1, s.PickupRadius * 1.5f);
            }

            // 8. Build Top HUD for Dimension Rush
            BuildHud();

            // 9. Activate Gold Rush: Suppress exp gems & subscribe to gold drop
            if (_gemManager != null)
            {
                _gemManager.IsSideScrollMode = true;
            }
            _playerView.EventBus?.Subscribe<Domain.Events.MonsterDiedEvent>(OnSideScrollMonsterDied);

            // 10. Sound and Juice
            _playerView.EventBus?.Publish(new Domain.Events.PlaySoundEvent(Domain.Events.SoundEffectType.LevelUp));
            _cameraFollow?.TriggerShake("meteor_strike", 0.5f, 0.35f);
        }

        private void BuildHud()
        {
            _hudGo = new GameObject("SideScrollHUD");
            var canvas = _hudGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 80;

            var scaler = _hudGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            _hudGo.AddComponent<GraphicRaycaster>();

            // Banner Container
            var bannerGo = new GameObject("Banner");
            bannerGo.transform.SetParent(_hudGo.transform, false);
            var bannerRt = bannerGo.AddComponent<RectTransform>();
            bannerRt.anchorMin = new Vector2(0.5f, 1f);
            bannerRt.anchorMax = new Vector2(0.5f, 1f);
            bannerRt.pivot = new Vector2(0.5f, 1f);
            bannerRt.anchoredPosition = new Vector2(0f, -25f);
            bannerRt.sizeDelta = new Vector2(620f, 75f);

            var bannerBg = bannerGo.AddComponent<Image>();
            bannerBg.sprite = SpriteHelper.GetOrCreateWhiteSprite();
            bannerBg.color = new Color(0.10f, 0.05f, 0.22f, 0.92f);

            var outline = bannerGo.AddComponent<Outline>();
            outline.effectColor = new Color(0.6f, 0.2f, 1f, 0.8f);
            outline.effectDistance = new Vector2(2f, -2f);

            // Title / Distance Text
            var titleGo = new GameObject("DistanceText");
            titleGo.transform.SetParent(bannerGo.transform, false);
            var titleRt = titleGo.AddComponent<RectTransform>();
            titleRt.anchorMin = Vector2.zero;
            titleRt.anchorMax = Vector2.one;
            titleRt.sizeDelta = Vector2.zero;

            _distanceText = titleGo.AddComponent<Text>();
            _distanceText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _distanceText.fontSize = 20;
            _distanceText.alignment = TextAnchor.MiddleCenter;
            _distanceText.color = Color.white;
            _distanceText.text = "🌀 [차원 돌파]: 0m / 300m (⚡ 마법 오버드라이브 가동!)";

            // Progress Bar Under Banner
            var barBgGo = new GameObject("ProgressBarBg");
            barBgGo.transform.SetParent(bannerGo.transform, false);
            var barBgRt = barBgGo.AddComponent<RectTransform>();
            barBgRt.anchorMin = new Vector2(0.05f, 0f);
            barBgRt.anchorMax = new Vector2(0.95f, 0f);
            barBgRt.pivot = new Vector2(0.5f, 0f);
            barBgRt.anchoredPosition = new Vector2(0f, 6f);
            barBgRt.sizeDelta = new Vector2(0f, 8f);

            var barBgImg = barBgGo.AddComponent<Image>();
            barBgImg.sprite = SpriteHelper.GetOrCreateWhiteSprite();
            barBgImg.color = new Color(0.2f, 0.2f, 0.35f, 0.7f);

            var fillGo = new GameObject("ProgressBarFill");
            fillGo.transform.SetParent(barBgGo.transform, false);
            var fillRt = fillGo.AddComponent<RectTransform>();
            fillRt.anchorMin = Vector2.zero;
            fillRt.anchorMax = new Vector2(0f, 1f);
            fillRt.pivot = new Vector2(0f, 0.5f);
            fillRt.sizeDelta = Vector2.zero;

            _progressBarFill = fillGo.AddComponent<Image>();
            _progressBarFill.sprite = SpriteHelper.GetOrCreateWhiteSprite();
            _progressBarFill.color = new Color(0.2f, 0.9f, 1.0f, 0.95f);
        }

        private void Update()
        {
            if (!_isActive || _playerView == null) return;

            _currentDistance = Mathf.Max(0f, _playerView.transform.position.x - _startX);
            float progress = Mathf.Clamp01(_currentDistance / TargetDistance);

            if (_distanceText != null)
            {
                string lifeIcons = _currentLives >= 2 ? "❤️❤️" : "❤️💔";
                if (_currentDistance >= TargetDistance)
                    _distanceText.text = $"⚔️ [{lifeIcons} 차원의 핵 출현!] 차원 핵을 파괴하여 탈출하세요!";
                else
                    _distanceText.text = $"🌀 [{lifeIcons} 차원 돌파]: {_currentDistance:F0}m / {TargetDistance:F0}m (⚡ 쿨다운 -50% 폭주!)";
            }

            if (_progressBarFill != null)
            {
                _progressBarFill.rectTransform.anchorMax = new Vector2(progress, 1f);
            }
        }

        private void OnLifeChanged(int lives)
        {
            _currentLives = lives;
            if (_distanceText != null && _currentLives == 1)
            {
                _distanceText.text = "⚠️ [차원 틈새 추락!] 시공간 마법으로 부활! (남은 생명: ❤️💔 - 1번 더 떨어지면 탈락!)";
            }
        }

        private void OnChasmEliminated()
        {
            Debug.Log("[SideScrollModeController] 💀 2nd Chasm Fall! Player Eliminated from Dimension Run!");
            if (_distanceText != null)
            {
                _distanceText.text = "💀 [차원 추락 탈락!] 시공간 왜곡에 휩쓸려 원래 세계로 귀환합니다...";
            }

            _playerView?.EventBus?.Publish(new Domain.Events.PlaySoundEvent(Domain.Events.SoundEffectType.PlayerHurt, 1.4f));
            Invoke(nameof(CompleteVictory), 2.2f);
        }

        private void OnVoidCoreDefeated()
        {
            Debug.Log("[SideScrollModeController] 🏆 Void Core Defeated! Triggering Final True Victory!");

            if (_distanceText != null)
            {
                _distanceText.text = "🎉 [차원 정복 성공!] 황금빛 탈출 포탈이 열렸습니다!";
            }

            // Cleanup side scroll mode after small delay
            Invoke(nameof(CompleteVictory), 1.8f);
        }

        private void OnSideScrollMonsterDied(Domain.Events.MonsterDiedEvent evt)
        {
            if (!_isActive || _playerView == null) return;

            // Spawn juicy bouncing Gold Coin in side-scroll mode
            var coinGo = new GameObject("SideScrollGoldCoin");
            var coinView = coinGo.AddComponent<SideScrollGoldCoinView>();
            int gold = (evt.MonsterType == Domain.Entities.MonsterType.Golem) ? 35 : 10;
            coinView.Initialize(_playerView, _gameSession, gold, new Vector3((float)evt.Position.X, (float)evt.Position.Y, 0f));
        }

        private void CompleteVictory()
        {
            if (_platformManager != null) Destroy(_platformManager);
            if (_hudGo != null) Destroy(_hudGo);
            if (_bgView != null) Destroy(_bgView.gameObject);
            if (_sideSpawner != null) _sideSpawner.Cleanup();

            if (_gemManager != null)
            {
                _gemManager.IsSideScrollMode = false;
            }
            _playerView?.EventBus?.Unsubscribe<Domain.Events.MonsterDiedEvent>(OnSideScrollMonsterDied);

            _isActive = false;

            // Restore top-down systems if returning to normal
            if (_cameraFollow != null)
            {
                _cameraFollow.LockYAxis = false;
                _cameraFollow.TargetOrthoSize = 5.0f;
                _cameraFollow.OffsetX = 0f;
            }
            if (BackgroundManager.Instance != null)
            {
                BackgroundManager.Instance.gameObject.SetActive(true);
            }
            if (_mainSpawner != null)
            {
                _mainSpawner.IsSpawningSuppressed = false;
            }

            // Restore companions for top-down normal gameplay
            var companions = FindObjectsByType<CompanionView>(FindObjectsSortMode.None);
            for (int i = 0; i < companions.Length; i++)
            {
                companions[i].IsSideScrollMode = false;
                companions[i].gameObject.SetActive(true);
            }

            // Trigger final true victory panel
            _victoryUiView?.ShowVictoryPopup();
        }

        private void OnDestroy()
        {
            if (_cameraFollow != null)
            {
                _cameraFollow.OffsetX = 0f;
            }
            if (_gemManager != null)
            {
                _gemManager.IsSideScrollMode = false;
            }
            _playerView?.EventBus?.Unsubscribe<Domain.Events.MonsterDiedEvent>(OnSideScrollMonsterDied);
            if (Instance == this) Instance = null;
        }
    }
}
