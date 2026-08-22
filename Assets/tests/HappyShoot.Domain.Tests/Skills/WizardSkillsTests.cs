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

            var m1 = CreateMonster(101, new Vector2D(5f, 0f)); // Direct hit position
            var m2 = CreateMonster(102, new Vector2D(6f, 0.5f)); // Within 2.0m radius of (5, 0)
            var mFar = CreateMonster(103, new Vector2D(9f, 0f)); // Outside radius

            _grid.Register(m1);
            _grid.Register(m2);
            _grid.Register(mFar);

            bool eventReceived = false;
            _eventBus.Subscribe<FireballExplodedEvent>(e =>
            {
                eventReceived = true;
                Assert.That(e.CenterPosition.X, Is.EqualTo(5f));
            });

            fireball.ApplyEffect(_context, new List<Vector2D> { new Vector2D(5f, 0f) });

            Assert.That(eventReceived, Is.True);
            Assert.That(m1.CurrentHealth, Is.EqualTo(65f)); // 100 - 35
            Assert.That(m2.CurrentHealth, Is.EqualTo(65f)); // 100 - 35
            Assert.That(mFar.CurrentHealth, Is.EqualTo(100f)); // Unharmed
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
    }
}
