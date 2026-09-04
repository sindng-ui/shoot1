using System;
using UnityEngine;
using HappyShoot.Domain.Session;
using HappyShoot.Domain.Events;
using HappyShoot.View.Player;
using HappyShoot.View.Utils;

namespace HappyShoot.View.SideScroll
{
    /// <summary>
    /// Interactive floating gold coin in side-scrolling dimension mode.
    /// Pops out on monster defeat with a juicy bounce, gets magnetically drawn to player,
    /// and grants gold without any level-up popup interruption!
    /// Strictly modular and under 500 lines.
    /// </summary>
    public class SideScrollGoldCoinView : MonoBehaviour
    {
        private PlayerView _playerView;
        private GameSessionEntity _gameSession;
        private int _goldAmount = 10;

        private float _bounceVelocityY;
        private float _bounceY;
        private float _baseY;
        private bool _isMagnetized;
        private float _magnetSpeed = 6.0f;
        private float _lifetime = 18.0f;

        private SpriteRenderer _sr;

        public void Initialize(PlayerView playerView, GameSessionEntity gameSession, int goldAmount, Vector3 spawnPos)
        {
            _playerView = playerView;
            _gameSession = gameSession;
            _goldAmount = Mathf.Max(5, goldAmount);

            transform.position = spawnPos;
            _baseY = spawnPos.y;
            _bounceY = 0f;
            _bounceVelocityY = UnityEngine.Random.Range(3.5f, 5.5f);

            _sr = gameObject.AddComponent<SpriteRenderer>();
            var coinSprite = CustomResourceSpriteLoader.TryGetGoldCoinSprite();
            if (coinSprite != null)
            {
                _sr.sprite = coinSprite;
                transform.localScale = Vector3.one * 0.22f;
            }
            else
            {
                _sr.sprite = SpriteHelper.GetOrCreateWhiteSprite();
                _sr.color = new Color(1.0f, 0.85f, 0.2f, 1.0f);
                transform.localScale = Vector3.one * 0.4f;
            }

            _sr.sortingOrder = 13;
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            _lifetime -= dt;
            if (_lifetime <= 0f)
            {
                Destroy(gameObject);
                return;
            }

            if (_playerView == null) return;

            Vector3 playerPos = _playerView.transform.position;
            float dist = Vector2.Distance(transform.position, playerPos);

            // 1. Initial juicy pop & bounce
            if (!_isMagnetized)
            {
                if (_bounceVelocityY != 0f || _bounceY > 0f)
                {
                    _bounceVelocityY += -18f * dt;
                    _bounceY += _bounceVelocityY * dt;

                    if (_bounceY <= 0f)
                    {
                        _bounceY = 0f;
                        _bounceVelocityY = 0f;
                    }
                    transform.position = new Vector3(transform.position.x, _baseY + _bounceY, 0f);
                }

                // Magnet trigger radius: 2.8m (comfortably snaps to player)
                if (dist <= 2.8f)
                {
                    _isMagnetized = true;
                }
            }
            else
            {
                // 2. Magnetic fly-in towards player
                _magnetSpeed += 24f * dt;
                transform.position = Vector3.MoveTowards(transform.position, playerPos, _magnetSpeed * dt);

                if (dist <= 0.6f)
                {
                    Collect();
                }
            }
        }

        private void Collect()
        {
            if (_gameSession != null)
            {
                _gameSession.AddGold(_goldAmount);
            }

            _playerView?.EventBus?.Publish(new PlaySoundEvent(SoundEffectType.GemCollect, 0.9f));
            Destroy(gameObject);
        }
    }
}
