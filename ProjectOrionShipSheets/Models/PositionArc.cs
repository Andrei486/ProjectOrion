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
        /// <summary>
        /// Gets a human-friendly string representation of a position arc (its first letter).
        /// </summary>
        /// <param name="arc">the value to convert</param>
        /// <returns>a human-friendly string to represent this value</returns>
        public static string GetDisplayString(this PositionArc arc)
        {
            return arc.ToString().Substring(0, 1);
        }
    }
}
