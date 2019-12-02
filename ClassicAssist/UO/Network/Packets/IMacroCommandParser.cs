using ClassicAssist.UO.Network.PacketFilter;

namespace ClassicAssist.UO.Network.Packets
{
    public interface IMacroCommandParser
    {
        string Parse( byte[] packet, int length, PacketDirection direction );
    }
}