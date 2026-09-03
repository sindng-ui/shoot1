using System.Collections.Generic;
using UnityEngine;

namespace HappyShoot.View.Player
{
    /// <summary>
    /// Overhead floating charge indicator dots for player Dash action.
    /// White with a soft arcane cyan tint. Dynamically supports 1, 2, or 3 charges.
    /// Strictly modular, zero-allocation, and under 500 lines.
    /// </summary>
    public class PlayerDashChargeIndicatorView : MonoBehaviour
    {
        [SerializeField] private Vector3 _baseOffset = new Vector3(0f, 1.30f, 0f);

        private static Sprite _dotSprite;
        private readonly List<SpriteRenderer> _dots = new List<SpriteRenderer>(3);
        private int _currentCharges = 1;
        private int _maxCharges = 1;
        private float _bobTimer;

        public void Initialize(Transform playerTransform, int maxCharges = 1, int currentCharges = 1)
        {
            transform.SetParent(playerTransform, false);
            transform.localPosition = _baseOffset;

            EnsureDotSprite();
            BuildDotPool(3);

            UpdateCharges(currentCharges, maxCharges);
        }

        private static void EnsureDotSprite()
        {
            if (_dotSprite != null) return;

            const int size = 8;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear };
            var pixels = new Color[size * size];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.clear;

            Color coreWhite = new Color(1.0f, 1.0f, 1.0f, 1.0f);
            Color cyanGlow = new Color(0.75f, 0.92f, 1.0f, 0.90f);
            Color softEdge = new Color(0.50f, 0.80f, 1.0f, 0.40f);

            float center = (size - 1) / 2f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                    if (dist <= 1.4f)
                        pixels[y * size + x] = coreWhite;
                    else if (dist <= 2.6f)
                        pixels[y * size + x] = cyanGlow;
                    else if (dist <= 3.5f)
                        pixels[y * size + x] = softEdge;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            _dotSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 32);
        }

        private void BuildDotPool(int capacity)
        {
            for (int i = 0; i < capacity; i++)
            {
                var go = new GameObject($"DashDot_{i}");
                go.transform.SetParent(transform, false);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = _dotSprite;
                sr.sortingOrder = 25; // Render cleanly above character and hat
                go.SetActive(false);
                _dots.Add(sr);
            }
        }

        public void UpdateCharges(int currentCharges, int maxCharges)
        {
            _currentCharges = Mathf.Max(0, currentCharges);
            _maxCharges = Mathf.Max(1, maxCharges);

            // Compute symmetric spacing for dots
            float spacing = 0.14f;
            float totalWidth = (_maxCharges - 1) * spacing;
            float startX = -totalWidth * 0.5f;

            for (int i = 0; i < _dots.Count; i++)
            {
                if (i < _maxCharges)
                {
                    var dot = _dots[i];
                    dot.transform.localPosition = new Vector3(startX + i * spacing, 0f, 0f);
                    // Light up dot only if charge is currently available!
                    dot.gameObject.SetActive(i < _currentCharges);
                }
                else
                {
                    _dots[i].gameObject.SetActive(false);
                }
            }
        }

        private void Update()
        {
            // Gentle floating bob
            _bobTimer += Time.deltaTime * 3.5f;
            float bobY = Mathf.Sin(_bobTimer) * 0.025f;
            transform.localPosition = _baseOffset + new Vector3(0f, bobY, 0f);
        }
    }
}
