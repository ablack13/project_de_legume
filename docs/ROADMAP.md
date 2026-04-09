# Roadmap — Proiect de Legume

## Completed

### v1.0 — KorGE Prototype
- [x] 2D top-down city map 80x60 (roads, buildings, decorations)
- [x] Player movement relative to look direction (WASD + mouse)
- [x] Tile-based collision (walls, furniture, trees)
- [x] FOV (Fog of War) — 120° cone, DDA raycasting, 3 tile states
- [x] Windows — see through, can't walk through
- [x] Loot system — 5 container types, 10 items, weighted loot tables
- [x] Inventory with weight limit, stacking, UI
- [x] Survival mechanics — HP, Hunger, Thirst, Fatigue
- [x] Consumable items (food, water, bandages)
- [x] HUD stat bars with color indicators
- [x] Localization (EN/UK) via JSON

### v1.1 — Migration to Godot 4.6.2 + C#
- [x] Full port of all logic to C#
- [x] TileMap with programmatic tileset generation
- [x] CharacterBody2D + MoveAndSlide collisions
- [x] Camera2D with smoothing
- [x] FOV via Image + ImageTexture overlay
- [x] HUD with Godot ProgressBar
- [x] Container/Inventory UI with Godot Control nodes
- [x] Input Map (WASD, Shift, E, Tab, Esc)

---

## In Development

### v1.2 — Art Improvements
- [ ] Sprite sheet for tiles (replace colored rectangles)
- [ ] Animated player sprite (walk, idle)
- [ ] Item icons in inventory
- [ ] Improved UI (Godot Theme)

### v1.3 — Day/Night Cycle
- [ ] Day/night cycle (CanvasModulate or DirectionalLight2D)
- [ ] 1 game day = 10 real minutes
- [ ] Reduced FOV at night
- [ ] Flashlight extends FOV at night

### v1.4 — Save/Load
- [ ] Game state serialization to JSON (position, inventory, stats, containers)
- [ ] Save/Load via FileAccess
- [ ] Main menu (New Game / Continue)

---

## Planned

### v2.0 — Crafting
- [ ] Basic crafting system
- [ ] Newspaper + Matches = Torch
- [ ] Old Shirt → Improvised bandages (x2)
- [ ] Empty Can + Water = Full water can
- [ ] Screwdriver + Boards = Barricade
- [ ] Crafting UI

### v2.1 — Temperature & Clothing
- [ ] Temperature system (hypothermia ↔ overheating)
- [ ] Clothing affects insulation
- [ ] Indoors is warmer than outdoors

### v2.2 — Barricades
- [ ] Lock doors
- [ ] Build barricades from furniture
- [ ] Board up windows

### v2.3 — Noise
- [ ] Running generates noise (visualized as pulsing circles)
- [ ] Opening containers = noise
- [ ] Preparation for zombie aggro system

### v2.4 — Diseases & Injuries
- [ ] Cuts from glass, requires bandage
- [ ] Infection if untreated
- [ ] Cold from low temperature
- [ ] Food poisoning from spoiled food

### v2.5 — Food Spoilage
- [ ] Expiration dates (game days)
- [ ] Canned food: 30 days, fresh food: 2-3 days
- [ ] Spoiled food = poisoning risk

---

## Long Term

### v3.0 — Zombies!
- [ ] Zombie AI with NavigationAgent2D
- [ ] Patrol, aggro on noise and sight
- [ ] Melee combat (kitchen knife)
- [ ] Bites = infection
- [ ] Wave spawning

### v3.1 — Expanded World
- [ ] Larger map (procedural generation)
- [ ] New building types (school, police station, supermarket)
- [ ] Transport (bicycle?)
- [ ] NPC survivors

### v4.0 — Multiplayer (Co-op)
- [ ] Local or online co-op for 2-4 players

---

## Low Priority

### Sound
- [ ] Footsteps (different surfaces)
- [ ] Container opening
- [ ] Eating/drinking
- [ ] Ambient (wind, birds)
- [ ] Music (tense atmosphere)

### Particles
- [ ] GPUParticles2D: rain, smoke, dust
- [ ] Effects on container opening
