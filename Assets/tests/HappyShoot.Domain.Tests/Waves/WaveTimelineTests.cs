using NUnit.Framework;
using HappyShoot.Domain.Entities;
using HappyShoot.Domain.Spatial;
using HappyShoot.Domain.Waves;

namespace HappyShoot.Domain.Tests.Waves
{
    [TestFixture]
    public class WaveTimelineTests
    {
        private SpatialGrid2D<MonsterEntity> _monsterGrid;
        private MonsterSpawner _spawner;
        private WaveTimelineManager _timeline;

        [SetUp]
        public void SetUp()
        {
            _monsterGrid = new SpatialGrid2D<MonsterEntity>(cellSize: 2.0f);
            _spawner = new MonsterSpawner(_monsterGrid);
            _timeline = new WaveTimelineManager();
        }

        [Test]
        public void Timeline_StartsAtZero_AndProgressesWithDelta()
        {
            Assert.That(_timeline.ElapsedTime, Is.EqualTo(0f));
            Assert.That(_timeline.IsRunComplete, Is.False);

            _timeline.Update(60f, _spawner, Vector2D.Zero); // 1 minute elapsed
            Assert.That(_timeline.ElapsedTime, Is.EqualTo(60f));
        }

        [Test]
        public void Timeline_At180Seconds_TriggersEliteBoss()
        {
            WaveBossType triggeredBoss = WaveBossType.None;
            _timeline.OnBossSpawnTriggered += (bossType, pos) => triggeredBoss = bossType;

            // Fast-forward to 3:00 (180s)
            _timeline.Update(180f, _spawner, Vector2D.Zero);

            Assert.That(triggeredBoss, Is.EqualTo(WaveBossType.Elite));
        }

        [Test]
        public void Timeline_At480Seconds_TriggersMidBoss()
        {
            WaveBossType triggeredBoss = WaveBossType.None;
            _timeline.OnBossSpawnTriggered += (bossType, pos) => triggeredBoss = bossType;

            // Fast-forward to 8:00 (480s)
            _timeline.Update(480f, _spawner, Vector2D.Zero);

            Assert.That(triggeredBoss, Is.EqualTo(WaveBossType.MidBoss));
        }

        [Test]
        public void Timeline_At900Seconds_TriggersFinalBossAndCompletesRun()
        {
            WaveBossType triggeredBoss = WaveBossType.None;
            _timeline.OnBossSpawnTriggered += (bossType, pos) => triggeredBoss = bossType;

            // Fast-forward to 15:00 (900s)
            _timeline.Update(900f, _spawner, Vector2D.Zero);

            Assert.That(triggeredBoss, Is.EqualTo(WaveBossType.FinalBoss));
            Assert.That(_timeline.IsRunComplete, Is.True);
        }
    }
}
