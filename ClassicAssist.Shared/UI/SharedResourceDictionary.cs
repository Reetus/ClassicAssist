using System;
using System.Collections.Generic;
using System.Windows;

namespace ClassicAssist.Shared.UI
{
    public class SharedResourceDictionary : ResourceDictionary
    {
        private static readonly Dictionary<Uri, ResourceDictionary> _cache =
            new Dictionary<Uri, ResourceDictionary>();

        private Uri _sourceUri;

        public new Uri Source
        {
            get => _sourceUri;
            set
            {
                _sourceUri = value;

                if ( _cache.TryGetValue( value, out ResourceDictionary cached ) )
                {
                    MergedDictionaries.Add( cached );
                    return;
                }

                base.Source = value;
                _cache[value] = this;
            }
        }
    }
}
