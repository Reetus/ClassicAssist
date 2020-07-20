using ClassicAssist.UO.Network.PacketFilter;

namespace ClassicAssist.UO.Network.Packets
{
    public class UnicodePromptRequest : BasePacket, IMacroCommandParser
    {
        // ReSharper disable once EmptyConstructor
        public UnicodePromptRequest()
        {
            // TODO
            // byte[] packet = new byte[] { 0xC2, 0x00, 0x15, 0x00, 0x00, 0x04, 0x82, 0x00, 0x00, 0x04, 0x82, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
        }

        public string Parse( byte[] packet, int length, PacketDirection direction )
        {
            if ( packet[0] == 0xC2 && direction == PacketDirection.Incoming )
            {
                return "WaitForPrompt(5000)\r\n";
            }

            return null;
        }
    }
}