using System.Collections.Generic;
using UnityEngine;
using HappyShoot.Domain.Entities;
using HappyShoot.Domain.Events;
using HappyShoot.View.Player;

namespace HappyShoot.View.Projectiles
{
    /// <summary>
    /// Manages the Boss's 6-directional radial Doom Ray (Laser Beam) attack.
    /// Centered directly on the boss body, tracking boss movement in real-time.
    /// Timeline: 0.5s warning charge -> 3.0s thick blooming laser burst -> fade out.
    /// Zero GC allocations during lifecycle.
    /// </summary>
    public class BossLaserBeamManagerView : MonoBehaviour
    {
        private struct LaserBeam
        {
            public Vector2 Direction;
            public float LifeTimer;   // total beam lifetime seconds
            public float MaxLife;
            public float ChargeTimer; // warn phase remaining
            public bool IsCharging;
        }

        private readonly List<LaserBeam> _beams = new List<LaserBeam>(12);
        private readonly List<GameObject> _beamGos = new List<GameObject>(12);
        private readonly List<SpriteRenderer> _beamSrs = new List<SpriteRenderer>(12);

        private const float BeamLength   = 18f;
        private const float BeamMaxWidth  = 1.6f;
        private const float BeamMinWidth  = 0.05f;
        private const float ChargeTime    = 0.5f;
        private const float BeamDuration  = 3.0f;
        private const float DamagePerSec  = 25f;
        private const float DamageTickInterval = 0.15f;

        private EventBus _eventBus;
        private PlayerView _playerView;
        private float _dmgTick;

        // Boss fire interval
        private const float FireInterval = 8.0f;
        private float _fireTimer;
        private MonsterEntity _boss;

        public void Initialize(EventBus eventBus, PlayerView playerView)
        {
            _eventBus = eventBus;
            _playerView = playerView;
            _fireTimer = FireInterval * 0.4f; // first beam fires sooner
        }

        public void SetActiveBoss(MonsterEntity boss)
        {
            _boss = boss;
        }

        public void ClearBoss()
        {
            _boss = null;
            ClearAllBeams();
        }

        private void Update()
        {
            if (_boss == null || _boss.IsDead || !_boss.IsActive) return;

            // Auto-fire cooldown
            _fireTimer -= Time.deltaTime;
            if (_fireTimer <= 0f)
            {
                _fireTimer = FireInterval;
                FireSixBeams();
            }

            UpdateBeams();
        }

        private void FireSixBeams()
        {
            if (_boss == null) return;

            // 6 radial directions (60 degree spread around boss)
            float baseAngle = Random.Range(0f, 360f);
            for (int i = 0; i < 6; i++)
            {
                float angle = (baseAngle + i * 60f) * Mathf.Deg2Rad;
                Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                SpawnBeam(dir);
            }
        }

        private void SpawnBeam(Vector2 direction)
        {
            var go = new GameObject("BossLaser");
            go.transform.SetParent(transform, false);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = CreateBeamSprite();
            sr.sortingOrder = 20;

            _beamGos.Add(go);
            _beamSrs.Add(sr);
            _beams.Add(new LaserBeam
            {
                Direction   = direction,
                LifeTimer   = ChargeTime + BeamDuration,
                MaxLife     = ChargeTime + BeamDuration,
                ChargeTimer = ChargeTime,
                IsCharging  = true
            });
        }

        private void UpdateBeams()
        {
            float dt = Time.deltaTime;
            _dmgTick -= dt;

            Vector2 bossPos = _boss != null
                ? new Vector2((float)_boss.Position.X, (float)_boss.Position.Y)
                : Vector2.zero;

            for (int i = _beams.Count - 1; i >= 0; i--)
            {
                var b = _beams[i];
                b.LifeTimer -= dt;

                if (b.IsCharging)
                {
                    b.ChargeTimer -= dt;
                    if (b.ChargeTimer <= 0f) b.IsCharging = false;
                }

                if (b.LifeTimer <= 0f || _boss == null || _boss.IsDead)
                {
                    Destroy(_beamGos[i]);
                    _beams.RemoveAt(i);
                    _beamGos.RemoveAt(i);
                    _beamSrs.RemoveAt(i);
                    continue;
                }

                float beamAge = (b.MaxLife - b.LifeTimer) - ChargeTime;
                float t = Mathf.Clamp01(beamAge / BeamDuration);

                // Width curve: grows 0->1 quickly, then shrinks 1->0 near end
                float widthCurve = b.IsCharging
                    ? Mathf.Lerp(0.05f, 0.3f, 1f - b.ChargeTimer / ChargeTime)   // charge pulse
                    : Mathf.Sin(t * Mathf.PI);                                    // bloom then fade
                float width = b.IsCharging ? BeamMaxWidth * 0.25f * widthCurve : BeamMaxWidth * widthCurve;

                // Color: charge=yellow, fire=intense red-orange -> dim red
                Color col;
                if (b.IsCharging)
                    col = new Color(1.0f, 0.9f, 0.2f, 0.6f + widthCurve * 0.4f);
                else
                    col = Color.Lerp(new Color(1.0f, 0.3f, 0.05f, 0.95f), new Color(0.6f, 0.05f, 0.0f, 0.3f), t);

                var sr = _beamSrs[i];
                sr.color = col;

                // Position beam directly at the Boss Center (Sprite Pivot is at (0, 0.5) left-edge)
                var go = _beamGos[i];
                go.transform.position = new Vector3(bossPos.x, bossPos.y, -0.05f);
                float angleZ = Mathf.Atan2(b.Direction.y, b.Direction.x) * Mathf.Rad2Deg;
                go.transform.rotation = Quaternion.Euler(0f, 0f, angleZ);
                go.transform.localScale = new Vector3(BeamLength, Mathf.Max(BeamMinWidth, width), 1f);

                _beams[i] = b;
            }

            // Damage tick
            if (_dmgTick <= 0f && _playerView != null && _eventBus != null)
            {
                _dmgTick = DamageTickInterval;
                CheckPlayerBeamCollision(bossPos);
            }
        }

        private void CheckPlayerBeamCollision(Vector2 bossPos)
        {
            if (_playerView == null || _playerView.Entity == null) return;
            var entity = _playerView.Entity;
            if (entity.IsDead) return;

            Vector2 playerPos = _playerView.transform.position;
            const float playerRadius = 0.45f;

            for (int i = 0; i < _beams.Count; i++)
            {
                if (_beams[i].IsCharging) continue;

                // Point-to-segment distance check from Boss Origin
                Vector2 origin = bossPos;
                Vector2 dir    = _beams[i].Direction;
                float beamWidth = BeamMaxWidth * 0.5f;

                Vector2 toPlayer = playerPos - origin;
                float proj = Vector2.Dot(toPlayer, dir);
                proj = Mathf.Clamp(proj, 0f, BeamLength);
                Vector2 closest = origin + dir * proj;
                float dist = (playerPos - closest).magnitude;

                if (dist < beamWidth + playerRadius)
                {
                    float dmg = DamagePerSec * DamageTickInterval;
                    _eventBus.Publish(new PlayerDamagedEvent(
                        entity.Id, dmg, entity.CurrentHealth, entity.Stats.MaxHealth));
                    break; // one beam hit per tick is enough
                }
            }
        }

        private void ClearAllBeams()
        {
            for (int i = 0; i < _beamGos.Count; i++)
            {
                if (_beamGos[i] != null) Destroy(_beamGos[i]);
            }
            _beams.Clear();
            _beamGos.Clear();
            _beamSrs.Clear();
        }

        private static Sprite _beamSprite;
        private static Sprite CreateBeamSprite()
        {
            if (_beamSprite != null) return _beamSprite;
            int w = 64; int h = 8;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            Color[] px = new Color[w * h];
            for (int y = 0; y < h; y++)
            {
                float fy = (y - h * 0.5f) / (h * 0.5f); // -1 to 1
                float falloff = Mathf.Max(0f, 1f - fy * fy);
                for (int x = 0; x < w; x++)
                {
                    float fx = (float)x / w; // 0-1 along length
                    float xFade = Mathf.Lerp(0.3f, 1f, Mathf.Clamp01(fx * 4f)) * Mathf.Clamp01((1f - fx) * 4f);
                    px[y * w + x] = new Color(1f, 1f, 1f, falloff * xFade);
                }
            }
            tex.SetPixels(px);
            tex.Apply();
            _beamSprite = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0f, 0.5f), h);
            return _beamSprite;
        }
    }
}
