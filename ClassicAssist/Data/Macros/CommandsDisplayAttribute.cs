using System;
using System.Runtime.CompilerServices;
using ClassicAssist.Resources;
using ClassicAssist.Shared.Resources;

namespace ClassicAssist.Data.Macros
{
    public class CommandsDisplayAttribute : Attribute
    {
        private string _category;

        public CommandsDisplayAttribute( [CallerMemberName] string member = "" )
        {
            string memberName = member.ToUpper();

            string description = MacroCommandHelp.ResourceManager.GetString( $"{memberName}_COMMAND_DESCRIPTION" );

            if ( string.IsNullOrEmpty( description ) )
            {
                throw new ArgumentNullException( $"Macro command help not translated: {member}" );
            }

            Description = description;

            string insertText = MacroCommandHelp.ResourceManager.GetString( $"{memberName}_COMMAND_INSERTTEXT" );

            if ( string.IsNullOrEmpty( insertText ) )
            {
                throw new ArgumentNullException( $"Macro command help not translated: {member}" );
            }

            InsertText = insertText;

            string example = MacroCommandHelp.ResourceManager.GetString( $"{memberName}_COMMAND_EXAMPLE" );

            Example = string.IsNullOrEmpty( example ) ? insertText : example;
        }

        public string Category
        {
            get => _category;
            set => SetCategory( value );
        }

        public string Description { get; set; }
        public string Example { get; set; }
        public string InsertText { get; set; }
        public string[] Parameters { get; set; }

        private void SetCategory( string value )
        {
            string resourceName = Strings.ResourceManager.GetString( value.Replace( '_', ' ' ) );

            if ( string.IsNullOrEmpty( resourceName ) )
            {
                throw new ArgumentNullException( $"String not translated: {value}" );
            }

            _category = resourceName;
        }
    }
}