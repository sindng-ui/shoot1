using NUnit.Framework;
using HappyShoot.Domain.Entities;
using HappyShoot.Domain.Events;
using HappyShoot.Domain.Spatial;

namespace HappyShoot.Domain.Tests.Entities
{
    [TestFixture]
    public class MonsterEntityTests
    {
        private EventBus _eventBus;
        private MonsterEntity _monster;

        [SetUp]
        public void SetUp()
        {
            _eventBus = new EventBus();
            _monster = new MonsterEntity();
            _monster.Initialize(
                id: 101,
                typeName: "Goblin",
                maxHealth: 50f,
                moveSpeed: 3.0f,
                contactDamage: 10f,
                expValue: 2,
                goldValue: 1,
                startPosition: new Vector2D(10f, 0f),
                eventBus: _eventBus
            );
        }

        [Test]
        public void UpdateAI_MovesTowardTargetPlayer()
        {
            // Target is at (0, 0), monster is at (10, 0), speed is 3.0
            _monster.UpdateAI(new Vector2D(0f, 0f), deltaTime: 1.0f);

            // Monster moves left by 3 units -> (7, 0)
            Assert.That(_monster.Position.X, Is.EqualTo(7.0f).Within(1e-4f));
            Assert.That(_monster.Position.Y, Is.EqualTo(0f).Within(1e-4f));
        }

        [Test]
        public void TakeDamage_Lethal_FiresMonsterDiedEvent()
        {
            bool diedEventFired = false;
            int droppedExp = 0;

            _eventBus.Subscribe<MonsterDiedEvent>(evt =>
            {
                diedEventFired = true;
                droppedExp = evt.ExpValue;
            });

            _monster.TakeDamage(60f);

            Assert.That(_monster.IsDead, Is.True);
            Assert.That(_monster.CurrentHealth, Is.EqualTo(0f));
            Assert.That(diedEventFired, Is.True);
            Assert.That(droppedExp, Is.EqualTo(2));
        }

        [Test]
        public void DespawnAndReinitialize_ResetsStatsCorrectly()
        {
            _monster.TakeDamage(50f);
            _monster.OnDespawn();

            _monster.Initialize(
                id: 102,
                typeName: "Bat",
                maxHealth: 20f,
                moveSpeed: 5.0f,
                contactDamage: 5f,
                expValue: 1,
                goldValue: 1,
                startPosition: new Vector2D(0f, 5f)
            );

            Assert.That(_monster.IsActive, Is.True);
            Assert.That(_monster.IsDead, Is.False);
            Assert.That(_monster.CurrentHealth, Is.EqualTo(20f));
            Assert.That(_monster.TypeName, Is.EqualTo("Bat"));
        }
    }
}
