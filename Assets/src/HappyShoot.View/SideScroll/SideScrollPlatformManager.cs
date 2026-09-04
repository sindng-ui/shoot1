using System;
using System.Collections.Generic;
using UnityEngine;
using HappyShoot.Domain.Spatial;
using HappyShoot.View.Cameras;
using HappyShoot.View.Player;

namespace HappyShoot.View.SideScroll
{
    /// <summary>
    /// Master platform manager for side-scrolling dimension mode.
    /// Manages:
    /// 1. Dynamic stepping platforms with variable heights (low -1.8f, mid -1.0f, high -0.2f).
    /// 2. 100% unified visual & collision surfaces (Zero sinking glitch).
    /// 3. Chasm gaps and 2-lives fall elimination rule.
    /// Strictly modular and under 500 lines.
    /// </summary>
    public class SideScrollPlatformManager : MonoBehaviour
    {
        public static SideScrollPlatformManager Instance { get; private set; }

        public const float PlatformSpacing = 5.0f;
        public const float PlatformWidth = 3.3f;
        public const float HalfWidth = PlatformWidth * 0.5f; // 1.65m (Gap = 1.7m)

        private PlayerView _playerView;
        private Action<int> _onLifeChanged;
        private Action _onEliminated;

        private int _remainingLives = 2;
        private float _lastSafePlatformCenterX = 0f;
        private float _lastSafeSurfaceY = -1.8f;
        private bool _isFalling;
        private float _fallVelocity;
        private const float FallGravity = -32f;
        private const float OffScreenFallLimitY = -10.5f; // Deep off-screen abyss

        private static Sprite _platformSprite;

        public class PlatformInstance
        {
            public int Index;
            public float CenterX;
            public float Width;
            public float SurfaceY;
            public GameObject Go;
        }

        private readonly Dictionary<int, PlatformInstance> _platforms = new Dictionary<int, PlatformInstance>();

        public int RemainingLives => _remainingLives;
        public bool IsFalling => _isFalling;

        private void Awake()
        {
            Instance = this;
        }

        public void Initialize(PlayerView playerView, Action<int> onLifeChanged, Action onEliminated)
        {
            _playerView = playerView;
            _onLifeChanged = onLifeChanged;
            _onEliminated = onEliminated;
            _remainingLives = 2;
            _isFalling = false;
            _fallVelocity = 0f;
            _lastSafePlatformCenterX = 0f;
            _lastSafeSurfaceY = -1.8f;

            _onLifeChanged?.Invoke(_remainingLives);

            // Initial spawn of stepping platforms
            UpdatePlatforms(0f);
        }

        public float CalculateSurfaceY(int index)
        {
            // First 3 platforms are always stable base ground for safe start
            if (index <= 2) return -1.8f;

            // Heights: -1.8f (ground), -1.2f (low hill), -0.6f (mid), 0.0f (high), 0.6f (sky plateau)
            float[] heights = { -1.8f, -1.2f, -0.6f, 0.0f, 0.6f };
            int heightIdx = GetHeightIndex(index);
            return heights[heightIdx];
        }

        private int GetHeightIndex(int index)
        {
            if (index <= 2) return 0; // ground = index 0
            int current = 0;
            for (int i = 3; i <= index; i++)
            {
                int hash = ((i * 7919) ^ (i * 104729 + 31)) & 0x7FFFFFFF;
                int action = hash % 10;

                // Varied level design:
                // 0~3 (40%): Hold current elevation (creates plateaus/runways to run and fight on)
                // 4~6 (30%): Gentle climb (+1 level, +0.6m - very comfortable and forgiving to jump up)
                // 7 (10%): Steep drop (-2 levels, -1.2m plunge)
                // 8 (10%): Step down (-1 level, -0.6m)
                // 9 (10%): Reset to base ground (-1.8f cliff dive)
                if (action >= 0 && action <= 3)
                {
                    // Maintain height
                }
                else if (action >= 4 && action <= 6)
                {
                    current = Mathf.Min(4, current + 1); // Climb at most +1 level for easy jump
                }
                else if (action == 7)
                {
                    current = Mathf.Max(0, current - 2); // Plunge 2 levels down
                }
                else if (action == 8)
                {
                    current = Mathf.Max(0, current - 1); // Step 1 level down
                }
                else
                {
                    current = 0; // Drop to base ground
                }
            }
            return current;
        }

        /// <summary>
        /// Checks whether there is a platform beneath horizontal position x, returning its surface height and platform center X.
        /// </summary>
        public bool TryGetPlatformAtX(float x, out float surfaceY, out float platformCenterX)
        {
            surfaceY = -999f;
            platformCenterX = x;
            int nearestIdx = Mathf.RoundToInt(x / PlatformSpacing);

            // Check nearest and adjacent platforms
            for (int offset = -1; offset <= 1; offset++)
            {
                int idx = nearestIdx + offset;
                if (_platforms.TryGetValue(idx, out var p))
                {
                    if (Mathf.Abs(x - p.CenterX) <= p.Width * 0.5f)
                    {
                        surfaceY = p.SurfaceY;
                        platformCenterX = p.CenterX;
                        return true;
                    }
                }
            }
            return false;
        }

        public bool TryGetPlatformAtX(float x, out float surfaceY)
        {
            return TryGetPlatformAtX(x, out surfaceY, out _);
        }

        /// <summary>
        /// Precision landing detection using continuous vertical sweep between prevY and currentY.
        /// Prevents premature snapping or teleporting while airborne, with generous landing window.
        /// </summary>
        public bool TryGetPlatformLanding(float x, float prevY, float currentY, out float surfaceY)
        {
            surfaceY = -999f;
            if (TryGetPlatformAtX(x, out surfaceY))
            {
                // Generous landing condition: Player's feet crossed the surface from above or are right at the surface
                if (currentY <= surfaceY + 0.25f && prevY >= surfaceY - 0.45f)
                {
                    return true;
                }
            }
            return false;
        }

        public bool TryGetPlatformSurface(float x, float currentY, out float surfaceY)
        {
            return TryGetPlatformLanding(x, currentY, currentY, out surfaceY);
        }

        public float GetHighestSurfaceYAt(float x)
        {
            int idx = Mathf.RoundToInt(x / PlatformSpacing);
            if (_platforms.TryGetValue(idx, out var p))
            {
                if (Mathf.Abs(x - p.CenterX) <= p.Width * 0.5f)
                    return p.SurfaceY;
            }
            return -1.8f;
        }

        private void Update()
        {
            if (_playerView == null) return;

            Vector3 playerPos = _playerView.transform.position;
            UpdatePlatforms(playerPos.x);

            // Handle Chasm Abyss Fall
            if (_isFalling)
            {
                _fallVelocity += FallGravity * Time.deltaTime;
                playerPos.y += _fallVelocity * Time.deltaTime;
                _playerView.transform.position = playerPos;
                _playerView.Entity?.SetPosition(new Vector2D(playerPos.x, playerPos.y));

                // Fallen deep into the abyss below screen viewport: Respawn or Eliminate
                if (playerPos.y <= OffScreenFallLimitY)
                {
                    HandleChasmFall();
                }
            }
        }

        private void UpdatePlatforms(float playerX)
        {
            int currentIdx = Mathf.RoundToInt(playerX / PlatformSpacing);
            int minIdx = currentIdx - 3;
            int maxIdx = currentIdx + 9; // Extended forward range for right-biased camera view

            // Spawn needed platforms
            for (int i = minIdx; i <= maxIdx; i++)
            {
                if (!_platforms.ContainsKey(i))
                {
                    SpawnPlatform(i);
                }
            }

            // Remove far platforms
            var toRemove = new List<int>();
            foreach (var kvp in _platforms)
            {
                if (kvp.Key < minIdx - 2 || kvp.Key > maxIdx + 4)
                {
                    toRemove.Add(kvp.Key);
                }
            }

            for (int i = 0; i < toRemove.Count; i++)
            {
                int k = toRemove[i];
                if (_platforms[k].Go != null) Destroy(_platforms[k].Go);
                _platforms.Remove(k);
            }
        }

        private void SpawnPlatform(int index)
        {
            float centerX = index * PlatformSpacing;
            float surfaceY = CalculateSurfaceY(index);

            var pGo = new GameObject($"Platform_{index}");
            pGo.transform.SetParent(transform, false);
            // Center of sprite is offset so top edge aligns with surfaceY
            pGo.transform.position = new Vector3(centerX, surfaceY - 1.0f, 0f);
            pGo.transform.localScale = new Vector3(PlatformWidth, 2.0f, 1f);

            var sr = pGo.AddComponent<SpriteRenderer>();
            sr.sprite = GetOrCreatePlatformSprite();
            sr.sortingOrder = 6;

            var instance = new PlatformInstance
            {
                Index = index,
                CenterX = centerX,
                Width = PlatformWidth,
                SurfaceY = surfaceY,
                Go = pGo
            };
            _platforms[index] = instance;
        }

        public void RegisterSafePlatform(float centerX, float surfaceY)
        {
            _lastSafePlatformCenterX = centerX;
            _lastSafeSurfaceY = surfaceY;
        }

        public void TriggerChasmFall()
        {
            if (_isFalling) return;
            _isFalling = true;
            _fallVelocity = -6.5f;

            _playerView.EventBus?.Publish(new Domain.Events.PlaySoundEvent(Domain.Events.SoundEffectType.PlayerHurt));
            CameraFollowView.Instance?.TriggerShake(null, 0.45f, 0.35f);
        }

        private void HandleChasmFall()
        {
            _remainingLives--;
            _onLifeChanged?.Invoke(_remainingLives);

            if (_remainingLives > 0)
            {
                // 1st Fall: Rewind and respawn on last safe platform!
                _isFalling = false;
                _fallVelocity = 0f;

                Vector3 respawnPos = new Vector3(_lastSafePlatformCenterX, _lastSafeSurfaceY, 0f);
                _playerView.transform.position = respawnPos;
                _playerView.Entity?.SetPosition(new Vector2D(respawnPos.x, respawnPos.y));

                // Reset player input grounded state
                var input = _playerView.GetComponent<PlayerInputHandler>();
                if (input != null) input.ResetGroundedState(_lastSafeSurfaceY);

                _playerView.EventBus?.Publish(new Domain.Events.PlaySoundEvent(Domain.Events.SoundEffectType.MagicExplosion));
                CameraFollowView.Instance?.TriggerShake(null, 0.5f, 0.4f);
            }
            else
            {
                // 2nd Fall: Elimination! Player completely disappears into the abyss
                _isFalling = false;
                _fallVelocity = 0f;

                if (_playerView != null)
                {
                    _playerView.gameObject.SetActive(false);
                }

                _onEliminated?.Invoke();
            }
        }

        private void OnDestroy()
        {
            if (_playerView != null && !_playerView.gameObject.activeSelf)
            {
                _playerView.gameObject.SetActive(true);
            }

            foreach (var kvp in _platforms)
            {
                if (kvp.Value.Go != null) Destroy(kvp.Value.Go);
            }
            _platforms.Clear();
            if (Instance == this) Instance = null;
        }

        public static Sprite GetOrCreatePlatformSprite(int size = 32)
        {
            if (_platformSprite != null) return _platformSprite;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            var pixels = new Color[size * size];

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    if (y >= 29) // Glowing cyan electric rail surface
                        pixels[y * size + x] = new Color(0.35f, 0.95f, 1.0f, 1.0f);
                    else if (y >= 26) // Deep electric blue rim
                        pixels[y * size + x] = new Color(0.12f, 0.55f, 0.95f, 1.0f);
                    else if (y >= 21) // Ancient rune casing
                        pixels[y * size + x] = new Color(0.20f, 0.14f, 0.35f, 1.0f);
                    else // Solid dark metallic foundation block
                        pixels[y * size + x] = new Color(0.08f, 0.05f, 0.16f, 1.0f);
                }
            }
            tex.SetPixels(pixels);
            tex.Apply();
            _platformSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
            return _platformSprite;
        }
    }
}
