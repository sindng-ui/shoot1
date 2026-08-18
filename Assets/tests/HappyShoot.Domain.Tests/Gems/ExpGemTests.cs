using NUnit.Framework;
using HappyShoot.Domain.Events;
using HappyShoot.Domain.Gems;
using HappyShoot.Domain.Spatial;

namespace HappyShoot.Domain.Tests.Gems
{
    [TestFixture]
    public class ExpGemTests
    {
        private EventBus _eventBus;
        private GemManager _gemManager;

        [SetUp]
        public void SetUp()
        {
            _eventBus = new EventBus();
            _gemManager = new GemManager(_eventBus, initialCapacity: 10);
        }

        [Test]
        public void MonsterDiedEvent_SpawnsGemAutomatically()
        {
            _eventBus.Publish(new MonsterDiedEvent(101, new Vector2D(5f, 5f), expValue: 3, goldValue: 1));

            Assert.That(_gemManager.ActiveCount, Is.EqualTo(1));
            Assert.That(_gemManager.ActiveGems[0].ExpValue, Is.EqualTo(3));
            Assert.That(_gemManager.ActiveGems[0].Position.X, Is.EqualTo(5f));
        }

        [Test]
        public void Gem_AttractsTowardsPlayer_WhenInPickupRadius()
        {
            var gem = _gemManager.SpawnGem(new Vector2D(2f, 0f), expValue: 1);

            // Player at (0, 0) with pickup radius 3.0 (Gem is at distance 2.0 -> inside range)
            _gemManager.Update(new Vector2D(0f, 0f), pickupRadius: 3.0f, deltaTime: 0.1f);

            Assert.That(gem.IsMagnetized, Is.True);
            // Gem moved closer to player (less than 2.0)
            Assert.That(gem.Position.X, Is.LessThan(2.0f));
        }

        [Test]
        public void Gem_Collected_TriggersExpCallbackAndDespawns()
        {
            int collectedExp = 0;
            _gemManager.OnExpCollected += exp => collectedExp += exp;

            // Spawn gem very close to player (0.2, 0) -> within 0.4 collection threshold
            _gemManager.SpawnGem(new Vector2D(0.2f, 0f), expValue: 5);

            _gemManager.Update(new Vector2D(0f, 0f), pickupRadius: 2.0f, deltaTime: 0.1f);

            Assert.That(collectedExp, Is.EqualTo(5));
            Assert.That(_gemManager.ActiveCount, Is.EqualTo(0)); // Despawned
        }
    }
}
