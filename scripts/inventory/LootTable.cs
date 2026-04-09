using System;
using System.Collections.Generic;

namespace ProiectDeLegume.Scripts.Inventory;

public record LootEntry(string ItemId, int Weight);

public static class LootTables
{
    private static readonly Random Rng = new();

    private static readonly Dictionary<string, List<LootEntry>> Tables = new()
    {
        ["trash_can"] = new()
        {
            new("newspaper", 30), new("empty_can", 25), new("old_shirt", 10),
            new("canned_beans", 8), new("water_bottle", 5), new("screwdriver", 5),
            new("flashlight", 2)
        },
        ["kitchen"] = new()
        {
            new("canned_beans", 25), new("kitchen_knife", 15), new("matches", 15),
            new("water_bottle", 10), new("empty_can", 10)
        },
        ["wardrobe"] = new()
        {
            new("old_shirt", 40), new("flashlight", 5), new("matches", 5)
        },
        ["medkit"] = new()
        {
            new("bandage", 60), new("water_bottle", 10)
        },
        ["garage"] = new()
        {
            new("screwdriver", 30), new("flashlight", 15), new("matches", 10)
        }
    };

    public static List<ItemStack> Generate(string containerType)
    {
        if (!Tables.TryGetValue(containerType, out var table)) return new();

        int totalWeight = 0;
        foreach (var entry in table) totalWeight += entry.Weight;

        int itemCount = Rng.Next(1, 5);
        var result = new List<ItemStack>();

        for (int i = 0; i < itemCount; i++)
        {
            int roll = Rng.Next(totalWeight);
            int cumulative = 0;
            foreach (var entry in table)
            {
                cumulative += entry.Weight;
                if (roll < cumulative)
                {
                    var existing = result.Find(s => s.Def.Id == entry.ItemId && s.CanAdd());
                    if (existing != null)
                        existing.Count++;
                    else
                        result.Add(new ItemStack(ItemRegistry.Get(entry.ItemId)));
                    break;
                }
            }
        }

        return result;
    }
}
