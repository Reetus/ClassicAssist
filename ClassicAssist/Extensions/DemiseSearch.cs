#region License

// Copyright (C) 2020 Reetus
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

#endregion

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using Assistant;
using ClassicAssist.Data;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Network;
using ClassicAssist.UO.Network.PacketFilter;
using ClassicAssist.UO.Objects.Gumps;
using Newtonsoft.Json;

namespace ClassicAssist.Extensions
{
    public class DemiseSearchExtension : IExtension
    {
        private PacketFilterInfo _pfi;

        public void Initialize()
        {
            OutgoingPacketHandlers.ShardChangedEvent += OnShardChanged;
            Engine.DisconnectedEvent += OnDisconnected;
        }

        private void OnDisconnected()
        {
            if ( _pfi != null )
            {
                Engine.RemoveSendPreFilter( _pfi );
            }
        }

        private void OnShardChanged( string name )
        {
            if ( !name.Equals( "Demise - AOS" ) )
            {
                return;
            }

            _pfi = new PacketFilterInfo( 0xFA, OnStoreClick );
            Engine.AddSendPreFilter( _pfi );
        }

        private static void OnStoreClick( byte[] arg1, PacketFilterInfo arg2 )
        {
            DemiseSearchGump gump = new DemiseSearchGump();

            gump.SendGump();
        }
    }

    public class DemiseSearchGump : Gump
    {
        private const int SEARCH_BUTTON = 1;
        private const int PREVIOUS_BUTTON = 2;
        private const int NEXT_BUTTON = 3;
        private const int SEARCH_ENTRY_ID = 4;
        private const int PER_PAGE = 6;
        private const string API_URL = "https://dsearchapi.azurewebsites.net/api/Items";
        private readonly string _message;
        private readonly PacketFilterInfo _pfi;
        private readonly IReadOnlyCollection<VendorItem> _results;
        private readonly string _searchTerm;

        public DemiseSearchGump( string message = "", string searchTerm = "",
            IReadOnlyCollection<VendorItem> results = null ) : base( 100, 100 )
        {
            _results = results;
            _message = message;
            _searchTerm = searchTerm;

            Movable = true;
            Closable = true;
            Resizable = false;
            Disposable = false;

            AddPage( 0 );
            AddBackground( 100, 0, 600, 600, 30536 );
            AddLabel( 140, 45, 2100, "Search Term:" );
            AddBackground( 220, 40, 339, 30, 3000 );
            AddTextEntry( 225, 45, 340, 30, 0, SEARCH_ENTRY_ID, searchTerm );
            AddButton( 620, 45, 4005, 4006, SEARCH_BUTTON, GumpButtonType.Reply, 0 );

            AddLabel( 140, 80, 2100, message );

            _pfi = new PacketFilterInfo( 0xB1, new[] { PacketFilterConditions.UIntAtPositionCondition( ID, 7 ) } );

            Engine.AddSendPostFilter( _pfi );

            if ( _results == null )
            {
                return;
            }

            // ReSharper disable once PossibleLossOfFraction
            int pages = (int) Math.Ceiling( (double) ( _results.Count / PER_PAGE ) ) + 1;

            for ( int i = 1; i < pages + 1; i++ )
            {
                AddPage( i );

                VendorItem[] items = _results.Skip( ( i - 1 ) * PER_PAGE ).Take( PER_PAGE ).ToArray();

                int startY = 110;

                foreach ( VendorItem item in items )
                {
                    SendProperties( item );
                    Bitmap art = Art.GetStatic( item.ItemID );

                    AddImageTiledButton( 140, startY, 2328, 2328, 0, GumpButtonType.Page, i, item.ItemID, item.Hue,
                        40 - art.Width / 2, 30 - art.Height / 2 );
                    AddItemProperty( item.Serial );
                    AddLabel( 240, startY, 2100, $"Name: {item.Name}" );
                    AddLabel( 240, startY + 20, 2100, $"Shop Name: {item.ShopName}" );
                    AddLabel( 240, startY + 40, 2100, $"Price: {item.Price}" );
                    AddButton( 620, startY + 20, 4005, 4006, item.Serial, GumpButtonType.Reply, 0 );
                    AddLabel( 540, startY + 20, 2100, $"{(Map) item.Map}" );
                    AddHtml( 140, 520, 460, 20,
                        $"<A HREF=\"https://demisesearch.azurewebsites.net/search/{Uri.EscapeDataString( searchTerm )}\">Open results on website</A>",
                        false, false );
                    startY += 70;
                }

                if ( i > 1 )
                {
                    AddButton( 140, 540, 5603, 5607, PREVIOUS_BUTTON, GumpButtonType.Page, i - 1 );
                }

                if ( i < pages )
                {
                    AddButton( 640, 540, 5601, 5605, NEXT_BUTTON, GumpButtonType.Page, i + 1 );
                }
            }
        }

        public override void OnResponse( int buttonID, int[] switches,
            List<(int Key, string Value)> textEntries = null )
        {
            if ( buttonID == SEARCH_BUTTON )
            {
                string searchText = textEntries?.FirstOrDefault( i => i.Key == SEARCH_ENTRY_ID ).Value ?? "";

                HttpClient httpClient = new HttpClient();

                httpClient.GetAsync( $"{API_URL}/searchFull/{Uri.EscapeDataString( searchText )}" ).ContinueWith(
                    async t =>
                    {
                        try
                        {
                            string response = await t.Result.Content.ReadAsStringAsync();

                            if ( !t.Result.IsSuccessStatusCode )
                            {
                                throw new Exception( "An error occurred." );
                            }

                            VendorItem[] results = JsonConvert.DeserializeObject<VendorItem[]>( response );

                            if ( results?.Length == 0 )
                            {
                                DemiseSearchGump gump = new DemiseSearchGump( "No results found for search." );
                                gump.SendGump();
                            }
                            else
                            {
                                DemiseSearchGump gump =
                                    new DemiseSearchGump( $"{results?.Length} result(s) for search term.", searchText,
                                        results );
                                gump.SendGump();
                            }
                        }
                        catch ( Exception e )
                        {
                            DemiseSearchGump gump = new DemiseSearchGump( e.Message );
                            gump.SendGump();
                        }
                    } );
            }
            else if ( buttonID >= 0x40000000 )
            {
                DemiseSearchGump gump = new DemiseSearchGump( _message, _searchTerm, _results );
                gump.SendGump();

                VendorItem item = _results.FirstOrDefault( i => i.Serial == buttonID );

                if ( item == null )
                {
                    return;
                }

                SendMap( item.Serial, item.X, item.Y, item.Map );
            }

            Engine.RemoveSendPostFilter( _pfi );
        }

        private static void SendProperties( VendorItem item )
        {
            PacketWriter writer = new PacketWriter();
            writer.Write( (byte) 0xD6 );
            writer.Write( (short) 0 ); //len
            writer.Write( (short) 0x01 );
            writer.Write( item.Serial );
            writer.Write( (short) 0x00 );
            writer.Write( item.Serial );

            foreach ( VendorItemProperties properties in item.Properties )
            {
                writer.Write( properties.Cliloc );

                if ( properties.Arguments == null )
                {
                    writer.Write( (short) ( properties.Arguments?.Length ?? 0 ) );
                }
                else
                {
                    string arguments = string.Join( "\t", properties.Arguments ) + '\0';
                    byte[] argumentBytes = Encoding.Unicode.GetBytes( arguments );
                    writer.Write( (short) argumentBytes.Length );
                    writer.Write( argumentBytes, 0, argumentBytes.Length );
                }
            }

            writer.Seek( 1, SeekOrigin.Begin );
            writer.Write( (short) writer.Length );
            writer.Seek( 0, SeekOrigin.End );

            Engine.SendPacketToClient( writer );
        }

        public static void SendMap( int serial, int x, int y, int map )
        {
            Rectangle rect = new Rectangle( x - 150, y - 150, 300, 300 );

            PacketWriter writer = new PacketWriter( 21 );
            writer.Write( (byte) 0xF5 );
            writer.Write( serial );
            writer.Write( (short) 0x139d );
            writer.Write( (short) rect.Left );
            writer.Write( (short) rect.Top );
            writer.Write( (short) rect.Right );
            writer.Write( (short) rect.Bottom );
            writer.Write( (short) 600 );
            writer.Write( (short) 600 );
            writer.Write( (short) map );

            Engine.SendPacketToClient( writer );

            PacketWriter writer2 = new PacketWriter( 11 );
            writer2.Write( (byte) 0x56 );
            writer2.Write( serial );
            writer2.Write( (byte) 1 );
            writer2.Write( (byte) 0 );
            writer2.Write( (short) 300 );
            writer2.Write( (short) 300 );

            Engine.SendPacketToClient( writer2 );
        }
    }

    public class VendorItem
    {
        public string Description { get; set; } = string.Empty;
        public int Hue { get; set; }
        public string Id { get; set; }
        public int ItemID { get; set; }
        public DateTime LastSeen { get; set; }
        public int Map { get; set; }
        public string Name { get; set; }
        public string ObjectID { get; set; }
        public int Price { get; set; }
        public VendorItemProperties[] Properties { get; set; }
        public int Score { get; set; }
        public int Serial { get; set; }
        public string ShopName { get; set; }
        public string TileFlags { get; set; }
        public string VendorName { get; set; }
        public string VendorObjectID { get; set; }
        public int VendorSerial { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
    }

    public class VendorItemProperties
    {
        public string[] Arguments { get; set; }
        public int Cliloc { get; set; }
        public string Text { get; set; }
    }
}