# Changelog

## v1.1 — Godot Migration (2026-04-10)

**Engine migration from KorGE (Kotlin) to Godot 4.6.2 (C#).**

- Rewrote all game logic from Kotlin to C#
- TileMap with programmatic tileset generation (21 tile types)
- CharacterBody2D with MoveAndSlide physics collisions
- Camera2D with position smoothing (replaces manual camera follow)
- FOV via Image + ImageTexture Sprite2D overlay
- HUD using Godot ProgressBar nodes
- Container/Inventory UI using Godot Control nodes (PanelContainer, Button, Label)
- Input Map for all controls (WASD, Shift, E, Tab, Esc)
- Collision polygons on all solid tiles (walls, furniture, trees, etc.)

## v1.0 — KorGE Prototype (2026-04-09)

**Initial prototype built with Kotlin + KorGE 6.0.**

### Core
- 2D top-down city map (80x60 tiles, 32px each)
- 16+ buildings: family homes, cottages, apartments, garages, clinics, shops
- Roads, sidewalks, trees, bushes, benches, fences
- Windows (see through, can't walk through)

### Player
- WASD movement relative to look direction (mouse cursor)
- Sprint with Shift
- Tile-based collision with walls, furniture, trees

### Field of View
- 120° vision cone with DDA raycasting
- Walls and trees block vision, windows don't
- 3 tile states: visible, explored (dimmed), unknown (black)
- Wall-aware peripheral vision (360° short rays)

### Survival
- HP, Hunger, Thirst, Fatigue stats
- Running and heavy inventory drain stats faster
- Low stats reduce speed, block sprinting, shrink FOV
- Starvation/dehydration causes HP loss
- HP regeneration when well-fed

### Items & Loot
- 10 item types (food, water, bandages, tools, weapons, clothing, junk)
- Items loaded from JSON registry
- 5 container types with weighted loot tables
- Loot generated on first container open

### Inventory
- Weight-limited (15kg)
- Item stacking
- Consumable items: Eat (+hunger), Drink (+thirst), Use bandage (+HP)
- Drop items

### UI
- HUD stat bars with color transitions (green → yellow → red)
- Container search panel ([E] nearby)
- Inventory panel ([Tab])
- Prompt messages

### Localization
- All texts externalized to JSON
- English (en) and Ukrainian (uk)
