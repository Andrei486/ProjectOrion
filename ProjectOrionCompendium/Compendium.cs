using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.IO;
using System.Diagnostics;
using ShipSheets;
using System.Text.Json.Serialization;

namespace Compendium
{
    /// <summary>
    /// The Compendium class manages the master lists of weapons, systems, crafts and ships,
    /// as well as any other similar lists. It provides methods for other classes to access those lists.
    /// </summary>
    public class Compendium
    {
        // paths to JSON files where lists are stored
        private const string WEAPON_LIST = "Resources/WeaponList.json";
        private const string SYSTEM_LIST = "Resources/SystemList.json";
        private const string SHIP_LIST = "Resources/ShipList.json";
        private const string CRAFT_LIST = "Resources/CraftList.json";
        private const string DEFAULT_TRAITS = "Resources/DefaultTraits.json";

        private List<Weapon> weapons;
        private List<Ship> shipTemplates;
        private List<Craft> crafts;
        private List<ShipSystem> defaultSystems;
        private List<ShipSystem> slotSystems;
        private static Compendium singleton;
        private Dictionary<ShipClass, Dictionary<string, string>> defaultTraits;

        /// <summary>
        /// Creates a Compendium and loads all resources. Private because Compendium is a singleton -
        /// use Compendium.GetCompendium instead.
        /// </summary>
        private Compendium()
        {
            LoadWeapons();
            LoadSystems();
            LoadShips();
            LoadCrafts();
            LoadTraits();
        }

        /// <summary>
        /// Gets the Compendium singleton object.
        /// </summary>
        /// <returns>a Compendium object with resources loaded</returns>
        public static Compendium GetCompendium()
        {
            if (singleton == null)
            {
                singleton = new Compendium();
            }
            return singleton;
        }

        /// <summary>
        /// Loads the weapon list from the relevant file.
        /// </summary>
        private void LoadWeapons()
        {
            using (StreamReader r = new StreamReader(WEAPON_LIST))
            {
                var options = new JsonSerializerOptions();
                options.Converters.Add(new JsonStringEnumConverter());
                var contents = r.ReadToEnd();
                JsonNode weaponsNode = JsonNode.Parse(contents);
                weapons = JsonSerializer.Deserialize<List<Weapon>>(weaponsNode["Weapons"]);
            }
        }

        /// <summary>
        /// Loads the system list from the relevant file.
        /// </summary>
        private void LoadSystems()
        {
            using (StreamReader r = new StreamReader(SYSTEM_LIST))
            {
                var options = new JsonSerializerOptions();
                options.Converters.Add(new JsonStringEnumConverter()); //to handle ShipClass enums
                var contents = r.ReadToEnd();
                JsonNode systemsNode = JsonNode.Parse(contents);
                defaultSystems = JsonSerializer.Deserialize<List<ShipSystem>>(systemsNode["Default"], options);
                slotSystems = JsonSerializer.Deserialize<List<ShipSystem>>(systemsNode["Slots"], options);
            }
        }

        /// <summary>
        /// Loads the ship template list from the relevant file.
        /// </summary>
        private void LoadShips()
        {
            using (StreamReader r = new StreamReader(SHIP_LIST))
            {
                var options = new JsonSerializerOptions();
                options.Converters.Add(new JsonStringEnumConverter()); //to handle enums
                var contents = r.ReadToEnd();
                JsonNode shipsNode = JsonNode.Parse(contents);
                shipTemplates = JsonSerializer.Deserialize<List<Ship>>(shipsNode["Ships"], options);
            }
        }

        /// <summary>
        /// Loads the craft list from the relevant file.
        /// </summary>
        private void LoadCrafts()
        {
            using (StreamReader r = new StreamReader(CRAFT_LIST))
            {
                var options = new JsonSerializerOptions();
                options.Converters.Add(new JsonStringEnumConverter()); //to handle enums
                var contents = r.ReadToEnd();
                JsonNode craftsNode = JsonNode.Parse(contents);
                crafts = new List<Craft>();
                crafts.AddRange(JsonSerializer.Deserialize<List<Payload>>(craftsNode["Payloads"], options));
                crafts.AddRange(JsonSerializer.Deserialize<List<Deployable>>(craftsNode["Deployables"], options));
            }
        }

        /// <summary>
        /// Loads the default traits map for ships from the relevant file.
        /// </summary>
        private void LoadTraits()
        {
            using (StreamReader r = new StreamReader(DEFAULT_TRAITS))
            {
                var options = new JsonSerializerOptions();
                options.Converters.Add(new JsonStringEnumConverter()); //to handle enums
                var contents = r.ReadToEnd();
                JsonNode traitsNode = JsonNode.Parse(contents);
                defaultTraits = JsonSerializer.Deserialize<Dictionary<ShipClass, Dictionary<string, string>>>(traitsNode);
            }
        }

        /// <summary>
        /// Gets all weapons from the master list that match a specified filter.
        /// If no filter is provided, return all weapons.
        /// </summary>
        /// <param name="filter">a predicate to filter weapons by, if provided only weapons that match this predicate will be returned</param>
        /// <returns>all Weapons in the master list that match the filter</returns>
        public List<Weapon> GetWeapons(Func<Weapon, bool> filter=null)
        {
            Func<Weapon, bool> predicate = filter ?? (_ => true);
            return weapons.Where(predicate).ToList();
        }

        /// <summary>
        /// Gets the Weapon with the specified name.
        /// If two or more Weapons have the same name, this is not guarantee to return any particular one.
        /// </summary>
        /// <param name="name">the name of the Weapon to get</param>
        /// <returns>a Weapon with the specified name, or null if none exist</returns>
        public Weapon GetWeapon(string name)
        {
            return weapons.FirstOrDefault(w => w.Name == name);  
        }

        /// <summary>
        /// Gets all systems from the master list that match a specified filter.
        /// If no filter is provided, return all systems.
        /// </summary>
        /// <param name="filter">a predicate to filter systems by, if provided only systems that match this predicate will be returned</param>
        /// <returns>all ShipSystems in the master list that match the filter</returns>
        public List<ShipSystem> GetSystems(Func<ShipSystem, bool> filter=null)
        {
            Func<ShipSystem, bool> predicate = filter ?? (_ => true);
            return defaultSystems.Concat(slotSystems).Where(predicate).ToList();
        }

        /// <summary>
        /// Gets all systems that all ships have equipped by default.
        /// </summary>
        /// <returns>all default systems</returns>
        public List<ShipSystem> GetDefaultSystems()
        {
            return GetSystems(s => s.Slots == 0);
        }

        /// <summary>
        /// Gets the ShipSystem with the specified name.
        /// If two or more ShipSystem have the same name, this is not guarantee to return any particular one.
        /// </summary>
        /// <param name="name">the name of the ShipSystem to get</param>
        /// <returns>a ShipSystem with the specified name, or null if none exist</returns>
        public ShipSystem GetSystem(string name)
        {
            return GetSystems().FirstOrDefault(s => s.Name == name);
        }

        /// <summary>
        /// Gets all ship templates that match a specified filter.
        /// If no filter is provided, return all ship templates.
        /// </summary>
        /// <param name="filter">a predicate to filter ships by, if provided only ships that match this predicate will be returned</param>
        /// <returns>all Ship templates that match the filter</returns>
        public List<Ship> GetShips(Func<Ship, bool> filter=null)
        {
            Func<Ship, bool> predicate = filter ?? (_ => true);
            return shipTemplates.Where(predicate).ToList();
        }

        /// <summary>
        /// Gets a Ship template that matches the specified name at least in part.
        /// If there are multiple, any matching template can be returned.
        /// </summary>
        /// <param name="name">part of the name of the Ship template to get</param>
        /// <returns>a template Ship with the specified name, or null if none exist</returns>
        public Ship GetShip(string name)
        {
            return shipTemplates.FirstOrDefault(s => s.Name.Contains(name));
        }

        public List<Craft> GetCrafts(Func<Craft, bool> filter=null)
        {
            Func<Craft, bool> predicate = filter ?? (_ => true);
            return crafts.Where(predicate).ToList();
        }

        public Craft GetCraft(string name)
        {
            return crafts.FirstOrDefault(c => c.Name.Contains(name));
        }

        /// <summary>
        /// Equips all default systems to a specified Ship. Does not equip systems that the Ship already has.
        /// </summary>
        /// <param name="ship">the Ship to equip systems to</param>
        public void EquipDefaultSystems(Ship ship)
        {
            foreach (ShipSystem system in GetDefaultSystems())
            {
                if (ship.CanEquip(system) && (from s in ship.Systems where s.Name == system.Name select system).Count() == 0)
                {
                    ship.Equip(system);
                }
            }
        }

        /// <summary>
        /// Adds all default traits to a specified Ship. Traits depend on the ship's class.
        /// </summary>
        /// <param name="ship">the Ship to add traits to</param>
        public void AddDefaultTraits(Ship ship)
        {
            Dictionary<string, string> toAdd = defaultTraits[ship.Class];
            foreach (KeyValuePair<string, string> pair in toAdd)
            {
                if (!ship.Traits.ContainsKey(pair.Key))
                {
                    ship.Traits.Add(pair.Key, pair.Value);
                }
            }
        }

        static void Main()
        {
            var compendium = Compendium.GetCompendium();
            Debug.WriteLine(compendium.weapons.Count);
            Debug.WriteLine(compendium.weapons.First().Name);
            Debug.WriteLine(compendium.GetWeapon("Dual Autocannons").ArmorPenetration);
            Debug.WriteLine(compendium.GetSystem("Engine Booster").Description);
            Debug.WriteLine(compendium.GetShip("Emblem").Mounts.Length);
            Debug.WriteLine(compendium.crafts.Count);
            Debug.WriteLine(compendium.GetCraft("Chaff").Size);
            Debug.WriteLine(compendium.GetCraft("Light Torpedo").Size);
        }
    }


}
