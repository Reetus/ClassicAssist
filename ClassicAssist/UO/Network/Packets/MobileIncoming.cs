using System;
using System.IO;
using Assistant;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Objects;

namespace ClassicAssist.UO.Network.Packets
{
    public class MobileIncoming : Packets
    {
        //public MobileIncoming( Mobile player )
        //{
        //    int length = 23 + player.Equipment.GetItemCount() * 9;
        //    _writer = new PacketWriter( length );
        //    _writer.Write( (byte) 0x78 );
        //    _writer.Write( (short) length );
        //    _writer.Write( player.Serial );
        //    _writer.Write( (short) player.ID );
        //    _writer.Write( (short) player.X );
        //    _writer.Write( (short) player.Y );
        //    _writer.Write( (sbyte) player.Z );
        //    _writer.Write( (byte) player.Direction );
        //    _writer.Write( (short) player.Hue );
        //    _writer.Write( (byte) player.Status );
        //    _writer.Write( (byte) player.Notoriety );

        //    foreach ( Item item in player.Equipment.GetItems() )
        //    {
        //        _writer.Write( item.Serial );
        //        _writer.Write( (short) item.ID );
        //        _writer.Write( (byte) item.Layer );
        //        _writer.Write( (short) item.Hue );
        //    }

        //    _writer.Write( 0 );
        //}
    }

    public class MobileDeadIncoming : Packets
    {
        public MobileDeadIncoming( Mobile player )
        {
            bool useNewIncoming = Engine.ClientVersion >= new Version( 7, 0, 33, 1 );

            int length = 23 + player.Equipment.GetItemCount() * 9;
            _writer = new PacketWriter();
            _writer.Write( (byte) 0x78 );
            _writer.Write( (short) length );
            _writer.Write( player.Serial );
            int bodyValue = player.ID == 0x191 ? 0x193 : 0x192;
            _writer.Write( (short) bodyValue );
            _writer.Write( (short) player.X );
            _writer.Write( (short) player.Y );
            _writer.Write( (sbyte) player.Z );
            _writer.Write( (byte) player.Direction );
            _writer.Write( (short) player.Hue );
            _writer.Write( (byte) player.Status );
            _writer.Write( (byte) player.Notoriety );

            foreach ( Item item in player.Equipment.GetItems() )
            {
                _writer.Write( item.Serial );

                if ( useNewIncoming )
                {
                    _writer.Write( (short) item.ID );
                    _writer.Write( (byte) item.Layer );
                    _writer.Write( (short) item.Hue );
                }
                else
                {
                    if ( item.Hue > 0 )
                    {
                        int id = item.ID ^ 0x8000;
                        _writer.Write( (short) id );
                        _writer.Write( (byte) item.Layer );
                        _writer.Write( (short) item.Hue );
                    }
                    else
                    {
                        _writer.Write( (short) item.ID );
                        _writer.Write( (byte) item.Layer );
                    }
                }
            }

            _writer.Write( 0 );

            _writer.Seek( 2, SeekOrigin.Begin );
            _writer.Write( (short) _writer.Length );
            _writer.Seek( 0, SeekOrigin.End );
        }
    }
}