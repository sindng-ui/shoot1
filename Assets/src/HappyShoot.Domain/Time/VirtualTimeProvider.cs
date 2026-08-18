using System;

namespace HappyShoot.Domain.Time
{
    /// <summary>
    /// Controllable time provider for TDD deterministic unit tests.
    /// </summary>
    public class VirtualTimeProvider : ITimeProvider
    {
        private float _time;
        private float _deltaTime;
        private float _timeScale = 1.0f;

        public float Time => _time;
        public float DeltaTime => _deltaTime;

        public float TimeScale
        {
            get => _timeScale;
            set
            {
                if (value < 0f)
                    throw new ArgumentOutOfRangeException(nameof(value), "TimeScale cannot be negative.");
                _timeScale = value;
            }
        }

        public VirtualTimeProvider(float initialTime = 0f)
        {
            _time = initialTime;
            _deltaTime = 0f;
            _timeScale = 1.0f;
        }

        /// <summary>
        /// Advances the virtual clock by the given delta time (scaled by TimeScale).
        /// </summary>
        public void Tick(float deltaSeconds)
        {
            if (deltaSeconds < 0f)
                throw new ArgumentOutOfRangeException(nameof(deltaSeconds), "Delta time cannot be negative.");

            _deltaTime = deltaSeconds * _timeScale;
            _time += _deltaTime;
        }

        /// <summary>
        /// Resets the virtual clock.
        /// </summary>
        public void Reset(float targetTime = 0f)
        {
            _time = targetTime;
            _deltaTime = 0f;
        }
    }
}
