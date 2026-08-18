using System;
using NUnit.Framework;
using HappyShoot.Domain.Pool;

namespace HappyShoot.Domain.Tests.Pool
{
    public class MockPoolable : IPoolable
    {
        public bool IsActive { get; private set; }
        public int SpawnCount { get; private set; }
        public int DespawnCount { get; private set; }

        public void OnSpawn()
        {
            IsActive = true;
            SpawnCount++;
        }

        public void OnDespawn()
        {
            IsActive = false;
            DespawnCount++;
        }
    }

    [TestFixture]
    public class ObjectPoolTests
    {
        [Test]
        public void Prewarm_InstantiatesExpectedCount()
        {
            var pool = new ObjectPool<MockPoolable>(() => new MockPoolable(), initialCapacity: 10);

            Assert.That(pool.TotalCreated, Is.EqualTo(10));
            Assert.That(pool.InactiveCount, Is.EqualTo(10));
            Assert.That(pool.ActiveCount, Is.EqualTo(0));
        }

        [Test]
        public void Spawn_ReturnsItemAndCallsOnSpawn()
        {
            var pool = new ObjectPool<MockPoolable>(() => new MockPoolable(), initialCapacity: 5);

            var item = pool.Spawn();

            Assert.That(item, Is.Not.Null);
            Assert.That(item.IsActive, Is.True);
            Assert.That(item.SpawnCount, Is.EqualTo(1));
            Assert.That(pool.ActiveCount, Is.EqualTo(1));
            Assert.That(pool.InactiveCount, Is.EqualTo(4));
        }

        [Test]
        public void Despawn_ReturnsItemToPoolAndCallsOnDespawn()
        {
            var pool = new ObjectPool<MockPoolable>(() => new MockPoolable(), initialCapacity: 2);
            var item = pool.Spawn();

            pool.Despawn(item);

            Assert.That(item.IsActive, Is.False);
            Assert.That(item.DespawnCount, Is.EqualTo(1));
            Assert.That(pool.ActiveCount, Is.EqualTo(0));
            Assert.That(pool.InactiveCount, Is.EqualTo(2));
        }

        [Test]
        public void ReusingSpawnedItem_PreservesReferenceAndIncrementsLifecycle()
        {
            var pool = new ObjectPool<MockPoolable>(() => new MockPoolable(), initialCapacity: 1);
            var item1 = pool.Spawn();
            pool.Despawn(item1);

            var item2 = pool.Spawn();

            Assert.That(item2, Is.SameAs(item1));
            Assert.That(item2.SpawnCount, Is.EqualTo(2));
            Assert.That(item2.DespawnCount, Is.EqualTo(1));
        }

        [Test]
        public void MaxCapacity_ThrowsWhenExceeded()
        {
            var pool = new ObjectPool<MockPoolable>(() => new MockPoolable(), initialCapacity: 1, maxCapacity: 1);
            pool.Spawn();

            Assert.Throws<InvalidOperationException>(() => pool.Spawn());
        }
    }
}
