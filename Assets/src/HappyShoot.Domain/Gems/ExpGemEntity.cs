using System;
using HappyShoot.Domain.Pool;
using HappyShoot.Domain.Spatial;

namespace HappyShoot.Domain.Gems
{
    /// <summary>
    /// Pure C# Experience Gem entity supporting zero-allocation pooling and magnet attraction.
    /// </summary>
    public class ExpGemEntity : ISpatialEntity, IPoolable
    {
        public int Id { get; private set; }
        public Vector2D Position { get; private set; }
        public float Radius { get; set; } = 0.3f;

        public bool IsActive { get; private set; }
        public int ExpValue { get; private set; }
        public bool IsMagnetized { get; private set; }
        public float CurrentSpeed { get; private set; }

        private const float InitialMagnetSpeed = 6.0f;
        private const float MagnetAcceleration = 18.0f;
        private const float CollectDistanceThreshold = 0.4f;

        public ExpGemEntity()
        {
            IsActive = false;
        }

        public void Initialize(int id, Vector2D spawnPosition, int expValue = 1)
        {
            Id = id;
            Position = spawnPosition;
            ExpValue = Math.Max(1, expValue);
            IsMagnetized = false;
            CurrentSpeed = InitialMagnetSpeed;
            IsActive = true;
        }

        public void OnSpawn()
        {
            IsActive = true;
        }

        public void OnDespawn()
        {
            IsActive = false;
            IsMagnetized = false;
            CurrentSpeed = InitialMagnetSpeed;
        }

        /// <summary>
        /// Updates gem position and magnet attraction towards the player.
        /// Returns true if collected by the player.
        /// </summary>
        public bool Update(Vector2D playerPosition, float playerPickupRadius, float deltaTime)
        {
            if (!IsActive) return false;

            float sqrDist = Vector2D.SqrDistance(Position, playerPosition);

            // Check if player's magnet range covers the gem
            if (!IsMagnetized && sqrDist <= playerPickupRadius * playerPickupRadius)
            {
                IsMagnetized = true;
                CurrentSpeed = InitialMagnetSpeed;
            }

            if (IsMagnetized)
            {
                // Accelerate towards player
                CurrentSpeed += MagnetAcceleration * deltaTime;
                Vector2D dir = (playerPosition - Position).Normalized;
                Position += dir * (CurrentSpeed * deltaTime);

                // Check collection
                if (sqrDist <= CollectDistanceThreshold * CollectDistanceThreshold)
                {
                    IsActive = false;
                    return true;
                }
            }

            return false;
        }
    }
}
