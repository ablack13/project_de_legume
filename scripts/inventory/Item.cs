#nullable enable
using System.Text.Json.Serialization;

namespace ProiectDeLegume.Scripts.Inventory;

public class ItemDef
{
    [JsonPropertyName("id")] public string Id { get; set; } = "";
    [JsonPropertyName("name")] public string Name { get; set; } = "";
    [JsonPropertyName("description")] public string Description { get; set; } = "";
    [JsonPropertyName("weight")] public double Weight { get; set; }
    [JsonPropertyName("category")] public string Category { get; set; } = "";
    [JsonPropertyName("stackable")] public bool Stackable { get; set; }
    [JsonPropertyName("maxStack")] public int MaxStack { get; set; } = 1;
    [JsonPropertyName("useAction")] public string? UseAction { get; set; }
    [JsonPropertyName("hungerRestore")] public double HungerRestore { get; set; }
    [JsonPropertyName("thirstRestore")] public double ThirstRestore { get; set; }
    [JsonPropertyName("hpRestore")] public double HpRestore { get; set; }

    public bool IsUsable => UseAction != null;

    public string LocalizedName => Localization.Lang.Get($"item.{Id}");

    public string UseLabel => UseAction switch
    {
        "eat" => Localization.Lang.Get("action.eat"),
        "drink" => Localization.Lang.Get("action.drink"),
        "heal" => Localization.Lang.Get("action.use"),
        _ => Localization.Lang.Get("action.use")
    };
}

public class ItemStack
{
    public ItemDef Def { get; }
    public int Count { get; set; }

    public ItemStack(ItemDef def, int count = 1)
    {
        Def = def;
        Count = count;
    }

    public double TotalWeight => Def.Weight * Count;
    public bool CanAdd() => Def.Stackable && Count < Def.MaxStack;
}
