using UnityEngine;

namespace HappyShoot.View.Background
{
    /// <summary>
    /// Represents a single repeating ground tile in the infinite background grid.
    /// Manages its own SpriteRenderer, sorting order (-100), and tile variation.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class BackgroundTileView : MonoBehaviour
    {
        public const float TileSize = 24.0f; // 24m x 24m world units

        [SerializeField] private SpriteRenderer _spriteRenderer;
        private int _currentVariation = 0;

        public Vector2 GridCoordinate { get; private set; }

        private void Awake()
        {
            if (_spriteRenderer == null)
            {
                _spriteRenderer = GetComponent<SpriteRenderer>();
            }

            // Ensure background tiles are strictly rendered behind everything else
            _spriteRenderer.sortingOrder = -100;
        }

        public void Initialize(int gridX, int gridY, int variationIndex)
        {
            GridCoordinate = new Vector2(gridX, gridY);
            SetVariation(variationIndex);
        }

        public void SetVariation(int variationIndex)
        {
            _currentVariation = variationIndex;
            if (_spriteRenderer == null)
            {
                _spriteRenderer = GetComponent<SpriteRenderer>();
            }
            _spriteRenderer.sprite = BackgroundSpriteHelper.GetTileSprite(variationIndex);
        }

        public void SetWorldPosition(Vector2 position)
        {
            transform.position = new Vector3(position.x, position.y, 0f);
        }

        public Vector3 GetWorldPosition()
        {
            return transform.position;
        }
    }
}
