#region License

// Copyright (C) 2020 Reetus
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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ClassicAssist.Browser.Data
{
    public class CategoriesConverter : JsonConverter
    {
        public override void WriteJson( JsonWriter writer, object value, JsonSerializer serializer )
        {
            throw new NotImplementedException();
        }

        public override object ReadJson( JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer )
        {
            switch ( reader.TokenType )
            {
                case JsonToken.StartArray:
                {
                    JToken token = JToken.Load( reader );
                    return token.ToObject<string[]>();
                }
                case JsonToken.String:
                {
                    JToken token = JToken.Load( reader );
                    return new[] { token.ToObject<string>() };
                }
                default:
                    return null;
            }
        }

        public override bool CanConvert( Type objectType )
        {
            throw new NotImplementedException();
        }
    }
}