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
    public class Compendium
    {
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

        private Compendium()
        {
            LoadWeapons();
            LoadSystems();
            LoadShips();
            LoadCrafts();
            LoadTraits();
        }

        public static Compendium GetCompendium()
        {
            if (singleton == null)
            {
                singleton = new Compendium();
            }
            return singleton;
        }

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

        public List<Weapon> GetWeapons(Func<Weapon, bool> filter=null)
        {
            Func<Weapon, bool> predicate = filter ?? (_ => true);
            return weapons.Where(predicate).ToList();
        }
        public Weapon GetWeapon(string name)
        {
            return weapons.FirstOrDefault(w => w.Name == name);  
        }

        public List<ShipSystem> GetSystems(Func<ShipSystem, bool> filter=null)
        {
            Func<ShipSystem, bool> predicate = filter ?? (_ => true);
            return defaultSystems.Concat(slotSystems).Where(predicate).ToList();
        }

        public List<ShipSystem> GetDefaultSystems()
        {
            return GetSystems(s => s.Slots == 0);
        }

        public ShipSystem GetSystem(string name)
        {
            return GetSystems().FirstOrDefault(s => s.Name == name);
        }

        public List<Ship> GetShips(Func<Ship, bool> filter=null)
        {
            Func<Ship, bool> predicate = filter ?? (_ => true);
            return shipTemplates.Where(predicate).ToList();
        }

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
