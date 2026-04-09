using System.Collections.Generic;
using System.Text.Json;
using Godot;

namespace ProiectDeLegume.Scripts.Localization;

public static class Lang
{
    private static Dictionary<string, string> _strings = new();

    public static void Load(string locale = "en")
    {
        var file = FileAccess.Open($"res://assets/data/lang_{locale}.json", FileAccess.ModeFlags.Read);
        if (file == null)
        {
            GD.PrintErr($"Cannot open lang_{locale}.json");
            return;
        }

        string jsonText = file.GetAsText();
        file.Close();

        var dict = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonText);
        if (dict != null) _strings = dict;

        GD.Print($"Loaded {_strings.Count} strings for locale '{locale}'");
    }

    public static string Get(string key)
    {
        return _strings.TryGetValue(key, out var value) ? value : key;
    }

    public static string Get(string key, params (string placeholder, string value)[] args)
    {
        if (!_strings.TryGetValue(key, out var result)) return key;
        foreach (var (placeholder, value) in args)
        {
            result = result.Replace($"{{{placeholder}}}", value);
        }
        return result;
    }
}
