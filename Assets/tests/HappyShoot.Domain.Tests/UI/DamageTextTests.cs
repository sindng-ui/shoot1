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

        [Test]
        public void MonsterDamagedEvent_WithElementalDamageTypes_PropagatesToEntity()
        {
            // Fire damage
            _eventBus.Publish(new MonsterDamagedEvent(102, damage: 50f, remainingHealth: 50f, maxHealth: 100f,
                position: new Vector2D(1f, 1f), isCritical: false, damageType: DamageType.Fireball));
            Assert.That(_manager.ActiveTexts[0].DamageType, Is.EqualTo(DamageType.Fireball));
            Assert.That(_manager.ActiveTexts[0].IsCritical, Is.False);

            // Ice damage
            _eventBus.Publish(new MonsterDamagedEvent(103, damage: 30f, remainingHealth: 20f, maxHealth: 100f,
                position: new Vector2D(2f, 2f), isCritical: true, damageType: DamageType.Ice));
            Assert.That(_manager.ActiveTexts[1].DamageType, Is.EqualTo(DamageType.Ice));
            Assert.That(_manager.ActiveTexts[1].IsCritical, Is.True);

            // Lightning damage
            _eventBus.Publish(new MonsterDamagedEvent(104, damage: 75f, remainingHealth: 0f, maxHealth: 100f,
                position: new Vector2D(3f, 3f), isCritical: false, damageType: DamageType.Lightning));
            Assert.That(_manager.ActiveTexts[2].DamageType, Is.EqualTo(DamageType.Lightning));
        }
    }
}
