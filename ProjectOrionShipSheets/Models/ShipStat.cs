namespace ShipSheets
{
    public enum ShipStat
    {
        HP,
        Shields,
        Reactor,
        Ammo,
        Restores,
        Evasion,
        Armour,
        Speed,
        Sensors,
        Signature
    }

    public static class ShipStatMethods
    {
        public static bool IsGauge(this ShipStat stat)
        {
            return (stat <= ShipStat.Restores);
        }

        public static string GetDisplayString(this ShipStat stat)
        {
            //string statName = stat.ToString();
            //return statName[0].ToString().ToUpper() + statName.Substring(1).ToLower();
            return stat.ToString();
        }
    }
}
