using System.Collections.Generic;
using NUnit.Framework;
using HappyShoot.Domain.Entities;
using HappyShoot.Domain.Events;
using HappyShoot.Domain.Skills;
using HappyShoot.Domain.Skills.Effects;
using HappyShoot.Domain.Spatial;

namespace HappyShoot.Domain.Tests.Skills
{
    [TestFixture]
    public class RangerReworkSkillsTests
    {
        [Test]
        public void PiercingArrowEffect_GainsArrowsOnLevelUp()
        {
            var effect = new PiercingArrowEffect(25f, 16f, 1, 999);
            Assert.That(effect.ArrowCount, Is.EqualTo(1));

            effect.OnLevelUp(2);
            Assert.That(effect.ArrowCount, Is.EqualTo(2));

            effect.OnLevelUp(3);
            Assert.That(effect.ArrowCount, Is.EqualTo(3));

            effect.OnLevelUp(5);
            Assert.That(effect.ArrowCount, Is.EqualTo(5));
            Assert.That(effect.BaseDamage, Is.EqualTo(25f + 7f * 4));
        }

        [Test]
        public void WindGlaiveEffect_PublishesDomainEventAndDamagesTargets()
        {
            var eventBus = new EventBus();
            bool eventFired = false;
            eventBus.Subscribe<WindGlaiveExecutedEvent>(evt =>
            {
                eventFired = true;
                Assert.That(evt.GlaiveCount, Is.EqualTo(1));
                Assert.That(evt.Damage, Is.GreaterThan(30f));
            });

            var grid = new SpatialGrid2D<ISpatialEntity>(cellSize: 2.0f);
            var monster = new MonsterEntity();
            monster.Initialize(
                id: 1,
                typeName: "Slime",
                maxHealth: 100f,
                moveSpeed: 2.0f,
                contactDamage: 10f,
                expValue: 1,
                goldValue: 1,
                startPosition: new Vector2D(3f, 0f),
                eventBus: eventBus
            );
            grid.Register(monster);

            var context = new SkillContext
            {
                CasterId = 0,
                CasterPosition = new Vector2D(0f, 0f),
                EventBus = eventBus,
                TargetGrid = grid,
                BaseDamage = 10f,
                AreaMultiplier = 1.0f,
                SpeedMultiplier = 1.0f
            };

            var glaive = new WindGlaiveEffect(35f, 9.0f, 16f, 1);
            glaive.ApplyEffect(context, new List<Vector2D> { new Vector2D(5f, 0f) });

            Assert.That(eventFired, Is.True);
        }
    }
}
