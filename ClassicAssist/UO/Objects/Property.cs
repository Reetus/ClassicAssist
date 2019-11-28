namespace ClassicAssist.UO.Objects
{
    public sealed class Property
    {
        /// <summary>
        /// Entire property decoded to a string.
        /// </summary>
        public string Text { get; internal set; }

        /// <summary>
        /// Array of arguments supplied with this property.
        /// </summary>
        public string[] Arguments { get; internal set; }

        /// <summary>
        /// Property number.
        /// </summary>
        public int Cliloc { get; internal set; }
    }
}