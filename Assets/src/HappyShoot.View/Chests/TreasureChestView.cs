using UnityEngine;
using HappyShoot.Domain.Chests;
using HappyShoot.View.Utils;

namespace HappyShoot.View.Chests
{
    /// <summary>
    /// Visual representation of a dropped golden treasure chest with shimmer animation.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class TreasureChestView : MonoBehaviour
    {
        private TreasureChestEntity _entity;
        private SpriteRenderer _spriteRenderer;
        private float _animTimer;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _spriteRenderer.sprite = SpriteHelper.GetOrCreateChestSprite();
            _spriteRenderer.sortingOrder = 5;
            transform.localScale = Vector3.one * 1.5f;
            _animTimer = Random.Range(0f, 5f);
        }

        public void Bind(TreasureChestEntity entity)
        {
            _entity = entity;
            transform.position = new Vector3(entity.Position.X, entity.Position.Y, 0f);
            gameObject.SetActive(true);
        }

        public void UpdateView()
        {
            if (_entity == null || !_entity.IsActive || _entity.IsOpened)
            {
                gameObject.SetActive(false);
                return;
            }

            transform.position = new Vector3(_entity.Position.X, _entity.Position.Y, 0f);

            // Shimmer / slight hover pulse
            _animTimer += Time.deltaTime * 5f;
            float pulse = 1f + Mathf.Sin(_animTimer) * 0.08f;
            transform.localScale = new Vector3(1.5f * pulse, 1.5f * pulse, 1f);
        }
    }
}
