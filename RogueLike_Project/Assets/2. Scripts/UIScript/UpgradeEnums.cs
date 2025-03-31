public enum UpgradeTier
{
    common,
    weapon,
    special
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
    Shock
}
public enum RareUpgradeSet
{
    damage,
    duration,
    probability,
    interval,
    effect
}
#endregion

