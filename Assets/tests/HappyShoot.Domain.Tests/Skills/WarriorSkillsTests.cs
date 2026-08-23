using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using HappyShoot.Domain.Entities;
using HappyShoot.Domain.Events;
using HappyShoot.Domain.Skills;
using HappyShoot.Domain.Skills.Effects;
using HappyShoot.Domain.Spatial;

namespace HappyShoot.Domain.Tests.Skills
{
    [TestFixture]
    public class WarriorSkillsTests
    {
        private EventBus _eventBus;
        private SpatialGrid2D<ISpatialEntity> _grid;
        private PlayerEntity _player;

        [SetUp]
        public void SetUp()
        {
            _eventBus = new EventBus();
            _grid = new SpatialGrid2D<ISpatialEntity>(cellSize: 2f);
            _player = PlayerClassFactory.CreatePlayer(1, CharacterClassType.Warrior, Vector2D.Zero, _eventBus);
        }

        private MonsterEntity CreateSlime(int id, Vector2D pos)
        {
            var m = new MonsterEntity();
            m.Initialize(
                id: id,
                typeName: "Slime",
                maxHealth: 100f,
                moveSpeed: 2.0f,
                contactDamage: 10f,
                expValue: 1,
                goldValue: 1,
                startPosition: pos,
                eventBus: _eventBus
            );
            return m;
        }

        [Test]
        public void GroundStompEffect_DealsDamageAndPublishesGroundStompExecutedEvent()
        {
            var stomp = new GroundStompEffect(baseDamage: 40f, stompRadius: 2.5f);
            var monsterNear = CreateSlime(10, new Vector2D(1.5f, 0f));
            var monsterFar = CreateSlime(11, new Vector2D(5.0f, 0f));

            _grid.Register(monsterNear);
            _grid.Register(monsterFar);

            bool eventFired = false;
            float publishedRadius = 0f;
            _eventBus.Subscribe<GroundStompExecutedEvent>(evt =>
            {
                eventFired = true;
                publishedRadius = evt.Radius;
            });

            var context = new SkillContext
            {
                CasterId = _player.Id,
                CasterPosition = _player.Position,
                TargetGrid = _grid,
                EventBus = _eventBus,
                BaseDamage = 10f,
                AreaMultiplier = 1.0f,
                SpeedMultiplier = 1.0f
            };

            stomp.ApplyEffect(context, new List<Vector2D>());

            Assert.That(eventFired, Is.True);
            Assert.That(publishedRadius, Is.EqualTo(2.5f));
            Assert.That(monsterNear.CurrentHealth, Is.LessThan(monsterNear.MaxHealth));
            Assert.That(monsterFar.CurrentHealth, Is.EqualTo(monsterFar.MaxHealth));
        }

        [Test]
        public void WhirlwindEffect_Deals360DegreeDamageAndPublishesWhirlwindExecutedEvent()
        {
            var whirlwind = new WhirlwindEffect(baseDamage: 35f, radius: 2.2f);
            var m1 = CreateSlime(10, new Vector2D(1.2f, 0f)); // East
            var m2 = CreateSlime(11, new Vector2D(0f, 1.2f)); // North
            var m3 = CreateSlime(12, new Vector2D(-1.2f, 0f)); // West
            var m4 = CreateSlime(13, new Vector2D(0f, -1.2f)); // South
            var mFar = CreateSlime(14, new Vector2D(4.0f, 0f)); // Out of range

            _grid.Register(m1);
            _grid.Register(m2);
            _grid.Register(m3);
            _grid.Register(m4);
            _grid.Register(mFar);

            bool eventFired = false;
            float publishedRadius = 0f;
            _eventBus.Subscribe<WhirlwindExecutedEvent>(evt =>
            {
                eventFired = true;
                publishedRadius = evt.Radius;
            });

            var context = new SkillContext
            {
                CasterId = _player.Id,
                CasterPosition = _player.Position,
                TargetGrid = _grid,
                EventBus = _eventBus,
                BaseDamage = 10f,
                AreaMultiplier = 1.0f,
                SpeedMultiplier = 1.0f
            };

            whirlwind.ApplyEffect(context, new List<Vector2D>());

            Assert.That(eventFired, Is.True);
            Assert.That(publishedRadius, Is.EqualTo(2.2f));
            Assert.That(m1.CurrentHealth, Is.LessThan(m1.MaxHealth));
            Assert.That(m2.CurrentHealth, Is.LessThan(m2.MaxHealth));
            Assert.That(m3.CurrentHealth, Is.LessThan(m3.MaxHealth));
            Assert.That(m4.CurrentHealth, Is.LessThan(m4.MaxHealth));
            Assert.That(mFar.CurrentHealth, Is.EqualTo(mFar.MaxHealth));
        }

        [Test]
        public void WhirlwindEffect_LevelUp_IncreasesDamageAndRadius()
        {
            var whirlwind = new WhirlwindEffect(baseDamage: 30f, radius: 2.0f);
            Assert.That(whirlwind.BaseDamage, Is.EqualTo(30f));
            Assert.That(whirlwind.Radius, Is.EqualTo(2.0f));

            whirlwind.OnLevelUp(3); // Level 3 (+2 level steps)
            Assert.That(whirlwind.BaseDamage, Is.EqualTo(50f)); // 30 + 10 * 2
            Assert.That(whirlwind.Radius, Is.EqualTo(2.9f)); // 2.0 + 0.45 * 2
        }

        [Test]
        public void BloodEaterEffect_DealsForwardSectorDamageAndLifeSteals()
        {
            var bloodEater = new BloodEaterEffect(baseDamage: 85f, radius: 4.8f, lifeStealPerHit: 3.0f, arcAngleDegrees: 150f);
            var mFront = CreateSlime(20, new Vector2D(2.0f, 0f)); // In front arc
            var mSide = CreateSlime(21, new Vector2D(1.5f, 1.5f)); // Within 150 deg arc
            var mBehind = CreateSlime(22, new Vector2D(-2.0f, 0f)); // Behind player (not hit)
            var mFar = CreateSlime(23, new Vector2D(8.0f, 0f)); // Out of range

            _grid.Register(mFront);
            _grid.Register(mSide);
            _grid.Register(mBehind);
            _grid.Register(mFar);

            _player.TakeDamage(50f); // Injure player
            float hpBeforeHeal = _player.CurrentHealth;

            bool eventFired = false;
            float publishedHeal = 0f;
            int hitCount = 0;
            _eventBus.Subscribe<BloodEaterExecutedEvent>(evt =>
            {
                eventFired = true;
                publishedHeal = evt.HealedAmount;
                hitCount = evt.HitPositions.Count;
            });

            var context = new SkillContext
            {
                CasterId = _player.Id,
                CasterPosition = _player.Position,
                CasterEntity = _player,
                TargetGrid = _grid,
                EventBus = _eventBus,
                BaseDamage = 10f,
                AreaMultiplier = 1.0f,
                SpeedMultiplier = 1.0f
            };

            bloodEater.ApplyEffect(context, new List<Vector2D> { new Vector2D(10f, 0f) }); // Aim Right

            Assert.That(eventFired, Is.True);
            Assert.That(hitCount, Is.EqualTo(2)); // mFront & mSide
            Assert.That(publishedHeal, Is.EqualTo(6.0f)); // 2 hits * 3.0
            Assert.That(mFront.CurrentHealth, Is.LessThan(mFront.MaxHealth));
            Assert.That(mSide.CurrentHealth, Is.LessThan(mSide.MaxHealth));
            Assert.That(mBehind.CurrentHealth, Is.EqualTo(mBehind.MaxHealth));
            Assert.That(_player.CurrentHealth, Is.EqualTo(hpBeforeHeal + 6.0f)); // Player life-stealed
        }

        [Test]
        public void TempestWhirlwindEffect_Deals360DamageAndPublishesTempestWhirlwindExecutedEvent()
        {
            var tempest = new TempestWhirlwindEffect(baseDamage: 75f, radius: 4.2f, slashWaveCount: 4);
            var m1 = CreateSlime(20, new Vector2D(3.0f, 0f));
            var m2 = CreateSlime(21, new Vector2D(-3.0f, 0f));
            var mFar = CreateSlime(22, new Vector2D(8.0f, 0f));

            _grid.Register(m1);
            _grid.Register(m2);
            _grid.Register(mFar);

            bool eventFired = false;
            float publishedRadius = 0f;
            _eventBus.Subscribe<TempestWhirlwindExecutedEvent>(evt =>
            {
                eventFired = true;
                publishedRadius = evt.Radius;
            });

            var context = new SkillContext
            {
                CasterId = _player.Id,
                CasterPosition = _player.Position,
                TargetGrid = _grid,
                EventBus = _eventBus,
                BaseDamage = 10f,
                AreaMultiplier = 1.0f,
                SpeedMultiplier = 1.0f
            };

            tempest.ApplyEffect(context, new List<Vector2D>());

            Assert.That(eventFired, Is.True);
            Assert.That(publishedRadius, Is.EqualTo(4.2f));
            Assert.That(m1.CurrentHealth, Is.LessThan(m1.MaxHealth));
            Assert.That(m2.CurrentHealth, Is.LessThan(m2.MaxHealth));
            Assert.That(mFar.CurrentHealth, Is.EqualTo(mFar.MaxHealth));
        }

        [Test]
        public void EarthshakerEffect_DealsDamageAndPublishesEarthshakerExecutedEvent()
        {
            var earthshaker = new EarthshakerEffect(baseDamage: 80f, radius: 4.8f, fissureCount: 4);
            var m1 = CreateSlime(30, new Vector2D(3.5f, 0f));
            var mFar = CreateSlime(31, new Vector2D(9.0f, 0f));

            _grid.Register(m1);
            _grid.Register(mFar);

            bool eventFired = false;
            float publishedRadius = 0f;
            _eventBus.Subscribe<EarthshakerExecutedEvent>(evt =>
            {
                eventFired = true;
                publishedRadius = evt.Radius;
            });

            var context = new SkillContext
            {
                CasterId = _player.Id,
                CasterPosition = _player.Position,
                TargetGrid = _grid,
                EventBus = _eventBus,
                BaseDamage = 10f,
                AreaMultiplier = 1.0f,
                SpeedMultiplier = 1.0f
            };

            earthshaker.ApplyEffect(context, new List<Vector2D>());

            Assert.That(eventFired, Is.True);
            Assert.That(publishedRadius, Is.EqualTo(4.8f));
            Assert.That(m1.CurrentHealth, Is.LessThan(m1.MaxHealth));
            Assert.That(mFar.CurrentHealth, Is.EqualTo(mFar.MaxHealth));
        }
    }
}
