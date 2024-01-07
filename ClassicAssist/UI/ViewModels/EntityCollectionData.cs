#region License

// Copyright (C) 2022 Reetus
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
using System.Linq;
using System.Windows.Media.Imaging;
using ClassicAssist.Misc;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Objects;

namespace ClassicAssist.UI.ViewModels
{
    public class EntityCollectionData
    {
        private readonly Dictionary<long, BitmapSource> _cache = new Dictionary<long, BitmapSource>();

        public BitmapSource Bitmap
        {
            get
            {
                int key = ( Entity.ID << 16 ) | Entity.Hue;

                if ( _cache.TryGetValue( key, out BitmapSource bitmap ) )
                {
                    return bitmap;
                }

                BitmapSource result = Art.GetStatic( Entity.ID, Entity.Hue ).ToBitmapSource();

                if ( !( Entity is Item item ) || item.Layer != Layer.Mount )
                {
                    _cache.Add( key, result );

                    return result;
                }

                if ( !( EntityCollectionViewerViewModel.MountIDEntries.Value?.ContainsKey( Entity.ID ) ?? false ) )
                {
                    _cache.Add( key, result );

                    return result;
                }

                if ( !EntityCollectionViewerViewModel.MountIDEntries.Value.TryGetValue( Entity.ID, out int id ) )
                {
                    return null;
                }

                result = Art.GetStatic( id, Entity.Hue ).ToBitmapSource();

                _cache.Add( key, result );

                return result;
            }
        }

        public Entity Entity { get; set; }
        public string FullName => GetProperties( Entity );

        public string Name => GetName( Entity );

        private string GetProperties( Entity entity )
        {
            return entity.Properties == null
                ? GetName( entity )
                : entity.Properties.Aggregate( "", ( current, entityProperty ) => current + entityProperty.Text + "\r\n" ).TrimTrailingNewLine();
        }

        private static string GetName( Entity entity )
        {
            if ( !( entity is Item item ) || item.Layer != Layer.Mount )
            {
                return entity.Name;
            }

            if ( !( EntityCollectionViewerViewModel.MountIDEntries.Value?.ContainsKey( entity.ID ) ?? false ) )
            {
                return entity.Name;
            }

            int id = EntityCollectionViewerViewModel.MountIDEntries.Value[entity.ID];

            if ( id == 0 )
            {
                return entity.Name;
            }

            StaticTile tileData = TileData.GetStaticTile( id );

            return !string.IsNullOrEmpty( tileData.Name ) ? tileData.Name : entity.Name;
        }
    }
}