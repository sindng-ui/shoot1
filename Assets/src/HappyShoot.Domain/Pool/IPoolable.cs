namespace HappyShoot.Domain.Pool
{
    /// <summary>
    /// Contract for objects managed by ObjectPool.
    /// </summary>
    public interface IPoolable
    {
        /// <summary>
        /// Gets whether this object is currently active and in use outside the pool.
        /// </summary>
        bool IsActive { get; }

        /// <summary>
        /// Called when the object is retrieved from the pool.
        /// </summary>
        void OnSpawn();

        /// <summary>
        /// Called when the object is returned to the pool.
        /// </summary>
        void OnDespawn();
    }
}
