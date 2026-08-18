using System;
using System.Collections.Generic;

namespace HappyShoot.Domain.Spatial
{
    /// <summary>
    /// High-performance 2D spatial hash grid for fast O(1) proximity queries and zero-allocation collision checks.
    /// </summary>
    public class SpatialGrid2D<T> : ISpatialGrid2D where T : class, ISpatialEntity
    {
        private readonly float _cellSize;
        private readonly float _invCellSize;
        private readonly Dictionary<long, List<T>> _grid;
        private readonly Dictionary<int, long> _entityCellMap;

        public float CellSize => _cellSize;
        public int EntityCount => _entityCellMap.Count;

        public SpatialGrid2D(float cellSize = 2.0f)
        {
            if (cellSize <= 0f) throw new ArgumentOutOfRangeException(nameof(cellSize));
            _cellSize = cellSize;
            _invCellSize = 1.0f / cellSize;
            _grid = new Dictionary<long, List<T>>(256);
            _entityCellMap = new Dictionary<int, long>(256);
        }

        private long GetCellKey(int cellX, int cellY)
        {
            return ((long)cellX << 32) | (uint)cellY;
        }

        private long GetCellKeyFromPosition(Vector2D position)
        {
            int cellX = (int)Math.Floor(position.X * _invCellSize);
            int cellY = (int)Math.Floor(position.Y * _invCellSize);
            return GetCellKey(cellX, cellY);
        }

        /// <summary>
        /// Registers an entity in the spatial grid.
        /// </summary>
        public void Register(T entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            long cellKey = GetCellKeyFromPosition(entity.Position);
            if (!_grid.TryGetValue(cellKey, out var list))
            {
                list = new List<T>(8);
                _grid[cellKey] = list;
            }

            list.Add(entity);
            _entityCellMap[entity.Id] = cellKey;
        }

        /// <summary>
        /// Updates the entity's cell assignment if it moved across cells.
        /// </summary>
        public void UpdatePosition(T entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            if (!_entityCellMap.TryGetValue(entity.Id, out long oldKey))
            {
                Register(entity);
                return;
            }

            long newKey = GetCellKeyFromPosition(entity.Position);
            if (oldKey != newKey)
            {
                if (_grid.TryGetValue(oldKey, out var oldList))
                {
                    oldList.Remove(entity);
                }

                if (!_grid.TryGetValue(newKey, out var newList))
                {
                    newList = new List<T>(8);
                    _grid[newKey] = newList;
                }

                newList.Add(entity);
                _entityCellMap[entity.Id] = newKey;
            }
        }

        /// <summary>
        /// Removes an entity from the spatial grid.
        /// </summary>
        public void Unregister(T entity)
        {
            if (entity == null) return;

            if (_entityCellMap.TryGetValue(entity.Id, out long cellKey))
            {
                if (_grid.TryGetValue(cellKey, out var list))
                {
                    list.Remove(entity);
                }
                _entityCellMap.Remove(entity.Id);
            }
        }

        /// <summary>
        /// Performs a zero-allocation query for all active entities within a circular radius.
        /// </summary>
        public int QueryRadiusNonAlloc(Vector2D center, float radius, IList<T> resultsBuffer)
        {
            if (resultsBuffer == null) throw new ArgumentNullException(nameof(resultsBuffer));
            resultsBuffer.Clear();

            int minX = (int)Math.Floor((center.X - radius) * _invCellSize);
            int maxX = (int)Math.Floor((center.X + radius) * _invCellSize);
            int minY = (int)Math.Floor((center.Y - radius) * _invCellSize);
            int maxY = (int)Math.Floor((center.Y + radius) * _invCellSize);

            float sqrRadius = radius * radius;

            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    long key = GetCellKey(x, y);
                    if (_grid.TryGetValue(key, out var cellList))
                    {
                        for (int i = 0; i < cellList.Count; i++)
                        {
                            T candidate = cellList[i];
                            if (!candidate.IsActive) continue;

                            float sqrDist = Vector2D.SqrDistance(center, candidate.Position);
                            float combinedRadius = radius + candidate.Radius;
                            if (sqrDist <= combinedRadius * combinedRadius)
                            {
                                resultsBuffer.Add(candidate);
                            }
                        }
                    }
                }
            }

            return resultsBuffer.Count;
        }

        /// <summary>
        /// Finds the closest active entity within maxRadius.
        /// </summary>
        public bool TryGetClosest(Vector2D center, float maxRadius, out T closest)
        {
            closest = null;
            float closestSqrDist = maxRadius * maxRadius;

            int minX = (int)Math.Floor((center.X - maxRadius) * _invCellSize);
            int maxX = (int)Math.Floor((center.X + maxRadius) * _invCellSize);
            int minY = (int)Math.Floor((center.Y - maxRadius) * _invCellSize);
            int maxY = (int)Math.Floor((center.Y + maxRadius) * _invCellSize);

            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    long key = GetCellKey(x, y);
                    if (_grid.TryGetValue(key, out var cellList))
                    {
                        for (int i = 0; i < cellList.Count; i++)
                        {
                            T candidate = cellList[i];
                            if (!candidate.IsActive) continue;

                            float sqrDist = Vector2D.SqrDistance(center, candidate.Position);
                            if (sqrDist < closestSqrDist)
                            {
                                closestSqrDist = sqrDist;
                                closest = candidate;
                            }
                        }
                    }
                }
            }

            return closest != null;
        }

        int ISpatialGrid2D.QueryRadiusNonAlloc(Vector2D center, float radius, IList<ISpatialEntity> resultsBuffer)
        {
            if (resultsBuffer == null) throw new ArgumentNullException(nameof(resultsBuffer));
            resultsBuffer.Clear();

            int minX = (int)Math.Floor((center.X - radius) * _invCellSize);
            int maxX = (int)Math.Floor((center.X + radius) * _invCellSize);
            int minY = (int)Math.Floor((center.Y - radius) * _invCellSize);
            int maxY = (int)Math.Floor((center.Y + radius) * _invCellSize);

            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    long key = GetCellKey(x, y);
                    if (_grid.TryGetValue(key, out var cellList))
                    {
                        for (int i = 0; i < cellList.Count; i++)
                        {
                            T candidate = cellList[i];
                            if (!candidate.IsActive) continue;

                            float sqrDist = Vector2D.SqrDistance(center, candidate.Position);
                            float combinedRadius = radius + candidate.Radius;
                            if (sqrDist <= combinedRadius * combinedRadius)
                            {
                                resultsBuffer.Add(candidate);
                            }
                        }
                    }
                }
            }

            return resultsBuffer.Count;
        }

        bool ISpatialGrid2D.TryGetClosest(Vector2D center, float maxRadius, out ISpatialEntity closest)
        {
            bool found = TryGetClosest(center, maxRadius, out T typedClosest);
            closest = typedClosest;
            return found;
        }

        /// <summary>
        /// Clears all grid cell mappings.
        /// </summary>
        public void Clear()
        {
            _grid.Clear();
            _entityCellMap.Clear();
        }
    }
}
