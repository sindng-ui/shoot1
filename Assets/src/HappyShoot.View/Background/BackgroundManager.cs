using UnityEngine;

namespace HappyShoot.View.Background
{
    /// <summary>
    /// Infinite background tiling system controller.
    /// Manages a 3x3 grid of 24m x 24m procedural stone floor tiles (total 72m x 72m span)
    /// that seamlessly wrap around the camera as the player moves across the world.
    /// Guarantees zero GC allocations per frame and zero seams.
    /// </summary>
    public class BackgroundManager : MonoBehaviour
    {
        private const int GridDimension = 3; // 3x3 Grid (9 tiles total)
        private const float TileSize = BackgroundTileView.TileSize; // 24.0m
        private const float GridSpan = GridDimension * TileSize; // 72.0m
        private const float HalfGridSpan = GridSpan * 0.5f; // 36.0m

        private Camera _mainCamera;
        private Transform _cameraTransform;
        private readonly BackgroundTileView[] _tiles = new BackgroundTileView[GridDimension * GridDimension];
        private BackgroundAmbientDustView _ambientDustView;

        public static BackgroundManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
                return;
            }
        }

        public void Initialize(Camera cam = null)
        {
            _mainCamera = cam != null ? cam : Camera.main;
            if (_mainCamera != null)
            {
                _cameraTransform = _mainCamera.transform;
            }

            Vector3 startPos = _cameraTransform != null ? _cameraTransform.position : Vector3.zero;

            // Spawn 3x3 Tile Grid
            int tileIndex = 0;
            for (int y = -1; y <= 1; y++)
            {
                for (int x = -1; x <= 1; x++)
                {
                    var tileGo = new GameObject($"BgTile_{x + 1}_{y + 1}");
                    tileGo.transform.SetParent(transform);

                    var tileView = tileGo.AddComponent<BackgroundTileView>();
                    float worldX = SnapToTileGrid(startPos.x) + (x * TileSize);
                    float worldY = SnapToTileGrid(startPos.y) + (y * TileSize);

                    tileView.SetWorldPosition(new Vector2(worldX, worldY));
                    int variation = CalculateVariation(worldX, worldY);
                    tileView.Initialize(x, y, variation);

                    _tiles[tileIndex++] = tileView;
                }
            }

            // Spawn Ambient Floating Dust Motes for depth
            var dustGo = new GameObject("AmbientDustSystem");
            dustGo.transform.SetParent(transform);
            _ambientDustView = dustGo.AddComponent<BackgroundAmbientDustView>();
            _ambientDustView.Initialize(_cameraTransform);

            Debug.Log($"[BackgroundManager] Initialized 3x3 Infinite Background Grid ({GridSpan}m x {GridSpan}m) with 4-variation procedural dungeon tiles.");
        }

        private void LateUpdate()
        {
            if (_cameraTransform == null)
            {
                if (Camera.main != null)
                {
                    _cameraTransform = Camera.main.transform;
                }
                else
                {
                    return;
                }
            }

            Vector3 camPos = _cameraTransform.position;

            // Seamless wrap-around check for all 9 tiles
            for (int i = 0; i < _tiles.Length; i++)
            {
                var tile = _tiles[i];
                if (tile == null) continue;

                Vector3 tilePos = tile.GetWorldPosition();
                bool moved = false;

                // Horizontal wrap
                float diffX = tilePos.x - camPos.x;
                if (diffX > HalfGridSpan)
                {
                    tilePos.x -= GridSpan;
                    moved = true;
                }
                else if (diffX < -HalfGridSpan)
                {
                    tilePos.x += GridSpan;
                    moved = true;
                }

                // Vertical wrap
                float diffY = tilePos.y - camPos.y;
                if (diffY > HalfGridSpan)
                {
                    tilePos.y -= GridSpan;
                    moved = true;
                }
                else if (diffY < -HalfGridSpan)
                {
                    tilePos.y += GridSpan;
                    moved = true;
                }

                if (moved)
                {
                    tile.SetWorldPosition(new Vector2(tilePos.x, tilePos.y));
                    // Update variation deterministically based on new world coordinates
                    int newVariation = CalculateVariation(tilePos.x, tilePos.y);
                    tile.SetVariation(newVariation);
                }
            }
        }

        private static float SnapToTileGrid(float pos)
        {
            return Mathf.Round(pos / TileSize) * TileSize;
        }

        /// <summary>
        /// Calculates a deterministic tile variation (0 to 3) based on world tile coordinates,
        /// ensuring a rich, non-repetitive dungeon floor layout.
        /// </summary>
        private static int CalculateVariation(float worldX, float worldY)
        {
            int gridX = Mathf.RoundToInt(worldX / TileSize);
            int gridY = Mathf.RoundToInt(worldY / TileSize);
            int hash = (gridX * 73856093) ^ (gridY * 19349663);
            return Mathf.Abs(hash) % 4;
        }
    }
}
