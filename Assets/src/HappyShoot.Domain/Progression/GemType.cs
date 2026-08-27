namespace HappyShoot.Domain.Progression
{
    /// <summary>
    /// Three gem types that fund class-specific skill trees.
    /// </summary>
    public enum GemType
    {
        Ruby = 0,      // Red  - Warrior tree
        Emerald = 1,   // Green - Ranger tree
        Amethyst = 2   // Purple - Wizard tree
    }

    /// <summary>
    /// Elemental branch type within each skill tree.
    /// Each tree has Fire / Ice / Lightning branches; only one can be "awakened" at a time.
    /// </summary>
    public enum BranchType
    {
        None = 0,
        Fire = 1,
        Ice = 2,
        Lightning = 3
    }

    /// <summary>
    /// Zero-allocation helper extensions for GemType and BranchType.
    /// </summary>
    public static class GemTypeExtensions
    {
        public static Entities.CharacterClassType ToClassType(this GemType gem)
        {
            switch (gem)
            {
                case GemType.Ruby: return Entities.CharacterClassType.Warrior;
                case GemType.Emerald: return Entities.CharacterClassType.Ranger;
                case GemType.Amethyst: return Entities.CharacterClassType.Wizard;
                default: return Entities.CharacterClassType.Warrior;
            }
        }

        public static GemType FromClassType(Entities.CharacterClassType classType)
        {
            switch (classType)
            {
                case Entities.CharacterClassType.Warrior: return GemType.Ruby;
                case Entities.CharacterClassType.Ranger: return GemType.Emerald;
                case Entities.CharacterClassType.Wizard: return GemType.Amethyst;
                default: return GemType.Ruby;
            }
        }

        public static string GetDisplayName(this GemType gem)
        {
            switch (gem)
            {
                case GemType.Ruby: return "루비";
                case GemType.Emerald: return "에메랄드";
                case GemType.Amethyst: return "아메시스트";
                default: return "???";
            }
        }

        public static string GetBranchDisplayName(this BranchType branch)
        {
            switch (branch)
            {
                case BranchType.Fire: return "화염";
                case BranchType.Ice: return "빙결";
                case BranchType.Lightning: return "전기";
                default: return "없음";
            }
        }
    }
}
