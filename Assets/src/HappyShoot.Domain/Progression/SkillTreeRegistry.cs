namespace HappyShoot.Domain.Progression
{
    /// <summary>
    /// Registers all 54 skill tree nodes (3 classes × 18 nodes each) into a SkillTreeManager.
    /// Warrior = Ruby, Ranger = Emerald, Wizard = Amethyst.
    /// Each tree: 6 core stat nodes + 3 elemental branches × 4 nodes.
    /// </summary>
    public static class SkillTreeRegistry
    {
        public static void RegisterAll(SkillTreeManager mgr)
        {
            // Wizard-Only Mode: Only register the Wizard's 18-node master skill tree
            RegisterWizardTree(mgr);
        }

        // ═══════════════════════════════════════════════════
        //  🗡️ WARRIOR TREE (Ruby)
        // ═══════════════════════════════════════════════════

        private static void RegisterWarriorTree(SkillTreeManager mgr)
        {
            var g = GemType.Ruby;
            var n = BranchType.None;

            // ── Core Stat Nodes (6) ──
            mgr.RegisterNode(new SkillTreeNodeDef("w_hp1", "강인한 육체 I", "최대 체력 +15", g, 3, 1, n, NodeEffectType.MaxHealth, 15f));
            mgr.RegisterNode(new SkillTreeNodeDef("w_hp2", "강인한 육체 II", "최대 체력 +30", g, 3, 1, n, NodeEffectType.MaxHealth, 30f, new[] { "w_hp1" }));
            mgr.RegisterNode(new SkillTreeNodeDef("w_hp3", "강인한 육체 III", "최대 체력 +50", g, 3, 1, n, NodeEffectType.MaxHealth, 50f, new[] { "w_hp2" }));
            mgr.RegisterNode(new SkillTreeNodeDef("w_armor1", "강철 피부 I", "방어력 +5", g, 3, 1, n, NodeEffectType.Armor, 5f));
            mgr.RegisterNode(new SkillTreeNodeDef("w_armor2", "강철 피부 II", "방어력 +10", g, 3, 1, n, NodeEffectType.Armor, 10f, new[] { "w_armor1" }));
            mgr.RegisterNode(new SkillTreeNodeDef("w_atkspd", "전투 숙련", "공격 속도 +10%", g, 3, 1, n, NodeEffectType.AttackSpeed, 0.10f));

            // ── 🔥 Fire Branch: Inferno Berserker (4) ──
            var f = BranchType.Fire;
            mgr.RegisterNode(new SkillTreeNodeDef("w_fire1", "타오르는 검", "대검 베기에 화상 부여 (3초 DoT)", g, 5, 1, f, NodeEffectType.FireBurnOnHit, 3f));
            mgr.RegisterNode(new SkillTreeNodeDef("w_fire2", "연소 폭발", "화상 적 처치 시 주변 소규모 폭발", g, 8, 1, f, NodeEffectType.FireDeathExplosion, 1.5f, new[] { "w_fire1" }));
            mgr.RegisterNode(new SkillTreeNodeDef("w_fire3", "화염 휠윈드", "휠윈드에 불꽃 후광 → 범위 내 적 화상", g, 12, 1, f, NodeEffectType.FireWhirlwindAura, 1f, new[] { "w_fire2" }));
            mgr.RegisterNode(new SkillTreeNodeDef("w_fire4", "용암 지대", "지면 강타 시 용암 장판 생성 (5초)", g, 20, 1, f, NodeEffectType.FireGroundLava, 5f, new[] { "w_fire3" }));

            // ── ❄️ Ice Branch: Frost Knight (4) ──
            var i = BranchType.Ice;
            mgr.RegisterNode(new SkillTreeNodeDef("w_ice1", "서리 검", "대검 베기에 빙결 부여 (이속 40% 감소)", g, 5, 1, i, NodeEffectType.IceChillOnHit, 0.40f));
            mgr.RegisterNode(new SkillTreeNodeDef("w_ice2", "빙하 산산조각", "빙결 적 막타 시 5% 확률 즉사", g, 8, 1, i, NodeEffectType.IceShatterExecute, 0.05f, new[] { "w_ice1" }));
            mgr.RegisterNode(new SkillTreeNodeDef("w_ice3", "동토의 강타", "지면 강타 → 빙하 균열 (적 동결 1.5초)", g, 12, 1, i, NodeEffectType.IceStompFreeze, 1.5f, new[] { "w_ice2" }));
            mgr.RegisterNode(new SkillTreeNodeDef("w_ice4", "서리 반격", "피격 시 15% 확률 서리 노바 반격", g, 20, 1, i, NodeEffectType.IceFrostCounter, 0.15f, new[] { "w_ice3" }));

            // ── ⚡ Lightning Branch: Storm Champion (4) ──
            var l = BranchType.Lightning;
            mgr.RegisterNode(new SkillTreeNodeDef("w_elec1", "전기 충격검", "대검 베기에 감전 부여 (피해 15% 증폭)", g, 5, 1, l, NodeEffectType.LightningShockOnHit, 0.15f));
            mgr.RegisterNode(new SkillTreeNodeDef("w_elec2", "전자기 폭풍", "감전 적 5체 이상 시 전자기 폭풍", g, 8, 1, l, NodeEffectType.LightningStormOverload, 5f, new[] { "w_elec1" }));
            mgr.RegisterNode(new SkillTreeNodeDef("w_elec3", "번개 휠윈드", "휠윈드 회전 시 번개 방전", g, 12, 1, l, NodeEffectType.LightningWhirlwindDischarge, 1f, new[] { "w_elec2" }));
            mgr.RegisterNode(new SkillTreeNodeDef("w_elec4", "낙뢰의 투사", "공격 속도 +20% & 4타마다 번개 낙뢰", g, 20, 1, l, NodeEffectType.LightningThunderStrike, 0.20f, new[] { "w_elec3" }));
        }

        // ═══════════════════════════════════════════════════
        //  🏹 RANGER TREE (Emerald)
        // ═══════════════════════════════════════════════════

        private static void RegisterRangerTree(SkillTreeManager mgr)
        {
            var g = GemType.Emerald;
            var n = BranchType.None;

            // ── Core Stat Nodes (6) ──
            mgr.RegisterNode(new SkillTreeNodeDef("r_spd1", "경쾌한 발걸음 I", "이동속도 +8%", g, 3, 1, n, NodeEffectType.MoveSpeed, 0.08f));
            mgr.RegisterNode(new SkillTreeNodeDef("r_spd2", "경쾌한 발걸음 II", "이동속도 +15%", g, 3, 1, n, NodeEffectType.MoveSpeed, 0.15f, new[] { "r_spd1" }));
            mgr.RegisterNode(new SkillTreeNodeDef("r_crit1", "급소 포착 I", "크리티컬 확률 +5%", g, 3, 1, n, NodeEffectType.CritChance, 0.05f));
            mgr.RegisterNode(new SkillTreeNodeDef("r_crit2", "급소 포착 II", "크리티컬 확률 +10%", g, 3, 1, n, NodeEffectType.CritChance, 0.10f, new[] { "r_crit1" }));
            mgr.RegisterNode(new SkillTreeNodeDef("r_projspd", "고속 사격", "투사체 속도 +15%", g, 3, 1, n, NodeEffectType.ProjectileSpeed, 0.15f));
            mgr.RegisterNode(new SkillTreeNodeDef("r_dodge", "회피 본능", "피격 시 10% 확률 회피", g, 3, 1, n, NodeEffectType.DodgeChance, 0.10f));

            // ── 🔥 Fire Branch: Phoenix Archer (4) ──
            var f = BranchType.Fire;
            mgr.RegisterNode(new SkillTreeNodeDef("r_fire1", "화염 화살", "관통 화살에 화염 → 화상 3초", g, 5, 1, f, NodeEffectType.FireArrowBurn, 3f));
            mgr.RegisterNode(new SkillTreeNodeDef("r_fire2", "폭발 크리티컬", "화상 적 크리티컬 시 AoE 폭발", g, 8, 1, f, NodeEffectType.FireCritExplosion, 1.5f, new[] { "r_fire1" }));
            mgr.RegisterNode(new SkillTreeNodeDef("r_fire3", "유성우 화살비", "화살비 → 유성우 (화염 웅덩이 2초)", g, 12, 1, f, NodeEffectType.FireMeteorRain, 2f, new[] { "r_fire2" }));
            mgr.RegisterNode(new SkillTreeNodeDef("r_fire4", "불사조 소환", "5연속 명중 시 불사조 소환 (8초)", g, 20, 1, f, NodeEffectType.FirePhoenixSummon, 8f, new[] { "r_fire3" }));

            // ── ❄️ Ice Branch: Frost Hunter (4) ──
            var i = BranchType.Ice;
            mgr.RegisterNode(new SkillTreeNodeDef("r_ice1", "빙결 화살", "관통 화살에 빙결 → 이속 40% 감소", g, 5, 1, i, NodeEffectType.IceArrowChill, 0.40f));
            mgr.RegisterNode(new SkillTreeNodeDef("r_ice2", "파편 분쇄", "빙결 적 처치 시 파편 → 주변 추가 피해", g, 8, 1, i, NodeEffectType.IceShardBurst, 1f, new[] { "r_ice1" }));
            mgr.RegisterNode(new SkillTreeNodeDef("r_ice3", "서리 글레이브", "글레이브에 서리 → 동결 0.8초", g, 12, 1, i, NodeEffectType.IceGlaiveFrost, 0.8f, new[] { "r_ice2" }));
            mgr.RegisterNode(new SkillTreeNodeDef("r_ice4", "빙결 자동포탑", "매 10초 빙결 화살 자동 발사", g, 20, 1, i, NodeEffectType.IceAutoTurret, 10f, new[] { "r_ice3" }));

            // ── ⚡ Lightning Branch: Thunder Marksman (4) ──
            var l = BranchType.Lightning;
            mgr.RegisterNode(new SkillTreeNodeDef("r_elec1", "전격 화살", "관통 화살에 감전 (피해 15% 증폭)", g, 5, 1, l, NodeEffectType.LightningArrowShock, 0.15f));
            mgr.RegisterNode(new SkillTreeNodeDef("r_elec2", "연쇄 전이", "감전 적 사이 자동 번개 연쇄 (2체)", g, 8, 1, l, NodeEffectType.LightningChainJump, 2f, new[] { "r_elec1" }));
            mgr.RegisterNode(new SkillTreeNodeDef("r_elec3", "크리티컬 낙뢰", "크리티컬 시 번개 낙뢰 (감전 적 300%)", g, 12, 1, l, NodeEffectType.LightningCritThunder, 3f, new[] { "r_elec2" }));
            mgr.RegisterNode(new SkillTreeNodeDef("r_elec4", "전격 관통", "모든 투사체 관통 시 감전 보장", g, 20, 1, l, NodeEffectType.LightningFullPierce, 1f, new[] { "r_elec3" }));
        }

        // ═══════════════════════════════════════════════════
        //  🔮 WIZARD TREE (Amethyst)
        // ═══════════════════════════════════════════════════

        private static void RegisterWizardTree(SkillTreeManager mgr)
        {
            var g = GemType.Amethyst;
            var n = BranchType.None;

            // ── Core Stat Nodes (6) ──
            mgr.RegisterNode(new SkillTreeNodeDef("m_cdr1", "마력 순환 I", "쿨다운 감소 +5%", g, 3, 1, n, NodeEffectType.CooldownReduction, 0.05f, null, 150));
            mgr.RegisterNode(new SkillTreeNodeDef("m_cdr2", "마력 순환 II", "쿨다운 감소 +10%", g, 3, 1, n, NodeEffectType.CooldownReduction, 0.10f, new[] { "m_cdr1" }, 300));
            mgr.RegisterNode(new SkillTreeNodeDef("m_area1", "마력 확산 I", "스킬 범위 +10%", g, 3, 1, n, NodeEffectType.AreaMultiplier, 0.10f, null, 150));
            mgr.RegisterNode(new SkillTreeNodeDef("m_area2", "마력 확산 II", "스킬 범위 +20%", g, 3, 1, n, NodeEffectType.AreaMultiplier, 0.20f, new[] { "m_area1" }, 300));
            mgr.RegisterNode(new SkillTreeNodeDef("m_ap1", "주술 강화", "공격력 +12%", g, 3, 1, n, NodeEffectType.AttackPower, 0.12f, null, 350));
            mgr.RegisterNode(new SkillTreeNodeDef("m_mana", "마나 흐름", "대미지 +8% & 쿨다운 -3%", g, 3, 1, n, NodeEffectType.ComboStatCdrArea, 0.08f, null, 500));

            // ── 🔥 Fire Branch: Inferno Archmage (4) ──
            var f = BranchType.Fire;
            mgr.RegisterNode(new SkillTreeNodeDef("m_fire1", "업화의 촉매", "화염구 화상 DoT +50% 강화", g, 5, 1, f, NodeEffectType.FireballDotBoost, 0.50f, null, 300));
            mgr.RegisterNode(new SkillTreeNodeDef("m_fire2", "폭렬 화염구", "화염구 폭발 범위 +30%", g, 8, 1, f, NodeEffectType.FireballAreaBoost, 0.30f, new[] { "m_fire1" }, 600));
            mgr.RegisterNode(new SkillTreeNodeDef("m_fire3", "소형 유성 낙하", "10초마다 소형 유성 자동 낙하", g, 12, 1, f, NodeEffectType.FireAutoMeteor, 10f, new[] { "m_fire2" }, 1000));
            mgr.RegisterNode(new SkillTreeNodeDef("m_fire4", "연쇄 폭발", "화상 적 3체 이상 시 자발적 연쇄 폭발", g, 20, 1, f, NodeEffectType.FireChainExplosion, 3f, new[] { "m_fire3" }, 1500));

            // ── ❄️ Ice Branch: Absolute Zero (4) ──
            var i = BranchType.Ice;
            mgr.RegisterNode(new SkillTreeNodeDef("m_ice1", "극한의 한기", "서리 폭발 감속 60%로 강화", g, 5, 1, i, NodeEffectType.IceNovaSlowBoost, 0.60f, null, 300));
            mgr.RegisterNode(new SkillTreeNodeDef("m_ice2", "빙하 파편", "동결 해제 시 빙하 파편 4방향 발사", g, 8, 1, i, NodeEffectType.IceShardOnThaw, 4f, new[] { "m_ice1" }, 600));
            mgr.RegisterNode(new SkillTreeNodeDef("m_ice3", "만상의 마법", "모든 마법에 10% 확률 즉시 동결", g, 12, 1, i, NodeEffectType.IceChanceFreeze, 0.10f, new[] { "m_ice2" }, 1000));
            mgr.RegisterNode(new SkillTreeNodeDef("m_ice4", "빙결 오라", "주변 5m 적 자동 감속 오라", g, 20, 1, i, NodeEffectType.IceFrostAura, 5f, new[] { "m_ice3" }, 1500));

            // ── ⚡ Lightning Branch: Storm Sage (4) ──
            var l = BranchType.Lightning;
            mgr.RegisterNode(new SkillTreeNodeDef("m_elec1", "전류 증폭", "연쇄 번개 전이 횟수 +3", g, 5, 1, l, NodeEffectType.LightningChainCountBoost, 3f, null, 300));
            mgr.RegisterNode(new SkillTreeNodeDef("m_elec2", "연쇄 재발동", "감전 적 처치 시 연쇄 번개 즉시 재발동", g, 8, 1, l, NodeEffectType.LightningChainOnKill, 1f, new[] { "m_elec1" }, 600));
            mgr.RegisterNode(new SkillTreeNodeDef("m_elec3", "전기 공명", "모든 마법 10% 확률 연쇄 번개 자동 발사", g, 12, 1, l, NodeEffectType.LightningChainOnHit, 0.10f, new[] { "m_elec2" }, 1000));
            mgr.RegisterNode(new SkillTreeNodeDef("m_elec4", "전격 보호막", "피격 시 30% 확률 감전 반격", g, 20, 1, l, NodeEffectType.LightningShockShield, 0.30f, new[] { "m_elec3" }, 1500));
        }
    }
}
