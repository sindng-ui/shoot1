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
    public class StatusEffectTests
    {
        private SpatialGrid2D<ISpatialEntity> _grid;
        private EventBus _eventBus;
        private SkillContext _context;

        [SetUp]
        public void SetUp()
        {
            _grid = new SpatialGrid2D<ISpatialEntity>(cellSize: 2.0f);
            _eventBus = new EventBus();
            _context = new SkillContext
            {
                CasterId = 1,
                CasterPosition = new Vector2D(0f, 0f),
                BaseDamage = 10f,
                AreaMultiplier = 1.0f,
                SpeedMultiplier = 1.0f,
                TargetGrid = _grid,
                EventBus = _eventBus
            };
        }

        private MonsterEntity CreateMonster(int id, Vector2D position, float maxHealth = 100f, float moveSpeed = 3.0f)
        {
            var monster = new MonsterEntity();
            monster.Initialize(
                id: id,
                typeName: "Slime",
                maxHealth: maxHealth,
                moveSpeed: moveSpeed,
                contactDamage: 10f,
                expValue: 1,
                goldValue: 1,
                startPosition: position,
                eventBus: _eventBus
            );
            return monster;
        }

        [Test]
        public void ApplyChill_SlowsMonsterMovementSpeed_By40Percent()
        {
            var monster = CreateMonster(101, new Vector2D(0f, 0f), maxHealth: 100f, moveSpeed: 10.0f);
            monster.ApplyChill(duration: 3.5f, slowFactor: 0.40f);

            Assert.That(monster.IsChilled, Is.True);

            // Move toward (100, 0) for 1.0 second. Without chill: 10 units. With 40% slow: 6 units.
            monster.UpdateAI(new Vector2D(100f, 0f), deltaTime: 1.0f);

            Assert.That(monster.Position.X, Is.EqualTo(6.0f).Within(0.01f));
        }

        [Test]
        public void Chill_RecoversToNormalSpeed_WhenTimerExpires()
        {
            var monster = CreateMonster(101, new Vector2D(0f, 0f), maxHealth: 100f, moveSpeed: 10.0f);
            monster.ApplyChill(duration: 1.0f, slowFactor: 0.40f);

            // UpdateAI 1.5 seconds (Chill expires after 1.0s)
            monster.UpdateAI(new Vector2D(100f, 0f), deltaTime: 1.5f);

            Assert.That(monster.IsChilled, Is.False);
        }

        [Test]
        public void ApplyBurn_DealsPeriodicTickDamage_Over7Seconds()
        {
            var monster = CreateMonster(101, new Vector2D(0f, 0f), maxHealth: 100f);
            monster.ApplyBurn(duration: 7.0f, damagePerTick: 5.0f);

            Assert.That(monster.IsBurning, Is.True);

            // 0.5s ticks 1st damage (-5 HP -> 95)
            monster.UpdateAI(new Vector2D(0f, 0f), deltaTime: 0.5f);
            Assert.That(monster.CurrentHealth, Is.EqualTo(95f));

            // Another 0.5s ticks 2nd damage (-5 HP -> 90)
            monster.UpdateAI(new Vector2D(0f, 0f), deltaTime: 0.5f);
            Assert.That(monster.CurrentHealth, Is.EqualTo(90f));
        }

        [Test]
        public void ApplyShock_DealsPeriodicTickDamage_Over7Seconds()
        {
            var monster = CreateMonster(101, new Vector2D(0f, 0f), maxHealth: 100f);
            monster.ApplyShock(duration: 7.0f, damagePerTick: 6.0f);

            Assert.That(monster.IsShocked, Is.True);

            // 0.7s ticks 1st shock damage (-6 HP -> 94)
            monster.UpdateAI(new Vector2D(0f, 0f), deltaTime: 0.7f);
            Assert.That(monster.CurrentHealth, Is.EqualTo(94f));
        }

        [Test]
        public void Monster_Publishes_MonsterShatteredEvent_WhenDyingWhileChilled()
        {
            var monster = CreateMonster(101, new Vector2D(2f, 3f), maxHealth: 20f);
            monster.ApplyChill(duration: 3.5f, slowFactor: 0.40f);

            bool shattered = false;
            _eventBus.Subscribe<MonsterShatteredEvent>(e =>
            {
                shattered = true;
                Assert.That(e.MonsterId, Is.EqualTo(101));
                Assert.That(e.Position.X, Is.EqualTo(2f));
            });

            monster.TakeDamage(25f); // Lethal damage while chilled

            Assert.That(shattered, Is.True);
        }

        [Test]
        public void MeteorStrike_DealsDamage_AndAppliesBurn_AndPublishes_MeteorStrikeExecutedEvent()
        {
            var meteor = new MeteorStrikeEffect(baseDamage: 120f, explosionRadius: 5.0f);
            var monster = CreateMonster(101, new Vector2D(3f, 0f), maxHealth: 200f);
            _grid.Register(monster);

            bool eventReceived = false;
            _eventBus.Subscribe<MeteorStrikeExecutedEvent>(e =>
            {
                eventReceived = true;
                Assert.That(e.TargetPosition.X, Is.EqualTo(3f));
                Assert.That(e.Damage, Is.EqualTo(120f));
            });

            meteor.ApplyEffect(_context, new List<Vector2D> { new Vector2D(3f, 0f) });

            Assert.That(eventReceived, Is.True);
            Assert.That(monster.CurrentHealth, Is.EqualTo(80f)); // 200 - 120
            Assert.That(monster.IsBurning, Is.True); // 7-second Burn DoT applied
        }
    }
}
