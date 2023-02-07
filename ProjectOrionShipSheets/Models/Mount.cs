using System;
using System.Text.Json.Serialization;
using static ShipSheets.Mount;

namespace ShipSheets
{
    public class Mount
    {
        public enum MountType
        {
            Fixed,
            Turret,
            Omni
        }

        public int Size { get; }
        public int Count { get; }
        public MountType Type { get; }
        public PositionArc MainArc { get; }
        public bool IsSpinal { get; }
        private Weapon _weapon;
        public Weapon Weapon 
        { 
            get { return _weapon; } 
            set { 
                if (value == null || CanEquip(value))
                { 
                    _weapon = value;
                } else
                {
                    throw new ArgumentException("Weapon " + value.Name + " cannot be equipped on this mount.");
                }
            }
        }

        [JsonConstructor]
        public Mount(int size, int count, MountType type, PositionArc mainArc, bool isSpinal)
        {
            Size = size;
            Count = count;
            Type = type;
            MainArc = mainArc;
            IsSpinal = isSpinal;
        }

        public bool CanEquip(Weapon weapon)
        {
            return (Size >= weapon.Size) && (!weapon.IsSpinal() || IsSpinal);
        }

        public void Equip(Weapon weapon)
        {
            if (!CanEquip(weapon))
            {
                throw new ArgumentException("Weapon " + weapon.Name + " cannot be equipped to this mount.");
            }
            Weapon = weapon;
        }
    }

    static class MountTypeMethods
    {
        public static string GetDisplayString(this MountType type)
        {
            return type.ToString().Substring(0, 1);
        }
    }
}
