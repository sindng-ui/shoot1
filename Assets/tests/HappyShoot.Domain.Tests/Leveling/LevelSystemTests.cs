using NUnit.Framework;
using HappyShoot.Domain.Events;
using HappyShoot.Domain.Leveling;
using HappyShoot.Domain.Skills;

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

        [Test]
        public void ExpGrowthScale_CalculatesCorrectProportion_WithConfig()
        {
            var config = new HappyShoot.Domain.Skills.ExpConfig
            {
                BaseRequiredExp = 4,
                ExpGrowthFactor = 1.5f,
                EnableLevelExpScaling = true
            };

            var levelSystem = new LevelSystem(_eventBus, 1, config);
            int exp1 = levelSystem.CalculateRequiredExp(1);
            int exp2 = levelSystem.CalculateRequiredExp(2);
            int exp3 = levelSystem.CalculateRequiredExp(3);

            float scale2 = (float)exp2 / exp1;
            float scale3 = (float)exp3 / exp1;

            Assert.That(scale2, Is.GreaterThan(1.0f));
            Assert.That(scale3, Is.GreaterThan(scale2));

            // Verify 30% Mob Scaling Formula: 1.0 + ((scale - 1.0) * 0.30)
            float mobScale2 = 1.0f + ((scale2 - 1.0f) * config.MobScalingRatio);
            Assert.That(mobScale2, Is.GreaterThan(1.0f));
            Assert.That(mobScale2, Is.LessThan(scale2)); // Scaled mob count increases more gently
        }

        [Test]
        public void LevelExpScaling_WithMobHpScalingRatio_CalculatesExpectedHpScale()
        {
            var config = new ExpConfig
            {
                BaseRequiredExp = 10,
                ExpGrowthFactor = 1.5f,
                EnableLevelExpScaling = true,
                MobHpScalingRatio = 0.50f, // 50% ratio
                EnableHitStop = true,
                HitStopDuration = 0.05f,
                HitStopSlowScale = 0.02f
            };

            var levelSystem = new LevelSystem(_eventBus, 1, config);
            int exp1 = levelSystem.CalculateRequiredExp(1);
            int exp2 = levelSystem.CalculateRequiredExp(2);

            float rawExpScale = (float)exp2 / exp1;
            float expectedHpScale = 1.0f + ((rawExpScale - 1.0f) * config.MobHpScalingRatio);

            Assert.That(expectedHpScale, Is.GreaterThan(1.0f));
            Assert.That(expectedHpScale, Is.LessThan(rawExpScale));
            Assert.That(config.EnableHitStop, Is.True);
            Assert.That(config.HitStopDuration, Is.EqualTo(0.05f));
            Assert.That(config.HitStopSlowScale, Is.EqualTo(0.02f));
        }
    }
}
