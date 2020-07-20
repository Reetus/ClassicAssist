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

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ClassicAssist.Shared;
using ClassicAssist.Shared.Resources;
using ClassicAssist.UI.ViewModels;
using ClassicAssist.UI.Views;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Objects;

namespace ClassicAssist.UO
{
    public class Commands
    {
        public static async Task InspectObjectAsync()
        {
            ( TargetType targetType, TargetFlags _, int serial, int x, int y, int z, int itemID ) =
                await Shared.UO.Commands.GetTargeInfoAsync( Strings.Target_object___ );

            if ( targetType == TargetType.Object && serial != 0 )
            {
                Entity entity = UOMath.IsMobile( serial )
                    ? (Entity) Engine.Mobiles.GetMobile( serial )
                    : Engine.Items.GetItem( serial );

                if ( entity == null )
                {
                    return;
                }

                Thread t = new Thread( () =>
                {
                    ObjectInspectorWindow window =
                        new ObjectInspectorWindow { DataContext = new ObjectInspectorViewModel( entity ) };

                    window.ShowDialog();
                } ) { IsBackground = true };

                t.SetApartmentState( ApartmentState.STA );
                t.Start();
            }
            else
            {
                if ( itemID == 0 )
                {
                    if ( x == 65535 && y == 65535 )
                    {
                        return;
                    }

                    LandTile landTile = MapInfo.GetLandTile( (int) Engine.Player.Map, x, y );
                    Thread t = new Thread( () =>
                    {
                        ObjectInspectorWindow window = new ObjectInspectorWindow
                        {
                            DataContext = new ObjectInspectorViewModel( landTile )
                        };

                        window.ShowDialog();
                    } ) { IsBackground = true };

                    t.SetApartmentState( ApartmentState.STA );
                    t.Start();
                }
                else
                {
                    StaticTile[] statics = Statics.GetStatics( (int) Engine.Player.Map, x, y );

                    if ( statics == null )
                    {
                        return;
                    }

                    StaticTile selectedStatic = statics.FirstOrDefault( i => i.ID == itemID );

                    if ( selectedStatic.ID == 0 )
                    {
                        selectedStatic = TileData.GetStaticTile( itemID );
                        selectedStatic.X = x;
                        selectedStatic.Y = y;
                        selectedStatic.Z = z;
                    }

                    Thread t = new Thread( () =>
                    {
                        ObjectInspectorWindow window = new ObjectInspectorWindow
                        {
                            DataContext = new ObjectInspectorViewModel( selectedStatic )
                        };

                        window.ShowDialog();
                    } ) { IsBackground = true };

                    t.SetApartmentState( ApartmentState.STA );
                    t.Start();
                }
            }
        }
    }
}