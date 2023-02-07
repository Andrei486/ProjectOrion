using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ShipSheets
{
    public class Deployable : Craft
    {
        [JsonConstructor]
        public Deployable(string name, Dictionary<ShipStat, int> stats, int size, int ammoCost, int powerCost, string description = "", string[] tags = null)
            : base(name, stats, size, ammoCost, powerCost, description, tags)
        {

        }
    }
}
