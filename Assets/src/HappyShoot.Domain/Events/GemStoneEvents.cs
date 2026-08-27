using HappyShoot.Domain.Progression;
using HappyShoot.Domain.Spatial;

namespace HappyShoot.Domain.Events
{
    /// <summary>
    /// Fired when a gem stone is dropped in the world (from monster death).
    /// </summary>
    public readonly struct GemStoneDroppedEvent : IDomainEvent
    {
        public readonly GemType GemType;
        public readonly Vector2D Position;
        public readonly int GemStoneId;

        public GemStoneDroppedEvent(GemType gemType, Vector2D position, int gemStoneId)
        {
            GemType = gemType;
            Position = position;
            GemStoneId = gemStoneId;
        }
    }

    /// <summary>
    /// Fired when a gem stone is collected (magnet pickup) by the player during a run.
    /// </summary>
    public readonly struct GemStoneCollectedEvent : IDomainEvent
    {
        public readonly GemType GemType;

        public GemStoneCollectedEvent(GemType gemType)
        {
            GemType = gemType;
        }
    }
}
