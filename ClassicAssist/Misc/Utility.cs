using System;
using System.Linq;

namespace ClassicAssist.Misc
{
    public class Utility
    {
        public static T GetEnumValueByName<T>( string value )
        {
            //TODO robust enough?

            value = value.ToLower().Replace( ' ', '_' );

            string[] enumValues = Enum.GetNames( typeof( T ) );

            string enumValue = enumValues.FirstOrDefault( ev => ev.ToLower() == value );

            T enumEntry = (T) Enum.Parse( typeof( T ), enumValue ?? throw new InvalidOperationException() );

            return enumEntry;
        }

        public static (byte[] data, int length) CopyBuffer( byte[] source, int sourceLength )
        {
            byte[] data = new byte[sourceLength];
            int dataLength = sourceLength;

            Buffer.BlockCopy( source, 0, data, 0, sourceLength );

            return ( data, dataLength );
        }
    }
}