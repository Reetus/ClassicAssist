namespace ClassicAssist.UO.Objects
{
    public sealed class Property
    {
        /// <summary>
        ///     Array of arguments supplied with this property.
        /// </summary>
        public string[] Arguments { get; set; }

        /// <summary>
        ///     Property number.
        /// </summary>
        public int Cliloc { get; set; }

        /// <summary>
        ///     Entire property decoded to a string.
        /// </summary>
        public string Text { get; set; }
    }
}