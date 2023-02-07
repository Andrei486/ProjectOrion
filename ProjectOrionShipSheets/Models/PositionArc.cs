namespace ShipSheets
{
    public enum PositionArc
    {
        Forward,
        Port,
        Starboard,
        Rear
    }

    public static class PositionArcMethods
    {
        public static string GetDisplayString(this PositionArc arc)
        {
            return arc.ToString().Substring(0, 1);
        }
    }
}
