namespace HappyShoot.Domain.Time
{
    /// <summary>
    /// Abstracts time operations to enable deterministic unit testing without UnityEngine.Time.
    /// </summary>
    public interface ITimeProvider
    {
        /// <summary>
        /// Gets the elapsed time in seconds since the game started.
        /// </summary>
        float Time { get; }

        /// <summary>
        /// Gets the completion time in seconds since the last frame/tick.
        /// </summary>
        float DeltaTime { get; }

        /// <summary>
        /// Gets the time scale factor.
        /// </summary>
        float TimeScale { get; set; }
    }
}
