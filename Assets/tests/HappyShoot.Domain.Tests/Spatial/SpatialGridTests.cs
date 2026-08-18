using System.Collections.Generic;
using NUnit.Framework;
using HappyShoot.Domain.Spatial;

namespace HappyShoot.Domain.Tests.Spatial
{
    public class MockSpatialEntity : ISpatialEntity
    {
        public int Id { get; set; }
        public Vector2D Position { get; set; }
        public float Radius { get; set; } = 0.5f;
        public bool IsActive { get; set; } = true;

        public MockSpatialEntity(int id, Vector2D position, float radius = 0.5f)
        {
            Id = id;
            Position = position;
            Radius = radius;
            IsActive = true;
        }
    }

    [TestFixture]
    public class SpatialGridTests
    {
        private SpatialGrid2D<MockSpatialEntity> _grid;
        private List<MockSpatialEntity> _queryBuffer;

        [SetUp]
        public void SetUp()
        {
            _grid = new SpatialGrid2D<MockSpatialEntity>(cellSize: 2.0f);
            _queryBuffer = new List<MockSpatialEntity>(16);
        }

        [Test]
        public void RegisterAndQueryRadius_FindsEntityWithinRange()
        {
            var e1 = new MockSpatialEntity(1, new Vector2D(1.0f, 1.0f));
            var e2 = new MockSpatialEntity(2, new Vector2D(10.0f, 10.0f));

            _grid.Register(e1);
            _grid.Register(e2);

            int count = _grid.QueryRadiusNonAlloc(new Vector2D(0.5f, 0.5f), radius: 2.0f, _queryBuffer);

            Assert.That(count, Is.EqualTo(1));
            Assert.That(_queryBuffer[0].Id, Is.EqualTo(1));
        }

        [Test]
        public void UpdatePosition_MovesEntityBetweenCellsCorrectly()
        {
            var e1 = new MockSpatialEntity(1, new Vector2D(0.5f, 0.5f));
            _grid.Register(e1);

            // Move far away
            e1.Position = new Vector2D(10.0f, 10.0f);
            _grid.UpdatePosition(e1);

            // Query old position
            int oldCount = _grid.QueryRadiusNonAlloc(new Vector2D(0f, 0f), radius: 2.0f, _queryBuffer);
            Assert.That(oldCount, Is.EqualTo(0));

            // Query new position
            int newCount = _grid.QueryRadiusNonAlloc(new Vector2D(10.0f, 10.0f), radius: 2.0f, _queryBuffer);
            Assert.That(newCount, Is.EqualTo(1));
        }

        [Test]
        public void TryGetClosest_ReturnsNearestActiveEntity()
        {
            var near = new MockSpatialEntity(1, new Vector2D(2.0f, 0.0f));
            var far = new MockSpatialEntity(2, new Vector2D(5.0f, 0.0f));
            var inactiveNear = new MockSpatialEntity(3, new Vector2D(1.0f, 0.0f)) { IsActive = false };

            _grid.Register(near);
            _grid.Register(far);
            _grid.Register(inactiveNear);

            bool found = _grid.TryGetClosest(new Vector2D(0f, 0f), maxRadius: 10.0f, out MockSpatialEntity closest);

            Assert.That(found, Is.True);
            Assert.That(closest.Id, Is.EqualTo(1)); // near (inactive is ignored)
        }

        [Test]
        public void Unregister_RemovesEntityFromQueries()
        {
            var e1 = new MockSpatialEntity(1, new Vector2D(0f, 0f));
            _grid.Register(e1);
            _grid.Unregister(e1);

            int count = _grid.QueryRadiusNonAlloc(new Vector2D(0f, 0f), radius: 2.0f, _queryBuffer);
            Assert.That(count, Is.EqualTo(0));
            Assert.That(_grid.EntityCount, Is.EqualTo(0));
        }
    }
}
