using System;
using ClassicAssist.Shared.UI;
using ClassicAssist.UO.Data;

namespace ClassicAssist.Data.Friends
{
    public class RehueLayerEntry : SetPropertyNotifyChanged
    {
        private readonly Action<RehueLayerEntry> _onChanged;
        private bool _enabled;

        public RehueLayerEntry( string name, Layer? layer, bool enabled, Action<RehueLayerEntry> onChanged )
        {
            Name = name;
            Layer = layer;
            _enabled = enabled;
            _onChanged = onChanged;
        }

        public bool Enabled
        {
            get => _enabled;
            set => SetProperty( ref _enabled, value, afterChange: v => _onChanged?.Invoke( this ) );
        }

        /// <summary>
        ///     Null represents the mobile body hue
        /// </summary>
        public Layer? Layer { get; }

        public string Name { get; }

        public void SetEnabledSilent( bool enabled )
        {
            SetProperty( ref _enabled, enabled, propertyName: nameof( Enabled ) );
        }
    }
}
