using UnityEngine;

namespace HappyShoot.View.SideScroll
{
    /// <summary>
    /// Triple-layer parallax background dedicated to the side-scrolling dimension corridor.
    /// Layers:
    /// 0. Solid Screen-Filling Void Backdrop (SortingOrder -60, Alpha 1.0) - Completely blocks top-down tiles.
    /// 1. Far: Cosmic nebula & starry void (Scroll Speed 0.15x)
    /// 2. Mid: Floating ancient arcane monoliths (Scroll Speed 0.40x)
    /// 3. Near: Glowing runic ground highway & solid abyss foundation (Y = -2.1f ~ -4.0f)
    /// Strictly modular and under 500 lines.
    /// </summary>
    public class SideScrollBackgroundView : MonoBehaviour
    {
        private Transform _cameraTransform;
        private Transform _farLayer;
        private Transform _midLayer;
        private GameObject _backdropGo;

        private Vector3 _lastCamPos;
        private static Sprite _backdropSprite;
        private static Sprite _nebulaSprite;
        private static Sprite _monolithSprite;
        private static Sprite _railFloorSprite;

        public void Initialize(Transform cameraTransform)
        {
            _cameraTransform = cameraTransform;
            _lastCamPos = cameraTransform != null ? cameraTransform.position : Vector3.zero;

            BuildLayers();
        }

        private void BuildLayers()
        {
            // 0. Fullscreen Solid Cosmic Void Backdrop (Tracks camera directly, 100% opaque, hides any top-down tiles)
            if (_cameraTransform != null)
            {
                _backdropGo = new GameObject("Solid_Cosmic_Backdrop");
                _backdropGo.transform.SetParent(_cameraTransform, false);
                _backdropGo.transform.localPosition = new Vector3(0f, 0f, 15f);
                _backdropGo.transform.localScale = new Vector3(45f, 28f, 1f);

                var sr = _backdropGo.AddComponent<SpriteRenderer>();
                sr.sprite = GetOrCreateBackdropSprite();
                sr.sortingOrder = -60;
            }

            // 1. Far Layer (Nebula & Deep Starry Space)
            var farGo = new GameObject("Parallax_Far_Nebula");
            farGo.transform.SetParent(transform, false);
            _farLayer = farGo.transform;
            CreateTileRow(_farLayer, GetOrCreateNebulaSprite(), count: 8, spacing: 14f, yPos: 1.8f, sortingOrder: -40, scale: new Vector3(3.8f, 3.8f, 1f));

            // 2. Mid Layer (Floating Runes & Obelisk Pillars)
            var midGo = new GameObject("Parallax_Mid_Monoliths");
            midGo.transform.SetParent(transform, false);
            _midLayer = midGo.transform;
            CreateTileRow(_midLayer, GetOrCreateMonolithSprite(), count: 10, spacing: 10f, yPos: 0.6f, sortingOrder: -20, scale: new Vector3(2.2f, 2.8f, 1f));
        }

        private void CreateTileRow(Transform parent, Sprite sprite, int count, float spacing, float yPos, int sortingOrder, Vector3 scale)
        {
            float halfSpan = (count - 1) * spacing * 0.5f;
            for (int i = 0; i < count; i++)
            {
                var tileGo = new GameObject($"Tile_{i}");
                tileGo.transform.SetParent(parent, false);
                tileGo.transform.localPosition = new Vector3(-halfSpan + i * spacing, yPos, 0f);
                tileGo.transform.localScale = scale;

                var sr = tileGo.AddComponent<SpriteRenderer>();
                sr.sprite = sprite;
                sr.sortingOrder = sortingOrder;
            }
        }

        private void LateUpdate()
        {
            if (_cameraTransform == null) return;

            Vector3 deltaCam = _cameraTransform.position - _lastCamPos;
            _lastCamPos = _cameraTransform.position;

            float dx = deltaCam.x;

            if (_farLayer != null)
            {
                _farLayer.position += new Vector3(dx * 0.85f, 0f, 0f);
                RepositionChildren(_farLayer, _cameraTransform.position.x, spacing: 14f);
            }

            if (_midLayer != null)
            {
                _midLayer.position += new Vector3(dx * 0.55f, 0f, 0f);
                RepositionChildren(_midLayer, _cameraTransform.position.x, spacing: 10f);
            }
        }

        private void RepositionChildren(Transform layer, float camX, float spacing)
        {
            int childCount = layer.childCount;
            float totalSpan = childCount * spacing;
            float halfSpan = totalSpan * 0.5f;

            for (int i = 0; i < childCount; i++)
            {
                var child = layer.GetChild(i);
                float diff = child.position.x - camX;
                if (diff < -halfSpan)
                {
                    child.position += new Vector3(totalSpan, 0f, 0f);
                }
                else if (diff > halfSpan)
                {
                    child.position -= new Vector3(totalSpan, 0f, 0f);
                }
            }
        }

        private void OnDestroy()
        {
            if (_backdropGo != null)
            {
                Destroy(_backdropGo);
            }
        }

        private static Sprite GetOrCreateBackdropSprite(int size = 64)
        {
            if (_backdropSprite != null) return _backdropSprite;
            var tex = new Texture2D(size, size, TextureFormat.RGB24, false) { filterMode = FilterMode.Bilinear };
            var pixels = new Color[size * size];

            for (int y = 0; y < size; y++)
            {
                float t = (float)y / size;
                // Deep midnight violet gradient from bottom (deep navy) to top (abyssal purple)
                Color rowColor = Color.Lerp(new Color(0.04f, 0.02f, 0.10f, 1f), new Color(0.12f, 0.04f, 0.22f, 1f), t);
                for (int x = 0; x < size; x++)
                {
                    pixels[y * size + x] = rowColor;
                }
            }
            tex.SetPixels(pixels);
            tex.Apply();
            _backdropSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
            return _backdropSprite;
        }

        private static Sprite GetOrCreateNebulaSprite(int size = 64)
        {
            if (_nebulaSprite != null) return _nebulaSprite;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear };
            var pixels = new Color[size * size];

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float u = (float)x / size;
                    float v = (float)y / size;
                    float n = Mathf.PerlinNoise(u * 2.8f, v * 2.8f);
                    Color c = Color.Lerp(new Color(0.18f, 0.06f, 0.38f, 0.90f), new Color(0.05f, 0.35f, 0.65f, 0.90f), n);
                    pixels[y * size + x] = c;
                }
            }
            tex.SetPixels(pixels);
            tex.Apply();
            _nebulaSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
            return _nebulaSprite;
        }

        private static Sprite GetOrCreateMonolithSprite(int size = 32)
        {
            if (_monolithSprite != null) return _monolithSprite;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            var pixels = new Color[size * size];

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    bool isPillar = x >= 10 && x <= 22 && y >= 2 && y <= 30;
                    bool isRuneCore = x >= 13 && x <= 19 && y >= 13 && y <= 19;

                    if (isRuneCore)
                        pixels[y * size + x] = new Color(0.25f, 0.95f, 1.0f, 0.95f);
                    else if (isPillar)
                        pixels[y * size + x] = new Color(0.24f, 0.18f, 0.40f, 0.85f);
                    else
                        pixels[y * size + x] = Color.clear;
                }
            }
            tex.SetPixels(pixels);
            tex.Apply();
            _monolithSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
            return _monolithSprite;
        }

        private static Sprite GetOrCreateRailSprite(int size = 32)
        {
            if (_railFloorSprite != null) return _railFloorSprite;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            var pixels = new Color[size * size];

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    if (y >= 28) // Top glowing rail edge
                        pixels[y * size + x] = new Color(0.35f, 0.92f, 1.0f, 1.0f);
                    else if (y >= 25) // Electric blue neon stripe
                        pixels[y * size + x] = new Color(0.15f, 0.55f, 0.95f, 1.0f);
                    else if (y >= 20) // Deep runic casing
                        pixels[y * size + x] = new Color(0.18f, 0.12f, 0.32f, 1.0f);
                    else // Solid dark metallic foundation extending downward
                        pixels[y * size + x] = new Color(0.08f, 0.05f, 0.15f, 1.0f);
                }
            }
            tex.SetPixels(pixels);
            tex.Apply();
            _railFloorSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
            return _railFloorSprite;
        }
    }
}
