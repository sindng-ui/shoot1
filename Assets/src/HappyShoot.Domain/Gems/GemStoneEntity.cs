using System;
using HappyShoot.Domain.Pool;
using HappyShoot.Domain.Progression;
using HappyShoot.Domain.Spatial;

namespace HappyShoot.Domain.Gems
{
    /// <summary>
    /// Pure C# Gem Stone entity (Ruby, Emerald, Amethyst) for permanent meta progression skill tree unlocks.
    /// Supports zero-allocation pooling, spatial lookup, and magnet attraction.
    /// </summary>
    public class GemStoneEntity : ISpatialEntity, IPoolable
    {
        public int Id { get; private set; }
        public GemType GemType { get; private set; }
        public Vector2D Position { get; private set; }
        public float Radius { get; set; } = 0.35f;

        public bool IsActive { get; private set; }
        public bool IsMagnetized { get; private set; }
        public float CurrentSpeed { get; private set; }

        private const float InitialMagnetSpeed = 5.0f;
        private const float MagnetAcceleration = 16.0f;
        private const float CollectDistanceThreshold = 0.45f;

        public GemStoneEntity()
        {
            IsActive = false;
        }

        public void Initialize(int id, GemType gemType, Vector2D spawnPosition)
        {
            Id = id;
            GemType = gemType;
            Position = spawnPosition;
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
        /// Updates gem stone position and magnet attraction towards the player.
        /// Returns true if collected by the player.
        /// </summary>
        public bool Update(Vector2D playerPosition, float playerPickupRadius, float deltaTime)
        {
            if (!IsActive) return false;

            float sqrDist = Vector2D.SqrDistance(Position, playerPosition);

            // Check if player's magnet range covers the gem stone
            if (!IsMagnetized && sqrDist <= playerPickupRadius * playerPickupRadius)
            {
                IsMagnetized = true;
                CurrentSpeed = InitialMagnetSpeed;
            }

            if (IsMagnetized)
            {
                CurrentSpeed += MagnetAcceleration * deltaTime;
                Vector2D dir = (playerPosition - Position).Normalized;
                Position += dir * (CurrentSpeed * deltaTime);

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
