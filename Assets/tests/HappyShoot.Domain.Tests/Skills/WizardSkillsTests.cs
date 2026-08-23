using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using HappyShoot.Domain.Entities;
using HappyShoot.Domain.Events;
using HappyShoot.Domain.Leveling;
using HappyShoot.Domain.Skills;
using HappyShoot.Domain.Skills.Effects;
using HappyShoot.Domain.Skills.Targeters;
using HappyShoot.Domain.Skills.Triggers;
using HappyShoot.Domain.Spatial;

namespace HappyShoot.Domain.Tests.Skills
{
    [TestFixture]
    public class WizardSkillsTests
    {
        private SpatialGrid2D<ISpatialEntity> _grid;
        private EventBus _eventBus;
        private SkillContext _context;

        [SetUp]
        public void SetUp()
        {
            _grid = new SpatialGrid2D<ISpatialEntity>(cellSize: 2.0f);
            _eventBus = new EventBus();
            _context = new SkillContext
            {
                CasterId = 1,
                CasterPosition = new Vector2D(0f, 0f),
                BaseDamage = 10f, // 1.0x
                AreaMultiplier = 1.0f,
                SpeedMultiplier = 1.0f,
                TargetGrid = _grid,
                EventBus = _eventBus
            };
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
        public void Fireball_ExplodesAndDealsSplashDamage_ToEnemiesInRadius()
        {
            var fireball = new FireballEffect(baseDamage: 35f, radius: 2.0f);

            bool launchedEventReceived = false;
            _eventBus.Subscribe<FireballLaunchedEvent>(e =>
            {
                launchedEventReceived = true;
                Assert.That(e.TargetPosition.X, Is.EqualTo(5f));
                Assert.That(e.Radius, Is.EqualTo(2.0f));
                Assert.That(e.Damage, Is.EqualTo(35f));
            });

            fireball.ApplyEffect(_context, new List<Vector2D> { new Vector2D(5f, 0f) });

            Assert.That(launchedEventReceived, Is.True);
        }

        [Test]
        public void FrostNova_Deals360DegreeDamage_ToSurroundingEnemies()
        {
            var frostNova = new FrostNovaEffect(baseDamage: 28f, radius: 3.0f);

            var mNorth = CreateMonster(101, new Vector2D(0f, 2f));
            var mSouth = CreateMonster(102, new Vector2D(0f, -2f));
            var mEast = CreateMonster(103, new Vector2D(2f, 0f));
            var mWest = CreateMonster(104, new Vector2D(-2f, 0f));
            var mFar = CreateMonster(105, new Vector2D(5f, 5f));

            _grid.Register(mNorth);
            _grid.Register(mSouth);
            _grid.Register(mEast);
            _grid.Register(mWest);
            _grid.Register(mFar);

            bool eventReceived = false;
            _eventBus.Subscribe<FrostNovaExecutedEvent>(e =>
            {
                eventReceived = true;
                Assert.That(e.Radius, Is.EqualTo(3.0f));
            });

            frostNova.ApplyEffect(_context, null);

            Assert.That(eventReceived, Is.True);
            Assert.That(mNorth.CurrentHealth, Is.EqualTo(72f));
            Assert.That(mSouth.CurrentHealth, Is.EqualTo(72f));
            Assert.That(mEast.CurrentHealth, Is.EqualTo(72f));
            Assert.That(mWest.CurrentHealth, Is.EqualTo(72f));
            Assert.That(mFar.CurrentHealth, Is.EqualTo(100f));
        }

        [Test]
        public void ChainLightning_ChainsAcrossMultipleNearbyEnemies()
        {
            var chainLightning = new ChainLightningEffect(baseDamage: 30f, chainCount: 3, jumpRadius: 3.0f);

            var m1 = CreateMonster(101, new Vector2D(2f, 0f)); // 1st
            var m2 = CreateMonster(102, new Vector2D(4f, 0f)); // 2nd (within 2m of m1)
            var m3 = CreateMonster(103, new Vector2D(6f, 0f)); // 3rd (within 2m of m2)
            var m4 = CreateMonster(104, new Vector2D(8f, 0f)); // 4th (exceeds chainCount 3)

            _grid.Register(m1);
            _grid.Register(m2);
            _grid.Register(m3);
            _grid.Register(m4);

            int struckTargetsCount = 0;
            _eventBus.Subscribe<ChainLightningExecutedEvent>(e =>
            {
                struckTargetsCount = e.TargetPositions.Count;
            });

            chainLightning.ApplyEffect(_context, new List<Vector2D> { m1.Position });

            Assert.That(struckTargetsCount, Is.EqualTo(3));
            Assert.That(m1.CurrentHealth, Is.EqualTo(70f));
            Assert.That(m2.CurrentHealth, Is.EqualTo(70f));
            Assert.That(m3.CurrentHealth, Is.EqualTo(70f));
            Assert.That(m4.CurrentHealth, Is.EqualTo(100f));
        }

        [Test]
        public void Wizard_Factory_InitializesWithCorrectStats_AndFireballStartingSkill()
        {
            var player = PlayerClassFactory.CreatePlayer(1, CharacterClassType.Wizard, Vector2D.Zero, _eventBus);

            Assert.That(player.ClassType, Is.EqualTo(CharacterClassType.Wizard));
            Assert.That(player.Stats.MaxHealth, Is.EqualTo(85f));
            Assert.That(player.Stats.CooldownReduction, Is.EqualTo(0.15f));
            Assert.That(player.Stats.AreaMultiplier, Is.EqualTo(1.2f));
            Assert.That(player.Stats.AttackPowerMultiplier, Is.EqualTo(1.25f));

            Assert.That(player.Skills.Count, Is.EqualTo(1));
            Assert.That(player.Skills[0].Id, Is.EqualTo("fireball"));
        }

        [Test]
        public void SkillRewardManager_RollsWizardSkills_ExcludesWarriorAndRangerSkills_IncludesOrbital()
        {
            var rewardManager = new SkillRewardManager();

            // Register skills
            rewardManager.RegisterSkill("slash", "대검", "desc", () => null, new[] { CharacterClassType.Warrior });
            rewardManager.RegisterSkill("whirlwind", "휠윈드", "desc", () => null, new[] { CharacterClassType.Warrior });
            rewardManager.RegisterSkill("bow", "활", "desc", () => null, new[] { CharacterClassType.Ranger });
            rewardManager.RegisterSkill("multishot", "멀티샷", "desc", () => null, new[] { CharacterClassType.Ranger });
            rewardManager.RegisterSkill("fireball", "화염구", "desc", () => null, new[] { CharacterClassType.Wizard });
            rewardManager.RegisterSkill("frost_nova", "서리 폭발", "desc", () => null, new[] { CharacterClassType.Wizard });
            rewardManager.RegisterSkill("chain_lightning", "연쇄 번개", "desc", () => null, new[] { CharacterClassType.Wizard });
            rewardManager.RegisterSkill("orbital", "오비탈 블레이드", "desc", () => null); // Shared

            var wizardPlayer = PlayerClassFactory.CreatePlayer(1, CharacterClassType.Wizard, Vector2D.Zero);

            var rewards = rewardManager.RollRewards(wizardPlayer, count: 10);
            var skillIds = rewards.Select(r => r.Id).ToList();

            // Warrior and Ranger skills must NOT be in the rolled options
            Assert.That(skillIds.Contains("slash"), Is.False);
            Assert.That(skillIds.Contains("whirlwind"), Is.False);
            Assert.That(skillIds.Contains("bow"), Is.False);
            Assert.That(skillIds.Contains("multishot"), Is.False);

            // Wizard skills and Orbital should be rollable
            Assert.That(skillIds.Contains("frost_nova") || skillIds.Contains("chain_lightning") || skillIds.Contains("orbital"), Is.True);
        }

        [Test]
        public void GigastormLightning_ChainsAndAppliesGuaranteedShock()
        {
            var giga = new GigastormLightningEffect(baseDamage: 65f, chainCount: 5, chainRange: 6.0f, sparkRadius: 1.8f);
            var m1 = CreateMonster(201, new Vector2D(2f, 0f));
            var m2 = CreateMonster(202, new Vector2D(4f, 0f));
            var m3 = CreateMonster(203, new Vector2D(6f, 0f));

            _grid.Register(m1);
            _grid.Register(m2);
            _grid.Register(m3);

            bool eventReceived = false;
            _eventBus.Subscribe<GigastormLightningExecutedEvent>(e =>
            {
                eventReceived = true;
                Assert.That(e.TargetPositions.Count, Is.EqualTo(3));
            });

            giga.ApplyEffect(_context, new List<Vector2D> { m1.Position });

            Assert.That(eventReceived, Is.True);
            Assert.That(m1.CurrentHealth, Is.LessThan(m1.MaxHealth));
            Assert.That(m2.CurrentHealth, Is.LessThan(m2.MaxHealth));
            Assert.That(m3.CurrentHealth, Is.LessThan(m3.MaxHealth));
            Assert.That(m1.IsShocked, Is.True);
            Assert.That(m2.IsShocked, Is.True);
            Assert.That(m3.IsShocked, Is.True);
        }

        [Test]
        public void BlizzardNova_FreezesAndDamagesSurroundingEnemies()
        {
            var blizzard = new BlizzardNovaEffect(baseDamage: 70f, radius: 5.2f, shardCount: 8);
            var m1 = CreateMonster(301, new Vector2D(3f, 0f));
            var m2 = CreateMonster(302, new Vector2D(-3f, 0f));
            var mFar = CreateMonster(303, new Vector2D(10f, 0f));

            _grid.Register(m1);
            _grid.Register(m2);
            _grid.Register(mFar);

            bool eventReceived = false;
            _eventBus.Subscribe<BlizzardNovaExecutedEvent>(e =>
            {
                eventReceived = true;
                Assert.That(e.Radius, Is.EqualTo(5.2f));
                Assert.That(e.ShardCount, Is.EqualTo(8));
            });

            blizzard.ApplyEffect(_context, null);

            Assert.That(eventReceived, Is.True);
            Assert.That(m1.CurrentHealth, Is.LessThan(m1.MaxHealth));
            Assert.That(m2.CurrentHealth, Is.LessThan(m2.MaxHealth));
            Assert.That(mFar.CurrentHealth, Is.EqualTo(mFar.MaxHealth));
            Assert.That(m1.IsChilled, Is.True);
            Assert.That(m2.IsChilled, Is.True);
        }
    }
}
