using HappyShoot.Domain.Progression;

namespace HappyShoot.Domain.Forge
{
    /// <summary>
    /// Registers all 12 rune definitions into a RuneManager.
    /// 4 Common + 4 Rare + 4 Legendary.
    /// Costs calibrated for ~6 gems/color per completed run income.
    /// </summary>
    public static class RuneRegistry
    {
        public static void RegisterAll(RuneManager mgr)
        {
            RegisterCommonRunes(mgr);
            RegisterRareRunes(mgr);
            RegisterLegendaryRunes(mgr);
        }

        // ═══════════════════════════════════════════
        //  🟢 COMMON RUNES (4) — Unlock: 3 gems
        // ═══════════════════════════════════════════

        private static void RegisterCommonRunes(RuneManager mgr)
        {
            // 1. Rapid Rune: faster cooldown, slightly less damage
            mgr.RegisterRune(new RuneDefinition(
                "rune_rapid", "연사의 룬",
                "스킬 시전 속도를 높여 빈번한 공격을 가능하게 합니다. 대미지가 소폭 감소합니다.",
                RuneGrade.Common,
                unlockRuby: 3, unlockEmerald: 0, unlockAmethyst: 0,
                baseDmg: 0.90f, baseCd: 0.85f, baseArea: 1.0f,
                dmgPerLv: 0.01f, cdPerLv: -0.01f, areaPerLv: 0f,
                primaryGem: GemType.Ruby));

            // 2. Power Rune: more damage, slightly slower
            mgr.RegisterRune(new RuneDefinition(
                "rune_power", "위력의 룬",
                "스킬의 파괴력을 극대화합니다. 시전 속도가 소폭 느려집니다.",
                RuneGrade.Common,
                unlockRuby: 0, unlockEmerald: 3, unlockAmethyst: 0,
                baseDmg: 1.20f, baseCd: 1.10f, baseArea: 1.0f,
                dmgPerLv: 0.01f, cdPerLv: 0f, areaPerLv: 0f,
                primaryGem: GemType.Emerald));

            // 3. Spread Rune: wider area, slightly less damage
            mgr.RegisterRune(new RuneDefinition(
                "rune_spread", "확산의 룬",
                "스킬의 영향 범위를 크게 확장합니다. 대미지가 소폭 감소합니다.",
                RuneGrade.Common,
                unlockRuby: 0, unlockEmerald: 0, unlockAmethyst: 3,
                baseDmg: 0.95f, baseCd: 1.0f, baseArea: 1.25f,
                dmgPerLv: 0.005f, cdPerLv: 0f, areaPerLv: 0.01f,
                primaryGem: GemType.Amethyst));

            // 4. Focus Rune: concentrated power, smaller area
            mgr.RegisterRune(new RuneDefinition(
                "rune_focus", "집중의 룬",
                "범위를 좁히고 화력을 한 점에 집중합니다. 보스전에 효과적입니다.",
                RuneGrade.Common,
                unlockRuby: 2, unlockEmerald: 2, unlockAmethyst: 0,
                baseDmg: 1.30f, baseCd: 1.0f, baseArea: 0.80f,
                dmgPerLv: 0.01f, cdPerLv: 0f, areaPerLv: 0f,
                primaryGem: GemType.Ruby));
        }

        // ═══════════════════════════════════════════
        //  🔵 RARE RUNES (4) — Unlock: 10+5 gems
        // ═══════════════════════════════════════════

        private static void RegisterRareRunes(RuneManager mgr)
        {
            // 5. Split Rune: extra projectiles, reduced individual damage
            mgr.RegisterRune(new RuneDefinition(
                "rune_split", "분열의 룬",
                "투사체가 2갈래로 분열됩니다. 개별 대미지가 감소하지만 넓은 범위를 커버합니다.",
                RuneGrade.Rare,
                unlockRuby: 0, unlockEmerald: 10, unlockAmethyst: 5,
                baseDmg: 0.75f, baseCd: 1.0f, baseArea: 1.0f,
                dmgPerLv: 0.01f, cdPerLv: 0f, areaPerLv: 0f,
                primaryGem: GemType.Emerald,
                extraProj: 2));

            // 6. Pierce Rune: extra piercing, slight damage reduction
            mgr.RegisterRune(new RuneDefinition(
                "rune_pierce", "관통의 룬",
                "투사체의 관통력이 증가합니다. 일직선 상의 다수 적을 관통합니다.",
                RuneGrade.Rare,
                unlockRuby: 0, unlockEmerald: 5, unlockAmethyst: 10,
                baseDmg: 0.90f, baseCd: 1.0f, baseArea: 1.0f,
                dmgPerLv: 0.01f, cdPerLv: 0f, areaPerLv: 0f,
                primaryGem: GemType.Amethyst,
                extraPierce: 2));

            // 7. Leech Rune: life steal on hit, slower cooldown
            mgr.RegisterRune(new RuneDefinition(
                "rune_leech", "흡수의 룬",
                "스킬 적중 시 대미지의 일부를 생명력으로 흡수합니다.",
                RuneGrade.Rare,
                unlockRuby: 10, unlockEmerald: 0, unlockAmethyst: 5,
                baseDmg: 1.0f, baseCd: 1.15f, baseArea: 1.0f,
                dmgPerLv: 0f, cdPerLv: -0.005f, areaPerLv: 0f,
                primaryGem: GemType.Ruby,
                lifeSteal: 0.01f));

            // 8. Chain Rune: chance to hit additional target
            mgr.RegisterRune(new RuneDefinition(
                "rune_chain", "연쇄의 룬",
                "적중 시 일정 확률로 인접한 적 1체에 추가 타격을 가합니다.",
                RuneGrade.Rare,
                unlockRuby: 5, unlockEmerald: 0, unlockAmethyst: 10,
                baseDmg: 0.95f, baseCd: 1.0f, baseArea: 1.0f,
                dmgPerLv: 0.005f, cdPerLv: 0f, areaPerLv: 0f,
                primaryGem: GemType.Amethyst,
                chainChance: 0.25f));
        }

        // ═══════════════════════════════════════════
        //  🟣 LEGENDARY RUNES (4) — Unlock: 15+8+8 gems
        // ═══════════════════════════════════════════

        private static void RegisterLegendaryRunes(RuneManager mgr)
        {
            // 9. Detonate Rune: death explosion
            mgr.RegisterRune(new RuneDefinition(
                "rune_detonate", "폭발의 룬",
                "스킬로 적을 처치하면 해당 위치에서 대미지의 50%로 범위 폭발이 발생합니다.",
                RuneGrade.Legendary,
                unlockRuby: 15, unlockEmerald: 8, unlockAmethyst: 8,
                baseDmg: 1.0f, baseCd: 1.0f, baseArea: 1.0f,
                dmgPerLv: 0.005f, cdPerLv: 0f, areaPerLv: 0f,
                primaryGem: GemType.Ruby,
                deathExplosion: 0.50f));

            // 10. Tempo Rune: free cast every N uses
            mgr.RegisterRune(new RuneDefinition(
                "rune_tempo", "시간의 룬",
                "매 5번째 시전 시 즉시 1회 추가 시전이 발동됩니다 (무쿨다운).",
                RuneGrade.Legendary,
                unlockRuby: 8, unlockEmerald: 8, unlockAmethyst: 15,
                baseDmg: 1.0f, baseCd: 1.0f, baseArea: 1.0f,
                dmgPerLv: 0.005f, cdPerLv: 0f, areaPerLv: 0f,
                primaryGem: GemType.Amethyst,
                freecastN: 5));

            // 11. Resonance Rune: multiplied effect when 2+ same rune equipped
            mgr.RegisterRune(new RuneDefinition(
                "rune_resonance", "공명의 룬",
                "같은 룬이 2개 이상 장착되면 모든 공명 룬의 효과가 1.5배로 증폭됩니다.",
                RuneGrade.Legendary,
                unlockRuby: 8, unlockEmerald: 15, unlockAmethyst: 8,
                baseDmg: 1.0f, baseCd: 1.0f, baseArea: 1.0f,
                dmgPerLv: 0.005f, cdPerLv: -0.005f, areaPerLv: 0.005f,
                primaryGem: GemType.Emerald,
                resonance: 1.5f));

            // 12. Chaos Rune: all stats randomized each cast
            mgr.RegisterRune(new RuneDefinition(
                "rune_chaos", "혼돈의 룬",
                "시전마다 모든 수치가 랜덤으로 변동합니다. 운이 좋으면 대박, 나쁘면...",
                RuneGrade.Legendary,
                unlockRuby: 10, unlockEmerald: 10, unlockAmethyst: 10,
                baseDmg: 1.0f, baseCd: 1.0f, baseArea: 1.0f,
                dmgPerLv: 0.005f, cdPerLv: 0f, areaPerLv: 0.005f,
                primaryGem: GemType.Ruby,
                chaosMin: 0.90f, chaosMax: 1.40f));
        }
    }
}
