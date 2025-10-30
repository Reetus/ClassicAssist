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

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace ClassicAssist.Plugin.Shared.Reflection.ClassicUO.Objects
{
    public static class WorldMapGump
    {
        private static readonly Regex _csvRegex = new Regex( "^\\d+,\\d+,\\d,([^,]+).*$", RegexOptions.Compiled );

        public static bool AddMarker( string name, int x, int y, int facet, int zoomLevel = 3,
            string iconName = "bank" )
        {
            string mapFilePath = Path.Combine( ReflectionImpl.ClassicUOPath, "Data", "Client" );
            string markerFilePath = Path.Combine( mapFilePath, "ClassicAssist.csv" );

            using ( FileStream fileStream =
                   File.Open( markerFilePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Write ) )
            {
                using ( StreamWriter streamWriter = new StreamWriter( fileStream ) )
                {
                    streamWriter.BaseStream.Seek( 0, SeekOrigin.End );
                    streamWriter.WriteLine( $"{x},{y},{facet},{name},{iconName},blue,{zoomLevel}" );
                }
            }

            ReloadMarkers();

            return true;
        }

        private static void ReloadMarkers()
        {
            object gump = GetWorldMapGump();

            if ( gump == null )
            {
                return;
            }

            MethodInfo method =
                gump.GetType().GetMethod( "LoadMarkers", BindingFlags.Instance | BindingFlags.NonPublic );

            if ( method != null )
            {
                ReflectionImpl.TickWorkQueue.Enqueue( () => { method.Invoke( gump, null ); } );
            }
        }

        public static bool RemoveMarker( string name )
        {
            string mapFilePath = Path.Combine( ReflectionImpl.ClassicUOPath, "Data", "Client" );
            string markerFilePath = Path.Combine( mapFilePath, "ClassicAssist.csv" );

            string[] lines = File.ReadAllLines( markerFilePath );

            lines = lines.Where( l =>
            {
                Match match = _csvRegex.Match( l );

                return !match.Success || !match.Groups[1].Value.Equals( name );
            } ).ToArray();

            File.WriteAllLines( markerFilePath, lines );

            ReloadMarkers();

            return true;
        }

        private static object GetWorldMapGump()
        {
            IEnumerable<dynamic> gumps = ClassicUO.Gumps.GetGumps();

            return gumps.FirstOrDefault( e => e.GetType().ToString().Equals( "ClassicUO.Game.UI.Gumps.WorldMapGump" ) );
        }

        public static void ClearMarkers()
        {
            string mapFilePath = Path.Combine( ReflectionImpl.ClassicUOPath, "Data", "Client" );
            string markerFilePath = Path.Combine( mapFilePath, "ClassicAssist.csv" );

            if ( File.Exists( markerFilePath ) )
            {
                File.Delete( markerFilePath );
            }

            ReloadMarkers();
        }
    }
}