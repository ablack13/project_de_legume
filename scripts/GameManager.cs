using System.Collections.Generic;
using Godot;
using ProiectDeLegume.Scripts.Fov;
using ProiectDeLegume.Scripts.Inventory;
using ProiectDeLegume.Scripts.Localization;
using ProiectDeLegume.Scripts.UI;
using ProiectDeLegume.Scripts.World;

namespace ProiectDeLegume.Scripts;

public partial class GameManager : Node2D
{
    private static readonly HashSet<int> SolidTiles = new()
    {
        TileSetGenerator.Wall, TileSetGenerator.Tree, TileSetGenerator.Bush,
        TileSetGenerator.Bench, TileSetGenerator.Fence, TileSetGenerator.Window,
        TileSetGenerator.Wardrobe, TileSetGenerator.KitchenCabinet,
        TileSetGenerator.Medkit, TileSetGenerator.GarageShelf,
        TileSetGenerator.Table, TileSetGenerator.Bed,
    };

    private Player.Player _player;
    private FieldOfView _fov;
    private HUD _hud;
    private ContainerUI _containerUI;
    private InventoryUI _inventoryUI;
    private ContainerManager _containerManager;

    public override void _Ready()
    {
        Lang.Load("uk");
        ItemRegistry.Load();

        // Generate tileset
        var tileImage = TileSetGenerator.GenerateTileSetImage();
        var texture = ImageTexture.CreateFromImage(tileImage);

        var tileSet = new TileSet();
        tileSet.TileSize = new Vector2I(TileSetGenerator.TileSize, TileSetGenerator.TileSize);

        var source = new TileSetAtlasSource();
        source.Texture = texture;
        source.TextureRegionSize = new Vector2I(TileSetGenerator.TileSize, TileSetGenerator.TileSize);

        for (int i = 0; i < TileSetGenerator.TileCount; i++)
            source.CreateTile(new Vector2I(i, 0));

        tileSet.AddSource(source);

        // Collision on solid tiles
        tileSet.AddPhysicsLayer();
        tileSet.SetPhysicsLayerCollisionLayer(0, 1);

        foreach (int tileIdx in SolidTiles)
        {
            var tileData = source.GetTileData(new Vector2I(tileIdx, 0), 0);
            if (tileData != null)
            {
                float half = TileSetGenerator.TileSize / 2f;
                var polygon = new Vector2[]
                {
                    new(-half, -half), new(half, -half),
                    new(half, half), new(-half, half),
                };
                tileData.AddCollisionPolygon(0);
                tileData.SetCollisionPolygonPoints(0, 0, polygon);
            }
        }

        // TileMapLayers
        var groundLayer = new TileMapLayer { Name = "GroundLayer", TileSet = tileSet };
        AddChild(groundLayer);

        var buildingLayer = new TileMapLayer { Name = "BuildingLayer", TileSet = tileSet, CollisionEnabled = true };
        AddChild(buildingLayer);

        var objectLayer = new TileMapLayer { Name = "ObjectLayer", TileSet = tileSet, CollisionEnabled = true };
        AddChild(objectLayer);

        MapGenerator.Generate(groundLayer, buildingLayer, objectLayer);

        // Container manager — scan for lootable tiles
        _containerManager = new ContainerManager();
        _containerManager.ScanMap(objectLayer);

        // FOV overlay
        _fov = new FieldOfView();
        _fov.Name = "FOV";
        AddChild(_fov);
        _fov.Init(MapGenerator.MapWidth, MapGenerator.MapHeight, buildingLayer, objectLayer);

        // Player
        _player = GetNode<Player.Player>("Player");
        if (_player != null)
        {
            _player.Position = new Vector2(37 * 32 + 16, 27 * 32 + 16);
            _player.ZIndex = 10;
            MoveChild(_player, -1);
        }

        // HUD (CanvasLayer)
        _hud = new HUD();
        _hud.Name = "HUD";
        AddChild(_hud);

        // Container UI (on CanvasLayer so it's screen-space)
        var uiLayer = new CanvasLayer();
        uiLayer.Name = "UILayer";
        AddChild(uiLayer);

        _containerUI = new ContainerUI();
        _containerUI.Name = "ContainerUI";
        uiLayer.AddChild(_containerUI);

        _inventoryUI = new InventoryUI();
        _inventoryUI.Name = "InventoryUI";
        uiLayer.AddChild(_inventoryUI);

        GD.Print("Game ready!");
    }

    public override void _Process(double delta)
    {
        if (_player == null) return;

        if (_player.Stats.IsDead)
        {
            _hud?.ShowPrompt(Lang.Get("prompt.died"));
            return;
        }

        // Don't move player while UI is open
        _player.SetPhysicsProcess(!_containerUI.IsOpen && !_inventoryUI.IsOpen);

        // Update FOV
        _fov?.UpdateFov(_player.GlobalPosition.X, _player.GlobalPosition.Y,
            _player.FacingAngle, _player.Stats.FovMultiplier);

        // Update HUD
        _hud?.UpdateStats(_player.Stats);

        // Check for nearby container
        var nearby = _containerManager.FindNearby(_player.GlobalPosition.X, _player.GlobalPosition.Y);
        if (nearby != null && !_containerUI.IsOpen)
            _hud?.ShowPrompt(Lang.Get("prompt.search"));
        else if (!_containerUI.IsOpen)
            _hud?.HidePrompt();
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (_player == null || _player.Stats.IsDead) return;

        if (@event.IsActionPressed("interact"))
        {
            var nearby = _containerManager.FindNearby(_player.GlobalPosition.X, _player.GlobalPosition.Y);
            if (nearby != null)
            {
                if (_containerUI.IsOpen)
                {
                    _containerUI.Close();
                    _hud?.HidePrompt();
                }
                else
                {
                    _containerManager.Open(nearby);
                    _containerUI.Open(nearby, _player.PlayerInventory, _player.Stats, () =>
                    {
                        if (_inventoryUI.IsOpen)
                            _inventoryUI.RefreshItems();
                    });
                }
            }
        }
        else if (@event.IsActionPressed("inventory"))
        {
            _inventoryUI.Toggle(_player.PlayerInventory, _player.Stats);
        }
        else if (@event.IsActionPressed("ui_cancel")) // Esc
        {
            if (_containerUI.IsOpen) _containerUI.Close();
            if (_inventoryUI.IsOpen) _inventoryUI.Close();
        }
    }
}
