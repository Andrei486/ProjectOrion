using System;
using System.Linq;
using System.Text.Json.Serialization;

namespace ShipSheets
{
    public class Weapon
    {
        public string Name { get; }
        public int Size { get; }
        public int Range { get; }
        public string Damage { get; }
        public int AmmoCost { get; }
        public int PowerCost { get; }
        public int ArmorPenetration { get; }
        public string Description { get; }
        private string[] _tags;
        public string[] Tags { get { return _tags; } set { _tags = value; } }

        [JsonConstructor]
        public Weapon(string name, int size, int range, string damage, int ammoCost=0, int powerCost=0,
            int armorPenetration=0, string description="", string[] tags=null) {

            Name = name;
            Size = size;
            Range = range;
            Damage = damage;
            AmmoCost = ammoCost;
            PowerCost = powerCost;
            ArmorPenetration = armorPenetration;
            Description = description;
            _tags = tags ?? (new string[0]);
        }
        public string GetShots()
        {
            if (_tags.Contains("EWAR"))
            {
                return "1";
            }
            foreach (string tag in _tags)
            {
                if (tag.StartsWith("Shots"))
                {
                    return tag.Split(' ').Last();
                }
            }
            return "1";
        }

        public bool IsSpinal()
        {
            return _tags.Contains("Spinal");
        }
    }
}
