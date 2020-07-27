﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ClassicAssist.Shared;
using ClassicAssist.Data;
using ClassicAssist.Data.Skills;
using ClassicAssist.Data.Vendors;
using ClassicAssist.Misc;
using ClassicAssist.Shared.Resources;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Network;
using ClassicAssist.UO.Network.PacketFilter;
using ClassicAssist.UO.Network.Packets;
using ClassicAssist.UO.Objects;
using ClassicAssist.UO.Objects.Gumps;

namespace ClassicAssist.Shared.UO
{
    public class Commands
    {
        private static readonly object _dragDropLock = new object();

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

            lock ( _dragDropLock )
            {
                DragItem( serial, amount );

                Thread.Sleep( Options.CurrentOptions.ActionDelayMS );

                DropItem( serial, containerSerial, x, y, z );
            }

            await Task.CompletedTask;
        }

        public static Task EquipType( int id, Layer layer )
        {
            if ( id <= -1 )
            {
                SystemMessage( Strings.Invalid_type___ );
                return Task.CompletedTask;
            }

            int containerSerial = Engine.Player?.Serial ?? 0;

            if ( containerSerial == 0 || containerSerial == -1 )
            {
                return Task.CompletedTask;
            }

            Item backpack = Engine.Player?.Backpack;

            Item item = backpack?.Container?.SelectEntity( i => i.ID == id );

            if ( item == null )
            {
                return Task.CompletedTask;
            }

            if ( layer == Layer.Invalid )
            {
                StaticTile tileData = TileData.GetStaticTile( item.ID );
                layer = (Layer) tileData.Quality;
            }

            if ( layer == Layer.Invalid )
            {
                throw new ArgumentException( "EquipItem: Layer is invalid" );
            }

            return ActionPacketQueue.EnqueueActionPackets(
                new BasePacket[]
                {
                    new DragItem( item.Serial, 1 ), new EquipRequest( item.Serial, layer, containerSerial )
                }, QueuePriority.Medium );
        }

        public static Task EquipItem( Item item, Layer layer )
        {
            int containerSerial = Engine.Player?.Serial ?? 0;

            if ( containerSerial == 0 || containerSerial == -1 )
            {
                return Task.CompletedTask;
            }

            if ( layer == Layer.Invalid )
            {
                StaticTile tileData = TileData.GetStaticTile( item.ID );
                layer = (Layer) tileData.Quality;
            }

            if ( layer == Layer.Invalid )
            {
                throw new ArgumentException( "EquipItem: Layer is invalid" );
            }

            return ActionPacketQueue.EnqueueActionPackets(
                new BasePacket[]
                {
                    new DragItem( item.Serial, 1 ), new EquipRequest( item.Serial, layer, containerSerial )
                }, QueuePriority.Medium );
        }

        public static void SystemMessage( string text, int hue = 0x03b2 )
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
            pw.Write( (short) hue );
            pw.Write( (short) 0x03 );
            pw.WriteAsciiFixed( Strings.UO_LOCALE, 4 );
            pw.WriteAsciiFixed( "System\0", 30 );
            pw.Write( textBytes, 0, textBytes.Length );

            Engine.SendPacketToClient( pw );
        }

        public static void MobileQuery( int serial, MobileQueryType queryType = MobileQueryType.StatsRequest )
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
                bool wasTargetting = Engine.TargetExists;

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
                    new[] { PacketFilterConditions.UIntAtPositionCondition( value, 2 ) }, ( packet, info ) =>
                    {
                        serial = ( packet[7] << 24 ) | ( packet[8] << 16 ) | ( packet[9] << 8 ) | packet[10];

                        are.Set();
                    } );

                try
                {
                    Engine.AddSendPreFilter( pfi );

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
                    Engine.RemoveSendPreFilter( pfi );

                    if ( wasTargetting )
                    {
                        ResendTargetToClient();
                    }
                }
            } );
        }

        public static async Task<(TargetType, TargetFlags, int, int, int, int, int)> GetTargeInfoAsync(
            string message = "", int timeout = 30000 )
        {
            if ( string.IsNullOrEmpty( message ) )
            {
                message = Strings.Target_object___;
            }

            SystemMessage( message );

            Random random = new Random();

            return await Task.Run( () =>
            {
                bool wasTargetting = Engine.TargetExists;

                uint value = (uint) random.Next( 1, int.MaxValue );

                //TODO
                PacketWriter pw = new PacketWriter( 19 );
                pw.Write( (byte) 0x6C );
                pw.Write( (byte) 1 );
                pw.Write( value );
                pw.Write( (byte) 0 );
                pw.Fill();

                AutoResetEvent are = new AutoResetEvent( false );

                TargetType targetType = TargetType.Object;
                TargetFlags targetFlags = TargetFlags.None;
                int serial = 0;
                int x = 0;
                int y = 0;
                int z = 0;
                int itemID = 0;

                PacketFilterInfo pfi = new PacketFilterInfo( 0x6C,
                    new[] { PacketFilterConditions.UIntAtPositionCondition( value, 2 ) }, ( packet, info ) =>
                    {
                        targetType = (TargetType) packet[1];
                        targetFlags = (TargetFlags) packet[6];
                        serial = ( packet[7] << 24 ) | ( packet[8] << 16 ) | ( packet[9] << 8 ) | packet[10];
                        x = ( packet[11] << 8 ) | packet[12];
                        y = ( packet[13] << 8 ) | packet[14];
                        z = ( packet[15] << 8 ) | packet[16];
                        itemID = ( packet[17] << 8 ) | packet[18];

                        are.Set();
                    } );

                try
                {
                    Engine.AddSendPreFilter( pfi );

                    Engine.SendPacketToClient( pw );

                    bool result = are.WaitOne( timeout );

                    if ( result )
                    {
                        return ( targetType, targetFlags, serial, x, y, z, itemID );
                    }

                    Engine.SendPacketToClient( new CancelTargetCursor( value ) );

                    SystemMessage( Strings.Timeout___ );

                    return ( targetType, targetFlags, serial, x, y, z, itemID );
                }
                finally
                {
                    Engine.RemoveSendPreFilter( pfi );

                    if ( wasTargetting )
                    {
                        ResendTargetToClient();
                    }
                }
            } );
        }

        public static bool GumpButtonClick( uint gumpID, int buttonID, int[] switches = null )
        {
            if ( !Engine.GumpList.TryGetValue( gumpID, out int serial ) )
            {
                return false;
            }

            Engine.SendPacketToServer( new GumpButtonClick( (int) gumpID, serial, buttonID, switches ) );

            Engine.GumpList.TryRemove( gumpID, out _ );
            CloseClientGump( gumpID );

            return true;
        }

        public static void CloseClientGump( Type gumpType )
        {
            Engine.Gumps.GetGumps( out Gump[] gumps );

            IEnumerable<Gump> closeGumps = gumps.Where( t => t.GetType() == gumpType );

            foreach ( Gump closeGump in closeGumps )
            {
                CloseClientGump( closeGump.ID );
            }
        }

        public static void CloseClientGump( uint gumpID )
        {
            Engine.Gumps.Remove( gumpID );
            Engine.SendPacketToClient( new CloseClientGump( gumpID ) );
        }

        public static bool WaitForMenu( int gumpId, int timeout = 30000 )
        {
            PacketFilterInfo pfi = new PacketFilterInfo( 0x7C );

            if ( gumpId != 0 )
            {
                pfi = new PacketFilterInfo( 0x7C,
                    new[] { PacketFilterConditions.ShortAtPositionCondition( gumpId, 7 ) } );
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

        public static void ChangeStatLock( StatType stat, LockStatus lockStatus )
        {
            Engine.SendPacketToServer( new ChangeStatLock( stat, lockStatus ) );
        }

        public static (int, bool) WaitForTargetOrFizzle( int timeout )
        {
            PacketWaitEntry targetWe = CreateWaitEntry( new PacketFilterInfo( 0x6C ) );

            PacketWaitEntry fizzWe = CreateWaitEntry( new PacketFilterInfo( 0xC0,
                new[]
                {
                    PacketFilterConditions.IntAtPositionCondition( Engine.Player.Serial, 2 ),
                    PacketFilterConditions.ShortAtPositionCondition( 0x3735, 10 )
                } ) );

            PacketWaitEntry fizzMessageWe = CreateWaitEntry( new PacketFilterInfo( 0xC1,
                new[] { PacketFilterConditions.IntAtPositionCondition( 502632, 14 ) /* The spell fizzles. */ } ) );

            PacketWaitEntry recoveredMessageWe = CreateWaitEntry( new PacketFilterInfo( 0xC1,
                new[]
                {
                    PacketFilterConditions.IntAtPositionCondition( 502644,
                        14 ) /* You have not yet recovered from casting a spell. */
                } ) );

            PacketWaitEntry alreadyCastingWe = CreateWaitEntry( new PacketFilterInfo( 0xC1,
                new[]
                {
                    PacketFilterConditions.IntAtPositionCondition( 502642,
                        14 ) /* You are already casting a spell. */
                } ) );

            PacketWaitEntry alreadyCasting2We = CreateWaitEntry( new PacketFilterInfo( 0xC1,
                new[]
                {
                    PacketFilterConditions.IntAtPositionCondition( 502645,
                        14 ) /* You are already casting a spell. */
                } ) );

            PacketWaitEntry concentrationWe = CreateWaitEntry( new PacketFilterInfo( 0xC1,
                new[]
                {
                    PacketFilterConditions.IntAtPositionCondition( 500641,
                        14 ) /* Your concentration is disturbed, thus ruining thy spell. */
                } ) );

            PacketWaitEntry noManaWe = CreateWaitEntry( new PacketFilterInfo( 0xC1,
                new[]
                {
                    PacketFilterConditions.IntAtPositionCondition( 502625, 14 ) /* Insufficient mana etc... */
                } ) );

            PacketWaitEntry fizzChivWe = CreateWaitEntry( new PacketFilterInfo( 0x54,
                new[]
                {
                    PacketFilterConditions.ShortAtPositionCondition( 0x1D6, 2 ),
                    PacketFilterConditions.ShortAtPositionCondition( Engine.Player.X, 6 ),
                    PacketFilterConditions.ShortAtPositionCondition( Engine.Player.Y, 8 ),
                    PacketFilterConditions.ShortAtPositionCondition( Engine.Player.Z, 10 )
                } ) );

            Engine.WaitingForTarget = true;

            List<Task> tasks = new List<Task>();

            try
            {
                Task<bool> targetTask = Task.Run( () =>
                {
                    do
                    {
                        bool result = targetWe.Lock.WaitOne( timeout );

                        if ( !result )
                        {
                            return false;
                        }

                        if ( targetWe.Packet[6] == 0x03 )
                        {
                            continue;
                        }

                        return true;
                    }
                    while ( true );
                } );

                Task fizzTask = Task.Factory.StartNew( () => fizzWe.Lock.WaitOne( timeout ),
                    TaskCreationOptions.LongRunning );

                Task fizzMessageTask = Task.Factory.StartNew( () => fizzMessageWe.Lock.WaitOne( timeout + 100 ),
                    TaskCreationOptions.LongRunning );

                Task recoveredMessageTask =
                    Task.Factory.StartNew( () => recoveredMessageWe.Lock.WaitOne( timeout + 100 ),
                        TaskCreationOptions.LongRunning );

                Task alreadyCastingTask = Task.Factory.StartNew( () => alreadyCastingWe.Lock.WaitOne( timeout + 100 ),
                    TaskCreationOptions.LongRunning );

                Task alreadyCasting2Task = Task.Factory.StartNew( () => alreadyCasting2We.Lock.WaitOne( timeout + 100 ),
                    TaskCreationOptions.LongRunning );

                Task concentrationTask = Task.Factory.StartNew( () => concentrationWe.Lock.WaitOne( timeout + 100 ),
                    TaskCreationOptions.LongRunning );

                Task noManaTask = Task.Factory.StartNew( () => noManaWe.Lock.WaitOne( timeout + 100 ),
                    TaskCreationOptions.LongRunning );

                Task fizzChivTask = Task.Factory.StartNew( () => fizzChivWe.Lock.WaitOne( timeout + 100 ),
                    TaskCreationOptions.LongRunning );

                int index;

                tasks.AddRange( new[]
                {
                    targetTask, fizzTask, fizzMessageTask, recoveredMessageTask, alreadyCastingTask,
                    alreadyCasting2Task, concentrationTask, noManaTask, fizzChivTask
                } );

                try
                {
                    index = Task.WaitAny( tasks.ToArray() );
                }
                catch ( OperationCanceledException )
                {
                    return ( -1, false );
                }
                catch ( ThreadInterruptedException )
                {
                    return ( -1, false );
                }

                return ( index, index == 0 && targetTask.Result );
            }
            finally
            {
                Engine.PacketWaitEntries.Remove( targetWe );
                Engine.PacketWaitEntries.Remove( fizzWe );
                Engine.PacketWaitEntries.Remove( fizzMessageWe );
                Engine.PacketWaitEntries.Remove( recoveredMessageWe );
                Engine.PacketWaitEntries.Remove( alreadyCastingWe );
                Engine.PacketWaitEntries.Remove( alreadyCasting2We );
                Engine.PacketWaitEntries.Remove( concentrationWe );
                Engine.PacketWaitEntries.Remove( noManaWe );
                Engine.PacketWaitEntries.Remove( fizzChivWe );

                Engine.WaitingForTarget = false;
            }
        }

        private static PacketWaitEntry CreateWaitEntry( PacketFilterInfo packetFilterInfo,
            PacketDirection direction = PacketDirection.Incoming )
        {
            return Engine.PacketWaitEntries.Add( packetFilterInfo, direction );
        }

        public static bool WaitForTarget( int timeout )
        {
            PacketFilterInfo pfi = new PacketFilterInfo( 0x6C );

            Engine.WaitingForTarget = true;

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

                Engine.WaitingForTarget = false;
            }
        }

        public static async Task<bool> WaitForIncomingPacketFilterAsync( PacketFilterInfo pfi, int timeout,
            bool invokeHandler = true, bool fixedSize = false )
        {
            AutoResetEvent are = new AutoResetEvent( false );
            byte[] packet = null;

            Engine.AddReceiveFilter( new PacketFilterInfo( pfi.PacketID, pfi.GetConditions(), ( data, info ) =>
            {
                packet = data;
                are.Set();
            } ) );

            Task filterTask = are.ToTask();

            if ( await Task.WhenAny( filterTask, Task.Delay( timeout ) ) == filterTask )
            {
                if ( invokeHandler )
                {
                    PacketHandler handler = IncomingPacketHandlers.GetHandler( packet[0] );

                    handler?.OnReceive( new PacketReader( packet, packet.Length, fixedSize ) );
                }
            }

            Engine.RemoveReceiveFilter( pfi );

            return packet != null;
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
            pw.WriteAsciiFixed( entity.Name ?? "Unknown", 30 );
            pw.Write( textBytes, 0, textBytes.Length );

            Engine.SendPacketToClient( pw );
        }

        public static void ResendTargetToClient()
        {
            PacketWriter pw = new PacketWriter( 19 );
            pw.Write( (byte) 0x6C );
            pw.Write( (byte) TargetType.Object );
            pw.Write( Engine.TargetSerial );
            pw.Write( (byte) Engine.TargetFlags );
            pw.Write( 0 );
            pw.Write( (short) 0 );
            pw.Write( (short) 0 );
            pw.Write( (short) 0 );
            pw.Write( (short) 0 );

            Engine.TargetExists = true;
            Engine.SendPacketToClient( pw );
        }

        public static void VendorBuy( int serial, ShopListEntry[] shopListEntries )
        {
            int len = 8 + shopListEntries.Length * 7;
            PacketWriter pw = new PacketWriter( len );
            pw.Write( (byte) 0x3B );
            pw.Write( (short) len ); // length
            pw.Write( serial );
            pw.Write( (byte) 2 ); //item list

            foreach ( ShopListEntry entry in shopListEntries )
            {
                if ( entry == null )
                {
                    continue;
                }

                pw.Write( (byte) Layer.ShopBuy );
                pw.Write( entry.Item.Serial );
                pw.Write( (short) entry.Amount );
            }

            Engine.SendPacketToServer( pw );
        }

        public static void SetForceWalk( bool force )
        {
            PacketWriter pw = new PacketWriter( 6 );

            pw.Write( (byte) 0xBF );
            pw.Write( (short) 0x06 );
            pw.Write( (short) 0x26 );
            pw.Write( (byte) ( force ? 2 : 0 ) );

            Engine.SendPacketToClient( pw );
        }

        public static void UO3DEquipItems( int[] serials )
        {
            if ( serials == null || serials.Length == 0 )
            {
                return;
            }

            int len = 4 + serials.Length * 4;

            PacketWriter pw = new PacketWriter( len );

            pw.Write( (byte) 0xEC );
            pw.Write( (short) len ); //size
            pw.Write( (byte) serials.Length );

            foreach ( int serial in serials )
            {
                pw.Write( serial );
            }

            Engine.SendPacketToServer( pw );
        }

        public static void UO3DUnequipItems( int[] layers )
        {
            if ( layers == null || layers.Length == 0 )
            {
                return;
            }

            PacketWriter pw = new PacketWriter( 4 + layers.Length * 2 );

            pw.Write( (byte) 0xED );
            pw.Write( (short) ( 4 + layers.Length * 2 ) );
            pw.Write( (byte) layers.Length );

            foreach ( int layer in layers )
            {
                pw.Write( (short) layer );
            }

            Engine.SendPacketToServer( pw );
        }

        public static void ChatMsg( string text )
        {
            PacketWriter pw = new PacketWriter( 11 + text.Length * 2 );

            pw.Write( (byte) 0xB3 );
            pw.Write( (short) ( 11 + text.Length * 2 ) );
            pw.WriteAsciiFixed( Strings.UO_LOCALE, 4 );
            pw.Write( (short) 0x61 );
            pw.WriteBigUniNull( text );

            Engine.SendPacketToServer( pw );
        }

        public static void JoinChatChannel( string channel )
        {
            /*
             * Text 0x62 (Join Conference):
                BYTE[2] Holder (0x0022)
                BYTE[?] Unicode conference name
                BYTE[2] Holder (0x0022)
                BYTE[2] Holder (0x0020)
                BYTE[?] Unicode password if pass req'd
                BYTE[2] Null Teriminator (0x0000)
             */

            PacketWriter pw = new PacketWriter();
            pw.Write( (byte) 0xB3 );
            pw.Write( (short) ( 17 + channel.Length * 2 ) );
            pw.WriteAsciiFixed( Strings.UO_LOCALE, 4 );
            pw.Write( (short) 0x62 );
            pw.WriteBigUniNull( channel );
            pw.Write( (short) 0x22 );
            pw.Write( (short) 0x20 );
            pw.Write( (short) 0 );

            Engine.SendPacketToServer( pw );
        }

        public static void RemoveObject( int serial )
        {
            Engine.SendPacketToClient( new RemoveObject( serial ) );
        }
    }
}