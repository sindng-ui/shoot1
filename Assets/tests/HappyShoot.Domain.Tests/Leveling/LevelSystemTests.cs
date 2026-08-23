using NUnit.Framework;
using HappyShoot.Domain.Events;
using HappyShoot.Domain.Leveling;

namespace HappyShoot.Domain.Tests.Leveling
{
    [TestFixture]
    public class LevelSystemTests
    {
        private EventBus _eventBus;
        private LevelSystem _levelSystem;

        [SetUp]
        public void SetUp()
        {
            _eventBus = new EventBus();
            _levelSystem = new LevelSystem(_eventBus, startingLevel: 1);
        }

        [Test]
        public void AddExp_TriggersLevelUp_WhenThresholdMet()
        {
            int levelUpCount = 0;
            int lastLevel = 1;
            _levelSystem.OnLevelUp += lvl =>
            {
                levelUpCount++;
                lastLevel = lvl;
            };

            // Lv 1 required: 5 + 5(1) + 2(1) = 12
            int req = _levelSystem.RequiredExp;
            Assert.That(req, Is.EqualTo(8));

            _levelSystem.AddExp(8);

            Assert.That(_levelSystem.Level, Is.EqualTo(2));
            Assert.That(levelUpCount, Is.EqualTo(1));
            Assert.That(lastLevel, Is.EqualTo(2));
            Assert.That(_levelSystem.CurrentExp, Is.EqualTo(0));
        }

        [Test]
        public void AddExp_MultiLevelUp_CarriesOverOverflowExp()
        {
            // Lv 1 req = 8, Lv 2 req = 15 -> total for 2 level ups = 23
            // Add 40 exp -> Should reach Lv 3 with 17 leftover exp!
            _levelSystem.AddExp(40);

            Assert.That(_levelSystem.Level, Is.EqualTo(3));
            Assert.That(_levelSystem.CurrentExp, Is.EqualTo(17));
        }
    }
}
