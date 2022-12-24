﻿using ClassicAssist.Shared.UI;

namespace ClassicAssist.Data.Vendors
{
    public class VendorBuyAgentItem : SetPropertyNotifyChanged
    {
        private int _amount;
        private bool _enabled;
        private int _graphic;
        private int _hue;
        private int _maxPrice;
        private string _name;

        public int Amount
        {
            get => _amount;
            set => SetProperty( ref _amount, value );
        }

        public bool Enabled
        {
            get => _enabled;
            set => SetProperty( ref _enabled, value );
        }

        public int Graphic
        {
            get => _graphic;
            set => SetProperty( ref _graphic, value );
        }

        public int Hue
        {
            get => _hue;
            set => SetProperty( ref _hue, value );
        }

        public int MaxPrice
        {
            get => _maxPrice;
            set => SetProperty( ref _maxPrice, value );
        }

        public string Name
        {
            get => _name;
            set => SetProperty( ref _name, value );
        }
    }
}