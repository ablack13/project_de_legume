using System;
using System.Collections.Generic;
using Godot;
using ProiectDeLegume.Scripts.World;

namespace ProiectDeLegume.Scripts.Fov;

/// <summary>
/// Fog of War with DDA raycasting. Port from KorGE version.
/// Renders as a Sprite2D with a per-tile Image overlay.
/// </summary>
public partial class FieldOfView : Sprite2D
{
    private const double FovAngle = 120.0;     // degrees
    private const int FovRadius = 12;          // tiles
    private const double RayStep = 0.5;        // degrees between rays
    private const int BehindRadius = 3;        // peripheral vision

    private const int TileSize = TileSetGenerator.TileSize;

    private int _mapW;
    private int _mapH;

    // 0 = unknown, 1 = explored, 2 = visible
    private int[] _visibility;

    private Image _fogImage;
    private ImageTexture _fogTexture;

    // Tile data for vision blocking
    private TileMapLayer _buildingLayer;
    private TileMapLayer _objectLayer;

    // Vision-blocking tile indices
    private static readonly HashSet<int> VisionBlockers = new()
    {
        TileSetGenerator.Wall,
        TileSetGenerator.Tree,
    };

    public void Init(int mapW, int mapH, TileMapLayer buildingLayer, TileMapLayer objectLayer)
    {
        _mapW = mapW;
        _mapH = mapH;
        _buildingLayer = buildingLayer;
        _objectLayer = objectLayer;

        _visibility = new int[_mapW * _mapH];

        _fogImage = Image.CreateEmpty(_mapW, _mapH, false, Image.Format.Rgba8);
        // Fill black
        for (int y = 0; y < _mapH; y++)
            for (int x = 0; x < _mapW; x++)
                _fogImage.SetPixel(x, y, new Color(0, 0, 0, 0.96f));

        _fogTexture = ImageTexture.CreateFromImage(_fogImage);
        Texture = _fogTexture;
        Centered = false; // origin = top-left, not center
        TextureFilter = TextureFilterEnum.Nearest;

        // Scale to cover the whole map (1 pixel per tile → TileSize pixels per tile)
        Scale = new Vector2(TileSize, TileSize);
        Position = Vector2.Zero;
        ZIndex = 5; // above tiles, below player
    }

    public void UpdateFov(double playerX, double playerY, double facingAngle, double fovMultiplier = 1.0)
    {
        int playerTileX = (int)(playerX / TileSize);
        int playerTileY = (int)(playerY / TileSize);
        int effectiveRadius = Math.Max(3, (int)(FovRadius * fovMultiplier));

        // Demote visible -> explored
        for (int i = 0; i < _mapW * _mapH; i++)
        {
            if (_visibility[i] == 2) _visibility[i] = 1;
        }

        // Peripheral vision (short rays in all directions, wall-aware)
        for (int deg = 0; deg < 360; deg += 2)
        {
            CastRay(playerX, playerY, deg * Math.PI / 180.0, BehindRadius);
        }

        // FOV cone rays
        double halfFov = FovAngle / 2.0;
        double startAngle = facingAngle * 180.0 / Math.PI - halfFov;
        double endAngle = facingAngle * 180.0 / Math.PI + halfFov;

        for (double angle = startAngle; angle <= endAngle; angle += RayStep)
        {
            CastRay(playerX, playerY, angle * Math.PI / 180.0, effectiveRadius);
        }

        // Update fog image
        UpdateFogImage();
    }

    private void CastRay(double originX, double originY, double angleRad, int maxTiles)
    {
        double dirX = Math.Cos(angleRad);
        double dirY = Math.Sin(angleRad);
        double ts = TileSize;
        double maxDistSq = (maxTiles * ts) * (maxTiles * ts);

        int tileX = (int)(originX / ts);
        int tileY = (int)(originY / ts);

        int stepX = dirX >= 0 ? 1 : -1;
        int stepY = dirY >= 0 ? 1 : -1;

        double tDeltaX = dirX != 0 ? Math.Abs(ts / dirX) : double.MaxValue;
        double tDeltaY = dirY != 0 ? Math.Abs(ts / dirY) : double.MaxValue;

        double tMaxX = dirX != 0
            ? Math.Abs(((dirX > 0 ? (tileX + 1) * ts : tileX * ts) - originX) / dirX)
            : double.MaxValue;
        double tMaxY = dirY != 0
            ? Math.Abs(((dirY > 0 ? (tileY + 1) * ts : tileY * ts) - originY) / dirY)
            : double.MaxValue;

        int steps = 0;
        int maxSteps = maxTiles * 3;

        while (steps < maxSteps)
        {
            if (tileX < 0 || tileX >= _mapW || tileY < 0 || tileY >= _mapH) break;

            double cx = tileX * ts + ts / 2 - originX;
            double cy = tileY * ts + ts / 2 - originY;
            if (cx * cx + cy * cy > maxDistSq) break;

            _visibility[tileY * _mapW + tileX] = 2;

            // Stop at vision-blocking tiles
            if (BlocksVision(tileX, tileY)) break;

            if (tMaxX < tMaxY)
            {
                tMaxX += tDeltaX;
                tileX += stepX;
            }
            else
            {
                tMaxY += tDeltaY;
                tileY += stepY;
            }
            steps++;
        }
    }

    private bool BlocksVision(int x, int y)
    {
        var pos = new Vector2I(x, y);

        // Check building layer
        var buildingCoords = _buildingLayer.GetCellAtlasCoords(pos);
        if (buildingCoords.X >= 0 && VisionBlockers.Contains(buildingCoords.X))
            return true;

        // Check object layer
        var objectCoords = _objectLayer.GetCellAtlasCoords(pos);
        if (objectCoords.X >= 0 && VisionBlockers.Contains(objectCoords.X))
            return true;

        return false;
    }

    private void UpdateFogImage()
    {
        for (int y = 0; y < _mapH; y++)
        {
            for (int x = 0; x < _mapW; x++)
            {
                int v = _visibility[y * _mapW + x];
                Color color = v switch
                {
                    2 => new Color(0, 0, 0, 0),        // visible — clear
                    1 => new Color(0, 0, 0, 0.55f),    // explored — dimmed
                    _ => new Color(0, 0, 0, 0.96f),    // unknown — nearly black
                };
                _fogImage.SetPixel(x, y, color);
            }
        }

        _fogTexture.Update(_fogImage);
    }
}
