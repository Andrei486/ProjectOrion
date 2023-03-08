using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace ShipSheets
{
    public abstract class Craft
    {
        private static Dictionary<ShipStat, int> DefaultStats = new Dictionary<ShipStat, int>()
        {
            [ShipStat.Shields] = 0,
            [ShipStat.Reactor] = 0,
            [ShipStat.Ammo] = 0,
            [ShipStat.Restores] = 0,
            [ShipStat.Sensors] = 0,
            [ShipStat.Signature] = 0
        };
        public string Name { get; set; }
        public string Description { get; set; }
        public Dictionary<ShipStat, int> Stats { get; set; }
        public int Size { get; set; }
        public int AmmoCost { get; set; }
        public int PowerCost { get; set; }
        private string[] _tags;
        public string[] Tags { get { return _tags; } set { _tags = value; } }

        /// <summary>
        /// Creates a new Craft.
        /// </summary>
        /// <param name="name">the name of the Craft</param>
        /// <param name="stats">the stats of the Craft, unset or non-applicable stats default to 0</param>
        /// <param name="size">the size of the Craft</param>
        /// <param name="ammoCost">the ammo cost to deploy the Craft</param>
        /// <param name="powerCost">the power cost to deploy the Craft</param>
        /// <param name="description">a description of the Craft</param>
        /// <param name="tags">tags applicable to the Craft</param>
        [JsonConstructor]
        public Craft(string name, Dictionary<ShipStat, int> stats, int size, int ammoCost, int powerCost, string description="", string[] tags=null)
        {
            Name = name;
            var shipStats = DefaultStats;
            foreach (var item in stats)
            {
                if (shipStats.ContainsKey(item.Key))
                {
                    shipStats[item.Key] = item.Value;
                } else {
                    shipStats.Add(item.Key, item.Value);
                }
            }
            Stats = shipStats;
            Size = size;
            AmmoCost = ammoCost;
            PowerCost = powerCost;
            Description = description;
            _tags = tags ?? (new string[0]);
        }
        /// <summary>
        /// Gets one stat of this craft.
        /// </summary>
        /// <param name="stat">The stat to get</param>
        /// <returns>The value of the specified stat</returns>
        public int GetStat(ShipStat stat)
        {
            return Stats[stat];
        }
        public string GetSwarm()
        {
            foreach (string tag in _tags)
            {
                if (tag.StartsWith("Swarm"))
                {
                    return tag.Split(' ').Last();
                }
            }
            return "1";
        }

        public virtual string GetDamage()
        {
            return null;
        }
        
        public virtual int GetArmorPenetration()
        {
            return 0;
        }
    }
}
