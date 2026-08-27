using UnityEngine;

namespace HappyShoot.View.Background
{
    /// <summary>
    /// Lightweight zero-allocation ambient floating dust motes / magical ember particles.
    /// Wraps around the camera viewport to provide atmospheric depth and living battleground immersion.
    /// </summary>
    public class BackgroundAmbientDustView : MonoBehaviour
    {
        private const int ParticleCount = 28;
        private const float ViewportPaddingX = 20.0f; // Covers wide screens
        private const float ViewportPaddingY = 12.0f;

        private struct DustParticle
        {
            public Transform Transform;
            public SpriteRenderer Renderer;
            public Vector2 Velocity;
            public float BaseAlpha;
            public float PulseSpeed;
            public float PulseOffset;
        }

        private DustParticle[] _particles;
        private Transform _cameraTransform;

        public void Initialize(Transform cameraTransform)
        {
            _cameraTransform = cameraTransform;
            _particles = new DustParticle[ParticleCount];

            var dustSprite = BackgroundSpriteHelper.GetOrCreateDustMoteSprite();
            Vector3 camPos = cameraTransform != null ? cameraTransform.position : Vector3.zero;

            for (int i = 0; i < ParticleCount; i++)
            {
                var pGo = new GameObject($"DustMote_{i:D2}");
                pGo.transform.SetParent(transform);

                var sr = pGo.AddComponent<SpriteRenderer>();
                sr.sprite = dustSprite;
                sr.sortingOrder = -50; // In front of tiles (-100), behind shadows (-10)

                float randX = Random.Range(-ViewportPaddingX, ViewportPaddingX);
                float randY = Random.Range(-ViewportPaddingY, ViewportPaddingY);
                pGo.transform.position = new Vector3(camPos.x + randX, camPos.y + randY, 0f);

                float scale = Random.Range(0.6f, 1.4f);
                pGo.transform.localScale = Vector3.one * scale;

                // Subtle drifting velocity (mostly upwards and drifting sideways)
                float vx = Random.Range(-0.35f, 0.35f);
                float vy = Random.Range(0.15f, 0.55f);

                _particles[i] = new DustParticle
                {
                    Transform = pGo.transform,
                    Renderer = sr,
                    Velocity = new Vector2(vx, vy),
                    BaseAlpha = Random.Range(0.15f, 0.35f),
                    PulseSpeed = Random.Range(1.0f, 2.5f),
                    PulseOffset = Random.Range(0f, Mathf.PI * 2f)
                };
            }
        }

        private void LateUpdate()
        {
            if (_particles == null || _cameraTransform == null) return;

            Vector3 camPos = _cameraTransform.position;
            float dt = Time.deltaTime;
            float time = Time.time;

            for (int i = 0; i < _particles.Length; i++)
            {
                ref var p = ref _particles[i];
                if (p.Transform == null) continue;

                Vector3 pos = p.Transform.position;

                // Gentle drift
                pos.x += p.Velocity.x * dt;
                pos.y += p.Velocity.y * dt;

                // Relative offset from camera
                float diffX = pos.x - camPos.x;
                float diffY = pos.y - camPos.y;

                // Horizontal wrap
                if (diffX > ViewportPaddingX)
                {
                    pos.x -= ViewportPaddingX * 2.0f;
                }
                else if (diffX < -ViewportPaddingX)
                {
                    pos.x += ViewportPaddingX * 2.0f;
                }

                // Vertical wrap
                if (diffY > ViewportPaddingY)
                {
                    pos.y -= ViewportPaddingY * 2.0f;
                }
                else if (diffY < -ViewportPaddingY)
                {
                    pos.y += ViewportPaddingY * 2.0f;
                }

                p.Transform.position = pos;

                // Subtle alpha breathing
                if (p.Renderer != null)
                {
                    float alphaMod = Mathf.Sin(time * p.PulseSpeed + p.PulseOffset) * 0.08f;
                    float finalAlpha = Mathf.Clamp01(p.BaseAlpha + alphaMod);
                    Color c = p.Renderer.color;
                    c.a = finalAlpha;
                    p.Renderer.color = c;
                }
            }
        }
    }
}
