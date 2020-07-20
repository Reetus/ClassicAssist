using ClassicAssist.Data.Skills;
using ClassicAssist.UO.Data;

namespace ClassicAssist.UO.Network.Packets
{
    public class ChangeSkillLock : BasePacket
    {
        public ChangeSkillLock( SkillEntry skill, LockStatus lockStatus )
        {
            _writer = new PacketWriter( 6 );
            _writer.Write( (byte) 0x3A );
            _writer.Write( (short) 6 );
            _writer.Write( (byte) 0 );
            _writer.Write( (byte) skill.Skill.ID );
            _writer.Write( (byte) lockStatus );
        }
    }
}