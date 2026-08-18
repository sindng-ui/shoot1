using System;
using System.Collections.Generic;

namespace HappyShoot.Domain.Spatial
{
    /// <summary>
    /// Lightweight 2D vector value type completely independent of UnityEngine.
    /// </summary>
    public readonly struct Vector2D : IEquatable<Vector2D>
    {
        public readonly float X;
        public readonly float Y;

        public static readonly Vector2D Zero = new Vector2D(0f, 0f);
        public static readonly Vector2D One = new Vector2D(1f, 1f);
        public static readonly Vector2D Up = new Vector2D(0f, 1f);
        public static readonly Vector2D Down = new Vector2D(0f, -1f);
        public static readonly Vector2D Left = new Vector2D(-1f, 0f);
        public static readonly Vector2D Right = new Vector2D(1f, 0f);

        public Vector2D(float x, float y)
        {
            X = x;
            Y = y;
        }

        public float SqrMagnitude => X * X + Y * Y;
        public float Magnitude => (float)Math.Sqrt(SqrMagnitude);

        public Vector2D Normalized
        {
            get
            {
                float mag = Magnitude;
                if (mag > 1e-5f)
                    return new Vector2D(X / mag, Y / mag);
                return Zero;
            }
        }

        public static float Distance(Vector2D a, Vector2D b)
        {
            float dx = a.X - b.X;
            float dy = a.Y - b.Y;
            return (float)Math.Sqrt(dx * dx + dy * dy);
        }

        public static float SqrDistance(Vector2D a, Vector2D b)
        {
            float dx = a.X - b.X;
            float dy = a.Y - b.Y;
            return dx * dx + dy * dy;
        }

        public static Vector2D operator +(Vector2D a, Vector2D b) => new Vector2D(a.X + b.X, a.Y + b.Y);
        public static Vector2D operator -(Vector2D a, Vector2D b) => new Vector2D(a.X - b.X, a.Y - b.Y);
        public static Vector2D operator *(Vector2D a, float d) => new Vector2D(a.X * d, a.Y * d);
        public static Vector2D operator /(Vector2D a, float d) => new Vector2D(a.X / d, a.Y / d);

        public bool Equals(Vector2D other) => Math.Abs(X - other.X) < 1e-5f && Math.Abs(Y - other.Y) < 1e-5f;
        public override bool Equals(object obj) => obj is Vector2D other && Equals(other);
        public override int GetHashCode()
        {
            unchecked
            {
                return (X.GetHashCode() * 397) ^ Y.GetHashCode();
            }
        }
        public override string ToString() => $"({X:F2}, {Y:F2})";
    }

    /// <summary>
    /// Contract for entities registered in the 2D spatial grid.
    /// </summary>
    public interface ISpatialEntity
    {
        int Id { get; }
        Vector2D Position { get; }
        float Radius { get; }
        bool IsActive { get; }
    }

    /// <summary>
    /// Non-generic spatial grid contract for proximity querying and hit detection.
    /// </summary>
    public interface ISpatialGrid2D
    {
        int QueryRadiusNonAlloc(Vector2D center, float radius, IList<ISpatialEntity> resultsBuffer);
        bool TryGetClosest(Vector2D center, float maxRadius, out ISpatialEntity closest);
    }
}
