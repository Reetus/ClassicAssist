using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Assistant;
using ClassicAssist.Data;
using ClassicAssist.Data.Skills;
using ClassicAssist.Resources;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Network.PacketFilter;
using ClassicAssist.UO.Network.Packets;
using ClassicAssist.UO.Objects;
using Skill = ClassicAssist.UO.Data.Skill;

namespace ClassicAssist.UO
{
    public class Commands
    {
        public static void DragItem( int serial, int amount )
        {
            Engine.SendPacketToServer( new DragItem( serial, amount ) );
        }

        public static void DropItem( int serial, int containerSerial, int x = -1, int y = -1, int z = 0 )
        {
            Engine.SendPacketToServer( new DropItem( serial, containerSerial, x, y, z ) );
        }

        public static async Task DragDropAsync( int serial, int amount, int containerSerial, int x = -1, int y = -1,
            int z = 0 )
        {
            if ( amount == -1 )
            {
                Item i = Engine.Items.GetItem( serial );

                amount = i?.Count ?? 1;
            }

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

            DragItem( item.Serial, 1 );

            Engine.SendPacketToServer( new EquipRequest( item.Serial, layer, containerSerial ) );
        }

        public static void SystemMessage( string text )
        {
            //TODO
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
            Engine.SendPacketToServer( new MobileQuery( serial, queryType ) );
        }

        public static async Task<int> GetTargeSerialAsync( string message = "", int timeout = 30000 )
        {
            if ( string.IsNullOrEmpty( message ) )
            {
                message = Strings.Target_object___;
            }

            SystemMessage( message );

            Random random = new Random();

            return await Task.Run( () =>
            {
                uint value = (uint) random.Next( 1, int.MaxValue );

                //TODO
                PacketWriter pw = new PacketWriter( 19 );
                pw.Write( (byte) 0x6C );
                pw.Write( (byte) 0 );
                pw.Write( value );
                pw.Write( (byte) 0 );
                pw.Fill();

                AutoResetEvent are = new AutoResetEvent( false );

                int serial = -1;

                PacketFilterInfo pfi = new PacketFilterInfo( 0x6C,
                    new[] { PacketFilterConditions.UIntAtPositionCondition( value, 2 ) },
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

                    Engine.SendPacketToClient( new CancelTargetCursor( value ) );

                    SystemMessage( Strings.Timeout___ );

                    return serial;
                }
                finally
                {
                    Engine.RemoveSendFilter( pfi );
                }
            } );
        }

        public static bool GumpButtonClick( uint gumpID, int buttonID )
        {
            if ( !Engine.GumpList.TryGetValue( (int) gumpID, out int serial ) )
            {
                return false;
            }

            Engine.SendPacketToServer( new GumpButtonClick( (int) gumpID, serial, buttonID ) );

            Engine.GumpList.TryRemove( (int) gumpID, out _ );
            CloseClientGump( (int) gumpID );

            return true;
        }

        public static void CloseClientGump( int gumpID )
        {
            Engine.Gumps.Remove( gumpID );
            Engine.SendPacketToClient( new CloseClientGump( gumpID ) );
        }

        public static bool WaitForGump( uint gumpId, int timeout = 30000 )
        {
            PacketFilterInfo pfi = new PacketFilterInfo( 0xDD );

            if ( gumpId != 0 )
            {
                pfi = new PacketFilterInfo( 0xDD,
                    new[] { PacketFilterConditions.UIntAtPositionCondition( gumpId, 7 ) } );
            }

            PacketWaitEntry packetWaitEntry = Engine.PacketWaitEntries.Add( pfi, PacketDirection.Incoming, true );

            try
            {
                bool result = packetWaitEntry.Lock.WaitOne( timeout );

                return result;
            }
            finally
            {
                Engine.PacketWaitEntries.Remove( packetWaitEntry );
            }
        }

        public static void ChangeSkillLock( SkillEntry skill, LockStatus lockStatus, bool requery = true )
        {
            Engine.SendPacketToServer( new ChangeSkillLock( skill, lockStatus ) );

            if ( requery )
            {
                Engine.SendPacketToServer(
                    new MobileQuery( Engine.Player?.Serial ?? 0, MobileQueryType.SkillsRequest ) );
            }
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

            PacketWaitEntry we = Engine.PacketWaitEntries.Add( pfi, PacketDirection.Incoming );

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
                Engine.PacketWaitEntries.Remove( we );
            }
        }

        public static void Resync()
        {
            Engine.SendPacketToServer( new Resync() );
        }

        public static void ToggleGargoyleFlying()
        {
            Engine.SendPacketToServer( new ToggleGargoyleFlying() );
        }

        public static bool WaitForIncomingPacket( PacketFilterInfo pfi, int timeout, Action beforeWait = null )
        {
            PacketWaitEntry we = Engine.PacketWaitEntries.Add( pfi, PacketDirection.Incoming, true );

            bool result;

            beforeWait?.Invoke();

            try
            {
                result = we.Lock.WaitOne( timeout );
            }
            finally
            {
                Engine.PacketWaitEntries.Remove( we );
            }

            return result;
        }

        public static void SetWeaponAbility( int abilityIndex )
        {
            Engine.SendPacketToServer( new SetWeaponAbility( abilityIndex ) );
        }

        public static void ClearWeaponAbility()
        {
            Engine.SendPacketToServer( new ClearWeaponAbility() );
        }

        public static void RenameRequest( int serial, string name )
        {
            Engine.SendPacketToServer( new RenameRequest( serial, name ) );
        }

        public static bool WaitForContainerContents( int serial, int timeout )
        {
            PacketFilterInfo pfi = new PacketFilterInfo( 0x3C,
                new[] { PacketFilterConditions.IntAtPositionCondition( serial, 19 ) } );

            PacketWaitEntry we = Engine.PacketWaitEntries.Add( pfi, PacketDirection.Incoming, true );

            Engine.SendPacketToServer( new UseObject( serial ) );

            try
            {
                bool result = we.Lock.WaitOne( timeout );

                return result;
            }
            finally
            {
                Engine.PacketWaitEntries.Remove( we );
            }
        }

        private static byte[] GetKeywordBytes( IReadOnlyList<int> keywords )
        {
            int length = keywords.Count;

            List<byte> codeBytes = new List<byte> { (byte) ( length >> 4 ) };

            int num3 = length & 15;
            bool flag = false;
            int index = 0;

            while ( index < length )
            {
                int keywordID = keywords[index];

                if ( flag )
                {
                    codeBytes.Add( (byte) ( keywordID >> 4 ) );
                    num3 = keywordID & 15;
                }
                else
                {
                    codeBytes.Add( (byte) ( ( num3 << 4 ) | ( ( keywordID >> 8 ) & 15 ) ) );
                    codeBytes.Add( (byte) keywordID );
                }

                index++;
                flag = !flag;
            }

            if ( !flag )
            {
                codeBytes.Add( (byte) ( num3 << 4 ) );
            }

            return codeBytes.ToArray();
        }

        public static void Speak( string text, int hue = 34, JournalSpeech speechType = JournalSpeech.Say )
        {
            int[] keywords = new int[0];

            if ( speechType == JournalSpeech.Say )
            {
                keywords = Speech.GetKeywords( text.ToLower() );
            }

            PacketWriter writer = new PacketWriter();
            writer.Write( (byte) 0xAD );
            writer.Write( (short) 0 ); // len;
            writer.Write( keywords.Length > 0 ? (byte) 0xC0 : (byte) speechType );
            writer.Write( (short) hue );
            writer.Write( (short) 0x3 );
            writer.WriteAsciiFixed( Strings.UO_LOCALE, 4 );

            if ( keywords.Length > 0 )
            {
                byte[] bytes = GetKeywordBytes( keywords );

                writer.Write( bytes, 0, bytes.Length );
                writer.WriteAsciiNull( text );
            }
            else
            {
                writer.WriteBigUniNull( text );
            }

            byte[] packet = writer.ToArray();

            packet[1] = (byte) ( packet.Length << 8 );
            packet[2] = (byte) packet.Length;

            Engine.SendPacketToServer( packet, packet.Length );
        }

        public static void PartyMessage( string message )
        {
            int len = 6 + ( message.Length + 1 ) * 2;
            PacketWriter writer = new PacketWriter( len );
            writer.Write( (byte) 0xBF );
            writer.Write( (short) len );
            writer.Write( (short) 6 );
            writer.Write( (byte) 4 );
            writer.WriteBigUniNull( message );

            Engine.SendPacketToServer( writer );
        }

        public static void OverheadMessage( string message, int hue, int serial )
        {
            byte[] textBytes = Encoding.BigEndianUnicode.GetBytes( message + '\0' );

            int length = 48 + textBytes.Length;

            Entity entity = (Entity) Engine.Mobiles.GetMobile( serial ) ?? Engine.Items.GetItem( serial );

            if ( entity == null )
            {
                return;
            }

            PacketWriter pw = new PacketWriter( length );
            pw.Write( (byte) 0xAE );
            pw.Write( (short) length );
            pw.Write( entity.Serial );
            pw.Write( (ushort) entity.ID );
            pw.Write( (byte) JournalSpeech.Say );
            pw.Write( (short) hue );
            pw.Write( (short) 0x03 );
            pw.WriteAsciiFixed( Strings.UO_LOCALE, 4 );
            pw.WriteAsciiFixed( entity.Name, 30 );
            pw.Write( textBytes, 0, textBytes.Length );

            Engine.SendPacketToClient( pw );
        }
    }
}