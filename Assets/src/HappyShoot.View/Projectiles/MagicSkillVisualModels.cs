using UnityEngine;

namespace HappyShoot.View.Projectiles
{
    public class FrostNovaInstance
    {
        public GameObject GameObject;
        public Transform Transform;
        public SpriteRenderer Renderer;
        public float TargetScale;
        public float Timer;
        public float Duration;
        public bool IsActive;
    }

    public class LightningSegment
    {
        public GameObject GameObject;
        public Transform Transform;
        public SpriteRenderer Renderer;
        public Color BaseColor;
        public float BaseWidth;
        public float Timer;
        public float Duration;
        public bool IsActive;
    }

    public class IceShardInstance
    {
        public GameObject GameObject;
        public Transform Transform;
        public SpriteRenderer Renderer;
        public Vector2 Velocity;
        public float RotSpeed;
        public float Timer;
        public float Lifetime;
        public bool IsActive;
    }

    public class ElectricSparkInstance
    {
        public GameObject GameObject;
        public Transform Transform;
        public SpriteRenderer Renderer;
        public Vector2 Velocity;
        public float Timer;
        public float Lifetime;
        public bool IsActive;
    }
}
