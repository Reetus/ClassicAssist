using System;
using ClassicAssist.Resources;

namespace ClassicAssist.Data.Macros
{
    public class CommandsDisplayAttribute : Attribute
    {
        private string _category;

        public string Category
        {
            get => _category;
            set => SetCategory( value );
        }

        public string Description { get; set; }

        private void SetCategory( string value )
        {
            string resourceName = Strings.ResourceManager.GetString( value );

            if ( !string.IsNullOrEmpty( resourceName ) )
            {
                _category = resourceName;
                return;
            }

            _category = value;
        }
    }
}