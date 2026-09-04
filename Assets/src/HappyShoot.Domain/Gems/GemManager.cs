using System;
using System.Collections.Generic;
using HappyShoot.Domain.Events;
using HappyShoot.Domain.Pool;
using HappyShoot.Domain.Progression;
using HappyShoot.Domain.Skills;
using HappyShoot.Domain.Spatial;

namespace HappyShoot.Domain.Gems
{
    /// <summary>
    /// Manages spawning, magnet collection, and high-capacity pooling for both
    /// Experience Gems and Skill Tree Progression Gem Stones (Ruby, Emerald, Amethyst).
    /// Scaled to support high counts with zero GC allocation.
    /// Strictly modular and under 500 lines.
    /// </summary>
    public class GemManager
    {
        private readonly ObjectPool<ExpGemEntity> _pool;
        private readonly List<ExpGemEntity> _activeGems;

        private readonly ObjectPool<GemStoneEntity> _gemStonePool;
        private readonly List<GemStoneEntity> _activeGemStones;

        private readonly EventBus _eventBus;
        private readonly Random _random;
        private int _idCounter = 10000;
        private int _stoneIdCounter = 50000;

        public ExpConfig Config { get; set; }

        public IReadOnlyList<ExpGemEntity> ActiveGems => _activeGems;
        public int ActiveCount => _activeGems.Count;

        public IReadOnlyList<GemStoneEntity> ActiveGemStones => _activeGemStones;
        public int ActiveGemStoneCount => _activeGemStones.Count;

        public event Action<int> OnExpCollected;
        public event Action<ExpGemEntity> OnGemSpawned;

        public event Action<GemType> OnGemStoneCollected;
        public event Action<GemStoneEntity> OnGemStoneSpawned;

        public GemManager(EventBus eventBus = null, int initialCapacity = 1500, ExpConfig config = null, int? seed = null)
        {
            _eventBus = eventBus;
            Config = config;
            _random = seed.HasValue ? new Random(seed.Value) : new Random();

            _activeGems = new List<ExpGemEntity>(initialCapacity);
            _pool = new ObjectPool<ExpGemEntity>(() => new ExpGemEntity(), initialCapacity: initialCapacity);

            _activeGemStones = new List<GemStoneEntity>(64);
            _gemStonePool = new ObjectPool<GemStoneEntity>(() => new GemStoneEntity(), initialCapacity: 64);

            _eventBus?.Subscribe<MonsterDiedEvent>(OnMonsterDied);
        }

        public bool IsSideScrollMode { get; set; }

        private void OnMonsterDied(MonsterDiedEvent evt)
        {
            if (IsSideScrollMode)
            {
                // In side-scrolling dimension mode: all exp gems are suppressed so player is never interrupted by level-up popups!
                return;
            }

            if (evt.ExpValue > 0)
            {
                SpawnGem(evt.Position, evt.ExpValue);
            }

            // Progression Gem Drop Logic:
            // 1) Bosses guarantee 5 gems of mixed random types
            if (evt.IsBoss)
            {
                for (int i = 0; i < 5; i++)
                {
                    GemType randomType = (GemType)_random.Next(0, 3);
                    double angle = _random.NextDouble() * Math.PI * 2;
                    double dist = 0.3 + _random.NextDouble() * 0.7;
                    Vector2D offset = new Vector2D((float)(Math.Cos(angle) * dist), (float)(Math.Sin(angle) * dist));
                    SpawnGemStone(evt.Position + offset, randomType);
                }
            }
            // 2) Normal monsters have 1% chance to drop 1 random gem
            else if (_random.NextDouble() < 0.01)
            {
                GemType randomType = (GemType)_random.Next(0, 3);
                SpawnGemStone(evt.Position, randomType);
            }
        }

        /// <summary>
        /// Spawns an experience gem at the given position.
        /// </summary>
        public ExpGemEntity SpawnGem(Vector2D position, int expValue = 1)
        {
            var gem = _pool.Spawn();
            gem.Initialize(++_idCounter, position, expValue);
            _activeGems.Add(gem);
            OnGemSpawned?.Invoke(gem);
            return gem;
        }

        /// <summary>
        /// Spawns a permanent progression gem stone at the given position.
        /// </summary>
        public GemStoneEntity SpawnGemStone(Vector2D position, GemType gemType)
        {
            var stone = _gemStonePool.Spawn();
            stone.Initialize(++_stoneIdCounter, gemType, position);
            _activeGemStones.Add(stone);
            OnGemStoneSpawned?.Invoke(stone);
            _eventBus?.Publish(new GemStoneDroppedEvent(gemType, position, stone.Id));
            return stone;
        }

        /// <summary>
        /// Updates magnet attraction for all active gems and gem stones and collects completed ones.
        /// </summary>
        public void Update(Vector2D playerPosition, float pickupRadius, float deltaTime)
        {
            float mult = Config != null ? Config.GemExpMultiplier : 1.0f;

            // 1. Update Exp Gems
            for (int i = _activeGems.Count - 1; i >= 0; i--)
            {
                var gem = _activeGems[i];
                bool collected = gem.Update(playerPosition, pickupRadius, deltaTime);

                if (collected)
                {
                    int effectiveExp = Math.Max(1, (int)Math.Round(gem.ExpValue * mult));
                    OnExpCollected?.Invoke(effectiveExp);
                    _activeGems.RemoveAt(i);
                    _pool.Despawn(gem);
                }
                else if (!gem.IsActive)
                {
                    _activeGems.RemoveAt(i);
                    _pool.Despawn(gem);
                }
            }

            // 2. Update Progression Gem Stones
            for (int i = _activeGemStones.Count - 1; i >= 0; i--)
            {
                var stone = _activeGemStones[i];
                bool collected = stone.Update(playerPosition, pickupRadius, deltaTime);

                if (collected)
                {
                    OnGemStoneCollected?.Invoke(stone.GemType);
                    _eventBus?.Publish(new GemStoneCollectedEvent(stone.GemType));
                    _activeGemStones.RemoveAt(i);
                    _gemStonePool.Despawn(stone);
                }
                else if (!stone.IsActive)
                {
                    _activeGemStones.RemoveAt(i);
                    _gemStonePool.Despawn(stone);
                }
            }
        }

        /// <summary>
        /// Despawns all active gems and gem stones.
        /// </summary>
        public void Clear()
        {
            for (int i = _activeGems.Count - 1; i >= 0; i--)
            {
                _pool.Despawn(_activeGems[i]);
            }
            _activeGems.Clear();

            for (int i = _activeGemStones.Count - 1; i >= 0; i--)
            {
                _gemStonePool.Despawn(_activeGemStones[i]);
            }
            _activeGemStones.Clear();
        }
    }
}
