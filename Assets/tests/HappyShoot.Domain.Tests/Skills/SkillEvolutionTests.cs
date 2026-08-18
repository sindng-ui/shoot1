using System.Linq;
using NUnit.Framework;
using HappyShoot.Domain.Entities;
using HappyShoot.Domain.Events;
using HappyShoot.Domain.Skills;
using HappyShoot.Domain.Skills.Effects;
using HappyShoot.Domain.Skills.Evolution;
using HappyShoot.Domain.Skills.Targeters;
using HappyShoot.Domain.Skills.Triggers;
using HappyShoot.Domain.Spatial;

namespace HappyShoot.Domain.Tests.Skills
{
    [TestFixture]
    public class SkillEvolutionTests
    {
        private EventBus _eventBus;
        private SkillEvolutionManager _evolutionManager;
        private PlayerEntity _player;

        [SetUp]
        public void SetUp()
        {
            _eventBus = new EventBus();
            _evolutionManager = new SkillEvolutionManager(_eventBus);

            // Create warrior with default "slash" skill (maxLevel 5)
            _player = PlayerClassFactory.CreatePlayer(1, CharacterClassType.Warrior, Vector2D.Zero, _eventBus);

            // Register Blood Eater recipe (Slash Lv 5 + "blood_chalice" passive)
            _evolutionManager.RegisterRecipe(new SkillEvolutionRecipe(
                baseSkillId: "slash",
                requiredPassiveId: "blood_chalice",
                evolvedSkillId: "blood_eater",
                evolvedSkillName: "Blood Eater",
                evolvedSkillFactory: () => new CompositeSkill("blood_eater", "Blood Eater", new CooldownTrigger(0.9f), new ClosestEnemyTargeter(), new BloodEaterEffect())
            ));
        }

        [Test]
        public void GetAvailableEvolutions_ReturnsEmpty_WhenSkillNotMaxLevel()
        {
            _player.AddPassive("blood_chalice"); // Has passive, but Slash is Lv 1

            var available = _evolutionManager.GetAvailableEvolutions(_player);
            Assert.That(available.Count, Is.EqualTo(0));
        }

        [Test]
        public void GetAvailableEvolutions_ReturnsEmpty_WhenPassiveMissing()
        {
            var slash = _player.Skills.First(s => s.Id == "slash");
            while (!slash.IsMaxLevel) slash.LevelUp(); // Slash Lv 5, but no passive

            var available = _evolutionManager.GetAvailableEvolutions(_player);
            Assert.That(available.Count, Is.EqualTo(0));
        }

        [Test]
        public void EvolveSkill_ReplacesBaseSkillWithEvolvedSkill_WhenConditionsMet()
        {
            var slash = _player.Skills.First(s => s.Id == "slash");
            while (!slash.IsMaxLevel) slash.LevelUp();
            _player.AddPassive("blood_chalice");

            var available = _evolutionManager.GetAvailableEvolutions(_player);
            Assert.That(available.Count, Is.EqualTo(1));

            bool evolvedEventFired = false;
            _eventBus.Subscribe<SkillEvolvedEvent>(evt => evolvedEventFired = true);

            bool success = _evolutionManager.EvolveSkill(_player, available[0]);

            Assert.That(success, Is.True);
            Assert.That(evolvedEventFired, Is.True);
            Assert.That(_player.Skills.Any(s => s.Id == "slash"), Is.False);
            Assert.That(_player.Skills.Any(s => s.Id == "blood_eater"), Is.True);
        }
    }
}
