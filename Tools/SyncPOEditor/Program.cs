// Copyright (C) 2024 Reetus
//  
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//  
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

using System.Collections;
using System.Globalization;
using System.Resources;
using System.Text.Json;
using SyncPOEditor;

const int PROJECT_ID = 671847;
Dictionary<string, string> _langCodeFileName = new()
{
    { "zh-Hans", @"..\..\ClassicAssist.Shared\Resources\Strings.zh.resx" },
    { "cs", @"..\..\ClassicAssist.Shared\Resources\Strings.cs.resx" },
    { "it", @"..\..\ClassicAssist.Shared\Resources\Strings.it-IT.resx" },
    { "ko", @"..\..\ClassicAssist.Shared\Resources\Strings.ko-KR.resx" },
    { "pl", @"..\..\ClassicAssist.Shared\Resources\Strings.pl-PL.resx" },
    { "en", @"..\..\ClassicAssist.Shared\Resources\Strings.en-GB.resx" },
    { "en-au", @"..\..\ClassicAssist.Shared\Resources\Strings.en-AU.resx" },
    { "en-us", @"..\..\ClassicAssist.Shared\Resources\Strings.resx" }
};

string? apiToken = Environment.GetEnvironmentVariable( "API_TOKEN" );

ArgumentException.ThrowIfNullOrEmpty( apiToken );

HttpClient client = new();

Dictionary<string, string> resxNeutralTerms = new();

using ( ResXResourceReader reader = new( @"..\..\ClassicAssist.Shared\Resources\Strings.resx" ) )
{
    foreach ( DictionaryEntry entry in from DictionaryEntry entry in reader where !string.IsNullOrEmpty( entry.Key.ToString() ) && entry.Value != null select entry )
    {
        resxNeutralTerms[entry.Key.ToString()!] = entry.Value!.ToString()!;
    }
}

POEditorResponse<POEditorTerms>? neutralTerms = await GetNeutralTerms();

List<string?> missingTerms = resxNeutralTerms.Keys.Except( ( neutralTerms?.Result?.Terms ?? throw new InvalidOperationException() ).Select( x => x.Term ) ).ToList();

if ( missingTerms.Any() )
{
    POEditorTerm[] terms = missingTerms.Select( x => new POEditorTerm { Term = x } ).ToArray();

    await AddTerms( terms );
}

foreach ( ( string? lang, string? fileName ) in _langCodeFileName )
{
    CultureInfo culture = new( lang );
    Thread.CurrentThread.CurrentCulture = culture;
    Thread.CurrentThread.CurrentUICulture = culture;

    if ( !File.Exists( fileName ) )
    {
        throw new FileNotFoundException( $"File not found: {fileName}" );
    }

    Dictionary<string, string> resxTerms = new();

    using ( ResXResourceReader reader = new( fileName ) )
    {
        foreach ( DictionaryEntry entry in from DictionaryEntry entry in reader where !string.IsNullOrEmpty( entry.Key.ToString() ) && entry.Value != null select entry )
        {
            resxTerms[entry.Key.ToString()!] = entry.Value!.ToString()!;
        }
    }

    POEditorResponse<POEditorTerms>? terms = await GetLanguageTerms( lang );

    if ( terms?.Result?.Terms == null )
    {
        continue;
    }

    bool modified = false;

    Dictionary<string, (string, string)> modded = new();

    foreach ( POEditorTerm term in terms.Result.Terms )
    {
        if ( string.IsNullOrEmpty( term.Term ) || string.IsNullOrEmpty( term.Translation?.Content ) )
        {
            continue;
        }

        if ( resxTerms.TryGetValue( term.Term, out string? value ) )
        {
            if ( value.Equals( term.Translation.Content, StringComparison.CurrentCulture ) )
            {
                continue;
            }

            resxTerms[term.Term] = term.Translation.Content;
            modded[term.Term] = ( value, term.Translation.Content );
            modified = true;
        }
    }

    if ( modified )
    {
        foreach ( ( string? key, ( string? oldvalue, string? newvalue ) ) in modded )
        {
            Console.WriteLine( $"Updated: {key} - {oldvalue} -> {newvalue}" );
        }

        using ResXResourceWriter writer = new( fileName );

        foreach ( ( string? key, string value ) in resxTerms )
        {
            writer.AddResource( key, value );
        }
    }

    Dictionary<string, string> missingTranslations = new();

    foreach ( ( string key, string value ) in resxTerms )
    {
        POEditorTerm? termsRes = terms.Result.Terms.FirstOrDefault( x => x.Term == key );

        if ( termsRes == null || string.IsNullOrEmpty( termsRes.Translation?.Content ) )
        {
            missingTranslations[key] = value;
        }
    }

    if ( missingTranslations.Any() )
    {
        POEditorTerm[] translations = missingTranslations.Select( x => new POEditorTerm { Term = x.Key, Translation = new POEditorTranslation { Content = x.Value } } ).ToArray();
        await AddTranslations( lang, translations );
    }
}

return;

async Task<POEditorResponse<POEditorTerms>?> GetLanguageTerms( string lang )
{
    using HttpResponseMessage response = await client.PostAsync( "https://api.poeditor.com/v2/terms/list", new FormUrlEncodedContent( [
        new KeyValuePair<string, string>( "api_token", apiToken ), new KeyValuePair<string, string>( "id", PROJECT_ID.ToString() ),
        new KeyValuePair<string, string>( "language", lang )
    ] ) );
    response.EnsureSuccessStatusCode();
    string json = await response.Content.ReadAsStringAsync();
    return JsonSerializer.Deserialize<POEditorResponse<POEditorTerms>>( json );
}

async Task<POEditorResponse<POEditorTerms>?> GetNeutralTerms()
{
    using HttpResponseMessage response = await client.PostAsync( "https://api.poeditor.com/v2/terms/list",
        new FormUrlEncodedContent( [new KeyValuePair<string, string>( "api_token", apiToken ), new KeyValuePair<string, string>( "id", PROJECT_ID.ToString() )] ) );
    response.EnsureSuccessStatusCode();
    string json = response.Content.ReadAsStringAsync().Result;
    return JsonSerializer.Deserialize<POEditorResponse<POEditorTerms>>( json );
}

async Task AddTerms( POEditorTerm[] terms )
{   
    using HttpResponseMessage response = await client.PostAsync( "https://api.poeditor.com/v2/terms/add", new FormUrlEncodedContent( [
        new KeyValuePair<string, string>( "api_token", apiToken ), new KeyValuePair<string, string>( "id", PROJECT_ID.ToString() ),
        new KeyValuePair<string, string>( "data", JsonSerializer.Serialize( terms ) )
    ] ) );
    response.EnsureSuccessStatusCode();
    await response.Content.ReadAsStringAsync();
}

async Task AddTranslations( string lang, POEditorTerm[] translations )
{
    using HttpResponseMessage response = await client.PostAsync( "https://api.poeditor.com/v2/translations/add", new FormUrlEncodedContent( [
        new KeyValuePair<string, string>( "api_token", apiToken ), new KeyValuePair<string, string>( "id", PROJECT_ID.ToString() ),
        new KeyValuePair<string, string>( "language", lang ), new KeyValuePair<string, string>( "data", JsonSerializer.Serialize( translations ) )
    ] ) );
    response.EnsureSuccessStatusCode();
    await response.Content.ReadAsStringAsync();
}