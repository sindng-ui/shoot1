using NUnit.Framework;
using HappyShoot.Domain.Events;
using HappyShoot.Domain.Spatial;
using HappyShoot.Domain.UI;

namespace HappyShoot.Domain.Tests.UI
{
    [TestFixture]
    public class DamageTextTests
    {
        private EventBus _eventBus;
        private DamageTextManager _manager;

        [SetUp]
        public void SetUp()
        {
            _eventBus = new EventBus();
            _manager = new DamageTextManager(_eventBus, initialCapacity: 10);
        }

        [Test]
        public void MonsterDamagedEvent_SpawnsDamageText()
        {
            _eventBus.Publish(new MonsterDamagedEvent(101, damage: 25f, remainingHealth: 25f, maxHealth: 50f, position: new Vector2D(3f, 4f)));

            Assert.That(_manager.ActiveCount, Is.EqualTo(1));
            Assert.That(_manager.ActiveTexts[0].DamageValue, Is.EqualTo(25f));
            Assert.That(_manager.ActiveTexts[0].IsActive, Is.True);
        }

        [Test]
        public void Update_FloatsTextUpwardsAndDespawnsAfterLifetime()
        {
            var text = _manager.SpawnText(new Vector2D(0f, 0f), damage: 15f);

            // Tick 0.2s -> should float upwards (Y > 0)
            _manager.Update(0.2f);
            Assert.That(text.Position.Y, Is.GreaterThan(0f));
            Assert.That(_manager.ActiveCount, Is.EqualTo(1));

            // Tick 1.0s -> lifetime expired (default is 0.7s)
            _manager.Update(1.0f);
            Assert.That(_manager.ActiveCount, Is.EqualTo(0));
            Assert.That(text.IsActive, Is.False);
        }
    }
}
