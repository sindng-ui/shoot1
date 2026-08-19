using NUnit.Framework;
using HappyShoot.Domain.Entities;
using HappyShoot.Domain.Skills;
using HappyShoot.Domain.Skills.Effects;
using HappyShoot.Domain.Spatial;

namespace HappyShoot.Domain.Tests.Skills
{
    [TestFixture]
    public class OrbitingBladesTests
    {
        private SpatialGrid2D<MonsterEntity> _grid;
        private MonsterSpawner _spawner;
        private PlayerEntity _player;

        [SetUp]
        public void SetUp()
        {
            _grid = new SpatialGrid2D<MonsterEntity>(cellSize: 2.0f);
            _spawner = new MonsterSpawner(_grid);
            _player = new PlayerEntity(1, CharacterStats.Default, Vector2D.Zero);
        }

        [Test]
        public void Execute_DamagesMonstersInOrbitalPath()
        {
            // Place orbiting blade with radius 2.0
            var effect = new OrbitingBladesEffect(baseDamage: 30f, orbitRadius: 2.0f, rotationSpeed: 0f, bladeCount: 2);
            
            // Blade 0 will be at (2, 0). Place a monster at (2, 0)
            var monster = _spawner.SpawnMonster("Slime", maxHealth: 50f, moveSpeed: 1f, contactDamage: 5f, expValue: 1, goldValue: 1, position: new Vector2D(2f, 0f));

            var context = new SkillContext
            {
                CasterId = 1,
                CasterPosition = Vector2D.Zero,
                BaseDamage = 10f,
                AreaMultiplier = 1.0f,
                TargetGrid = _grid
            };

            effect.ApplyEffect(context, null);

            Assert.That(monster.CurrentHealth, Is.EqualTo(20f)); // 50 - 30 = 20
        }

        [Test]
        public void GetBladePosition_ReturnsCorrectCoordinates()
        {
            var effect = new OrbitingBladesEffect(baseDamage: 20f, orbitRadius: 3.0f, rotationSpeed: 0f, bladeCount: 4);

            Vector2D blade0 = effect.GetBladePosition(Vector2D.Zero, 0, areaMultiplier: 1.0f);
            Assert.That(blade0.X, Is.EqualTo(3.0f).Within(0.01f));
            Assert.That(blade0.Y, Is.EqualTo(0.0f).Within(0.01f));

            Vector2D blade1 = effect.GetBladePosition(Vector2D.Zero, 1, areaMultiplier: 1.0f);
            Assert.That(blade1.X, Is.EqualTo(0.0f).Within(0.01f));
            Assert.That(blade1.Y, Is.EqualTo(3.0f).Within(0.01f));
        }
    }
}
