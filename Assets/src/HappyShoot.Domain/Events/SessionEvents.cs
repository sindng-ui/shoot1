using System;

namespace HappyShoot.Domain.Events
{
    public enum GameState
    {
        None = 0,
        Playing = 1,
        Paused = 2,
        GameOver = 3,
        Victory = 4
    }

    /// <summary>
    /// Published when the overall game session state transitions.
    /// </summary>
    public readonly struct GameStateChangedEvent : IDomainEvent
    {
        public GameState PreviousState { get; }
        public GameState NewState { get; }

        public GameStateChangedEvent(GameState previousState, GameState newState)
        {
            PreviousState = previousState;
            NewState = newState;
        }
    }

    /// <summary>
    /// Published when elapsed survival time updates.
    /// </summary>
    public readonly struct SurvivalTimeUpdatedEvent : IDomainEvent
    {
        public float TotalSeconds { get; }
        public int Minutes { get; }
        public int Seconds { get; }

        public SurvivalTimeUpdatedEvent(float totalSeconds)
        {
            TotalSeconds = Math.Max(0f, totalSeconds);
            Minutes = (int)(TotalSeconds / 60f);
            Seconds = (int)(TotalSeconds % 60f);
        }
    }

    /// <summary>
    /// Published when monster kill count is updated.
    /// </summary>
    public readonly struct KillCountUpdatedEvent : IDomainEvent
    {
        public int TotalKills { get; }
        public int AddedKills { get; }

        public KillCountUpdatedEvent(int totalKills, int addedKills)
        {
            TotalKills = totalKills;
            AddedKills = addedKills;
        }
    }

    /// <summary>
    /// Published when player obtains gold during the run.
    /// </summary>
    public readonly struct GoldGainedEvent : IDomainEvent
    {
        public int TotalGold { get; }
        public int AddedGold { get; }

        public GoldGainedEvent(int totalGold, int addedGold)
        {
            TotalGold = totalGold;
            AddedGold = addedGold;
        }
    }
}
