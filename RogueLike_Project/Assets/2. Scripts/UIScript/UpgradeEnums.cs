public enum UpgradeTier
{
    decision,
    common,
    weapon,
    special
}
public enum UpgradeDecision
{
    BASIC,
    WEAPON,
    SPECIAL,
    EXIT
}
public enum CommonUpgrade
{
    ATK,
    UTIL,
    COIN
}
#region Common
public enum ATKUGType
{
    Damage,
    AttackSpeed,
    ReloadSpeed
}
public enum UTILUGType
{
    MoveSpeed,
    Heath,
}
public enum COINUGType
{
    CoinAcquisitonRate,
    PermanentCoinAcquisitionRate
}
#endregion

#region Weapon
public enum WeaponUpgrade
{
    Blaze,
    Freeze,
    Shock,
    Null
}
public enum WeaponUpgradeSet
{
    probability,
    damage,
    duration,
    interval,
    effect
}
#endregion

