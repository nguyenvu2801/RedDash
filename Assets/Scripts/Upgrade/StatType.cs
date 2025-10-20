public enum StatType
{
    MaxTimer,              // Additive: +seconds
    TimerDecayRate,        // Multiplicative: <1 for slower decay
    KillRechargeAmount,     // Additive: +seconds per hit
    ComboBonusDuration,    // Additive: +seconds to reset time
    DeathGraceWindow,      // Additive: +seconds grace on 0
    DashRange,             // Additive: +distance
    DashCooldown,          // Multiplicative: <1 for shorter
    DashDamage,            // Additive: +damage
    PierceCount,           // Additive: +count
    CriticalDashChance,    // Additive: +chance (0-1)
    DashImpactRadius,      // Additive: +radius
    DashComboPower,        // Additive: +multi per combo
    EssenceGain,           // Additive: +essence per kill
    DropMagnetRadius,      // Additive: +radius
    IncreaseComboDuration,         // Additive: +chance (0-1)
    LastBreath             // Level >0 enables
}