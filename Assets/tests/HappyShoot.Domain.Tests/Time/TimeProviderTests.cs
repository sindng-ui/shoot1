using System;
using NUnit.Framework;
using HappyShoot.Domain.Time;

namespace HappyShoot.Domain.Tests.Time
{
    [TestFixture]
    public class TimeProviderTests
    {
        private VirtualTimeProvider _timeProvider;

        [SetUp]
        public void SetUp()
        {
            _timeProvider = new VirtualTimeProvider(initialTime: 0f);
        }

        [Test]
        public void InitialState_TimeAndDeltaTime_ShouldBeZero()
        {
            Assert.That(_timeProvider.Time, Is.EqualTo(0f));
            Assert.That(_timeProvider.DeltaTime, Is.EqualTo(0f));
            Assert.That(_timeProvider.TimeScale, Is.EqualTo(1f));
        }

        [Test]
        public void Tick_AdvancesTimeAndDeltaTimeCorrectly()
        {
            _timeProvider.Tick(0.5f);
            Assert.That(_timeProvider.Time, Is.EqualTo(0.5f));
            Assert.That(_timeProvider.DeltaTime, Is.EqualTo(0.5f));

            _timeProvider.Tick(0.25f);
            Assert.That(_timeProvider.Time, Is.EqualTo(0.75f));
            Assert.That(_timeProvider.DeltaTime, Is.EqualTo(0.25f));
        }

        [Test]
        public void Tick_WithTimeScale_ScalesTimeCorrectly()
        {
            _timeProvider.TimeScale = 2.0f;
            _timeProvider.Tick(0.5f);

            Assert.That(_timeProvider.DeltaTime, Is.EqualTo(1.0f));
            Assert.That(_timeProvider.Time, Is.EqualTo(1.0f));
        }

        [Test]
        public void Tick_WithNegativeDelta_ThrowsException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _timeProvider.Tick(-0.1f));
        }

        [Test]
        public void Reset_ResetsTimeToTarget()
        {
            _timeProvider.Tick(5.0f);
            _timeProvider.Reset(10.0f);

            Assert.That(_timeProvider.Time, Is.EqualTo(10.0f));
            Assert.That(_timeProvider.DeltaTime, Is.EqualTo(0f));
        }
    }
}
