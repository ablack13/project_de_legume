# Migration Plan: KorGE → Godot 4.6.2 (C#)

## Overview

**Previous stack:** Kotlin + KorGE 6.0 (JVM)
**New stack:** C# + Godot 4.6.2 (official support)

### Why C# instead of Kotlin?

- **Official Godot support** — not a beta plugin, not a custom editor
- **Latest Godot version** (4.6.2) — Kotlin binding only supports 4.5.1
- **No limitations** — @Tool, signals, editor plugins — everything works
- **Huge community** — tutorials, examples, support
- **C# is very similar to Kotlin** — null safety, lambdas, generics, properties
- **IDE** — Rider (JetBrains, like IntelliJ) or VS Code

### What Godot gives us over KorGE

| Aspect | KorGE (before) | Godot (after) |
|--------|----------------|---------------|
| **Tile Map** | Manual solidRect rendering | TileMapLayer with editor |
| **UI** | Manual solidRect + text | Control nodes (Panel, Label, Button, ProgressBar) |
| **Camera** | CameraContainer manual follow | Camera2D with smoothing, limits |
| **Collisions** | Manual 4-corner check | CharacterBody2D + MoveAndSlide() |
| **Sound** | Not implemented | AudioStreamPlayer2D |
| **Save/Load** | Not implemented | FileAccess / JSON |
| **Editor** | None | Full visual editor |
| **Export** | JVM only | Windows, Linux, macOS, Android, iOS, Web |
| **Dependencies** | Gradle + KorGE plugin | NuGet (System.Text.Json built-in) |

---

## Project Structure

```
proiect-de-legume/
├── project.godot                       # Godot project
├── ProiectDeLegume.csproj              # C# project (auto-generated)
├── ProiectDeLegume.sln                 # Solution (auto-generated)
│
├── scripts/
│   ├── player/
│   │   ├── Player.cs                   # CharacterBody2D: movement, collisions
│   │   └── PlayerStats.cs              # HP, Hunger, Thirst, Fatigue
│   ├── world/
│   │   ├── ContainerManager.cs         # Loot containers
│   │   ├── MapGenerator.cs             # City map generation (80x60)
│   │   ├── TileSetGenerator.cs         # Programmatic tileset image
│   │   └── LootTable.cs               # Weighted probability tables
│   ├── inventory/
│   │   ├── Item.cs                     # ItemDef + ItemStack
│   │   ├── ItemRegistry.cs             # JSON item loader
│   │   └── Inventory.cs               # Player inventory
│   ├── fov/
│   │   └── FieldOfView.cs             # DDA raycasting, fog overlay
│   ├── localization/
│   │   └── Lang.cs                     # JSON-based i18n
│   ├── ui/
│   │   ├── HUD.cs                      # ProgressBar stat bars + prompts
│   │   ├── InventoryUI.cs              # Inventory panel
│   │   └── ContainerUI.cs             # Container loot panel
│   └── GameManager.cs                  # Main controller: init, game loop, input
│
├── scenes/
│   ├── main.tscn                       # Main scene
│   └── player.tscn                     # CharacterBody2D + Sprite + Collision + Camera
│
├── assets/
│   └── data/
│       ├── items.json                  # Item registry
│       ├── lang_en.json                # English
│       └── lang_uk.json                # Ukrainian
│
└── docs/                               # Documentation
```

---

## Kotlin → C# Mapping

### Syntax

| Kotlin | C# |
|--------|-----|
| `val name: String` | `string name` or `public string Name { get; }` |
| `var count: Int = 0` | `public int Count { get; set; } = 0;` |
| `data class Item(val id: String)` | `public record Item(string Id);` or regular class |
| `fun doStuff(): Boolean` | `public bool DoStuff()` |
| `listOf(1, 2, 3)` | `new List<int> { 1, 2, 3 }` |
| `map.forEach { (k, v) -> }` | `foreach (var (k, v) in map)` |
| `it` in lambdas | `x =>` (explicit parameter) |
| `when (x) { }` | `switch (x) { }` or pattern matching |
| `object Singleton` | `public static class` or singleton pattern |
| `companion object` | `public static` members |
| `?.` (null safe) | `?.` (identical) |
| `?: default` (elvis) | `?? default` (null coalescing) |
| `coerceIn(min, max)` | `Math.Clamp(val, min, max)` |
| `coerceAtLeast(min)` | `Math.Max(val, min)` |

### File Mapping

| Kotlin file | C# file | Changes |
|-------------|---------|---------|
| PlayerStats.kt | PlayerStats.cs | Minimal: syntax, `Math.Clamp` |
| Inventory.kt | Inventory.cs | `mutableListOf` → `List<>`, LINQ |
| Item.kt | Item.cs | Class with properties, `System.Text.Json` |
| ItemRegistry.kt | ItemRegistry.cs | Godot `FileAccess` API |
| LootTable.kt | LootTable.cs | `System.Random` |
| Lang.kt | Lang.cs | `JsonDocument` instead of kotlinx |
| Player.kt | Player.cs | `CharacterBody2D`, Godot Input API |
| FieldOfView.kt | FieldOfView.cs | `Image` + `ImageTexture` Godot API |
| HUD.kt | HUD.cs | Godot ProgressBar, Label nodes |
| InventoryUI.kt | InventoryUI.cs | Godot Panel, Button, VBoxContainer |
| ContainerUI.kt | ContainerUI.cs | Godot Panel, Button, VBoxContainer |
| GameManager.kt | GameManager.cs | Node2D controller |

---

## Node Tree (Main Scene)

```
Main (Node2D) — GameManager.cs
├── GroundLayer (TileMapLayer)          # grass, roads, sidewalks, floors
├── BuildingLayer (TileMapLayer)        # walls, windows, doors
├── ObjectLayer (TileMapLayer)          # furniture, decorations, containers
├── FOV (Sprite2D) — FieldOfView.cs    # fog of war overlay
├── Player (CharacterBody2D) — Player.cs
│   ├── Sprite (ColorRect)
│   ├── DirectionIndicator (ColorRect)
│   ├── CollisionShape2D
│   ├── InteractionArea (Area2D)
│   │   └── CollisionShape2D
│   └── Camera2D
├── HUD (CanvasLayer) — HUD.cs
└── UILayer (CanvasLayer)
    ├── ContainerUI (PanelContainer) — ContainerUI.cs
    └── InventoryUI (PanelContainer) — InventoryUI.cs
```

---

## Migration Phases (Completed)

| Phase | What | Status |
|-------|------|--------|
| 0 | Setup Godot 4.6.2 + C# solution | ✅ Done |
| 1 | Pure logic (Stats, Inventory, Items, Lang) | ✅ Done |
| 2 | Map (TileMap, 80x60, 20 buildings) | ✅ Done |
| 3 | Player (CharacterBody2D, movement, collision) | ✅ Done |
| 4 | FOV (DDA raycasting + overlay) + HUD | ✅ Done |
| 5 | Containers + Inventory UI | ✅ Done |

---

## Benefits After Migration

- **Sound** — drag & drop .wav/.ogg onto AudioStreamPlayer2D
- **Animations** — AnimatedSprite2D, AnimationPlayer
- **Day/night** — CanvasModulate or DirectionalLight2D
- **Particles** — GPUParticles2D (rain, smoke, dust)
- **Navigation** — NavigationAgent2D for zombie AI
- **Shaders** — fog, water, damage effects
- **Save/Load** — JSON serialization via FileAccess
- **Export** — one click to Windows/Linux/macOS/Android/iOS/Web
