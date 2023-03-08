using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace ShipSheets
{
    public class Ship
    {
        public string Name { get; set; }
        public string Identifier { get; set; }
        public Dictionary<ShipStat, int> Stats { get; }
        public ShipClass Class { get; }
        public Mount[] Mounts { get; }
        public Bay[] Bays { get; }
        public List<ShipSystem> Systems { get; }
        private int systemSlots;
        public int SystemSlots { 
            get { return systemSlots; }
            set { systemSlots = value; }
        }
        public int PointCost { get; set; }
        public Dictionary<string, string> Traits { get; }

        /// <summary>
        /// Creates a new Ship.
        /// </summary>
        /// <param name="name">the name of the Ship</param>
        /// <param name="stats">the stats of the Ship, unset stats default to 0</param>
        /// <param name="class">the Ship's class</param>
        /// <param name="mounts">the Ship's mounts</param>
        /// <param name="bays">the Ship's bays</param>
        /// <param name="systemSlots">the Ship's maximum system slots</param>
        /// <param name="pointCost">the Ship's point cost to deploy</param>
        /// <param name="traits">the Ship's non-default traits</param>
        /// <param name="systems">the Ship's non-default equipped systems</param>
        /// <param name="identifier">the Ship's identifier, a string to tell it apart from others of the same template</param>
        [JsonConstructor]
        public Ship(string name, Dictionary<ShipStat, int> stats, 
            ShipClass @class, Mount[] mounts, Bay[] bays, 
            int systemSlots, int pointCost, Dictionary<string, string> traits, List<ShipSystem> systems=null, string identifier="")
        {
            Name = name;
            var shipStats = stats;
            foreach (ShipStat stat in Enum.GetValues(typeof(ShipStat)))
            {
                if (!shipStats.ContainsKey(stat)) 
                {
                    shipStats[stat] = 0;
                }
            }
            Stats = shipStats;
            Class = @class;
            Mounts = mounts;
            Bays = bays;
            this.systemSlots = systemSlots;
            PointCost = pointCost;
            Traits = traits;
            Systems = systems ?? (new List<ShipSystem>());
            Identifier = identifier;
        }

        /// <summary>
        /// Gets the number of free/unused system slots on this ship.
        /// </summary>
        /// <returns>The number of free system slots on the ship</returns>
        public int GetFreeSystemSlots()
        {
            return SystemSlots - (from system in Systems select system.Slots).Sum();
        }

        /// <summary>
        /// Gets one stat of this ship.
        /// </summary>
        /// <param name="stat">The stat to get</param>
        /// <returns>The value of the specified stat</returns>
        public int GetStat(ShipStat stat)
        {
            return Stats[stat];
        }

        /// <summary>
        /// Checks whether a given system can be equipped on this ship. A system can be equipped if the system can be equipped
        /// on the ship's class AND the ship has the slots left to do so.
        /// </summary>
        /// <param name="system">The system to check</param>
        /// <returns>true if the system can be equipped, false otherwise</returns>
        public bool CanEquip(ShipSystem system)
        {
            return system.Slots <= GetFreeSystemSlots() && (system.EquippableClasses.Contains(Class));
        }

        /// <summary>
        /// Equips the specified system.
        /// </summary>
        /// <param name="system">The system to equip</param>
        /// <exception cref="ArgumentException">Thrown if the system could not be equipped</exception>
        public void Equip(ShipSystem system)
        {
            if (!CanEquip(system))
            {
                throw new ArgumentException("System " + system.Name + " cannot be equipped to this ship.");
            }
            Systems.Add(system);
        }
    }
}
