using NUnit.Framework;
using HappyShoot.Domain.Events;

namespace HappyShoot.Domain.Tests.Events
{
    [TestFixture]
    public class AudioEventsTests
    {
        private EventBus _eventBus;

        [SetUp]
        public void SetUp()
        {
            _eventBus = new EventBus();
        }

        [Test]
        public void Publish_PlaySoundEvent_DeliversCorrectSoundTypeAndVolume()
        {
            PlaySoundEvent received = default;
            bool called = false;

            _eventBus.Subscribe<PlaySoundEvent>(evt =>
            {
                received = evt;
                called = true;
            });

            _eventBus.Publish(new PlaySoundEvent(SoundEffectType.WeaponEvolve, 0.8f));

            Assert.That(called, Is.True);
            Assert.That(received.SoundType, Is.EqualTo(SoundEffectType.WeaponEvolve));
            Assert.That(received.Volume, Is.EqualTo(0.8f));
        }

        [Test]
        public void Publish_PlayBgmEvent_DeliversTrackNameAndVolume()
        {
            PlayBgmEvent received = default;
            bool called = false;

            _eventBus.Subscribe<PlayBgmEvent>(evt =>
            {
                received = evt;
                called = true;
            });

            _eventBus.Publish(new PlayBgmEvent("Battle_Retro_Loop", 0.6f));

            Assert.That(called, Is.True);
            Assert.That(received.BgmTrackName, Is.EqualTo("Battle_Retro_Loop"));
            Assert.That(received.Volume, Is.EqualTo(0.6f));
        }

        [Test]
        public void Publish_StopBgmEvent_TriggersHandler()
        {
            bool stopCalled = false;

            _eventBus.Subscribe<StopBgmEvent>(evt =>
            {
                stopCalled = true;
            });

            _eventBus.Publish(new StopBgmEvent());

            Assert.That(stopCalled, Is.True);
        }
    }
}
