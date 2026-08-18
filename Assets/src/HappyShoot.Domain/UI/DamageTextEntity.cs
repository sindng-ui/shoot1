using System;
using HappyShoot.Domain.Pool;
using HappyShoot.Domain.Spatial;

namespace HappyShoot.Domain.UI
{
    /// <summary>
    /// Pure C# floating damage number entity for hit feedback.
    /// </summary>
    public class DamageTextEntity : IPoolable
    {
        public int Id { get; private set; }
        public Vector2D Position { get; private set; }
        public float DamageValue { get; private set; }
        public bool IsCritical { get; private set; }
        public bool IsActive { get; private set; }
        public float RemainingLifetime { get; private set; }
        public float Alpha { get; private set; }

        private const float DefaultLifetime = 0.7f;
        private const float FloatSpeed = 1.5f;

        public DamageTextEntity()
        {
            IsActive = false;
        }

        public void Initialize(int id, Vector2D startPosition, float damage, bool isCritical = false)
        {
            Id = id;
            Position = startPosition;
            DamageValue = (float)Math.Round(damage, 0);
            IsCritical = isCritical;
            RemainingLifetime = DefaultLifetime;
            Alpha = 1.0f;
            IsActive = true;
        }

        public void OnSpawn()
        {
            IsActive = true;
        }

        public void OnDespawn()
        {
            IsActive = false;
        }

        public void Update(float deltaTime)
        {
            if (!IsActive) return;

            RemainingLifetime -= deltaTime;
            if (RemainingLifetime <= 0f)
            {
                IsActive = false;
                return;
            }

            // Float upwards
            Position += new Vector2D(0f, FloatSpeed * deltaTime);

            // Fade out in second half of lifetime
            Alpha = Math.Max(0f, Math.Min(1f, RemainingLifetime / (DefaultLifetime * 0.5f)));
        }
    }
}
