#region License

// Copyright (C) 2026 Reetus
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

#endregion

using System;
using System.Linq;
using System.Text;

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

        public static void FormatBuffer( byte[] bytes, string title = null, int bytesPerLine = 16 )
        {
            if ( bytes == null || bytes.Length == 0 )
            {
                return;
            }

            int bytesLength = bytes.Length;

            Console.WriteLine( !string.IsNullOrEmpty( title ) ? $"{title}, Packet: {bytes[0]:X}, Size: {bytes.Length}" : $"Packet: {bytes[0]:X}, Size: {bytes.Length}" );

            char[] HexChars = "0123456789ABCDEF".ToCharArray();

            const int firstHexColumn = 8 // 8 characters for the address
                                       + 3; // 3 spaces

            int firstCharColumn = firstHexColumn + bytesPerLine * 3 // - 2 digit for the hexadecimal value and 1 space
                                                 + ( bytesPerLine - 1 ) / 8 // - 1 extra space every 8 characters from the 9th
                                                 + 2; // 2 spaces 

            int lineLength = firstCharColumn + bytesPerLine // - characters to show the ascii value
                                             + Environment.NewLine.Length; // Carriage return and line feed (should normally be 2)

            char[] line = ( new string( ' ', lineLength - Environment.NewLine.Length ) + Environment.NewLine ).ToCharArray();
            int expectedLines = ( bytesLength + bytesPerLine - 1 ) / bytesPerLine;
            StringBuilder result = new StringBuilder( expectedLines * lineLength );

            for ( int i = 0; i < bytesLength; i += bytesPerLine )
            {
                line[0] = HexChars[( i >> 28 ) & 0xF];
                line[1] = HexChars[( i >> 24 ) & 0xF];
                line[2] = HexChars[( i >> 20 ) & 0xF];
                line[3] = HexChars[( i >> 16 ) & 0xF];
                line[4] = HexChars[( i >> 12 ) & 0xF];
                line[5] = HexChars[( i >> 8 ) & 0xF];
                line[6] = HexChars[( i >> 4 ) & 0xF];
                line[7] = HexChars[( i >> 0 ) & 0xF];

                int hexColumn = firstHexColumn;
                int charColumn = firstCharColumn;

                for ( int j = 0; j < bytesPerLine; j++ )
                {
                    if ( j > 0 && ( j & 7 ) == 0 )
                    {
                        hexColumn++;
                    }

                    if ( i + j >= bytesLength )
                    {
                        line[hexColumn] = ' ';
                        line[hexColumn + 1] = ' ';
                        line[charColumn] = ' ';
                    }
                    else
                    {
                        byte b = bytes[i + j];
                        line[hexColumn] = HexChars[( b >> 4 ) & 0xF];
                        line[hexColumn + 1] = HexChars[b & 0xF];
                        line[charColumn] = b < 32 ? '.' : (char) b;
                    }

                    hexColumn += 3;
                    charColumn++;
                }

                result.Append( line );
            }

            Console.WriteLine( result.ToString() );
        }
    }
}