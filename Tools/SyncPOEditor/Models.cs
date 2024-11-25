#region License

// Copyright (C) 2024 Reetus
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
#endregion

using System.Text.Json.Serialization;

namespace SyncPOEditor;

public class POEditorResponse<T>
{
    [JsonPropertyName( "result" )]
    public T? Result { get; set; }
}

public class POEditorTerms
{
    [JsonPropertyName( "terms" )]
    public List<POEditorTerm>? Terms { get; set; }
}

public class POEditorTerm
{
    [JsonPropertyName( "term" )]
    public string? Term { get; set; }

    [JsonPropertyName( "translation" )]
    [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
    public POEditorTranslation? Translation { get; set; }
}

public class POEditorTranslation
{
    [JsonPropertyName( "content" )]
    public string? Content { get; set; }
}