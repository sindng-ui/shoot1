using UnityEngine;

namespace HappyShoot.View.Player
{
    /// <summary>
    /// Smooth fading afterimage ghost trail left behind in world space during player Dash.
    /// Strictly modular, zero-allocation pooled, and under 500 lines.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class PlayerDashGhostTrail : MonoBehaviour
    {
        private SpriteRenderer _sr;
        private float _fadeDuration = 0.38f;
        private float _fadeTimer;
        private Color _baseColor;

        private void Awake()
        {
            _sr = GetComponent<SpriteRenderer>();
        }

        public void Spawn(Vector3 worldPos, Sprite sprite, bool flipX, Vector3 scale, int sortingOrder, float duration = 0.38f)
        {
            transform.position = worldPos;
            transform.localScale = scale;
            _fadeDuration = Mathf.Max(0.05f, duration);
            _fadeTimer = 0f;

            if (_sr == null) _sr = GetComponent<SpriteRenderer>();

            _sr.sprite = sprite;
            _sr.flipX = flipX;
            _sr.sortingOrder = Mathf.Max(0, sortingOrder - 1); // Render directly behind player

            // Vibrant Arcane cyan-violet luminous afterimage
            _baseColor = new Color(0.55f, 0.85f, 1.0f, 0.80f);
            _sr.color = _baseColor;

            gameObject.SetActive(true);
        }

        private void Update()
        {
            _fadeTimer += Time.deltaTime;
            float progress = _fadeTimer / _fadeDuration;

            if (progress >= 1.0f)
            {
                gameObject.SetActive(false);
                return;
            }

            // Smooth cubic fade-out so the ghost remains clearly visible during the dash
            float alpha = Mathf.Lerp(0.80f, 0f, progress * progress);
            _sr.color = new Color(_baseColor.r, _baseColor.g, _baseColor.b, alpha);
        }
    }
}
