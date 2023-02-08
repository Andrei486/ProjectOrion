using System;
using System.Text.Json.Serialization;

namespace ShipSheets
{
    public class ShipSystem
    {
        public string Name { get; }
        public string Description { get; }
        public int Slots { get; }
        public int HitPoints { get; }
        public string[] BubbleText { get; }
        public ShipClass[] EquippableClasses { get; }

        [JsonConstructor]
        public ShipSystem(string name, string description, int slots, int hitPoints, ShipClass[] equippableClasses=null, string[] bubbleText=null) { 
            Name = name;
            Description = description;
            Slots = slots;
            HitPoints = hitPoints;
            BubbleText = bubbleText;
            EquippableClasses = equippableClasses ?? ((ShipClass[])Enum.GetValues(typeof(ShipClass)));
        }

        public static ShipSystem Filler()
        {
            return new ShipSystem("", "", 1, 0);
        }
    }
}
