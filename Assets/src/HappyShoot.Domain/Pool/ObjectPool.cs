using System;
using System.Collections.Generic;

namespace HappyShoot.Domain.Pool
{
    /// <summary>
    /// High-performance, zero-allocation generic object pool.
    /// </summary>
    public class ObjectPool<T> where T : class, IPoolable
    {
        private readonly Func<T> _factory;
        private readonly Stack<T> _pool;
        private readonly int _maxCapacity;
        private int _totalCreated;

        public int InactiveCount => _pool.Count;
        public int TotalCreated => _totalCreated;
        public int ActiveCount => _totalCreated - _pool.Count;
        public int MaxCapacity => _maxCapacity;

        /// <summary>
        /// Creates an object pool with the specified factory function.
        /// </summary>
        /// <param name="factory">Method to create a new instance when pool is empty.</param>
        /// <param name="initialCapacity">Number of items to pre-instantiate.</param>
        /// <param name="maxCapacity">Maximum allowable objects in pool (int.MaxValue for unbounded).</param>
        public ObjectPool(Func<T> factory, int initialCapacity = 16, int maxCapacity = int.MaxValue)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            if (initialCapacity < 0) throw new ArgumentOutOfRangeException(nameof(initialCapacity));
            if (maxCapacity <= 0 || maxCapacity < initialCapacity) throw new ArgumentOutOfRangeException(nameof(maxCapacity));

            _maxCapacity = maxCapacity;
            _pool = new Stack<T>(initialCapacity);

            Prewarm(initialCapacity);
        }

        /// <summary>
        /// Pre-allocates items into the pool.
        /// </summary>
        public void Prewarm(int count)
        {
            int allowed = Math.Min(count, _maxCapacity - _totalCreated);
            for (int i = 0; i < allowed; i++)
            {
                T item = _factory();
                _totalCreated++;
                _pool.Push(item);
            }
        }

        /// <summary>
        /// Retrieves an object from the pool. Creates a new one if empty and capacity permits.
        /// </summary>
        public T Spawn()
        {
            T item;
            if (_pool.Count > 0)
            {
                item = _pool.Pop();
            }
            else
            {
                if (_totalCreated >= _maxCapacity)
                {
                    throw new InvalidOperationException($"ObjectPool<{typeof(T).Name}> reached maximum capacity ({_maxCapacity}).");
                }
                item = _factory();
                _totalCreated++;
            }

            item.OnSpawn();
            return item;
        }

        /// <summary>
        /// Returns an object back to the pool.
        /// </summary>
        public void Despawn(T item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));

            if (!item.IsActive)
            {
                // Already despawned or not managed
                return;
            }

            item.OnDespawn();
            _pool.Push(item);
        }

        /// <summary>
        /// Clears all objects in the pool.
        /// </summary>
        public void Clear()
        {
            _pool.Clear();
            _totalCreated = 0;
        }
    }
}
