namespace HappyShoot.Domain.Progression
{
    /// <summary>
    /// Zero-allocation readonly value type holding all unlocked progression effect flags and parameters.
    /// Created once by SkillTreeApplier and stored on PlayerEntity for combat systems to read.
    /// </summary>
    public struct PlayerProgressionFlags
    {
        // ── Warrior Fire (Inferno Berserker) ──
        public bool WFireBurnOnHit;           // w_fire1: Slash adds Burn DoT
        public float WFireBurnDuration;
        public bool WFireDeathExplosion;      // w_fire2: Kill burning enemy → AoE
        public float WFireExplosionRadius;
        public bool WFireWhirlwindAura;       // w_fire3: Whirlwind adds fire aura
        public bool WFireGroundLava;          // w_fire4: Ground Stomp → lava pool

        // ── Warrior Ice (Frost Knight) ──
        public bool WIceChillOnHit;           // w_ice1: Slash adds Chill
        public float WIceChillSlowFactor;
        public bool WIceShatterExecute;       // w_ice2: 5% execute frozen enemies
        public float WIceShatterChance;
        public bool WIceStompFreeze;          // w_ice3: Stomp → freeze 1.5s
        public float WIceFreezeDuration;
        public bool WIceFrostCounter;         // w_ice4: 15% frost nova on hit taken
        public float WIceCounterChance;

        // ── Warrior Lightning (Storm Champion) ──
        public bool WElecShockOnHit;          // w_elec1: Slash adds Shock
        public float WElecShockAmplify;
        public bool WElecStormOverload;       // w_elec2: 5+ shocked → EMP
        public bool WElecWhirlwindDischarge;  // w_elec3: Whirlwind → discharge
        public bool WElecThunderStrike;       // w_elec4: +20% AS + every 4th → thunder
        public float WElecBonusAttackSpeed;

        // ── Ranger Fire (Phoenix Archer) ──
        public bool RFireBurnOnHit;           // r_fire1: Arrow adds Burn
        public float RFireBurnDuration;
        public bool RFireCritExplosion;       // r_fire2: Crit on burning → AoE
        public bool RFireMeteorRain;          // r_fire3: Arrow Rain → meteor shower
        public bool RFirePhoenixSummon;       // r_fire4: 5-hit → phoenix
        public int RFirePhoenixHitThreshold;

        // ── Ranger Ice (Frost Hunter) ──
        public bool RIceChillOnHit;           // r_ice1: Arrow adds Chill
        public float RIceChillSlowFactor;
        public bool RIceShardBurst;           // r_ice2: Kill chilled → shards
        public bool RIceGlaiveFrost;          // r_ice3: Glaive → freeze 0.8s
        public float RIceGlaiveFreezeDuration;
        public bool RIceAutoTurret;           // r_ice4: auto ice arrow every 10s
        public float RIceAutoTurretInterval;

        // ── Ranger Lightning (Thunder Marksman) ──
        public bool RElecShockOnHit;          // r_elec1: Arrow adds Shock
        public float RElecShockAmplify;
        public bool RElecChainJump;           // r_elec2: Auto chain 2 targets
        public int RElecChainCount;
        public bool RElecCritThunder;         // r_elec3: Crit → thunder bolt
        public bool RElecFullPierce;          // r_elec4: All pierce → guaranteed shock

        // ── Wizard Fire (Inferno Archmage) ──
        public bool MFireDotBoost;            // m_fire1: Fireball DoT +50%
        public float MFireDotMultiplier;
        public bool MFireAreaBoost;           // m_fire2: Fireball area +30%
        public float MFireAreaMultiplier;
        public bool MFireAutoMeteor;          // m_fire3: Auto small meteor every 10s
        public float MFireAutoMeteorInterval;
        public bool MFireChainExplosion;      // m_fire4: 3+ burning → chain explosion

        // ── Wizard Ice (Absolute Zero) ──
        public bool MIceSlowBoost;            // m_ice1: Frost Nova slow 60%
        public float MIceSlowFactor;
        public bool MIceShardOnThaw;          // m_ice2: Thaw → 4-dir shards
        public bool MIceChanceFreeze;         // m_ice3: All magic 10% freeze
        public float MIceFreezeChance;
        public bool MIceFrostAura;            // m_ice4: Passive frost aura 5m

        // ── Wizard Lightning (Storm Sage) ──
        public bool MElecChainCountBoost;     // m_elec1: +3 chain count
        public int MElecExtraChainCount;
        public bool MElecChainOnKill;         // m_elec2: Kill shocked → refire chain
        public bool MElecChainOnHit;          // m_elec3: 10% any magic → auto chain
        public float MElecChainOnHitChance;
        public bool MElecShockShield;         // m_elec4: 30% shock counter on hit taken
        public float MElecShockShieldChance;

        // ── Ranger Dodge ──
        public bool HasDodgeChance;
        public float DodgeChance;

        /// <summary>Default (all-false, all-zero) flags.</summary>
        public static readonly PlayerProgressionFlags Empty = default;
    }
}
