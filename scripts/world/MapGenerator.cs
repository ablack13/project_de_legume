using Godot;

namespace ProiectDeLegume.Scripts.World;

using T = TileSetGenerator;

/// <summary>
/// Generates the city map programmatically on a TileMapLayer.
/// Port of KorGE GameWorld.generateCityMap().
/// </summary>
public static class MapGenerator
{
    public const int MapWidth = 80;
    public const int MapHeight = 60;

    public static void Generate(TileMapLayer ground, TileMapLayer buildings, TileMapLayer objects)
    {
        // Fill with grass
        for (int y = 0; y < MapHeight; y++)
            for (int x = 0; x < MapWidth; x++)
                ground.SetCell(new Vector2I(x, y), 0, new Vector2I(T.Grass, 0));

        // Dark grass patches
        int[][] darkPatches = { new[]{2,2}, new[]{15,5}, new[]{60,10}, new[]{35,45}, new[]{70,50}, new[]{5,50}, new[]{50,25} };
        foreach (var p in darkPatches)
            for (int dy = -2; dy <= 2; dy++)
                for (int dx = -2; dx <= 2; dx++)
                    if (dx*dx + dy*dy <= 5)
                        SetIfInBounds(ground, p[0]+dx, p[1]+dy, T.GrassDark);

        // === ROADS ===
        FillRow(ground, 0, MapWidth-1, 28, 29, T.Road);
        FillRow(ground, 0, MapWidth-1, 14, 15, T.Road);
        FillCol(ground, 38, 39, 0, MapHeight-1, T.Road);
        FillCol(ground, 60, 61, 0, MapHeight-1, T.Road);
        FillCol(ground, 15, 16, 42, MapHeight-1, T.Road);

        // === SIDEWALKS ===
        for (int x = 0; x < MapWidth; x++)
        {
            SetIfInBounds(ground, x, 13, T.Sidewalk); SetIfInBounds(ground, x, 16, T.Sidewalk);
            SetIfInBounds(ground, x, 27, T.Sidewalk); SetIfInBounds(ground, x, 30, T.Sidewalk);
        }
        for (int y = 0; y < MapHeight; y++)
        {
            SetIfInBounds(ground, 37, y, T.Sidewalk); SetIfInBounds(ground, 40, y, T.Sidewalk);
            SetIfInBounds(ground, 59, y, T.Sidewalk); SetIfInBounds(ground, 62, y, T.Sidewalk);
        }

        // === BUILDINGS ===
        PlaceHouse(ground, buildings, objects, 3, 3, 10, 8, "family");
        PlaceHouse(ground, buildings, objects, 16, 4, 7, 6, "cottage");
        PlaceHouse(ground, buildings, objects, 26, 3, 9, 7, "apartment");
        PlaceHouse(ground, buildings, objects, 42, 3, 8, 6, "garage");
        PlaceHouse(ground, buildings, objects, 53, 4, 6, 7, "shop");
        PlaceHouse(ground, buildings, objects, 64, 3, 8, 6, "family");
        PlaceHouse(ground, buildings, objects, 4, 18, 8, 7, "clinic");
        PlaceHouse(ground, buildings, objects, 15, 18, 8, 6, "cottage");
        PlaceHouse(ground, buildings, objects, 26, 18, 10, 8, "apartment");
        PlaceHouse(ground, buildings, objects, 42, 18, 12, 8, "garage");
        PlaceHouse(ground, buildings, objects, 64, 18, 9, 7, "family");
        PlaceHouse(ground, buildings, objects, 3, 33, 9, 7, "family");
        PlaceHouse(ground, buildings, objects, 20, 33, 8, 6, "cottage");
        PlaceHouse(ground, buildings, objects, 42, 33, 10, 8, "apartment");
        PlaceHouse(ground, buildings, objects, 55, 34, 5, 4, "garage");
        PlaceHouse(ground, buildings, objects, 64, 33, 9, 7, "family");
        PlaceHouse(ground, buildings, objects, 3, 46, 7, 6, "cottage");
        PlaceHouse(ground, buildings, objects, 20, 46, 10, 7, "apartment");
        PlaceHouse(ground, buildings, objects, 42, 46, 8, 6, "family");
        PlaceHouse(ground, buildings, objects, 64, 46, 8, 7, "clinic");

        // === TREES ===
        for (int x = 2; x < MapWidth; x += 5)
        {
            SetIfGrass(objects, ground, x, 12, T.Tree);
            SetIfGrass(objects, ground, x, 17, T.Tree);
            SetIfGrass(objects, ground, x, 26, T.Tree);
            SetIfGrass(objects, ground, x, 31, T.Tree);
        }
        for (int y = 2; y < MapHeight; y += 5)
        {
            SetIfGrass(objects, ground, 36, y, T.Tree);
            SetIfGrass(objects, ground, 41, y, T.Tree);
        }
        // Park
        for (int dy = 0; dy <= 4; dy++)
            for (int dx = 0; dx <= 4; dx++)
                if ((dx + dy) % 2 == 0)
                    SetIfGrass(objects, ground, 44 + dx, 10 + dy, T.Tree);

        int[][] scatteredTrees = { new[]{1,1}, new[]{14,2}, new[]{75,5}, new[]{78,12}, new[]{2,30}, new[]{18,31},
            new[]{70,42}, new[]{76,55}, new[]{3,55}, new[]{30,50}, new[]{55,52}, new[]{68,55} };
        foreach (var t in scatteredTrees)
            SetIfGrass(objects, ground, t[0], t[1], T.Tree);

        // === BUSHES ===
        int[][] bushes = { new[]{2,5}, new[]{13,3}, new[]{24,10}, new[]{35,5}, new[]{50,2}, new[]{63,10},
            new[]{75,3}, new[]{5,25}, new[]{20,27}, new[]{50,30}, new[]{65,27},
            new[]{10,40}, new[]{30,43}, new[]{55,45}, new[]{72,48},
            new[]{8,53}, new[]{25,55}, new[]{48,54}, new[]{60,56} };
        foreach (var b in bushes)
            SetIfGrass(objects, ground, b[0], b[1], T.Bush);

        // === BENCHES ===
        int[][] benchPositions = { new[]{6,13}, new[]{18,13}, new[]{30,13}, new[]{48,16}, new[]{66,16},
            new[]{8,27}, new[]{22,30}, new[]{45,27}, new[]{67,30}, new[]{10,44}, new[]{50,44}, new[]{65,44} };
        foreach (var b in benchPositions)
            SetOnSidewalk(objects, ground, b[0], b[1], T.Bench);

        // === FENCES ===
        for (int x = 2; x <= 13; x++) { SetIfGrass(objects, ground, x, 2, T.Fence); SetIfGrass(objects, ground, x, 11, T.Fence); }
        for (int y = 2; y <= 11; y++) { SetIfGrass(objects, ground, 2, y, T.Fence); SetIfGrass(objects, ground, 13, y, T.Fence); }
        objects.SetCell(new Vector2I(8, 11), -1); // gate
        for (int x = 43; x <= 49; x++) SetIfGrass(objects, ground, x, 9, T.Fence);

        // === TRASH CANS ===
        int[][] trashCans = { new[]{7,16}, new[]{19,16}, new[]{33,13}, new[]{47,13}, new[]{57,16}, new[]{70,13},
            new[]{5,30}, new[]{22,27}, new[]{50,30}, new[]{66,27},
            new[]{10,43}, new[]{25,43}, new[]{48,43}, new[]{67,43},
            new[]{8,56}, new[]{35,56}, new[]{55,56} };
        foreach (var tc in trashCans)
            objects.SetCell(new Vector2I(tc[0], tc[1]), 0, new Vector2I(T.TrashCan, 0));
    }

    private static void PlaceHouse(TileMapLayer ground, TileMapLayer buildings, TileMapLayer objects,
        int bx, int by, int w, int h, string type)
    {
        // Floor
        for (int y = by; y < by + h; y++)
            for (int x = bx; x < bx + w; x++)
                ground.SetCell(new Vector2I(x, y), 0, new Vector2I(T.Floor, 0));

        // Walls
        for (int y = by; y < by + h; y++)
            for (int x = bx; x < bx + w; x++)
            {
                bool isWall = y == by || y == by + h - 1 || x == bx || x == bx + w - 1;
                if (isWall)
                    buildings.SetCell(new Vector2I(x, y), 0, new Vector2I(T.Wall, 0));
            }

        // Door
        buildings.SetCell(new Vector2I(bx + w / 2, by + h - 1), 0, new Vector2I(T.Door, 0));

        // Windows
        for (int x = bx + 2; x < bx + w - 2; x += 3)
            buildings.SetCell(new Vector2I(x, by), 0, new Vector2I(T.Window, 0));
        for (int y = by + 2; y < by + h - 2; y += 3)
        {
            buildings.SetCell(new Vector2I(bx, y), 0, new Vector2I(T.Window, 0));
            buildings.SetCell(new Vector2I(bx + w - 1, y), 0, new Vector2I(T.Window, 0));
        }

        // Interior
        switch (type)
        {
            case "family":
                FillRect(ground, bx + 2, by + 2, bx + w - 3, by + h - 3, T.Carpet);
                objects.SetCell(new Vector2I(bx + 1, by + 1), 0, new Vector2I(T.KitchenCabinet, 0));
                objects.SetCell(new Vector2I(bx + 2, by + 1), 0, new Vector2I(T.KitchenCabinet, 0));
                objects.SetCell(new Vector2I(bx + w / 2, by + h / 2), 0, new Vector2I(T.Table, 0));
                objects.SetCell(new Vector2I(bx + w - 2, by + 1), 0, new Vector2I(T.Bed, 0));
                objects.SetCell(new Vector2I(bx + w - 2, by + 2), 0, new Vector2I(T.Bed, 0));
                objects.SetCell(new Vector2I(bx + w - 2, by + 3), 0, new Vector2I(T.Wardrobe, 0));
                break;
            case "cottage":
                objects.SetCell(new Vector2I(bx + 1, by + 1), 0, new Vector2I(T.KitchenCabinet, 0));
                objects.SetCell(new Vector2I(bx + w - 2, by + 1), 0, new Vector2I(T.Bed, 0));
                objects.SetCell(new Vector2I(bx + 1, by + h - 2), 0, new Vector2I(T.Wardrobe, 0));
                objects.SetCell(new Vector2I(bx + w / 2, by + h / 2), 0, new Vector2I(T.Table, 0));
                break;
            case "apartment":
                FillRect(ground, bx + 1, by + 1, bx + w - 2, by + h - 2, T.TileFloor);
                objects.SetCell(new Vector2I(bx + 1, by + 1), 0, new Vector2I(T.KitchenCabinet, 0));
                objects.SetCell(new Vector2I(bx + 2, by + 1), 0, new Vector2I(T.KitchenCabinet, 0));
                objects.SetCell(new Vector2I(bx + 3, by + 1), 0, new Vector2I(T.KitchenCabinet, 0));
                objects.SetCell(new Vector2I(bx + w / 2, by + 2), 0, new Vector2I(T.Table, 0));
                objects.SetCell(new Vector2I(bx + w / 2 + 1, by + 2), 0, new Vector2I(T.Table, 0));
                objects.SetCell(new Vector2I(bx + 1, by + h - 2), 0, new Vector2I(T.Bed, 0));
                objects.SetCell(new Vector2I(bx + 2, by + h - 2), 0, new Vector2I(T.Bed, 0));
                objects.SetCell(new Vector2I(bx + w - 2, by + h - 2), 0, new Vector2I(T.Bed, 0));
                objects.SetCell(new Vector2I(bx + w - 2, by + 1), 0, new Vector2I(T.Wardrobe, 0));
                objects.SetCell(new Vector2I(bx + w - 2, by + 2), 0, new Vector2I(T.Wardrobe, 0));
                break;
            case "garage":
                FillRect(ground, bx + 1, by + 1, bx + w - 2, by + h - 2, T.TileFloor);
                objects.SetCell(new Vector2I(bx + 1, by + 1), 0, new Vector2I(T.GarageShelf, 0));
                objects.SetCell(new Vector2I(bx + 2, by + 1), 0, new Vector2I(T.GarageShelf, 0));
                objects.SetCell(new Vector2I(bx + 1, by + 2), 0, new Vector2I(T.GarageShelf, 0));
                objects.SetCell(new Vector2I(bx + w - 2, by + 1), 0, new Vector2I(T.GarageShelf, 0));
                objects.SetCell(new Vector2I(bx + w - 2, by + 2), 0, new Vector2I(T.GarageShelf, 0));
                objects.SetCell(new Vector2I(bx + w / 2, by + h / 2), 0, new Vector2I(T.Table, 0));
                break;
            case "clinic":
                FillRect(ground, bx + 1, by + 1, bx + w - 2, by + h - 2, T.TileFloor);
                objects.SetCell(new Vector2I(bx + 1, by + 1), 0, new Vector2I(T.Medkit, 0));
                objects.SetCell(new Vector2I(bx + 2, by + 1), 0, new Vector2I(T.Medkit, 0));
                objects.SetCell(new Vector2I(bx + 1, by + 2), 0, new Vector2I(T.Medkit, 0));
                objects.SetCell(new Vector2I(bx + w / 2, by + h / 2), 0, new Vector2I(T.Table, 0));
                objects.SetCell(new Vector2I(bx + w - 2, by + 1), 0, new Vector2I(T.Bed, 0));
                objects.SetCell(new Vector2I(bx + w - 2, by + 2), 0, new Vector2I(T.Bed, 0));
                break;
            case "shop":
                objects.SetCell(new Vector2I(bx + 1, by + 1), 0, new Vector2I(T.GarageShelf, 0));
                objects.SetCell(new Vector2I(bx + 1, by + 2), 0, new Vector2I(T.GarageShelf, 0));
                objects.SetCell(new Vector2I(bx + 1, by + 3), 0, new Vector2I(T.GarageShelf, 0));
                objects.SetCell(new Vector2I(bx + w - 2, by + 1), 0, new Vector2I(T.KitchenCabinet, 0));
                objects.SetCell(new Vector2I(bx + w - 2, by + 2), 0, new Vector2I(T.KitchenCabinet, 0));
                objects.SetCell(new Vector2I(bx + w / 2, by + h - 2), 0, new Vector2I(T.Table, 0));
                break;
        }
    }

    // Helpers
    private static void SetIfInBounds(TileMapLayer layer, int x, int y, int tile)
    {
        if (x >= 0 && x < MapWidth && y >= 0 && y < MapHeight)
            layer.SetCell(new Vector2I(x, y), 0, new Vector2I(tile, 0));
    }

    private static void SetIfGrass(TileMapLayer objects, TileMapLayer ground, int x, int y, int tile)
    {
        if (x < 0 || x >= MapWidth || y < 0 || y >= MapHeight) return;
        var groundTile = ground.GetCellAtlasCoords(new Vector2I(x, y));
        if (groundTile.X == T.Grass || groundTile.X == T.GrassDark)
            objects.SetCell(new Vector2I(x, y), 0, new Vector2I(tile, 0));
    }

    private static void SetOnSidewalk(TileMapLayer objects, TileMapLayer ground, int x, int y, int tile)
    {
        if (x < 0 || x >= MapWidth || y < 0 || y >= MapHeight) return;
        var groundTile = ground.GetCellAtlasCoords(new Vector2I(x, y));
        if (groundTile.X == T.Sidewalk)
            objects.SetCell(new Vector2I(x, y), 0, new Vector2I(tile, 0));
    }

    private static void FillRow(TileMapLayer layer, int x1, int x2, int y1, int y2, int tile)
    {
        for (int y = y1; y <= y2; y++)
            for (int x = x1; x <= x2; x++)
                layer.SetCell(new Vector2I(x, y), 0, new Vector2I(tile, 0));
    }

    private static void FillCol(TileMapLayer layer, int x1, int x2, int y1, int y2, int tile)
    {
        for (int y = y1; y <= y2; y++)
            for (int x = x1; x <= x2; x++)
                layer.SetCell(new Vector2I(x, y), 0, new Vector2I(tile, 0));
    }

    private static void FillRect(TileMapLayer layer, int x1, int y1, int x2, int y2, int tile)
    {
        for (int y = y1; y <= y2; y++)
            for (int x = x1; x <= x2; x++)
                layer.SetCell(new Vector2I(x, y), 0, new Vector2I(tile, 0));
    }
}
