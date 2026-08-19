using System.Collections.Generic;
using NUnit.Framework;
using HappyShoot.Domain.Events;
using HappyShoot.Domain.Session;
using HappyShoot.Domain.Spatial;

namespace HappyShoot.Domain.Tests.Session
{
    [TestFixture]
    public class GameSessionTests
    {
        private EventBus _eventBus;
        private GameSessionEntity _session;

        [SetUp]
        public void SetUp()
        {
            _eventBus = new EventBus();
            _session = new GameSessionEntity(_eventBus);
        }

        [Test]
        public void InitialState_IsNone_AndStatsAreDefault()
        {
            Assert.That(_session.CurrentState, Is.EqualTo(GameState.None));
            Assert.That(_session.ElapsedTime, Is.EqualTo(0f));
            Assert.That(_session.KillCount, Is.EqualTo(0));
            Assert.That(_session.GoldEarned, Is.EqualTo(0));
            Assert.That(_session.PlayerLevel, Is.EqualTo(1));
            Assert.That(_session.IsPlaying, Is.False);
            Assert.That(_session.IsGameOver, Is.False);
        }

        [Test]
        public void StartGame_TransitionsToPlaying_AndPublishesEvent()
        {
            GameStateChangedEvent receivedEvent = default;
            bool eventFired = false;

            _eventBus.Subscribe<GameStateChangedEvent>(evt =>
            {
                receivedEvent = evt;
                eventFired = true;
            });

            _session.StartGame();

            Assert.That(_session.CurrentState, Is.EqualTo(GameState.Playing));
            Assert.That(_session.IsPlaying, Is.True);
            Assert.That(eventFired, Is.True);
            Assert.That(receivedEvent.PreviousState, Is.EqualTo(GameState.None));
            Assert.That(receivedEvent.NewState, Is.EqualTo(GameState.Playing));
        }

        [Test]
        public void PauseAndResume_TransitionsCorrectly()
        {
            _session.StartGame();

            var events = new List<GameStateChangedEvent>();
            _eventBus.Subscribe<GameStateChangedEvent>(evt => events.Add(evt));

            _session.Pause();
            Assert.That(_session.CurrentState, Is.EqualTo(GameState.Paused));
            Assert.That(_session.IsPaused, Is.True);

            _session.Resume();
            Assert.That(_session.CurrentState, Is.EqualTo(GameState.Playing));
            Assert.That(_session.IsPlaying, Is.True);

            Assert.That(events.Count, Is.EqualTo(2));
            Assert.That(events[0].NewState, Is.EqualTo(GameState.Paused));
            Assert.That(events[1].NewState, Is.EqualTo(GameState.Playing));
        }

        [Test]
        public void TogglePause_AlternatesBetweenPlayingAndPaused()
        {
            _session.StartGame();

            _session.TogglePause();
            Assert.That(_session.IsPaused, Is.True);

            _session.TogglePause();
            Assert.That(_session.IsPlaying, Is.True);
        }

        [Test]
        public void Update_WhilePlaying_AccumulatesTimeAndComputesMinSec()
        {
            _session.StartGame();

            _session.Update(65.5f); // 1 min 5.5 sec

            Assert.That(_session.ElapsedTime, Is.EqualTo(65.5f).Within(0.001f));
            Assert.That(_session.Minutes, Is.EqualTo(1));
            Assert.That(_session.Seconds, Is.EqualTo(5));
            Assert.That(_session.GetFormattedTime(), Is.EqualTo("01:05"));
        }

        [Test]
        public void Update_WhenPausedOrGameOver_DoesNotAccumulateTime()
        {
            _session.StartGame();
            _session.Pause();

            _session.Update(10f);
            Assert.That(_session.ElapsedTime, Is.EqualTo(0f));

            _session.Resume();
            _session.SetGameOver();

            _session.Update(10f);
            Assert.That(_session.ElapsedTime, Is.EqualTo(0f));
        }

        [Test]
        public void Update_PeriodicallyPublishes_SurvivalTimeUpdatedEvent()
        {
            _session.StartGame();

            var timeEvents = new List<SurvivalTimeUpdatedEvent>();
            _eventBus.Subscribe<SurvivalTimeUpdatedEvent>(evt => timeEvents.Add(evt));

            _session.Update(0.2f);
            Assert.That(timeEvents.Count, Is.EqualTo(0)); // Threshold is 0.5s

            _session.Update(0.35f); // Total 0.55s
            Assert.That(timeEvents.Count, Is.EqualTo(1));
            Assert.That(timeEvents[0].Minutes, Is.EqualTo(0));
            Assert.That(timeEvents[0].Seconds, Is.EqualTo(0));
        }

        [Test]
        public void OnMonsterDied_IncrementsKillCount_AndPublishesEvent()
        {
            _session.StartGame();

            KillCountUpdatedEvent received = default;
            bool fired = false;
            _eventBus.Subscribe<KillCountUpdatedEvent>(evt =>
            {
                received = evt;
                fired = true;
            });

            _eventBus.Publish(new MonsterDiedEvent(101, new Vector2D(0, 0), 10, 5));

            Assert.That(_session.KillCount, Is.EqualTo(1));
            Assert.That(_session.GoldEarned, Is.EqualTo(5));
            Assert.That(fired, Is.True);
            Assert.That(received.TotalKills, Is.EqualTo(1));
            Assert.That(received.AddedKills, Is.EqualTo(1));
        }

        [Test]
        public void AddGold_IncrementsGold_AndPublishesEvent()
        {
            _session.StartGame();

            GoldGainedEvent received = default;
            bool fired = false;
            _eventBus.Subscribe<GoldGainedEvent>(evt =>
            {
                received = evt;
                fired = true;
            });

            _session.AddGold(50);
            Assert.That(_session.GoldEarned, Is.EqualTo(50));
            Assert.That(fired, Is.True);
            Assert.That(received.TotalGold, Is.EqualTo(50));
            Assert.That(received.AddedGold, Is.EqualTo(50));

            // Negative or zero gold should be ignored
            _session.AddGold(-10);
            Assert.That(_session.GoldEarned, Is.EqualTo(50));
        }

        [Test]
        public void OnPlayerDied_AutomaticallySetsGameOver()
        {
            _session.StartGame();

            _eventBus.Publish(new PlayerDiedEvent(1));

            Assert.That(_session.CurrentState, Is.EqualTo(GameState.GameOver));
            Assert.That(_session.IsGameOver, Is.True);
            Assert.That(_session.IsPlaying, Is.False);
        }

        [Test]
        public void OnLevelUp_UpdatesPlayerLevel()
        {
            _session.StartGame();

            _eventBus.Publish(new PlayerLevelUpEvent(5));

            Assert.That(_session.PlayerLevel, Is.EqualTo(5));
        }

        [Test]
        public void ResetSession_ClearsAllData_AndResetsToPlaying()
        {
            _session.StartGame();
            _session.Update(120f);
            _session.AddKill(25);
            _session.AddGold(100);
            _session.SetGameOver();

            _session.ResetSession();

            Assert.That(_session.CurrentState, Is.EqualTo(GameState.Playing));
            Assert.That(_session.ElapsedTime, Is.EqualTo(0f));
            Assert.That(_session.KillCount, Is.EqualTo(0));
            Assert.That(_session.GoldEarned, Is.EqualTo(0));
            Assert.That(_session.PlayerLevel, Is.EqualTo(1));
        }

        [Test]
        public void SetVictory_TransitionsCorrectly_UnlessGameOver()
        {
            _session.StartGame();
            _session.SetVictory();

            Assert.That(_session.CurrentState, Is.EqualTo(GameState.Victory));
            Assert.That(_session.IsVictory, Is.True);

            // Once Game Over, cannot become Victory
            _session.ResetSession();
            _session.SetGameOver();
            _session.SetVictory();
            Assert.That(_session.IsGameOver, Is.True);
        }
    }
}
