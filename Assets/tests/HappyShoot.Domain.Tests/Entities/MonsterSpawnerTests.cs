using System.Collections.Generic;
using NUnit.Framework;
using HappyShoot.Domain.Entities;
using HappyShoot.Domain.Spatial;

namespace HappyShoot.Domain.Tests.Entities
{
    [TestFixture]
    public class MonsterSpawnerTests
    {
        private SpatialGrid2D<MonsterEntity> _grid;
        private MonsterSpawner _spawner;

        [SetUp]
        public void SetUp()
        {
            _grid = new SpatialGrid2D<MonsterEntity>(cellSize: 2.0f);
            _spawner = new MonsterSpawner(_grid, initialPoolSize: 10);
        }

        [Test]
        public void SpawnMonster_RegistersInGridAndActiveList()
        {
            var monster = _spawner.SpawnMonster("Slime", 30f, 2f, 5f, 1, 1, new Vector2D(5f, 5f));

            Assert.That(_spawner.ActiveCount, Is.EqualTo(1));
            Assert.That(monster.IsActive, Is.True);

            var queryBuffer = new List<MonsterEntity>();
            int count = _grid.QueryRadiusNonAlloc(new Vector2D(5f, 5f), 1.0f, queryBuffer);

            Assert.That(count, Is.EqualTo(1));
            Assert.That(queryBuffer[0], Is.SameAs(monster));
        }

        [Test]
        public void Update_MovesAllMonstersTowardPlayer()
        {
            var m1 = _spawner.SpawnMonster("Slime", 30f, 2f, 5f, 1, 1, new Vector2D(10f, 0f));
            var m2 = _spawner.SpawnMonster("Slime", 30f, 2f, 5f, 1, 1, new Vector2D(0f, 10f));

            // Player at (0, 0)
            _spawner.Update(new Vector2D(0f, 0f), deltaTime: 1.0f);

            Assert.That(m1.Position.X, Is.EqualTo(8.0f).Within(1e-4f));
            Assert.That(m2.Position.Y, Is.EqualTo(8.0f).Within(1e-4f));
        }

        [Test]
        public void Update_AutomaticallyDespawnsDeadMonsters()
        {
            var monster = _spawner.SpawnMonster("Slime", 30f, 2f, 5f, 1, 1, new Vector2D(2f, 2f));
            monster.TakeDamage(35f); // Kill

            Assert.That(monster.IsDead, Is.True);

            _spawner.Update(new Vector2D(0f, 0f), deltaTime: 0.1f);

            Assert.That(_spawner.ActiveCount, Is.EqualTo(0));
            Assert.That(_grid.EntityCount, Is.EqualTo(0));
        }
    }
}
