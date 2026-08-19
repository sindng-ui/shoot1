using System.Collections.Generic;
using NUnit.Framework;
using HappyShoot.Domain.Entities;
using HappyShoot.Domain.Events;
using HappyShoot.Domain.Spatial;

namespace HappyShoot.Domain.Tests.Monsters
{
    [TestFixture]
    public class MonsterVarietyTests
    {
        private SpatialGrid2D<MonsterEntity> _grid;
        private EventBus _eventBus;
        private MonsterSpawner _spawner;

        [SetUp]
        public void SetUp()
        {
            _grid = new SpatialGrid2D<MonsterEntity>(cellSize: 2.0f);
            _eventBus = new EventBus();
            _spawner = new MonsterSpawner(_grid, _eventBus);
        }

        [Test]
        public void Archetypes_HaveDistinctAttributes()
        {
            var slime = _spawner.SpawnByDefinition(MonsterDefinition.Slime, Vector2D.Zero);
            var bat = _spawner.SpawnByDefinition(MonsterDefinition.Bat, Vector2D.Zero);
            var skeleton = _spawner.SpawnByDefinition(MonsterDefinition.Skeleton, Vector2D.Zero);
            var golem = _spawner.SpawnByDefinition(MonsterDefinition.Golem, Vector2D.Zero);

            Assert.That(bat.MoveSpeed, Is.GreaterThan(slime.MoveSpeed));
            Assert.That(golem.MaxHealth, Is.GreaterThan(slime.MaxHealth));
            Assert.That(golem.ContactDamage, Is.GreaterThan(slime.ContactDamage));
            Assert.That(skeleton.IsRanged, Is.True);
            Assert.That(skeleton.PreferredDistance, Is.GreaterThan(0f));
        }

        [Test]
        public void SkeletonRangedAI_MaintainsDistanceAndTriggersAttack()
        {
            // Place skeleton at (3, 0), player at (0, 0). Preferred distance is 4.5
            var skeleton = _spawner.SpawnByDefinition(MonsterDefinition.Skeleton, new Vector2D(3f, 0f));
            Vector2D playerPos = Vector2D.Zero;

            // Since distance (3.0) < PreferredDistance (4.5), skeleton should back away
            skeleton.UpdateAI(playerPos, deltaTime: 0.5f);
            Assert.That(skeleton.Position.X, Is.GreaterThan(3f));

            // Tick past attack interval (2.0s)
            skeleton.UpdateAI(playerPos, deltaTime: 2.0f);
            Assert.That(skeleton.HasPendingRangedAttack, Is.True);

            skeleton.ConsumePendingAttack();
            Assert.That(skeleton.HasPendingRangedAttack, Is.False);
        }

        [Test]
        public void BossMonster_FiresBossSpawned_HealthUpdated_AndDiedEvents()
        {
            BossSpawnedEvent spawnEvt = default;
            BossHealthUpdatedEvent hpEvt = default;
            BossDiedEvent diedEvt = default;
            bool spawned = false, hpUpdated = false, died = false;

            _eventBus.Subscribe<BossSpawnedEvent>(evt => { spawnEvt = evt; spawned = true; });
            _eventBus.Subscribe<BossHealthUpdatedEvent>(evt => { hpEvt = evt; hpUpdated = true; });
            _eventBus.Subscribe<BossDiedEvent>(evt => { diedEvt = evt; died = true; });

            var boss = _spawner.SpawnBoss(Vector2D.Zero, "Goblin King", hp: 1000f, speed: 2.0f, damage: 30f, exp: 50, gold: 100);

            Assert.That(spawned, Is.True);
            Assert.That(spawnEvt.BossName, Is.EqualTo("Goblin King"));
            Assert.That(spawnEvt.MaxHealth, Is.EqualTo(1000f));
            Assert.That(boss.IsBoss, Is.True);

            // Take damage
            boss.TakeDamage(300f);
            Assert.That(hpUpdated, Is.True);
            Assert.That(hpEvt.CurrentHealth, Is.EqualTo(700f));

            // Lethal damage
            boss.TakeDamage(700f);
            Assert.That(died, Is.True);
            Assert.That(diedEvt.BossName, Is.EqualTo("Goblin King"));
            Assert.That(diedEvt.GoldReward, Is.EqualTo(500));
        }

        [Test]
        public void SpawnDefinitionAroundPlayer_CalculatesCoordinatesCorrectly()
        {
            Vector2D playerPos = new Vector2D(10f, 10f);
            var monster = _spawner.SpawnDefinitionAroundPlayer(playerPos, spawnRadius: 5f, angleRadians: 0f, MonsterDefinition.Bat);

            Assert.That(monster.Position.X, Is.EqualTo(15f).Within(0.01f));
            Assert.That(monster.Position.Y, Is.EqualTo(10f).Within(0.01f));
            Assert.That(monster.Type, Is.EqualTo(MonsterType.Bat));
        }
    }
}
