using System;
using ClassicAssist.Resources;

namespace ClassicAssist.Data.Macros
{
    public class CommandsDisplayAttribute : Attribute
    {
        private string _category;
        private string _example = string.Empty;

        public string Category
        {
            get => _category;
            set => SetCategory( value );
        }

        public string Description { get; set; }

        public string Example
        {
            get => _example;
            set => SetExample(value);
        }

        private void SetExample( string value )
        {
            string resourceName = MacroCommandHelp.ResourceManager.GetString( value );

            if ( !string.IsNullOrEmpty( resourceName ) )
            {
                _example = resourceName;
                return;
            }

            _example = value;
        }

        public string InsertText { get; set; }

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