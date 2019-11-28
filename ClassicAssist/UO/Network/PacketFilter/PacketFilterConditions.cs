namespace ClassicAssist.UO.Network.PacketFilter
{
    public class PacketFilterConditions
    {
        public static PacketFilterCondition IntAtPositionCondition(int value, int position)
        {
            byte[] valueBytes = { (byte)(value >> 24), (byte)(value >> 16), (byte)(value >> 8), (byte)value };

            return new PacketFilterCondition(position, valueBytes, 4);
        }

        public static PacketFilterCondition ShortAtPositionCondition(int value, int position)
        {
            byte[] valueBytes = { (byte)(value >> 8), (byte)value };

            return new PacketFilterCondition(position, valueBytes, 2);
        }

        public static PacketFilterCondition ByteAtPositionCondition(int value, int position)
        {
            byte[] valueBytes = { (byte)value };

            return new PacketFilterCondition(position, valueBytes, 1);
        }
    }
}