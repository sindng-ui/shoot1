using System.Collections.Generic;
using NUnit.Framework;
using HappyShoot.Domain.Skills;
using HappyShoot.Domain.Skills.Triggers;
using HappyShoot.Domain.Skills.Targeters;
using HappyShoot.Domain.Spatial;

namespace HappyShoot.Domain.Tests.Skills
{
    public class MockSkillEffect : ISkillEffect
    {
        public int ApplyCount { get; private set; }
        public List<Vector2D> LastTargets { get; } = new List<Vector2D>();

        public void ApplyEffect(SkillContext context, IList<Vector2D> targetPositions)
        {
            ApplyCount++;
            LastTargets.Clear();
            foreach (var pos in targetPositions)
            {
                LastTargets.Add(pos);
            }
        }
    }

    public class MockTargetEntity : ISpatialEntity
    {
        public int Id { get; }
        public Vector2D Position { get; }
        public float Radius => 0.5f;
        public bool IsActive => true;

        public MockTargetEntity(int id, Vector2D position)
        {
            Id = id;
            Position = position;
        }
    }

    [TestFixture]
    public class SkillCompositionTests
    {
        private SpatialGrid2D<ISpatialEntity> _grid;
        private SkillContext _context;

        [SetUp]
        public void SetUp()
        {
            _grid = new SpatialGrid2D<ISpatialEntity>(cellSize: 2.0f);
            _context = new SkillContext
            {
                CasterId = 1,
                CasterPosition = new Vector2D(0f, 0f),
                BaseDamage = 10f,
                TargetGrid = _grid
            };
        }

        [Test]
        public void CompositeSkill_FiresWhenCooldownReadyAndTargetFound()
        {
            var trigger = new CooldownTrigger(1.0f);
            var targeter = new ClosestEnemyTargeter();
            var effect = new MockSkillEffect();

            var skill = new CompositeSkill("bow", "Piercing Bow", trigger, targeter, effect, range: 10f);

            // Add enemy at (3, 0)
            _grid.Register(new MockTargetEntity(101, new Vector2D(3f, 0f)));

            // Update tick (1.0s)
            skill.Update(1.0f, _context);

            Assert.That(effect.ApplyCount, Is.EqualTo(1));
            Assert.That(effect.LastTargets.Count, Is.EqualTo(1));
            Assert.That(effect.LastTargets[0].X, Is.EqualTo(3f));
        }

        [Test]
        public void CompositeSkill_DoesNotFire_WhenNoTargetInRange()
        {
            var trigger = new CooldownTrigger(1.0f);
            var targeter = new ClosestEnemyTargeter();
            var effect = new MockSkillEffect();

            var skill = new CompositeSkill("slash", "Slash", trigger, targeter, effect, range: 2f);

            // Enemy is out of range at (10, 0)
            _grid.Register(new MockTargetEntity(102, new Vector2D(10f, 0f)));

            skill.Update(1.0f, _context);

            Assert.That(effect.ApplyCount, Is.EqualTo(0));
        }

        [Test]
        public void CompositeSkill_LevelUp_CapsAtMaxLevel()
        {
            var skill = new CompositeSkill(
                "fireball", "Fireball",
                new CooldownTrigger(1f),
                new ClosestEnemyTargeter(),
                new MockSkillEffect(),
                maxLevel: 5
            );

            Assert.That(skill.Level, Is.EqualTo(1));

            for (int i = 0; i < 10; i++)
            {
                skill.LevelUp();
            }

            Assert.That(skill.Level, Is.EqualTo(5));
            Assert.That(skill.IsMaxLevel, Is.True);
        }
    }
}
