#region License

// Copyright (C) 2022 Reetus
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
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;
using ClassicAssist.Shared.Resources;
using IronPython.Runtime.Operations;
using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;

namespace ClassicAssist.UI.Misc.ValueConverters
{
    public class MacroExceptionValueConverter : IValueConverter
    {
        public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
        {
            if ( !( value is Exception exception ) )
            {
                return string.Empty;
            }

            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.AppendLine( string.Format( Strings.Macro_error___0_, exception.Message ) );

            if ( exception is SyntaxErrorException syntaxError )
            {
                stringBuilder.AppendLine( $"{Strings.Line_Number}: {syntaxError.RawSpan.Start.Line}" );
            }
            else
            {
                DynamicStackFrame sf = PythonOps.GetDynamicStackFrames( exception ).FirstOrDefault();

                if ( sf != null )
                {
                    string fileName = sf.GetFileName();

                    if ( fileName != "<string>" )
                    {
                        stringBuilder.AppendLine( $"{Strings.Module}: {fileName}" );
                    }

                    stringBuilder.AppendLine( $"{Strings.Line_Number}: {sf.GetFileLineNumber()}" );
                }
            }

            string result = stringBuilder.ToString();

            if ( result.EndsWith( "\r\n" ) )
            {
                result = result.TrimEnd( '\r', '\n' );
            }

            return result;
        }

        public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
        {
            throw new NotImplementedException();
        }
    }
}