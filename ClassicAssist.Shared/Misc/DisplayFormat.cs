using System;
using System.Text;
using ClassicAssist.UO.Objects;

namespace ClassicAssist.Misc
{
    public class DisplayFormatAttribute : Attribute
    {
        private readonly Type _type;

        public DisplayFormatAttribute( Type type )
        {
            _type = type;
        }

        public string ToString( object value )
        {
            if ( _type.IsEnum )
            {
                return Enum.Parse( _type, value.ToString() ).ToString();
            }

            return typeof( IFormatProvider ).IsAssignableFrom( _type )
                ? string.Format( (IFormatProvider) Activator.CreateInstance( _type ), "{0}", value )
                : Convert.ChangeType( value, typeof( string ) ).ToString();
        }
    }

    internal class HexFormatProvider : IFormatProvider, ICustomFormatter
    {
        public string Format( string format, object arg, IFormatProvider formatProvider )
        {
            return $"0x{arg:x}";
        }

        public object GetFormat( Type formatType )
        {
            return this;
        }
    }

    public class PropertyArrayFormatProvider : IFormatProvider, ICustomFormatter
    {
        public string Format( string format, object arg, IFormatProvider formatProvider )
        {
            if ( arg == null )
            {
                return "null";
            }

            if ( !( arg is Property[] properties ) )
            {
                return arg.ToString();
            }

            StringBuilder sb = new StringBuilder();

            for ( int index = 0; index < properties.Length; index++ )
            {
                Property property = properties[index];

                if ( index != properties.Length - 1 )
                {
                    sb.AppendLine( property.Text );
                }
                else
                {
                    sb.Append( property.Text );
                }
            }

            return sb.ToString();
        }

        public object GetFormat( Type formatType )
        {
            return this;
        }
    }
}