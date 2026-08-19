using System;
using HappyShoot.Domain.Events;

namespace HappyShoot.Domain.Session
{
    /// <summary>
    /// Pure C# domain entity that manages the lifecycle, stats, and states of a single gameplay session run.
    /// </summary>
    public class GameSessionEntity
    {
        public GameState CurrentState { get; private set; } = GameState.None;
        public float ElapsedTime { get; private set; }
        public int Minutes => (int)(ElapsedTime / 60f);
        public int Seconds => (int)(ElapsedTime % 60f);
        public int KillCount { get; private set; }
        public int GoldEarned { get; private set; }
        public int PlayerLevel { get; private set; } = 1;

        public bool IsPlaying => CurrentState == GameState.Playing;
        public bool IsPaused => CurrentState == GameState.Paused;
        public bool IsGameOver => CurrentState == GameState.GameOver;
        public bool IsVictory => CurrentState == GameState.Victory;

        private readonly EventBus _eventBus;
        private float _timeSyncTimer;
        private const float TimeSyncInterval = 0.5f; // Sync event every 0.5s for performance

        public GameSessionEntity(EventBus eventBus = null)
        {
            _eventBus = eventBus;
            SubscribeEvents();
        }

        private void SubscribeEvents()
        {
            if (_eventBus == null) return;

            _eventBus.Subscribe<PlayerDiedEvent>(OnPlayerDied);
            _eventBus.Subscribe<MonsterDiedEvent>(OnMonsterDied);
            _eventBus.Subscribe<PlayerLevelUpEvent>(OnLevelUp);
        }

        public void StartGame()
        {
            SetState(GameState.Playing);
        }

        public void Pause()
        {
            if (CurrentState == GameState.Playing)
            {
                SetState(GameState.Paused);
            }
        }

        public void Resume()
        {
            if (CurrentState == GameState.Paused)
            {
                SetState(GameState.Playing);
            }
        }

        public void TogglePause()
        {
            if (CurrentState == GameState.Playing)
            {
                Pause();
            }
            else if (CurrentState == GameState.Paused)
            {
                Resume();
            }
        }

        public void SetGameOver()
        {
            if (CurrentState != GameState.GameOver)
            {
                SetState(GameState.GameOver);
            }
        }

        public void SetVictory()
        {
            if (CurrentState != GameState.Victory && CurrentState != GameState.GameOver)
            {
                SetState(GameState.Victory);
            }
        }

        public void Update(float deltaTime)
        {
            if (CurrentState != GameState.Playing || deltaTime <= 0f)
                return;

            ElapsedTime += deltaTime;
            _timeSyncTimer += deltaTime;

            if (_timeSyncTimer >= TimeSyncInterval)
            {
                _timeSyncTimer = 0f;
                _eventBus?.Publish(new SurvivalTimeUpdatedEvent(ElapsedTime));
            }
        }

        public void AddKill(int count = 1)
        {
            if (count <= 0 || !IsPlaying) return;

            KillCount += count;
            _eventBus?.Publish(new KillCountUpdatedEvent(KillCount, count));
        }

        public void AddGold(int amount)
        {
            if (amount <= 0) return;

            GoldEarned += amount;
            _eventBus?.Publish(new GoldGainedEvent(GoldEarned, amount));
        }

        public void ResetSession()
        {
            ElapsedTime = 0f;
            _timeSyncTimer = 0f;
            KillCount = 0;
            GoldEarned = 0;
            PlayerLevel = 1;
            SetState(GameState.Playing);
        }

        private void SetState(GameState newState)
        {
            if (CurrentState == newState) return;

            GameState oldState = CurrentState;
            CurrentState = newState;
            _eventBus?.Publish(new GameStateChangedEvent(oldState, newState));
        }

        private void OnPlayerDied(PlayerDiedEvent evt)
        {
            SetGameOver();
        }

        private void OnMonsterDied(MonsterDiedEvent evt)
        {
            AddKill(1);
            if (evt.GoldValue > 0)
            {
                AddGold(evt.GoldValue);
            }
        }

        private void OnLevelUp(PlayerLevelUpEvent evt)
        {
            PlayerLevel = evt.NewLevel;
        }

        /// <summary>
        /// Formats elapsed time as MM:SS with zero allocation where possible.
        /// </summary>
        public string GetFormattedTime()
        {
            int m = Minutes;
            int s = Seconds;
            return $"{m:D2}:{s:D2}";
        }
    }
}
