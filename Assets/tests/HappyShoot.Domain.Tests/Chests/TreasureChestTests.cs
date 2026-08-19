using System.Collections.Generic;
using NUnit.Framework;
using HappyShoot.Domain.Chests;
using HappyShoot.Domain.Entities;
using HappyShoot.Domain.Events;
using HappyShoot.Domain.Leveling;
using HappyShoot.Domain.Skills;
using HappyShoot.Domain.Skills.Effects;
using HappyShoot.Domain.Skills.Targeters;
using HappyShoot.Domain.Skills.Triggers;
using HappyShoot.Domain.Spatial;

namespace HappyShoot.Domain.Tests.Chests
{
    [TestFixture]
    public class TreasureChestTests
    {
        private EventBus _eventBus;
        private TreasureChestManager _chestManager;
        private PlayerEntity _player;
        private SkillRewardManager _rewardManager;

        [SetUp]
        public void SetUp()
        {
            _eventBus = new EventBus();
            _chestManager = new TreasureChestManager(_eventBus);

            _player = new PlayerEntity(1, CharacterStats.Default, Vector2D.Zero, _eventBus);

            _rewardManager = new SkillRewardManager();
            _rewardManager.RegisterSkill("slash", "Slash", "Slash effect",
                () => new CompositeSkill("slash", "Slash", new CooldownTrigger(1f), new ClosestEnemyTargeter(), new GreatswordSlashEffect(30f, 2f)));
            _rewardManager.RegisterSkill("bow", "Bow", "Bow effect",
                () => new CompositeSkill("bow", "Bow", new CooldownTrigger(1f), new ClosestEnemyTargeter(), new PiercingArrowEffect(20f, 10f, 2)));
        }

        [Test]
        public void SpawnChest_CreatesActiveChestAndPublishesEvent()
        {
            TreasureChestSpawnedEvent spawned = default;
            bool fired = false;
            _eventBus.Subscribe<TreasureChestSpawnedEvent>(evt => { spawned = evt; fired = true; });

            var chest = _chestManager.SpawnChest(new Vector2D(5f, 5f), bonusGold: 150);

            Assert.That(_chestManager.ActiveCount, Is.EqualTo(1));
            Assert.That(chest.IsActive, Is.True);
            Assert.That(chest.Position.X, Is.EqualTo(5f));
            Assert.That(fired, Is.True);
            Assert.That(spawned.Position.X, Is.EqualTo(5f));
        }

        [Test]
        public void Update_WhenPlayerApproaches_OpensChestAndAppliesRewards()
        {
            TreasureChestOpenedEvent opened = default;
            bool openedFired = false;
            _eventBus.Subscribe<TreasureChestOpenedEvent>(evt => { opened = evt; openedFired = true; });

            // Spawn chest right near player at (0.2, 0)
            _chestManager.SpawnChest(new Vector2D(0.2f, 0f), bonusGold: 100);

            _chestManager.Update(_player, _rewardManager, pickupRadius: 0.8f);

            Assert.That(_chestManager.ActiveCount, Is.EqualTo(0)); // Despawned after opening
            Assert.That(openedFired, Is.True);
            Assert.That(opened.BonusGold, Is.EqualTo(100));
            Assert.That(_player.Skills.Count, Is.GreaterThan(0)); // Rewards equipped to player
        }

        [Test]
        public void BossDiedEvent_AutomaticallySpawnsTreasureChest()
        {
            Assert.That(_chestManager.ActiveCount, Is.EqualTo(0));

            _eventBus.Publish(new BossDiedEvent(999, "Necromancer", new Vector2D(12f, -8f), 300));

            Assert.That(_chestManager.ActiveCount, Is.EqualTo(1));
            Assert.That(_chestManager.ActiveChests[0].Position.X, Is.EqualTo(12f));
            Assert.That(_chestManager.ActiveChests[0].Position.Y, Is.EqualTo(-8f));
            Assert.That(_chestManager.ActiveChests[0].BonusGold, Is.EqualTo(300));
        }
    }
}
