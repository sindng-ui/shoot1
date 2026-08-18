using NUnit.Framework;
using HappyShoot.Domain.Entities;
using HappyShoot.Domain.Events;
using HappyShoot.Domain.Spatial;

namespace HappyShoot.Domain.Tests.Entities
{
    [TestFixture]
    public class PlayerEntityTests
    {
        private EventBus _eventBus;
        private PlayerEntity _player;

        [SetUp]
        public void SetUp()
        {
            _eventBus = new EventBus();
            _player = new PlayerEntity(
                id: 1,
                stats: CharacterStats.Default,
                startPosition: new Vector2D(0f, 0f),
                eventBus: _eventBus
            );
        }

        [Test]
        public void Move_UpdatesPositionAndPublishesEvent()
        {
            Vector2D movedPos = Vector2D.Zero;
            _eventBus.Subscribe<PlayerMovedEvent>(evt => movedPos = evt.Position);

            // Move Right for 1 second (Speed is 5.0)
            _player.Move(Vector2D.Right, deltaTime: 1.0f);

            Assert.That(_player.Position.X, Is.EqualTo(5.0f).Within(1e-4f));
            Assert.That(_player.Position.Y, Is.EqualTo(0f).Within(1e-4f));
            Assert.That(movedPos.X, Is.EqualTo(5.0f).Within(1e-4f));
        }

        [Test]
        public void TakeDamage_WithArmor_ReducesDamageMitigated()
        {
            // Default player has Armor 0 (takes 100% damage)
            _player.TakeDamage(30f);
            Assert.That(_player.CurrentHealth, Is.EqualTo(70f).Within(1e-4f));

            // With Armor 100 -> takes 50% damage: 30 * (100 / 200) = 15
            var armoredStats = new CharacterStats(
                maxHealth: 100f, healthRegen: 0f, moveSpeed: 5f, attackPowerMultiplier: 1f,
                armor: 100f, critChance: 0f, critDamageMultiplier: 1f, cooldownReduction: 0f,
                areaMultiplier: 1f, projectileSpeedMultiplier: 1f, extraProjectiles: 0, pickupRadius: 2f
            );
            var armoredPlayer = new PlayerEntity(2, armoredStats, Vector2D.Zero);
            armoredPlayer.TakeDamage(30f);

            Assert.That(armoredPlayer.CurrentHealth, Is.EqualTo(85f).Within(1e-4f));
        }

        [Test]
        public void TakeDamage_Lethal_KillsPlayerAndFiresDiedEvent()
        {
            bool isDiedEventFired = false;
            _eventBus.Subscribe<PlayerDiedEvent>(evt => isDiedEventFired = true);

            _player.TakeDamage(150f);

            Assert.That(_player.IsDead, Is.True);
            Assert.That(_player.CurrentHealth, Is.EqualTo(0f));
            Assert.That(isDiedEventFired, Is.True);
        }

        [Test]
        public void Heal_IncreasesHealth_CappedAtMaxHealth()
        {
            _player.TakeDamage(50f);
            Assert.That(_player.CurrentHealth, Is.EqualTo(50f));

            _player.Heal(30f);
            Assert.That(_player.CurrentHealth, Is.EqualTo(80f));

            _player.Heal(50f); // Should cap at 100
            Assert.That(_player.CurrentHealth, Is.EqualTo(100f));
        }
    }
}
