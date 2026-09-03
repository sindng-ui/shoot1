using System;

namespace HappyShoot.Domain.Entities
{
    /// <summary>
    /// Configuration data for player Dash action and stock charges.
    /// Pure C# domain model for cross-platform portability.
    /// </summary>
    [Serializable]
    public class DashConfigData
    {
        public float Cooldown = 5.0f;          // Dash cooldown in seconds (default 5s)
        public float Distance = 4.5f;          // Dash travel distance in meters
        public float Duration = 0.28f;         // Dash duration in seconds
        public float DecelExponent = 2.5f;     // Ease-out deceleration curve exponent
        public float GhostInterval = 0.028f;   // Interval between afterimage ghost trails
        public float InvincibleDuration = 0.25f;// i-frame duration against contact damage
        public int MaxCharges = 1;             // Maximum dash stock charges (1 now, expandable to 2, 3)

        public DashConfigData() { }

        public DashConfigData(float cooldown, float distance, float duration, float decel, float ghostInterval, float iFrame, int maxCharges = 1)
        {
            Cooldown = cooldown;
            Distance = distance;
            Duration = duration;
            DecelExponent = decel;
            GhostInterval = ghostInterval;
            InvincibleDuration = iFrame;
            MaxCharges = maxCharges;
        }
    }
}
