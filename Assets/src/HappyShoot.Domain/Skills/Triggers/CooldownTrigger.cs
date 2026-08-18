using System;

namespace HappyShoot.Domain.Skills.Triggers
{
    /// <summary>
    /// Trigger component that fires at a fixed interval (cooldown).
    /// </summary>
    public class CooldownTrigger : ISkillTrigger
    {
        private float _cooldown;
        private float _timer;

        public float Cooldown
        {
            get => _cooldown;
            set
            {
                if (value <= 0f) throw new ArgumentOutOfRangeException(nameof(value), "Cooldown must be positive.");
                _cooldown = value;
            }
        }

        public float CurrentTimer => _timer;
        public float NormalizedProgress => Math.Max(0f, Math.Min(1f, _timer / _cooldown));

        public CooldownTrigger(float cooldown)
        {
            Cooldown = cooldown;
            _timer = cooldown; // Ready immediately on start
        }

        public bool CanTrigger(float deltaTime)
        {
            _timer += deltaTime;
            return _timer >= _cooldown;
        }

        public void OnTriggered()
        {
            _timer = 0f;
        }

        public void Reset()
        {
            _timer = 0f;
        }
    }
}
