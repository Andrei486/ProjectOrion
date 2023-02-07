using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace ShipSheets
{
    public class Payload : Craft
    {
        public int ArmorPenetration { get; }
        public string Damage { get; }
        [JsonIgnore]
        public Weapon Weapon { get; set; }

        public Payload(string name, Dictionary<ShipStat, int> stats, Weapon weapon, string description = "", string[] tags = null)
            : base(name, stats, weapon.Size, weapon.AmmoCost, weapon.PowerCost, description, (tags ?? new string[0]).Concat(weapon.Tags).ToArray())
        {
            ArmorPenetration = weapon.ArmorPenetration;
            Damage = weapon.Damage;
            Weapon = weapon;
        }

        [JsonConstructor]
        public Payload(string name, Dictionary<ShipStat, int> stats, int size, int ammoCost, int powerCost, int armorPenetration, string damage, string description = "", string[] tags = null)
            : this(name, stats, new Weapon(name, size, 0, damage, ammoCost, powerCost, armorPenetration, description, tags), description, null)
        {
            Damage = damage;
            ArmorPenetration = armorPenetration;
        }

        public override string GetDamage()
        {
            return Weapon.Damage;
        }

        public override int GetArmorPenetration()
        {
            return Weapon.ArmorPenetration;
        }
    }
}
