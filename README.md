# Proiect de Legume

A 2D top-down survival game inspired by Project Zomboid, built with **Kotlin** and **KorGE 6.0**.

![Screenshot](screenshot.png)

## Features

- **2D top-down world** — 80x60 tile city with roads, sidewalks, buildings, trees, bushes, benches, and fences
- **Field of View** — 120° vision cone with DDA raycasting; walls block sight, windows don't; fog of war with explored/unknown states
- **Relative movement** — WASD moves relative to where the player is looking (mouse cursor); sprint with Shift
- **Tile-based collision** — walls, furniture, and trees block movement; doors and windows are impassable but doors allow entry
- **16+ buildings** — family homes, cottages, apartments, garages, clinics, shops — each with unique interior (tables, beds, carpets, tile floors)
- **Windows** — glass panes in building walls; can see through but can't walk through
- **Lootable containers** — trash cans, kitchen cabinets, wardrobes, medkits, garage shelves — each with weighted loot tables
- **10 item types** — food, water, bandages, tools, weapons, clothing, junk — loaded from JSON registry
- **Inventory system** — weight-limited (15kg), item stacking, category color coding
- **Survival stats** — HP, Hunger, Thirst, Fatigue with real-time drain and interconnected effects
- **Consumable items** — eat food (+hunger), drink water (+thirst), use bandages (+HP) directly from inventory
- **Stat effects on gameplay** — low hunger/thirst slows movement; low fatigue blocks sprinting; critical stats reduce FOV; starvation/dehydration drains HP
- **HUD** — real-time stat bars with color transitions (green → yellow → red)
- **Camera** — smooth follow on the player, clamped to map bounds
- **Localization** — all texts externalized to JSON; English and Ukrainian included

## Controls

| Key | Action |
|-----|--------|
| **W** | Move forward (toward cursor) |
| **S** | Move backward |
| **A/D** | Strafe left/right |
| **Mouse** | Look direction |
| **Shift** | Sprint (if Fatigue ≥ 40) |
| **E** | Search nearby container |
| **Tab** | Open/close inventory |
| **Esc** | Close panels |

## Tech Stack

| Component | Choice |
|-----------|--------|
| Language | Kotlin (Multiplatform) |
| Engine | KorGE 6.0 |
| Build | Gradle (Kotlin DSL) |
| Platform | Desktop (JVM) — Android/iOS/Web ready |

## Run

```bash
./gradlew runJvm
```

## Project Structure

```
src/commonMain/kotlin/
├── Main.kt                 # Entry point
├── scene/GameScene.kt      # Game loop, UI wiring
├── world/
│   ├── GameWorld.kt        # Map generation, tile rendering
│   └── ContainerManager.kt # Lootable container logic
├── player/
│   ├── Player.kt           # Movement, collision, sprite
│   └── PlayerStats.kt      # HP, Hunger, Thirst, Fatigue
├── inventory/
│   ├── Item.kt             # ItemDef + ItemStack
│   ├── ItemRegistry.kt     # JSON loader
│   ├── Inventory.kt        # Weight-limited inventory
│   └── LootTable.kt        # Weighted random loot
├── fov/
│   └── FieldOfView.kt      # DDA raycasting, fog bitmap
├── localization/
│   └── Lang.kt             # JSON-based i18n (Lang["key"])
└── ui/
    ├── HUD.kt              # Stat bars, prompts
    ├── InventoryUI.kt      # Inventory panel
    └── ContainerUI.kt      # Container loot panel
```

## Localization

All in-game text is externalized to `src/commonMain/resources/data/lang_XX.json`.

Supported languages: **English** (`en`), **Ukrainian** (`uk`)

To switch language, change `Lang.load("en")` to `Lang.load("uk")` in `GameScene.kt`.
