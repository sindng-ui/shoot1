using HappyShoot.Domain.Events;
using HappyShoot.Domain.Skills;
using HappyShoot.Domain.Skills.Targeters;
using HappyShoot.Domain.Skills.Triggers;
using HappyShoot.Domain.Spatial;

namespace HappyShoot.Domain.Entities
{
    public enum CharacterClassType
    {
        Warrior,
        Ranger,
        Wizard
    }

    /// <summary>
    /// Factory that produces pre-configured PlayerEntity instances with unique stats and starting skills.
    /// </summary>
    public static class PlayerClassFactory
    {
        public static PlayerEntity CreatePlayer(
            int id,
            CharacterClassType classType,
            Vector2D startPosition,
            EventBus eventBus = null)
        {
            CharacterStats stats;
            ISkill startingSkill;

            switch (classType)
            {
                case CharacterClassType.Warrior:
                    // Warrior: +25% Max HP (125), +15 Armor, Melee Slash
                    stats = new CharacterStats(
                        maxHealth: 125f,
                        healthRegen: 0.5f,
                        moveSpeed: 4.8f,
                        attackPowerMultiplier: 1.1f,
                        armor: 15f,
                        critChance: 0.05f,
                        critDamageMultiplier: 1.5f,
                        cooldownReduction: 0f,
                        areaMultiplier: 1.15f,
                        projectileSpeedMultiplier: 1.0f,
                        extraProjectiles: 0,
                        pickupRadius: 2.0f
                    );
                    startingSkill = new CompositeSkill(
                        "slash", "Greatsword Slash",
                        new CooldownTrigger(1.2f),
                        new ClosestEnemyTargeter(),
                        new Skills.Effects.GreatswordSlashEffect(baseDamage: 35f, radius: 2.5f),
                        range: 2.8f
                    );
                    break;

                case CharacterClassType.Ranger:
                    // Ranger: +20% MoveSpeed (6.0), +10% CritChance (15%), Piercing Bow
                    stats = new CharacterStats(
                        maxHealth: 90f,
                        healthRegen: 0f,
                        moveSpeed: 6.0f,
                        attackPowerMultiplier: 1.0f,
                        armor: 0f,
                        critChance: 0.15f,
                        critDamageMultiplier: 1.75f,
                        cooldownReduction: 0f,
                        areaMultiplier: 1.0f,
                        projectileSpeedMultiplier: 1.3f,
                        extraProjectiles: 0,
                        pickupRadius: 2.5f
                    );
                    startingSkill = new CompositeSkill(
                        "bow", "Piercing Bow",
                        new CooldownTrigger(0.8f),
                        new ClosestEnemyTargeter(),
                        new Skills.Effects.PiercingArrowEffect(baseDamage: 22f, speed: 16f, pierceCount: 999),
                        range: 12.0f
                    );
                    break;

                case CharacterClassType.Wizard:
                default:
                    // Wizard: -15% Cooldown (CDR 0.15), +20% Area (1.2), Arcane Explosion
                    stats = new CharacterStats(
                        maxHealth: 80f,
                        healthRegen: 0f,
                        moveSpeed: 5.0f,
                        attackPowerMultiplier: 1.25f,
                        armor: 0f,
                        critChance: 0.05f,
                        critDamageMultiplier: 1.5f,
                        cooldownReduction: 0.15f,
                        areaMultiplier: 1.2f,
                        projectileSpeedMultiplier: 1.0f,
                        extraProjectiles: 0,
                        pickupRadius: 3.0f
                    );
                    startingSkill = new CompositeSkill(
                        "ground_stomp", "지면 강타",
                        new CooldownTrigger(1.4f),
                        new ClosestEnemyTargeter(),
                        new Skills.Effects.GroundStompEffect(baseDamage: 25f, stompRadius: 1.5f),
                        range: 1.8f
                    );
                    break;
            }

            var player = new PlayerEntity(id, stats, startPosition, eventBus);
            player.ClassType = classType;
            player.AddSkill(startingSkill);
            return player;
        }
    }
}
