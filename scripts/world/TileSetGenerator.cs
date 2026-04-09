using Godot;

namespace ProiectDeLegume.Scripts.World;

/// <summary>
/// Generates a tileset PNG at runtime for prototyping.
/// Each tile is 32x32, arranged in a single row.
/// </summary>
public static class TileSetGenerator
{
    public const int TileSize = 32;

    // Tile indices (matching the order in the generated image)
    public const int Grass = 0;
    public const int Road = 1;
    public const int Wall = 2;
    public const int Floor = 3;
    public const int Door = 4;
    public const int TrashCan = 5;
    public const int Tree = 6;
    public const int Bush = 7;
    public const int Bench = 8;
    public const int Fence = 9;
    public const int Sidewalk = 10;
    public const int GrassDark = 11;
    public const int Wardrobe = 12;
    public const int KitchenCabinet = 13;
    public const int Medkit = 14;
    public const int GarageShelf = 15;
    public const int Table = 16;
    public const int Bed = 17;
    public const int Carpet = 18;
    public const int TileFloor = 19;
    public const int Window = 20;

    public const int TileCount = 21;

    private static readonly Color[] TileColors =
    {
        new(0.29f, 0.49f, 0.35f),  // 0  Grass
        new(0.55f, 0.52f, 0.50f),  // 1  Road
        new(0.36f, 0.25f, 0.20f),  // 2  Wall
        new(0.77f, 0.66f, 0.51f),  // 3  Floor
        new(0.55f, 0.41f, 0.08f),  // 4  Door
        new(0.33f, 0.42f, 0.18f),  // 5  TrashCan
        new(0.18f, 0.35f, 0.12f),  // 6  Tree
        new(0.23f, 0.42f, 0.16f),  // 7  Bush
        new(0.55f, 0.45f, 0.33f),  // 8  Bench
        new(0.42f, 0.36f, 0.23f),  // 9  Fence
        new(0.63f, 0.60f, 0.54f),  // 10 Sidewalk
        new(0.24f, 0.42f, 0.29f),  // 11 GrassDark
        new(0.55f, 0.43f, 0.31f),  // 12 Wardrobe
        new(0.72f, 0.62f, 0.47f),  // 13 KitchenCabinet
        new(0.77f, 0.25f, 0.25f),  // 14 Medkit
        new(0.48f, 0.42f, 0.31f),  // 15 GarageShelf
        new(0.62f, 0.55f, 0.43f),  // 16 Table
        new(0.42f, 0.35f, 0.54f),  // 17 Bed
        new(0.55f, 0.25f, 0.25f),  // 18 Carpet
        new(0.83f, 0.78f, 0.69f),  // 19 TileFloor
        new(0.56f, 0.78f, 0.91f),  // 20 Window
    };

    public static Image GenerateTileSetImage()
    {
        var image = Image.CreateEmpty(TileSize * TileCount, TileSize, false, Image.Format.Rgba8);

        for (int tileIdx = 0; tileIdx < TileCount; tileIdx++)
        {
            var baseColor = TileColors[tileIdx];
            int ox = tileIdx * TileSize;

            // Fill base color
            for (int y = 0; y < TileSize; y++)
                for (int x = 0; x < TileSize; x++)
                    image.SetPixel(ox + x, y, baseColor);

            // Add details per tile type
            switch (tileIdx)
            {
                case Wall:
                    DrawRect(image, ox + 1, 1, TileSize - 2, TileSize - 2, new Color(0.42f, 0.31f, 0.24f));
                    break;
                case Door:
                    DrawRect(image, ox + 6, 0, 20, TileSize, new Color(0.72f, 0.53f, 0.04f));
                    break;
                case TrashCan:
                    // Grass background
                    for (int y = 0; y < TileSize; y++)
                        for (int x = 0; x < TileSize; x++)
                            image.SetPixel(ox + x, y, TileColors[Grass]);
                    DrawRect(image, ox + 8, 6, 16, 20, new Color(0.24f, 0.36f, 0.23f));
                    break;
                case Tree:
                    // Trunk
                    DrawRect(image, ox + 13, 20, 6, 10, new Color(0.36f, 0.24f, 0.12f));
                    // Canopy
                    DrawRect(image, ox + 4, 2, 24, 22, new Color(0.12f, 0.36f, 0.12f));
                    DrawRect(image, ox + 6, 4, 20, 18, new Color(0.15f, 0.45f, 0.15f));
                    break;
                case Bush:
                    for (int y = 0; y < TileSize; y++)
                        for (int x = 0; x < TileSize; x++)
                            image.SetPixel(ox + x, y, TileColors[Grass]);
                    DrawRect(image, ox + 5, 10, 22, 16, new Color(0.18f, 0.42f, 0.18f));
                    DrawRect(image, ox + 7, 12, 18, 12, new Color(0.23f, 0.54f, 0.23f));
                    break;
                case Bench:
                    for (int y = 0; y < TileSize; y++)
                        for (int x = 0; x < TileSize; x++)
                            image.SetPixel(ox + x, y, TileColors[Sidewalk]);
                    DrawRect(image, ox + 2, 12, 28, 8, new Color(0.42f, 0.31f, 0.18f));
                    DrawRect(image, ox + 4, 16, 4, 12, new Color(0.29f, 0.21f, 0.13f));
                    DrawRect(image, ox + 24, 16, 4, 12, new Color(0.29f, 0.21f, 0.13f));
                    break;
                case Fence:
                    for (int y = 0; y < TileSize; y++)
                        for (int x = 0; x < TileSize; x++)
                            image.SetPixel(ox + x, y, TileColors[Grass]);
                    DrawRect(image, ox, 14, TileSize, 4, new Color(0.35f, 0.29f, 0.19f));
                    DrawRect(image, ox + 2, 6, 4, 20, new Color(0.35f, 0.29f, 0.19f));
                    DrawRect(image, ox + 26, 6, 4, 20, new Color(0.35f, 0.29f, 0.19f));
                    break;
                case Wardrobe:
                    DrawRect(image, ox + 2, 2, TileSize - 4, TileSize - 4, new Color(0.48f, 0.37f, 0.24f));
                    DrawRect(image, ox + TileSize / 2 - 1, 4, 2, TileSize - 8, new Color(0.36f, 0.25f, 0.19f));
                    break;
                case KitchenCabinet:
                    DrawRect(image, ox + 2, 2, TileSize - 4, TileSize - 4, new Color(0.63f, 0.53f, 0.38f));
                    DrawRect(image, ox + 6, 10, 8, 3, new Color(0.75f, 0.66f, 0.44f));
                    DrawRect(image, ox + 18, 10, 8, 3, new Color(0.75f, 0.66f, 0.44f));
                    break;
                case Medkit:
                    DrawRect(image, ox + 3, 3, TileSize - 6, TileSize - 6, new Color(0.88f, 0.88f, 0.88f));
                    DrawRect(image, ox + 9, 14, 14, 4, new Color(0.80f, 0.20f, 0.20f));
                    DrawRect(image, ox + 14, 9, 4, 14, new Color(0.80f, 0.20f, 0.20f));
                    break;
                case GarageShelf:
                    DrawRect(image, ox + 2, 2, TileSize - 4, TileSize - 4, new Color(0.42f, 0.35f, 0.25f));
                    DrawRect(image, ox + 4, 10, TileSize - 8, 2, new Color(0.54f, 0.48f, 0.35f));
                    DrawRect(image, ox + 4, 20, TileSize - 8, 2, new Color(0.54f, 0.48f, 0.35f));
                    break;
                case Table:
                    DrawRect(image, ox + 3, 3, TileSize - 6, TileSize - 6, new Color(0.54f, 0.48f, 0.37f));
                    break;
                case Bed:
                    DrawRect(image, ox + 2, 2, TileSize - 4, TileSize - 4, new Color(0.35f, 0.29f, 0.48f));
                    DrawRect(image, ox + 10, 4, 12, 8, new Color(0.88f, 0.85f, 0.78f)); // pillow
                    break;
                case Carpet:
                    DrawRect(image, ox + 1, 1, TileSize - 2, TileSize - 2, new Color(0.48f, 0.21f, 0.21f));
                    break;
                case Window:
                    // Wall base
                    DrawRect(image, ox + 1, 1, TileSize - 2, TileSize - 2, new Color(0.42f, 0.31f, 0.24f));
                    // Glass
                    DrawRect(image, ox + 4, 4, TileSize - 8, TileSize - 8, new Color(0.56f, 0.78f, 0.91f));
                    // Cross frame
                    DrawRect(image, ox + 4, TileSize / 2 - 1, TileSize - 8, 2, new Color(0.36f, 0.25f, 0.20f));
                    DrawRect(image, ox + TileSize / 2 - 1, 4, 2, TileSize - 8, new Color(0.36f, 0.25f, 0.20f));
                    break;
            }
        }

        return image;
    }

    private static void DrawRect(Image image, int x, int y, int w, int h, Color color)
    {
        for (int dy = 0; dy < h; dy++)
            for (int dx = 0; dx < w; dx++)
            {
                int px = x + dx, py = y + dy;
                if (px >= 0 && px < image.GetWidth() && py >= 0 && py < image.GetHeight())
                    image.SetPixel(px, py, color);
            }
    }
}
