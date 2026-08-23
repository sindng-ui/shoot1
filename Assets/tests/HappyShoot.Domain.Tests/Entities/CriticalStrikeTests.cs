using System.Collections.Generic;
using NUnit.Framework;
using HappyShoot.Domain.Entities;
using HappyShoot.Domain.Events;
using HappyShoot.Domain.Projectiles;
using HappyShoot.Domain.Spatial;
using HappyShoot.Domain.UI;

namespace HappyShoot.Domain.Tests.Entities
{
    [TestFixture]
    public class CriticalStrikeTests
    {
        private EventBus _eventBus;

        [SetUp]
        public void SetUp()
        {
            _eventBus = new EventBus();
        }

        [Test]
        public void PlayerEntity_DefaultCritChance_IsTenPercent()
        {
            var player = new PlayerEntity(1, CharacterStats.Default, Vector2D.Zero, _eventBus);
            Assert.That(player.Stats.CritChance, Is.EqualTo(0.10f).Within(0.001f));
            Assert.That(player.Stats.CritDamageMultiplier, Is.EqualTo(1.50f).Within(0.001f));
        }

        [Test]
        public void PlayerEntity_RollDamage_GuaranteedCrit_MultipliesDamage()
        {
            // Set 100% crit chance, 2.5x multiplier
            var stats = new CharacterStats(100f, 0f, 5f, 1f, 0f, critChance: 1.0f, critDamageMultiplier: 2.5f, 0f, 1f, 1f, 0, 2f);
            var player = new PlayerEntity(1, stats, Vector2D.Zero, _eventBus);

            var (damage, isCritical) = player.RollDamage(40f);

            Assert.That(isCritical, Is.True);
            Assert.That(damage, Is.EqualTo(100f).Within(0.01f)); // 40 * 2.5 = 100
        }

        [Test]
        public void PlayerEntity_RollDamage_ZeroCrit_ReturnsRawDamage()
        {
            // Set 0% crit chance
            var stats = new CharacterStats(100f, 0f, 5f, 1f, 0f, critChance: 0.0f, critDamageMultiplier: 2.5f, 0f, 1f, 1f, 0, 2f);
            var player = new PlayerEntity(1, stats, Vector2D.Zero, _eventBus);

            var (damage, isCritical) = player.RollDamage(40f);

            Assert.That(isCritical, Is.False);
            Assert.That(damage, Is.EqualTo(40f).Within(0.01f));
        }

        [Test]
        public void MonsterEntity_TakeDamage_PublishesMonsterDamagedEvent_WithCriticalFlag()
        {
            var monster = new MonsterEntity();
            monster.Initialize(10, "Slime", maxHealth: 100f, moveSpeed: 2f, contactDamage: 5f, expValue: 10, goldValue: 5, startPosition: Vector2D.Zero, eventBus: _eventBus);

            MonsterDamagedEvent receivedEvent = default;
            bool eventFired = false;

            _eventBus.Subscribe<MonsterDamagedEvent>(evt =>
            {
                receivedEvent = evt;
                eventFired = true;
            });

            monster.TakeDamage(damage: 35f, isCritical: true);

            Assert.That(eventFired, Is.True);
            Assert.That(receivedEvent.MonsterId, Is.EqualTo(10));
            Assert.That(receivedEvent.Damage, Is.EqualTo(35f));
            Assert.That(receivedEvent.IsCritical, Is.True);
            Assert.That(receivedEvent.RemainingHealth, Is.EqualTo(65f));
        }

        [Test]
        public void DamageTextManager_SpawnsTextWithCriticalFlag_FromEvent()
        {
            var damageTextManager = new DamageTextManager(_eventBus, initialCapacity: 16);

            _eventBus.Publish(new MonsterDamagedEvent(101, damage: 50f, remainingHealth: 50f, maxHealth: 100f, position: new Vector2D(5f, 5f), isCritical: true));

            Assert.That(damageTextManager.ActiveCount, Is.EqualTo(1));
            var activeText = damageTextManager.ActiveTexts[0];
            Assert.That(activeText.IsCritical, Is.True);
            Assert.That(activeText.DamageValue, Is.EqualTo(50f));
        }

        [Test]
        public void ProjectileEntity_GuaranteedCrit_DealsMultiplierDamageToMonster()
        {
            var monsterGrid = new SpatialGrid2D<MonsterEntity>(cellSize: 2.0f);
            var spawner = new MonsterSpawner(monsterGrid, _eventBus);
            var monster = spawner.SpawnMonster("Slime", maxHealth: 200f, moveSpeed: 0f, contactDamage: 5f, expValue: 10, goldValue: 5, new Vector2D(2f, 0f));

            var projectileManager = new ProjectileManager(initialCapacity: 16, eventBus: _eventBus);
            // 100% crit chance, 2.0x multiplier, 30 base dmg
            projectileManager.LaunchProjectile(
                origin: Vector2D.Zero,
                direction: Vector2D.Right,
                speed: 10f,
                damage: 30f,
                pierceCount: 1,
                lifetime: 2f,
                explosionRadius: 0f,
                explosionDamage: 0f,
                critChance: 1.0f,
                critDamageMultiplier: 2.0f
            );

            // Move projectile into monster (0.2s * 10 = 2 units -> reaches (2, 0))
            projectileManager.Update(deltaTime: 0.25f, monsterGrid);

            Assert.That(monster.CurrentHealth, Is.EqualTo(140f).Within(0.01f)); // 200 - (30 * 2.0) = 140
        }
    }
}
