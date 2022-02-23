using System;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using ClassicAssist.Shared.Resources;

namespace ClassicAssist.UI.Misc
{
    public class EnumDescriptionTypeConverter : EnumConverter
    {
        public EnumDescriptionTypeConverter( Type type ) : base( type )
        {
        }

        public override object ConvertTo( ITypeDescriptorContext context, CultureInfo culture, object value,
            Type destinationType )
        {
            if ( destinationType != typeof( string ) )
            {
                return base.ConvertTo( context, culture, value, destinationType );
            }

            if ( value == null )
            {
                return string.Empty;
            }

            FieldInfo fi = value.GetType().GetField( value.ToString() );

            if ( fi != null )
            {
                DescriptionAttribute[] attributes =
                    (DescriptionAttribute[]) fi.GetCustomAttributes( typeof( DescriptionAttribute ), false );

                return attributes.Length > 0 && !string.IsNullOrEmpty( attributes[0].Description )
                    ? Strings.ResourceManager.GetString( attributes[0].Description ) ?? attributes[0].Description
                    : value.ToString();
            }

            return string.Empty;
        }
    }
}