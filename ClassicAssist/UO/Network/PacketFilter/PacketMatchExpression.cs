#region License

// Copyright (C) 2026 Reetus
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

#endregion

using System;
using System.Collections.Generic;
using System.Globalization;

namespace ClassicAssist.UO.Network.PacketFilter
{
    /// <summary>
    ///     A small boolean expression over a packet's raw bytes, used by the Debug window's packet log
    ///     filter. Examples:
    ///     <code>
    ///     [0] = 0xF3 &amp;&amp; [4..7] = 0x4001ABCD
    ///     [0] = 0x25 || [0] = 0x1A
    ///     [5] != 0xAA                  (=! and = ! are accepted as well)
    ///     contains 0x4001ABCD          (the bytes anywhere in the packet)
    ///     len &gt; 20 &amp;&amp; !([0] = 0x73)
    ///     </code>
    ///     <para>
    ///         Grammar: terms joined by <c>&amp;&amp;</c> / <c>and</c> and <c>||</c> / <c>or</c>
    ///         (<c>&amp;&amp;</c> binds tighter), negated with <c>!</c> / <c>not</c>, grouped with
    ///         parentheses. A term is one of
    ///         <list type="bullet">
    ///             <item><c>[i] op value</c> - the byte at offset <c>i</c>;</item>
    ///             <item>
    ///                 <c>[i..j] op value</c> - bytes <c>i</c> to <c>j</c> inclusive, read big-endian (as
    ///                 UO sends serials and ids), up to 8 bytes;
    ///             </item>
    ///             <item><c>len op value</c> - the packet length;</item>
    ///             <item>
    ///                 <c>contains value</c> - the value's bytes appear anywhere in the packet. A hex value
    ///                 is as many bytes as it has digit pairs, so <c>0x00001234</c> is four bytes.
    ///             </item>
    ///         </list>
    ///         <c>op</c> is <c>=</c>, <c>==</c>, <c>!=</c>, <c>=!</c>, <c>&lt;</c>, <c>&lt;=</c>,
    ///         <c>&gt;</c> or <c>&gt;=</c>. Numbers are decimal, or hex with a <c>0x</c> prefix.
    ///     </para>
    ///     <para>
    ///         A byte term never matches a packet too short to have those bytes, whichever operator it
    ///         uses - so <c>[5] != 0xAA</c> skips packets of five bytes or fewer, and
    ///         <c>!([5] = 0xAA)</c> is the form that includes them.
    ///     </para>
    /// </summary>
    public sealed class PacketMatchExpression
    {
        private const int MAX_FIELD_BYTES = 8;

        // The largest UO packet is 0xFFFF bytes; nothing past that can ever be addressed.
        private const int MAX_OFFSET = 0xFFFF;

        private readonly Func<byte[], bool> _predicate;

        private PacketMatchExpression( string text, Func<byte[], bool> predicate )
        {
            Text = text;
            _predicate = predicate;
        }

        public string Text { get; }

        /// <summary>True when <paramref name="data" /> satisfies the expression. An empty expression matches everything.</summary>
        public bool IsMatch( byte[] data )
        {
            return data != null && _predicate( data );
        }

        /// <exception cref="FormatException">The text is not a valid expression; the message says where and why.</exception>
        public static PacketMatchExpression Parse( string text )
        {
            if ( text == null )
            {
                text = string.Empty;
            }

            Parser parser = new Parser( Lexer.Tokenize( text ) );

            return new PacketMatchExpression( text, parser.ParseAll() );
        }

        public static bool TryParse( string text, out PacketMatchExpression expression, out string error )
        {
            try
            {
                expression = Parse( text );
                error = null;

                return true;
            }
            catch ( FormatException e )
            {
                expression = null;
                error = e.Message;

                return false;
            }
        }

        private static bool Compare( ulong actual, TokenKind op, ulong expected )
        {
            switch ( op )
            {
                case TokenKind.Equal:
                    return actual == expected;
                case TokenKind.NotEqual:
                    return actual != expected;
                case TokenKind.Less:
                    return actual < expected;
                case TokenKind.LessEqual:
                    return actual <= expected;
                case TokenKind.Greater:
                    return actual > expected;
                case TokenKind.GreaterEqual:
                    return actual >= expected;
                default:
                    return false;
            }
        }

        private static ulong ReadBigEndian( byte[] data, int offset, int length )
        {
            ulong value = 0;

            for ( int i = 0; i < length; i++ )
            {
                value = ( value << 8 ) | data[offset + i];
            }

            return value;
        }

        private static bool Contains( byte[] data, byte[] needle )
        {
            int last = data.Length - needle.Length;

            for ( int i = 0; i <= last; i++ )
            {
                int j = 0;

                while ( j < needle.Length && data[i + j] == needle[j] )
                {
                    j++;
                }

                if ( j == needle.Length )
                {
                    return true;
                }
            }

            return false;
        }

        private enum TokenKind
        {
            End,
            Number,
            Identifier,
            LeftBracket,
            RightBracket,
            Range,
            LeftParen,
            RightParen,
            Not,
            And,
            Or,
            Equal,
            NotEqual,
            Less,
            LessEqual,
            Greater,
            GreaterEqual
        }

        private sealed class Token
        {
            public Token( TokenKind kind, int column, string text, string hexDigits = null )
            {
                Kind = kind;
                Column = column;
                Text = text;
                HexDigits = hexDigits;
            }

            /// <summary>1-based, for error messages.</summary>
            public int Column { get; }

            /// <summary>Set on hex <see cref="TokenKind.Number" /> tokens: the digits after <c>0x</c>.</summary>
            public string HexDigits { get; }

            public TokenKind Kind { get; }

            public string Text { get; }

            public string Describe()
            {
                return Kind == TokenKind.End ? "end of filter" : $"'{Text}' at column {Column}";
            }
        }

        private static class Lexer
        {
            public static List<Token> Tokenize( string text )
            {
                List<Token> tokens = new List<Token>();
                int pos = 0;

                while ( pos < text.Length )
                {
                    char c = text[pos];
                    int column = pos + 1;

                    if ( char.IsWhiteSpace( c ) )
                    {
                        pos++;
                        continue;
                    }

                    if ( char.IsDigit( c ) )
                    {
                        tokens.Add( ReadNumber( text, ref pos ) );
                        continue;
                    }

                    if ( char.IsLetter( c ) || c == '_' )
                    {
                        int start = pos;

                        while ( pos < text.Length && ( char.IsLetterOrDigit( text[pos] ) || text[pos] == '_' ) )
                        {
                            pos++;
                        }

                        string word = text.Substring( start, pos - start );

                        tokens.Add( new Token( KeywordKind( word ), column, word ) );
                        continue;
                    }

                    char next = pos + 1 < text.Length ? text[pos + 1] : '\0';

                    if ( !TryReadSymbol( c, next, out TokenKind kind, out int length ) )
                    {
                        throw new FormatException( $"Unexpected '{c}' at column {column}" );
                    }

                    tokens.Add( new Token( kind, column, text.Substring( pos, length ) ) );
                    pos += length;
                }

                tokens.Add( new Token( TokenKind.End, text.Length + 1, string.Empty ) );

                return tokens;
            }

            private static TokenKind KeywordKind( string word )
            {
                switch ( word.ToLowerInvariant() )
                {
                    case "and":
                        return TokenKind.And;
                    case "or":
                        return TokenKind.Or;
                    case "not":
                        return TokenKind.Not;
                    default:
                        return TokenKind.Identifier;
                }
            }

            private static bool TryReadSymbol( char c, char next, out TokenKind kind, out int length )
            {
                length = 1;

                switch ( c )
                {
                    case '[':
                        kind = TokenKind.LeftBracket;
                        return true;
                    case ']':
                        kind = TokenKind.RightBracket;
                        return true;
                    case '(':
                        kind = TokenKind.LeftParen;
                        return true;
                    case ')':
                        kind = TokenKind.RightParen;
                        return true;
                    case '.' when next == '.':
                        kind = TokenKind.Range;
                        length = 2;
                        return true;
                    case '&' when next == '&':
                        kind = TokenKind.And;
                        length = 2;
                        return true;
                    case '|' when next == '|':
                        kind = TokenKind.Or;
                        length = 2;
                        return true;
                    case '!' when next == '=':
                        kind = TokenKind.NotEqual;
                        length = 2;
                        return true;
                    case '!':
                        kind = TokenKind.Not;
                        return true;
                    case '=' when next == '=':
                        kind = TokenKind.Equal;
                        length = 2;
                        return true;
                    // "[5]=!0xAA": an operator is always followed by a value, never by a negation,
                    // so "=!" can only mean not-equal.
                    case '=' when next == '!':
                        kind = TokenKind.NotEqual;
                        length = 2;
                        return true;
                    case '=':
                        kind = TokenKind.Equal;
                        return true;
                    case '<' when next == '=':
                        kind = TokenKind.LessEqual;
                        length = 2;
                        return true;
                    case '<':
                        kind = TokenKind.Less;
                        return true;
                    case '>' when next == '=':
                        kind = TokenKind.GreaterEqual;
                        length = 2;
                        return true;
                    case '>':
                        kind = TokenKind.Greater;
                        return true;
                    default:
                        kind = TokenKind.End;
                        length = 0;
                        return false;
                }
            }

            private static Token ReadNumber( string text, ref int pos )
            {
                int start = pos;
                int column = pos + 1;

                if ( text[pos] == '0' && pos + 1 < text.Length && ( text[pos + 1] == 'x' || text[pos + 1] == 'X' ) )
                {
                    pos += 2;

                    int digitsStart = pos;

                    while ( pos < text.Length && Uri.IsHexDigit( text[pos] ) )
                    {
                        pos++;
                    }

                    if ( pos == digitsStart )
                    {
                        throw new FormatException( $"Expected hex digits after '0x' at column {column}" );
                    }

                    RejectTrailingLetters( text, pos, start );

                    return new Token( TokenKind.Number, column, text.Substring( start, pos - start ),
                        text.Substring( digitsStart, pos - digitsStart ) );
                }

                while ( pos < text.Length && char.IsDigit( text[pos] ) )
                {
                    pos++;
                }

                RejectTrailingLetters( text, pos, start );

                return new Token( TokenKind.Number, column, text.Substring( start, pos - start ) );
            }

            // "12ab" or "0x12zz" is a typo, not a number followed by a word.
            private static void RejectTrailingLetters( string text, int pos, int start )
            {
                if ( pos < text.Length && ( char.IsLetterOrDigit( text[pos] ) || text[pos] == '_' ) )
                {
                    throw new FormatException( $"Invalid number '{text.Substring( start, pos + 1 - start )}' at column {start + 1}" );
                }
            }
        }

        private sealed class Parser
        {
            private readonly List<Token> _tokens;
            private int _index;

            public Parser( List<Token> tokens )
            {
                _tokens = tokens;
            }

            private Token Current => _tokens[_index];

            public Func<byte[], bool> ParseAll()
            {
                if ( Current.Kind == TokenKind.End )
                {
                    return _ => true;
                }

                Func<byte[], bool> expression = ParseOr();

                if ( Current.Kind != TokenKind.End )
                {
                    throw new FormatException( $"Expected '&&' or '||' but found {Current.Describe()}" );
                }

                return expression;
            }

            private Func<byte[], bool> ParseOr()
            {
                Func<byte[], bool> left = ParseAnd();

                while ( Accept( TokenKind.Or ) )
                {
                    Func<byte[], bool> lhs = left;
                    Func<byte[], bool> rhs = ParseAnd();
                    left = data => lhs( data ) || rhs( data );
                }

                return left;
            }

            private Func<byte[], bool> ParseAnd()
            {
                Func<byte[], bool> left = ParseUnary();

                while ( Accept( TokenKind.And ) )
                {
                    Func<byte[], bool> lhs = left;
                    Func<byte[], bool> rhs = ParseUnary();
                    left = data => lhs( data ) && rhs( data );
                }

                return left;
            }

            private Func<byte[], bool> ParseUnary()
            {
                if ( Accept( TokenKind.Not ) )
                {
                    Func<byte[], bool> inner = ParseUnary();

                    return data => !inner( data );
                }

                if ( Accept( TokenKind.LeftParen ) )
                {
                    Func<byte[], bool> inner = ParseOr();
                    Expect( TokenKind.RightParen, "')'" );

                    return inner;
                }

                return ParseTerm();
            }

            private Func<byte[], bool> ParseTerm()
            {
                Token token = Current;

                if ( Accept( TokenKind.LeftBracket ) )
                {
                    return ParseByteTerm( token );
                }

                if ( token.Kind != TokenKind.Identifier )
                {
                    throw new FormatException( $"Expected '[', 'len', 'contains', '!' or '(' but found {token.Describe()}" );
                }

                _index++;

                switch ( token.Text.ToLowerInvariant() )
                {
                    case "len":
                    case "length":
                    {
                        TokenKind op = ExpectComparison();
                        ulong expected = ReadValue( Expect( TokenKind.Number, "a number" ) );

                        return data => Compare( (ulong) data.Length, op, expected );
                    }
                    case "contains":
                    {
                        byte[] needle = ReadBytes( Expect( TokenKind.Number, "a number" ) );

                        return data => Contains( data, needle );
                    }
                    default:
                        throw new FormatException( $"Unknown keyword '{token.Text}' at column {token.Column} - expected 'len' or 'contains'" );
                }
            }

            private Func<byte[], bool> ParseByteTerm( Token open )
            {
                int start = ReadOffset( Expect( TokenKind.Number, "an offset" ) );
                int end = start;

                if ( Accept( TokenKind.Range ) )
                {
                    end = ReadOffset( Expect( TokenKind.Number, "an offset" ) );
                }

                Expect( TokenKind.RightBracket, "']'" );

                if ( end < start )
                {
                    throw new FormatException( $"Range [{start}..{end}] at column {open.Column} ends before it starts" );
                }

                int width = end - start + 1;

                if ( width > MAX_FIELD_BYTES )
                {
                    throw new FormatException(
                        $"Range [{start}..{end}] at column {open.Column} is {width} bytes - at most {MAX_FIELD_BYTES} can be compared, use 'contains' for longer sequences" );
                }

                TokenKind op = ExpectComparison();
                Token valueToken = Expect( TokenKind.Number, "a value" );
                ulong expected = ReadValue( valueToken );

                if ( width < MAX_FIELD_BYTES && expected >> ( width * 8 ) != 0 )
                {
                    throw new FormatException( $"{valueToken.Text} at column {valueToken.Column} does not fit in {width} byte{( width == 1 ? "" : "s" )}" );
                }

                return data => data.Length > end && Compare( ReadBigEndian( data, start, width ), op, expected );
            }

            private TokenKind ExpectComparison()
            {
                Token token = Current;

                switch ( token.Kind )
                {
                    case TokenKind.Equal:
                        _index++;

                        // "[5] = !0xAA" with a space: after '=' a '!' can only negate the value.
                        return Accept( TokenKind.Not ) ? TokenKind.NotEqual : TokenKind.Equal;
                    case TokenKind.NotEqual:
                    case TokenKind.Less:
                    case TokenKind.LessEqual:
                    case TokenKind.Greater:
                    case TokenKind.GreaterEqual:
                        _index++;

                        return token.Kind;
                    default:
                        throw new FormatException( $"Expected '=', '!=', '<', '<=', '>' or '>=' but found {token.Describe()}" );
                }
            }

            private bool Accept( TokenKind kind )
            {
                if ( Current.Kind != kind )
                {
                    return false;
                }

                _index++;

                return true;
            }

            private Token Expect( TokenKind kind, string description )
            {
                Token token = Current;

                if ( token.Kind != kind )
                {
                    throw new FormatException( $"Expected {description} but found {token.Describe()}" );
                }

                _index++;

                return token;
            }

            private static int ReadOffset( Token token )
            {
                ulong value = ReadValue( token );

                if ( value > MAX_OFFSET )
                {
                    throw new FormatException( $"Offset {token.Text} at column {token.Column} is past the largest packet size" );
                }

                return (int) value;
            }

            private static ulong ReadValue( Token token )
            {
                if ( token.HexDigits != null )
                {
                    // Leading zeros are allowed ("0x0000000000001234"); only significant digits count.
                    string significant = token.HexDigits.TrimStart( '0' );

                    if ( significant.Length > 16 )
                    {
                        throw new FormatException( $"{token.Text} at column {token.Column} is larger than 8 bytes" );
                    }

                    return significant.Length == 0 ? 0 : ulong.Parse( significant, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture );
                }

                if ( !ulong.TryParse( token.Text, NumberStyles.None, CultureInfo.InvariantCulture, out ulong value ) )
                {
                    throw new FormatException( $"{token.Text} at column {token.Column} is larger than 8 bytes" );
                }

                return value;
            }

            /// <summary>
            ///     The byte sequence a <c>contains</c> value stands for. Hex keeps its written width
            ///     (<c>0x0001</c> is two bytes, an odd digit count gets a leading zero nibble); decimal has
            ///     no written width, so it is the fewest bytes that hold it.
            /// </summary>
            private static byte[] ReadBytes( Token token )
            {
                if ( token.HexDigits != null )
                {
                    string digits = token.HexDigits.Length % 2 == 0 ? token.HexDigits : "0" + token.HexDigits;
                    byte[] hexBytes = new byte[digits.Length / 2];

                    for ( int i = 0; i < hexBytes.Length; i++ )
                    {
                        hexBytes[i] = byte.Parse( digits.Substring( i * 2, 2 ), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture );
                    }

                    return hexBytes;
                }

                ulong value = ReadValue( token );
                int length = 1;

                while ( length < MAX_FIELD_BYTES && value >> ( length * 8 ) != 0 )
                {
                    length++;
                }

                byte[] bytes = new byte[length];

                for ( int i = length - 1; i >= 0; i-- )
                {
                    bytes[i] = (byte) value;
                    value >>= 8;
                }

                return bytes;
            }
        }
    }
}
