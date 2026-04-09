using System.Collections.Generic;
using System.Text.Json;
using Godot;

namespace ProiectDeLegume.Scripts.Inventory;

public static class ItemRegistry
{
    private static readonly Dictionary<string, ItemDef> Items = new();

    public static void Load()
    {
        var file = FileAccess.Open("res://assets/data/items.json", FileAccess.ModeFlags.Read);
        if (file == null)
        {
            GD.PrintErr("Cannot open items.json");
            return;
        }

        string jsonText = file.GetAsText();
        file.Close();

        var itemList = JsonSerializer.Deserialize<List<ItemDef>>(jsonText);
        if (itemList == null) return;

        foreach (var item in itemList)
        {
            Items[item.Id] = item;
        }

        GD.Print($"Loaded {Items.Count} items");
    }

    public static ItemDef Get(string id)
    {
        return Items.TryGetValue(id, out var item) ? item : throw new KeyNotFoundException($"Unknown item: {id}");
    }

    public static IEnumerable<ItemDef> All() => Items.Values;
}
