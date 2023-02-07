using System;
using System.Text.Json.Serialization;

namespace ShipSheets
{
    public class Bay
    {
        public int Size { get; set; }
        public int Count { get; set; }
        public PositionArc[] Arcs { get; set; }
        private Craft _craft;
        public Craft Craft
        {
            get { return _craft; }
            set
            {
                if (value == null || CanEquip(value))
                {
                    _craft = value;
                }
                else
                {
                    throw new ArgumentException("Craft " + value.Name + " cannot be equipped to this bay.");
                }
            }
        }

        [JsonConstructor]
        public Bay(int size, int count, PositionArc[] arcs, Craft craft=null)
        {
            Size = size;
            Count = count;
            Arcs = arcs;
            Craft = craft;
        }

        public void Equip(Craft craft)
        {
            if (!CanEquip(craft)) throw new ArgumentException("Craft " + craft.Name + " cannot be equipped to this Bay.");
            Craft = craft;
        }

        public bool CanEquip(Craft craft)
        {
            return (Size >= craft.Size);
        }
    }
}
