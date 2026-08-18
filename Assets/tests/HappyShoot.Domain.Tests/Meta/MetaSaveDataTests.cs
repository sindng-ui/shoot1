using NUnit.Framework;
using HappyShoot.Domain.Entities;
using HappyShoot.Domain.Meta;

namespace HappyShoot.Domain.Tests.Meta
{
    [TestFixture]
    public class MetaSaveDataTests
    {
        [Test]
        public void ApplyUpgrades_AugmentsBaseStatsCorrectly()
        {
            var baseStats = CharacterStats.Default; // HP: 100, Armor: 0, Speed: 5.0, Damage: 1.0, ExtraProj: 0

            var saveData = new MetaUpgradeSaveData();
            saveData.SetLevel(MetaUpgradeApplier.UpgradeHealth, 3); // +30 HP
            saveData.SetLevel(MetaUpgradeApplier.UpgradeArmor, 2);  // +4 Armor
            saveData.SetLevel(MetaUpgradeApplier.UpgradeExtraProjectile, 1); // +1 Proj

            var augmentedStats = MetaUpgradeApplier.ApplyUpgrades(baseStats, saveData);

            Assert.That(augmentedStats.MaxHealth, Is.EqualTo(130f));
            Assert.That(augmentedStats.Armor, Is.EqualTo(4f));
            Assert.That(augmentedStats.ExtraProjectiles, Is.EqualTo(1));
        }
    }
}
