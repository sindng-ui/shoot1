using System.Collections.Generic;
using NUnit.Framework;
using HappyShoot.Domain.Entities;
using HappyShoot.Domain.Events;
using HappyShoot.Domain.Skills;
using HappyShoot.Domain.Skills.Effects;
using HappyShoot.Domain.Spatial;

namespace HappyShoot.Domain.Tests.Skills
{
    [TestFixture]
    public class GreatswordSlashTests
    {
        private SpatialGrid2D<ISpatialEntity> _grid;
        private EventBus _eventBus;
        private SkillContext _context;
        private GreatswordSlashEffect _slashEffect;

        [SetUp]
        public void SetUp()
        {
            _grid = new SpatialGrid2D<ISpatialEntity>(cellSize: 2.0f);
            _eventBus = new EventBus();
            _context = new SkillContext
            {
                CasterId = 1,
                CasterPosition = new Vector2D(0f, 0f),
                BaseDamage = 10f, // 10 / 10 = 1.0x multiplier
                AreaMultiplier = 1.0f,
                TargetGrid = _grid,
                EventBus = _eventBus
            };
            _slashEffect = new GreatswordSlashEffect(baseDamage: 35f, radius: 2.5f, arcAngleDegrees: 150f);
        }

        private MonsterEntity CreateMonster(int id, Vector2D position, float maxHealth = 100f)
        {
            var monster = new MonsterEntity();
            monster.Initialize(
                id: id,
                typeName: "Slime",
                maxHealth: maxHealth,
                moveSpeed: 2.0f,
                contactDamage: 10f,
                expValue: 1,
                goldValue: 1,
                startPosition: position,
                eventBus: _eventBus
            );
            return monster;
        }

        [Test]
        public void Slash_DealsDamageToEnemies_WithinFrontArcTrajectory()
        {
            // Front enemy at (2, 0)
            var frontMonster = CreateMonster(101, new Vector2D(2f, 0f), maxHealth: 100f);
            // Front-diagonal enemy at 45 degrees within range: (1.4f, 1.4f) (dist = 1.98f < 2.5f)
            var diagonalMonster = CreateMonster(102, new Vector2D(1.4f, 1.4f), maxHealth: 100f);

            _grid.Register(frontMonster);
            _grid.Register(diagonalMonster);

            // Aim target directly at front (3, 0) -> Forward = (1, 0)
            var targets = new List<Vector2D> { new Vector2D(3f, 0f) };
            _slashEffect.ApplyEffect(_context, targets);

            // Both front monsters within 150-degree arc should take 35 damage
            Assert.That(frontMonster.CurrentHealth, Is.EqualTo(65f));
            Assert.That(diagonalMonster.CurrentHealth, Is.EqualTo(65f));
        }

        [Test]
        public void Slash_DoesNotDamageEnemies_BehindPlayerOrOppositeSide()
        {
            // Front target to set forward direction = (1, 0)
            var frontMonster = CreateMonster(101, new Vector2D(2f, 0f), maxHealth: 100f);
            // Behind enemy at 180 degrees (-2f, 0f)
            var behindMonster = CreateMonster(102, new Vector2D(-2f, 0f), maxHealth: 100f);
            // Rear-diagonal enemy at 135 degrees (-1.4f, 1.4f)
            var rearDiagMonster = CreateMonster(103, new Vector2D(-1.4f, 1.4f), maxHealth: 100f);

            _grid.Register(frontMonster);
            _grid.Register(behindMonster);
            _grid.Register(rearDiagMonster);

            var targets = new List<Vector2D> { new Vector2D(2f, 0f) };
            _slashEffect.ApplyEffect(_context, targets);

            // Front monster takes damage
            Assert.That(frontMonster.CurrentHealth, Is.EqualTo(65f));
            // Enemies behind player should take NO damage
            Assert.That(behindMonster.CurrentHealth, Is.EqualTo(100f));
            Assert.That(rearDiagMonster.CurrentHealth, Is.EqualTo(100f));
        }

        [Test]
        public void Slash_DoesNotDamageEnemies_OutOfRadius()
        {
            // Enemy in front (1, 0) direction but far away at (5, 0) (radius is 2.5f)
            var farMonster = CreateMonster(104, new Vector2D(5f, 0f), maxHealth: 100f);
            _grid.Register(farMonster);

            var targets = new List<Vector2D> { new Vector2D(5f, 0f) };
            _slashEffect.ApplyEffect(_context, targets);

            Assert.That(farMonster.CurrentHealth, Is.EqualTo(100f));
        }

        [Test]
        public void Slash_Publishes_PlayerSlashExecutedEvent_And_PlaySoundEvent()
        {
            PlayerSlashExecutedEvent? receivedSlashEvent = null;
            PlaySoundEvent? receivedSoundEvent = null;

            _eventBus.Subscribe<PlayerSlashExecutedEvent>(evt => receivedSlashEvent = evt);
            _eventBus.Subscribe<PlaySoundEvent>(evt => receivedSoundEvent = evt);

            // Aiming at (0, 3) (angle = 90 degrees)
            var targets = new List<Vector2D> { new Vector2D(0f, 3f) };
            _slashEffect.ApplyEffect(_context, targets);

            Assert.That(receivedSlashEvent.HasValue, Is.True);
            Assert.That(receivedSlashEvent.Value.PlayerId, Is.EqualTo(1));
            Assert.That(receivedSlashEvent.Value.DirectionAngleDegrees, Is.EqualTo(90f).Within(0.01f));
            Assert.That(receivedSlashEvent.Value.Radius, Is.EqualTo(2.5f));
            Assert.That(receivedSlashEvent.Value.ArcAngleDegrees, Is.EqualTo(150f));

            Assert.That(receivedSoundEvent.HasValue, Is.True);
            Assert.That(receivedSoundEvent.Value.SoundType, Is.EqualTo(SoundEffectType.SlashAttack));
        }

        [Test]
        public void Slash_WithWideArc_Over180Degrees_DamagesEnemiesInExtendedRange()
        {
            // Set 270 degree arc angle (+-135 degrees from forward (1, 0))
            var wideSlash = new GreatswordSlashEffect(baseDamage: 50f, radius: 3.0f, arcAngleDegrees: 270f);

            var frontMonster = CreateMonster(201, new Vector2D(2f, 0f), maxHealth: 100f); // 0 deg -> HIT
            var sideMonster = CreateMonster(202, new Vector2D(0f, 2f), maxHealth: 100f); // 90 deg -> HIT
            var rearDiagMonster = CreateMonster(203, new Vector2D(-1.5f, 1.5f), maxHealth: 100f); // 135 deg -> HIT
            var directBehindMonster = CreateMonster(204, new Vector2D(-2.5f, 0f), maxHealth: 100f); // 180 deg -> NOT hit (outside 270 deg)

            _grid.Register(frontMonster);
            _grid.Register(sideMonster);
            _grid.Register(rearDiagMonster);
            _grid.Register(directBehindMonster);

            var targets = new List<Vector2D> { new Vector2D(2f, 0f) };
            wideSlash.ApplyEffect(_context, targets);

            // 0, 90, 135 degree monsters must be damaged
            Assert.That(frontMonster.CurrentHealth, Is.EqualTo(50f));
            Assert.That(sideMonster.CurrentHealth, Is.EqualTo(50f));
            Assert.That(rearDiagMonster.CurrentHealth, Is.EqualTo(50f));
            // 180 degree monster remains untouched
            Assert.That(directBehindMonster.CurrentHealth, Is.EqualTo(100f));
        }
    }
}
