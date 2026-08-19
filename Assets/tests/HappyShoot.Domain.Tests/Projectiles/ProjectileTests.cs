using NUnit.Framework;
using HappyShoot.Domain.Entities;
using HappyShoot.Domain.Projectiles;
using HappyShoot.Domain.Spatial;

namespace HappyShoot.Domain.Tests.Projectiles
{
    [TestFixture]
    public class ProjectileTests
    {
        private SpatialGrid2D<MonsterEntity> _monsterGrid;
        private MonsterSpawner _spawner;
        private ProjectileManager _projectileManager;

        [SetUp]
        public void SetUp()
        {
            _monsterGrid = new SpatialGrid2D<MonsterEntity>(cellSize: 2.0f);
            _spawner = new MonsterSpawner(_monsterGrid);
            _projectileManager = new ProjectileManager();
        }

        [Test]
        public void LaunchProjectile_HitsMonsterAndDealsDamage()
        {
            // Spawn monster at (5, 0)
            var monster = _spawner.SpawnMonster("Slime", maxHealth: 50f, moveSpeed: 0f, contactDamage: 5f, expValue: 1, goldValue: 1, new Vector2D(5f, 0f));

            // Launch projectile from (0, 0) to Right at speed 10, damage 25, pierce 1
            _projectileManager.LaunchProjectile(
                origin: new Vector2D(0f, 0f),
                direction: Vector2D.Right,
                speed: 10f,
                damage: 25f,
                pierceCount: 1,
                lifetime: 2f
            );

            // Update tick (0.5s -> projectile moves 5 units to (5, 0))
            _projectileManager.Update(deltaTime: 0.5f, _monsterGrid);

            Assert.That(monster.CurrentHealth, Is.EqualTo(25f));
            Assert.That(_projectileManager.ActiveCount, Is.EqualTo(0)); // Expired due to pierce consumed
        }

        [Test]
        public void PiercingProjectile_PiercesMultipleMonsters()
        {
            // Spawn 2 monsters in a line at (3, 0) and (6, 0)
            var m1 = _spawner.SpawnMonster("Slime1", 50f, 0f, 5f, 1, 1, new Vector2D(3f, 0f));
            var m2 = _spawner.SpawnMonster("Slime2", 50f, 0f, 5f, 1, 1, new Vector2D(6f, 0f));

            // Launch projectile with PierceCount = 2, speed 10
            _projectileManager.LaunchProjectile(
                origin: new Vector2D(0f, 0f),
                direction: Vector2D.Right,
                speed: 10f,
                damage: 20f,
                pierceCount: 2,
                lifetime: 2f
            );

            // Tick 1: reaches 3.0 units (hits m1 exactly at (3, 0))
            _projectileManager.Update(deltaTime: 0.3f, _monsterGrid);
            Assert.That(m1.CurrentHealth, Is.EqualTo(30f));
            Assert.That(m2.CurrentHealth, Is.EqualTo(50f));
            Assert.That(_projectileManager.ActiveCount, Is.EqualTo(1));

            // Tick 2: reaches 6.0 units (hits m2 exactly at (6, 0))
            _projectileManager.Update(deltaTime: 0.3f, _monsterGrid);
            Assert.That(m2.CurrentHealth, Is.EqualTo(30f));
            Assert.That(_projectileManager.ActiveCount, Is.EqualTo(0)); // All 2 pierces consumed
        }
    }
}
