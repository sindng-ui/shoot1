using NUnit.Framework;
using HappyShoot.Domain.Events;

namespace HappyShoot.Domain.Tests.Events
{
    public readonly struct TestDamageEvent : IDomainEvent
    {
        public readonly int TargetId;
        public readonly float Damage;

        public TestDamageEvent(int targetId, float damage)
        {
            TargetId = targetId;
            Damage = damage;
        }
    }

    [TestFixture]
    public class EventBusTests
    {
        private EventBus _eventBus;

        [SetUp]
        public void SetUp()
        {
            _eventBus = new EventBus();
        }

        [Test]
        public void Publish_InvokesSubscribedHandler()
        {
            float receivedDamage = 0f;
            int receivedTarget = 0;

            _eventBus.Subscribe<TestDamageEvent>(evt =>
            {
                receivedDamage = evt.Damage;
                receivedTarget = evt.TargetId;
            });

            _eventBus.Publish(new TestDamageEvent(targetId: 42, damage: 15.5f));

            Assert.That(receivedTarget, Is.EqualTo(42));
            Assert.That(receivedDamage, Is.EqualTo(15.5f));
        }

        [Test]
        public void Unsubscribe_StopsReceivingEvents()
        {
            int callCount = 0;
            void Handler(TestDamageEvent evt) => callCount++;

            _eventBus.Subscribe<TestDamageEvent>(Handler);
            _eventBus.Publish(new TestDamageEvent(1, 10f));
            Assert.That(callCount, Is.EqualTo(1));

            _eventBus.Unsubscribe<TestDamageEvent>(Handler);
            _eventBus.Publish(new TestDamageEvent(1, 10f));
            Assert.That(callCount, Is.EqualTo(1));
        }
    }
}
