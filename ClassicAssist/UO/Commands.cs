using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Assistant;
using ClassicAssist.Data;
using ClassicAssist.Data.Skills;
using ClassicAssist.Resources;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Network.PacketFilter;
using ClassicAssist.UO.Objects;
using Skill = ClassicAssist.UO.Data.Skill;

namespace ClassicAssist.UO
{
    public class Commands
    {
        public enum MobileQueryType : byte
        {
            StatsRequest = 4,
            SkillsRequest = 5
        }

        public static void DragItem( int serial, int amount )
        {
            PacketWriter pw = new PacketWriter( 7 );
            pw.Write( (byte) 0x07 );
            pw.Write( serial );
            pw.Write( (short) amount );

            Engine.SendPacketToServer( pw );
        }

        public static void DropItem( int serial, int containerSerial, int x = -1, int y = -1, int z = 0 )
        {
            PacketWriter pw = new PacketWriter( 15 );
            pw.Write( (byte) 0x08 );
            pw.Write( serial );
            pw.Write( (short) x );
            pw.Write( (short) y );
            pw.Write( (sbyte) z );
            pw.Write( (byte) 0 );
            pw.Write( containerSerial );

            Engine.SendPacketToServer( pw );
        }

        public static async Task DragDropAsync( int serial, int amount, int containerSerial, int x = -1, int y = -1, int z = 0 )
        {
            await Task.Run( async () =>
            {
                DragItem( serial, amount );

                await Task.Delay( Options.CurrentOptions.ActionDelayMS );

                DropItem( serial, containerSerial, x, y, z );
            } );
        }

        public static void EquipItem( Item item, Layer layer )
        {
            int containerSerial = Engine.Player?.Serial ?? 0;

            if ( layer == Layer.Invalid )
            {
                StaticTile tileData = TileData.GetStaticTile( item.ID );
                layer = (Layer) tileData.Quality;
            }

            if ( layer == Layer.Invalid )
            {
                throw new ArgumentException( "EquipItem: Layer is invalid" );
            }

            PacketWriter pw = new PacketWriter( 10 );

            pw.Write( (byte) 0x13 );
            pw.Write( item.Serial );
            pw.Write( (byte) layer );
            pw.Write( containerSerial );

            DragItem( item.Serial, 1 );

            Engine.SendPacketToServer( pw );
        }

        public static void SystemMessage( string text )
        {
            byte[] textBytes = Encoding.BigEndianUnicode.GetBytes( text + '\0' );

            int length = 48 + textBytes.Length;

            PacketWriter pw = new PacketWriter( length );
            pw.Write( (byte) 0xAE );
            pw.Write( (short) length );
            pw.Write( 0xFFFFFFFF );
            pw.Write( (ushort) 0xFFFF );
            pw.Write( (byte) 0 );
            pw.Write( (short) 0x03b2 );
            pw.Write( (short) 0x03 );
            pw.WriteAsciiFixed( "ENU\0", 4 );
            pw.WriteAsciiFixed( "System\0", 30 );
            pw.Write( textBytes, 0, textBytes.Length );

            Engine.SendPacketToClient( pw );
        }

        public static void MobileQuery( int serial,
            MobileQueryType queryType = MobileQueryType.StatsRequest )
        {
            PacketWriter pw = new PacketWriter( 10 );
            pw.Write( (byte) 0x34 );
            pw.Write( 0xEDEDEDED );
            pw.Write( (byte) queryType );
            pw.Write( serial );
            Engine.SendPacketToServer( pw );
        }

        public static async Task<int> GetTargeSerialAsync( string message = "", int timeout = 5000 )
        {
            if ( string.IsNullOrEmpty( message ) )
            {
                message = Strings.Target_object___;
            }

            SystemMessage( message );

            Random random = new Random();

            return await Task.Run( () =>
            {
                int value = random.Next( 1, int.MaxValue );

                PacketWriter pw = new PacketWriter( 19 );
                pw.Write( (byte) 0x6C );
                pw.Write( (byte) 0 );
                pw.Write( value );
                pw.Write( (byte) 0 );
                pw.Fill();

                AutoResetEvent are = new AutoResetEvent( false );

                int serial = -1;

                PacketFilterInfo pfi = new PacketFilterInfo( 0x6C,
                    new[] { PacketFilterConditions.IntAtPositionCondition( value, 2 ) },
                    ( packet, info ) =>
                    {
                        serial = ( packet[7] << 24 ) | ( packet[8] << 16 ) | ( packet[9] << 8 ) | packet[10];

                        are.Set();
                    } );

                try
                {
                    Engine.AddSendFilter( pfi );

                    Engine.SendPacketToClient( pw );

                    bool result = are.WaitOne( timeout );

                    if ( result )
                    {
                        return serial;
                    }

                    pw = new PacketWriter( 19 );
                    pw.Write( (byte) 0x6C );
                    pw.Write( (byte) 0 );
                    pw.Write( value );
                    pw.Write( (byte) 3 );
                    pw.Fill();

                    Engine.SendPacketToClient( pw );

                    SystemMessage( Strings.Timeout___ );

                    return serial;
                }
                finally
                {
                    Engine.RemoveSendFilter( pfi );
                }
            } );
        }

        public static bool GumpButtonClick( int gumpID, int buttonID )
        {
            if ( !Engine.GumpList.TryGetValue( gumpID, out int serial ) )
            {
                return false;
            }

            PacketWriter pw = new PacketWriter( 23 );
            pw.Write( (byte) 0xB1 );
            pw.Write( (short) 23 );
            pw.Write( serial );
            pw.Write( gumpID );
            pw.Write( buttonID );
            pw.Fill();

            Engine.SendPacketToServer( pw );

            Engine.GumpList.TryRemove( gumpID, out _ );
            CloseClientGump( gumpID );

            return true;
        }

        public static void CloseClientGump( int gumpID )
        {
            byte[] packet = new byte[13];
            packet[0] = 0xBF;
            packet[2] = 0x0D;
            packet[4] = 0x04;
            packet[5] = (byte) ( gumpID >> 24 );
            packet[6] = (byte) ( gumpID >> 16 );
            packet[7] = (byte) ( gumpID >> 8 );
            packet[8] = (byte) gumpID;
            Engine.SendPacketToClient( packet, packet.Length );
        }

        public static bool WaitForGump( int gumpId, int timeout = 30000 )
        {
            PacketFilterInfo pfi = new PacketFilterInfo( 0xDD );

            if ( gumpId != -1 )
            {
                pfi = new PacketFilterInfo( 0xDD,
                    new[] { PacketFilterConditions.IntAtPositionCondition( gumpId, 7 ) } );
            }

            WaitEntry waitEntry = Engine.WaitEntries.AddWait( pfi, PacketDirection.Incoming, true );

            try
            {
                bool result = waitEntry.Lock.WaitOne( timeout );

                return result;
            }
            finally
            {
                Engine.WaitEntries.RemoveWait( waitEntry );
            }
        }

        public static void ChangeSkillLock( SkillEntry skill, LockStatus lockStatus )
        {
            byte[] packet = { 0x3A, 0x00, 0x06, 0x00, 0x00, 0x00 };
            packet[4] = (byte) skill.Skill.ID;
            packet[5] = (byte) lockStatus;

            Engine.SendPacketToServer( packet, packet.Length );
        }

        public static void UseSkill( Skill skill )
        {
            byte[] shortBaseSkillPacket = { 0x12, 0x00, 0x08, 0x24, 0x00, 0x20, 0x30, 0x00 };
            byte[] longBaseSkillPacket = { 0x12, 0x00, 0x09, 0x24, 0x00, 0x00, 0x20, 0x30, 0x00 };

            switch ( skill )
            {
                case Skill.Anatomy:
                    shortBaseSkillPacket[4] = 0x31;
                    Engine.SendPacketToServer( shortBaseSkillPacket, shortBaseSkillPacket.Length );
                    break;
                case Skill.Animal_Lore:
                    shortBaseSkillPacket[4] = 0x32;
                    Engine.SendPacketToServer( shortBaseSkillPacket, shortBaseSkillPacket.Length );
                    break;
                case Skill.Animal_Taming:
                    longBaseSkillPacket[4] = 0x33;
                    longBaseSkillPacket[5] = 0x35;
                    Engine.SendPacketToServer( longBaseSkillPacket, longBaseSkillPacket.Length );
                    break;
                case Skill.Arms_Lore:
                    shortBaseSkillPacket[4] = 0x34;
                    Engine.SendPacketToServer( shortBaseSkillPacket, shortBaseSkillPacket.Length );
                    break;
                case Skill.Begging:
                    shortBaseSkillPacket[4] = 0x36;
                    Engine.SendPacketToServer( shortBaseSkillPacket, shortBaseSkillPacket.Length );
                    break;
                case Skill.Cartography:
                    longBaseSkillPacket[4] = 0x31;
                    longBaseSkillPacket[5] = 0x32;
                    Engine.SendPacketToServer( longBaseSkillPacket, longBaseSkillPacket.Length );
                    break;
                case Skill.Detecting_Hidden:
                    longBaseSkillPacket[4] = 0x31;
                    longBaseSkillPacket[5] = 0x34;
                    Engine.SendPacketToServer( longBaseSkillPacket, longBaseSkillPacket.Length );
                    break;
                case Skill.Discordance:
                    longBaseSkillPacket[4] = 0x31;
                    longBaseSkillPacket[5] = 0x35;
                    Engine.SendPacketToServer( longBaseSkillPacket, longBaseSkillPacket.Length );
                    break;
                case Skill.Evaluating_Intelligence:
                    longBaseSkillPacket[4] = 0x31;
                    longBaseSkillPacket[5] = 0x36;
                    Engine.SendPacketToServer( longBaseSkillPacket, longBaseSkillPacket.Length );
                    break;
                case Skill.Forensic_Evaluation:
                    longBaseSkillPacket[4] = 0x31;
                    longBaseSkillPacket[5] = 0x39;
                    Engine.SendPacketToServer( longBaseSkillPacket, longBaseSkillPacket.Length );
                    break;
                case Skill.Hiding:
                    longBaseSkillPacket[4] = 0x32;
                    longBaseSkillPacket[5] = 0x31;
                    Engine.SendPacketToServer( longBaseSkillPacket, longBaseSkillPacket.Length );
                    break;
                case Skill.Inscription:
                    longBaseSkillPacket[4] = 0x32;
                    longBaseSkillPacket[5] = 0x33;
                    Engine.SendPacketToServer( longBaseSkillPacket, longBaseSkillPacket.Length );
                    break;
                case Skill.Item_Identification:
                    shortBaseSkillPacket[4] = 0x33;
                    Engine.SendPacketToServer( shortBaseSkillPacket, shortBaseSkillPacket.Length );
                    break;
                case Skill.Meditation:
                    longBaseSkillPacket[4] = 0x34;
                    longBaseSkillPacket[5] = 0x36;
                    Engine.SendPacketToServer( longBaseSkillPacket, longBaseSkillPacket.Length );
                    break;
                case Skill.Peacemaking:
                    shortBaseSkillPacket[4] = 0x39;
                    Engine.SendPacketToServer( shortBaseSkillPacket, shortBaseSkillPacket.Length );
                    break;
                case Skill.Poisoning:
                    longBaseSkillPacket[4] = 0x33;
                    longBaseSkillPacket[5] = 0x30;
                    Engine.SendPacketToServer( longBaseSkillPacket, longBaseSkillPacket.Length );
                    break;
                case Skill.Provocation:
                    longBaseSkillPacket[4] = 0x32;
                    longBaseSkillPacket[5] = 0x32;
                    Engine.SendPacketToServer( longBaseSkillPacket, longBaseSkillPacket.Length );
                    break;
                case Skill.Remove_Trap:
                    longBaseSkillPacket[4] = 0x34;
                    longBaseSkillPacket[5] = 0x38;
                    Engine.SendPacketToServer( longBaseSkillPacket, longBaseSkillPacket.Length );
                    break;
                case Skill.Spirit_Speak:
                    longBaseSkillPacket[4] = 0x33;
                    longBaseSkillPacket[5] = 0x32;
                    Engine.SendPacketToServer( longBaseSkillPacket, longBaseSkillPacket.Length );
                    break;
                case Skill.Stealing:
                    longBaseSkillPacket[4] = 0x33;
                    longBaseSkillPacket[5] = 0x33;
                    Engine.SendPacketToServer( longBaseSkillPacket, longBaseSkillPacket.Length );
                    break;
                case Skill.Stealth:
                    longBaseSkillPacket[4] = 0x34;
                    longBaseSkillPacket[5] = 0x37;
                    Engine.SendPacketToServer( longBaseSkillPacket, longBaseSkillPacket.Length );
                    break;
                case Skill.Taste_Identification:
                    longBaseSkillPacket[4] = 0x33;
                    longBaseSkillPacket[5] = 0x36;
                    Engine.SendPacketToServer( longBaseSkillPacket, longBaseSkillPacket.Length );
                    break;
                case Skill.Tracking:
                    longBaseSkillPacket[4] = 0x33;
                    longBaseSkillPacket[5] = 0x38;
                    Engine.SendPacketToServer( longBaseSkillPacket, longBaseSkillPacket.Length );
                    break;
                case Skill.Alchemy:
                    break;
                case Skill.Parrying:
                    break;
                case Skill.Blacksmithy:
                    break;
                case Skill.Bowcraft_Fletching:
                    break;
                case Skill.Camping:
                    break;
                case Skill.Carpentry:
                    break;
                case Skill.Cooking:
                    break;
                case Skill.Healing:
                    break;
                case Skill.Fishing:
                    break;
                case Skill.Herding:
                    break;
                case Skill.Lockpicking:
                    break;
                case Skill.Magery:
                    break;
                case Skill.Resisting_Spells:
                    break;
                case Skill.Tactics:
                    break;
                case Skill.Snooping:
                    break;
                case Skill.Musicianship:
                    break;
                case Skill.Archery:
                    break;
                case Skill.Tailoring:
                    break;
                case Skill.Tinkering:
                    break;
                case Skill.Veterinary:
                    break;
                case Skill.Swordsmanship:
                    break;
                case Skill.Mace_Fighting:
                    break;
                case Skill.Fencing:
                    break;
                case Skill.Wrestling:
                    break;
                case Skill.Lumberjacking:
                    break;
                case Skill.Mining:
                    break;
                case Skill.Necromancy:
                    break;
                case Skill.Focus:
                    break;
                case Skill.Chivalry:
                    break;
                case Skill.Bushido:
                    break;
                case Skill.Ninjitsu:
                    break;
                case Skill.Spellweaving:
                    break;
                default:
                    throw new ArgumentOutOfRangeException( nameof( skill ), skill, null );
            }
        }

        public static bool WaitForTarget( int timeout )
        {
            PacketFilterInfo pfi = new PacketFilterInfo( 0x6C );

            WaitEntry we = Engine.WaitEntries.AddWait( pfi, PacketDirection.Incoming );

            try
            {
                do
                {
                    bool result = we.Lock.WaitOne( timeout );

                    if ( !result )
                    {
                        return false;
                    }

                    if ( we.Packet[6] == 0x03 )
                    {
                        continue;
                    }

                    return true;
                }
                while ( true );
            }
            finally
            {
                Engine.WaitEntries.RemoveWait( we );
            }
        }

        public static void Resync()
        {
            byte[] packet = { 0x22, 0x00, 0x00 };

            Engine.SendPacketToServer( packet, packet.Length );
        }

        public static void ToggleGargoyleFlying()
        {
            PacketWriter pw = new PacketWriter( 11 );

            pw.Write( (byte) 0xBF );
            pw.Write( (short) 11 );
            pw.Write( (short) 0x32 );
            pw.Write( (short) 1 );
            pw.Write( 0 );

            Engine.SendPacketToServer( pw );
        }

        public static bool WaitForIncomingPacket( PacketFilterInfo pfi, int timeout, Action beforeWait )
        {
            WaitEntry we = Engine.WaitEntries.AddWait( pfi, PacketDirection.Incoming, true );

            bool result;

            beforeWait?.Invoke();

            try
            {
                result = we.Lock.WaitOne( timeout );
            }
            finally
            {
                Engine.WaitEntries.RemoveWait( we );
            }

            return result;
        }
    }
}