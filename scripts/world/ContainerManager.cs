using System;
using System.Collections.Generic;
using Godot;
using ProiectDeLegume.Scripts.Inventory;
using ProiectDeLegume.Scripts.Localization;

namespace ProiectDeLegume.Scripts.World;

public class ContainerData
{
    public int TileX { get; }
    public int TileY { get; }
    public string Type { get; }
    public List<ItemStack> Items { get; } = new();
    public bool Opened { get; set; }

    public ContainerData(int tileX, int tileY, string type)
    {
        TileX = tileX;
        TileY = tileY;
        Type = type;
    }

    public string DisplayName => Lang.Get($"container.name.{Type}");
}

public class ContainerManager
{
    private readonly Dictionary<long, ContainerData> _containers = new();
    private const int TileSize = TileSetGenerator.TileSize;

    // Container tile indices and their loot table type
    private static readonly Dictionary<int, string> ContainerTiles = new()
    {
        { TileSetGenerator.TrashCan, "trash_can" },
        { TileSetGenerator.Wardrobe, "wardrobe" },
        { TileSetGenerator.KitchenCabinet, "kitchen" },
        { TileSetGenerator.Medkit, "medkit" },
        { TileSetGenerator.GarageShelf, "garage" },
    };

    public void ScanMap(TileMapLayer objectLayer)
    {
        for (int y = 0; y < MapGenerator.MapHeight; y++)
        {
            for (int x = 0; x < MapGenerator.MapWidth; x++)
            {
                var coords = objectLayer.GetCellAtlasCoords(new Vector2I(x, y));
                if (coords.X >= 0 && ContainerTiles.TryGetValue(coords.X, out var type))
                {
                    _containers[PosKey(x, y)] = new ContainerData(x, y, type);
                }
            }
        }

        GD.Print($"Found {_containers.Count} containers");
    }

    private long PosKey(int x, int y) => (long)y * MapGenerator.MapWidth + x;

    public ContainerData FindNearby(double px, double py, double range = 48.0)
    {
        double rangeSq = range * range;
        foreach (var container in _containers.Values)
        {
            double cx = container.TileX * TileSize + TileSize / 2.0;
            double cy = container.TileY * TileSize + TileSize / 2.0;
            double dx = px - cx;
            double dy = py - cy;
            if (dx * dx + dy * dy <= rangeSq)
                return container;
        }
        return null;
    }

    public List<ItemStack> Open(ContainerData container)
    {
        if (!container.Opened)
        {
            container.Items.AddRange(LootTables.Generate(container.Type));
            container.Opened = true;
        }
        return container.Items;
    }
}
