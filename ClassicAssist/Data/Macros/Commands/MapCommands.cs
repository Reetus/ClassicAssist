#region License

// Copyright (C) 2023 Reetus
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

#endregion

using Assistant;
using ClassicAssist.Plugin.Shared.Reflection;
using ClassicAssist.Plugin.Shared.Reflection.ClassicUO.Objects;
using ClassicAssist.Shared.Resources;

namespace ClassicAssist.Data.Macros.Commands
{
    public static class MapCommands
    {
        [CommandsDisplay( Category = nameof( Strings.World_Map ),
            Parameters = new[]
            {
                nameof( ParameterType.Name ), nameof( ParameterType.XCoordinate ),
                nameof( ParameterType.YCoordinate ), nameof( ParameterType.IntegerValue ),
                nameof( ParameterType.IntegerValue ), nameof( ParameterType.String )
            } )]
        public static void AddMapMarker( string name, int x, int y, int facet, int zoomLevel = 3,
            string iconName = "bank" )
        {
            ReflectionCommands.AddMapMarker( name, x, y, facet, zoomLevel, iconName );
        }

        [CommandsDisplay( Category = nameof( Strings.World_Map ),
            Parameters = new[] { nameof( ParameterType.String ) } )]
        public static void RemoveMapMarker( string name )
        {
            WorldMapGump.RemoveMarker( name );
        }

        [CommandsDisplay( Category = nameof( Strings.World_Map ) )]
        public static void ClearMapMarkers()
        {
            WorldMapGump.ClearMarkers();
        }
    }
}