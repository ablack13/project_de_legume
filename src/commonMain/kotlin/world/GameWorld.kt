package world

import korlibs.image.bitmap.*
import korlibs.image.color.*
import korlibs.image.tiles.*
import korlibs.korge.view.*
import korlibs.korge.view.tiles.*
import korlibs.math.geom.*

/**
 * Tile types for our map.
 * containerType: if set, this tile is a lootable container of that type.
 */
enum class TileType(
    val id: Int,
    val color: RGBA,
    val walkable: Boolean = true,
    val blocksVision: Boolean = false,
    val containerType: String? = null
) {
    GRASS(0, Colors["#4a7c59"]),
    ROAD(1, Colors["#8b8680"]),
    WALL(2, Colors["#5c4033"], walkable = false, blocksVision = true),
    FLOOR(3, Colors["#c4a882"]),
    DOOR(4, Colors["#8b6914"]),
    TRASH_CAN(5, Colors["#556b2f"], containerType = "trash_can"),
    TREE(6, Colors["#2d5a1e"], walkable = false, blocksVision = true),
    BUSH(7, Colors["#3a6b2a"], walkable = false),
    BENCH(8, Colors["#8b7355"], walkable = false),
    FENCE(9, Colors["#6b5b3a"], walkable = false),
    SIDEWALK(10, Colors["#a0988a"]),
    GRASS_DARK(11, Colors["#3d6b4a"]),
    WARDROBE(12, Colors["#8b6e4e"], walkable = false, containerType = "wardrobe"),
    KITCHEN_CABINET(13, Colors["#b89e78"], walkable = false, containerType = "kitchen"),
    MEDKIT(14, Colors["#c44040"], walkable = false, containerType = "medkit"),
    GARAGE_SHELF(15, Colors["#7a6a50"], walkable = false, containerType = "garage"),
    TABLE(16, Colors["#9e8b6e"], walkable = false),
    BED(17, Colors["#6b5a8a"], walkable = false),
    CARPET(18, Colors["#8b4040"]),
    TILE_FLOOR(19, Colors["#d4c8b0"]),
    WINDOW(20, Colors["#5c4033"], walkable = false, blocksVision = false);
}

class GameWorld {
    companion object {
        const val TILE_SIZE = 32
        const val MAP_WIDTH = 80
        const val MAP_HEIGHT = 60
    }

    private val mapData = IntArray(MAP_WIDTH * MAP_HEIGHT) { TileType.GRASS.id }

    init {
        generateCityMap()
    }

    private fun set(x: Int, y: Int, tile: TileType) {
        if (x in 0 until MAP_WIDTH && y in 0 until MAP_HEIGHT) {
            mapData[y * MAP_WIDTH + x] = tile.id
        }
    }

    private fun fill(x1: Int, y1: Int, x2: Int, y2: Int, tile: TileType) {
        for (y in y1..y2) for (x in x1..x2) set(x, y, tile)
    }

    private fun generateCityMap() {
        // Grass variation patches
        val darkPatches = listOf(
            2 to 2, 15 to 5, 60 to 10, 35 to 45, 70 to 50, 5 to 50, 50 to 25
        )
        for ((px, py) in darkPatches) {
            for (dy in -2..2) for (dx in -2..2) {
                if (dx * dx + dy * dy <= 5) set(px + dx, py + dy, TileType.GRASS_DARK)
            }
        }

        // === ROADS ===
        // Main horizontal road
        fill(0, 28, MAP_WIDTH - 1, 29, TileType.ROAD)
        // Main vertical road
        fill(38, 0, 39, MAP_HEIGHT - 1, TileType.ROAD)
        // Secondary horizontal road (north)
        fill(0, 14, MAP_WIDTH - 1, 15, TileType.ROAD)
        // Secondary vertical road (east)
        fill(60, 0, 61, MAP_HEIGHT - 1, TileType.ROAD)
        // Small street (south)
        fill(15, 42, 16, MAP_HEIGHT - 1, TileType.ROAD)

        // === SIDEWALKS along main roads ===
        for (x in 0 until MAP_WIDTH) {
            set(x, 13, TileType.SIDEWALK); set(x, 16, TileType.SIDEWALK)
            set(x, 27, TileType.SIDEWALK); set(x, 30, TileType.SIDEWALK)
        }
        for (y in 0 until MAP_HEIGHT) {
            set(37, y, TileType.SIDEWALK); set(40, y, TileType.SIDEWALK)
            set(59, y, TileType.SIDEWALK); set(62, y, TileType.SIDEWALK)
        }

        // === BUILDINGS ===

        // --- Block 1: Top-left residential ---
        // House 1: Family home (10x8)
        placeHouse(3, 3, 10, 8, "family")
        // House 2: Small cottage (7x6)
        placeHouse(16, 4, 7, 6, "cottage")
        // House 3: Apartment
        placeHouse(26, 3, 9, 7, "apartment")

        // --- Block 2: Top-right ---
        // Garage (8x6)
        placeHouse(42, 3, 8, 6, "garage")
        // Shop (10x7)
        placeHouse(53, 4, 6, 7, "shop")
        // House 4
        placeHouse(64, 3, 8, 6, "family")

        // --- Block 3: Middle-left ---
        // Clinic (8x7)
        placeHouse(4, 18, 8, 7, "clinic")
        // House 5
        placeHouse(15, 18, 8, 6, "cottage")
        // House 6 (big)
        placeHouse(26, 18, 10, 8, "apartment")

        // --- Block 4: Middle-right ---
        // Warehouse (12x8)
        placeHouse(42, 18, 12, 8, "garage")
        // House 7
        placeHouse(64, 18, 9, 7, "family")

        // --- Block 5: Bottom-left ---
        // House 8
        placeHouse(3, 33, 9, 7, "family")
        // House 9
        placeHouse(20, 33, 8, 6, "cottage")

        // --- Block 6: Bottom-right ---
        // Big house
        placeHouse(42, 33, 10, 8, "apartment")
        // Shed
        placeHouse(55, 34, 5, 4, "garage")
        // House 10
        placeHouse(64, 33, 9, 7, "family")

        // --- Block 7: Far south ---
        placeHouse(3, 46, 7, 6, "cottage")
        placeHouse(20, 46, 10, 7, "apartment")
        placeHouse(42, 46, 8, 6, "family")
        placeHouse(64, 46, 8, 7, "clinic")

        // === TREES ===
        // Tree rows along roads
        val treePositions = mutableListOf<Pair<Int, Int>>()
        for (x in 2 until MAP_WIDTH step 5) {
            treePositions.add(x to 12)  // north of road 1
            treePositions.add(x to 17)  // south of road 1
            treePositions.add(x to 26)  // north of road 2
            treePositions.add(x to 31)  // south of road 2
        }
        for (y in 2 until MAP_HEIGHT step 5) {
            treePositions.add(36 to y)  // west of road 1
            treePositions.add(41 to y)  // east of road 1
        }
        // Park area (center-ish)
        for (dy in 0..4) for (dx in 0..4) {
            if ((dx + dy) % 2 == 0) treePositions.add((44 + dx) to (10 + dy))
        }
        // Random scattered trees
        treePositions.addAll(listOf(
            1 to 1, 14 to 2, 75 to 5, 78 to 12, 2 to 30, 18 to 31,
            70 to 42, 76 to 55, 3 to 55, 30 to 50, 55 to 52, 68 to 55
        ))
        for ((x, y) in treePositions) {
            if (getTile(x, y) == TileType.GRASS || getTile(x, y) == TileType.GRASS_DARK) {
                set(x, y, TileType.TREE)
            }
        }

        // === BUSHES ===
        val bushPositions = listOf(
            2 to 5, 13 to 3, 24 to 10, 35 to 5, 50 to 2, 63 to 10,
            75 to 3, 5 to 25, 20 to 27, 50 to 30, 65 to 27,
            10 to 40, 30 to 43, 55 to 45, 72 to 48,
            8 to 53, 25 to 55, 48 to 54, 60 to 56
        )
        for ((x, y) in bushPositions) {
            if (getTile(x, y) == TileType.GRASS || getTile(x, y) == TileType.GRASS_DARK) {
                set(x, y, TileType.BUSH)
            }
        }

        // === BENCHES (along sidewalks) ===
        val benchPositions = listOf(
            6 to 13, 18 to 13, 30 to 13, 48 to 16, 66 to 16,
            8 to 27, 22 to 30, 45 to 27, 67 to 30,
            10 to 44, 50 to 44, 65 to 44
        )
        for ((x, y) in benchPositions) {
            if (getTile(x, y) == TileType.SIDEWALK) {
                set(x, y, TileType.BENCH)
            }
        }

        // === FENCES (around some properties) ===
        // Fence around house 1 yard
        for (x in 2..13) { set(x, 2, TileType.FENCE); set(x, 11, TileType.FENCE) }
        for (y in 2..11) { set(2, y, TileType.FENCE); set(13, y, TileType.FENCE) }
        set(8, 11, TileType.GRASS) // gate

        // Fence around park
        for (x in 43..49) { set(x, 9, TileType.FENCE); }

        // === TRASH CANS along roads ===
        val trashPositions = listOf(
            7 to 16, 19 to 16, 33 to 13, 47 to 13, 57 to 16, 70 to 13,
            5 to 30, 22 to 27, 50 to 30, 66 to 27,
            10 to 43, 25 to 43, 48 to 43, 67 to 43,
            8 to 56, 35 to 56, 55 to 56
        )
        for ((x, y) in trashPositions) {
            val t = getTile(x, y)
            if (t == TileType.SIDEWALK || t == TileType.GRASS) {
                set(x, y, TileType.TRASH_CAN)
            }
        }
    }

    /**
     * Place a building with interior furniture based on type.
     */
    private fun placeHouse(bx: Int, by: Int, w: Int, h: Int, type: String) {
        // Walls and floor
        for (y in by until by + h) {
            for (x in bx until bx + w) {
                val isWall = y == by || y == by + h - 1 || x == bx || x == bx + w - 1
                set(x, y, if (isWall) TileType.WALL else TileType.FLOOR)
            }
        }

        // Door at bottom center
        val doorX = bx + w / 2
        set(doorX, by + h - 1, TileType.DOOR)

        // Windows on side walls (every 3 tiles, avoiding corners)
        for (x in bx + 2 until bx + w - 2 step 3) {
            set(x, by, TileType.WINDOW)           // top wall
        }
        for (y in by + 2 until by + h - 2 step 3) {
            set(bx, y, TileType.WINDOW)            // left wall
            set(bx + w - 1, y, TileType.WINDOW)   // right wall
        }

        // Interior based on type
        when (type) {
            "family" -> {
                // Carpet in living room
                fill(bx + 2, by + 2, bx + w - 3, by + h - 3, TileType.CARPET)
                // Kitchen cabinet (top-left)
                set(bx + 1, by + 1, TileType.KITCHEN_CABINET)
                set(bx + 2, by + 1, TileType.KITCHEN_CABINET)
                // Table
                set(bx + w / 2, by + h / 2, TileType.TABLE)
                // Bed (top-right)
                set(bx + w - 2, by + 1, TileType.BED)
                set(bx + w - 2, by + 2, TileType.BED)
                // Wardrobe
                set(bx + w - 2, by + 3, TileType.WARDROBE)
            }
            "cottage" -> {
                // Simple: kitchen + bed + wardrobe
                set(bx + 1, by + 1, TileType.KITCHEN_CABINET)
                set(bx + w - 2, by + 1, TileType.BED)
                set(bx + 1, by + h - 2, TileType.WARDROBE)
                // Table center
                set(bx + w / 2, by + h / 2, TileType.TABLE)
            }
            "apartment" -> {
                // Tile floor
                fill(bx + 1, by + 1, bx + w - 2, by + h - 2, TileType.TILE_FLOOR)
                // Kitchen corner
                set(bx + 1, by + 1, TileType.KITCHEN_CABINET)
                set(bx + 2, by + 1, TileType.KITCHEN_CABINET)
                set(bx + 3, by + 1, TileType.KITCHEN_CABINET)
                // Table
                set(bx + w / 2, by + 2, TileType.TABLE)
                set(bx + w / 2 + 1, by + 2, TileType.TABLE)
                // Beds in back
                set(bx + 1, by + h - 2, TileType.BED)
                set(bx + 2, by + h - 2, TileType.BED)
                set(bx + w - 2, by + h - 2, TileType.BED)
                // Wardrobes
                set(bx + w - 2, by + 1, TileType.WARDROBE)
                set(bx + w - 2, by + 2, TileType.WARDROBE)
            }
            "garage" -> {
                // Concrete-ish floor
                fill(bx + 1, by + 1, bx + w - 2, by + h - 2, TileType.TILE_FLOOR)
                // Shelves along walls
                set(bx + 1, by + 1, TileType.GARAGE_SHELF)
                set(bx + 2, by + 1, TileType.GARAGE_SHELF)
                set(bx + 1, by + 2, TileType.GARAGE_SHELF)
                set(bx + w - 2, by + 1, TileType.GARAGE_SHELF)
                set(bx + w - 2, by + 2, TileType.GARAGE_SHELF)
                // Table
                set(bx + w / 2, by + h / 2, TileType.TABLE)
            }
            "clinic" -> {
                // Tile floor
                fill(bx + 1, by + 1, bx + w - 2, by + h - 2, TileType.TILE_FLOOR)
                // Medkits
                set(bx + 1, by + 1, TileType.MEDKIT)
                set(bx + 2, by + 1, TileType.MEDKIT)
                set(bx + 1, by + 2, TileType.MEDKIT)
                // Table
                set(bx + w / 2, by + h / 2, TileType.TABLE)
                // Bed
                set(bx + w - 2, by + 1, TileType.BED)
                set(bx + w - 2, by + 2, TileType.BED)
            }
            "shop" -> {
                // Shelves = garage shelves (general goods)
                set(bx + 1, by + 1, TileType.GARAGE_SHELF)
                set(bx + 1, by + 2, TileType.GARAGE_SHELF)
                set(bx + 1, by + 3, TileType.GARAGE_SHELF)
                set(bx + w - 2, by + 1, TileType.KITCHEN_CABINET)
                set(bx + w - 2, by + 2, TileType.KITCHEN_CABINET)
                // Table (counter)
                set(bx + w / 2, by + h - 2, TileType.TABLE)
            }
        }
    }

    fun buildMap(container: Container) {
        for (y in 0 until MAP_HEIGHT) {
            for (x in 0 until MAP_WIDTH) {
                val tileType = TileType.entries[mapData[y * MAP_WIDTH + x]]
                val px = x * TILE_SIZE
                val py = y * TILE_SIZE

                // Base tile
                container.solidRect(TILE_SIZE, TILE_SIZE, tileType.color).position(px, py)

                // Visual details
                when (tileType) {
                    TileType.WALL -> {
                        container.solidRect(TILE_SIZE - 2, TILE_SIZE - 2, Colors["#6b4e3d"])
                            .position(px + 1, py + 1)
                    }
                    TileType.TRASH_CAN -> {
                        container.solidRect(16, 20, Colors["#3d5c3a"]).position(px + 8, py + 6)
                    }
                    TileType.DOOR -> {
                        container.solidRect(20, TILE_SIZE, Colors["#b8860b"]).position(px + 6, py)
                    }
                    TileType.TREE -> {
                        // Trunk
                        container.solidRect(6, 10, Colors["#5c3d1e"]).position(px + 13, py + 20)
                        // Canopy (dark green circle-ish)
                        container.solidRect(24, 22, Colors["#1e5c1e"]).position(px + 4, py + 2)
                        container.solidRect(20, 18, Colors["#267326"]).position(px + 6, py + 4)
                    }
                    TileType.BUSH -> {
                        container.solidRect(22, 16, Colors["#2d6b2d"]).position(px + 5, py + 10)
                        container.solidRect(18, 12, Colors["#3a8a3a"]).position(px + 7, py + 12)
                    }
                    TileType.BENCH -> {
                        // Seat
                        container.solidRect(28, 8, Colors["#6b4e2e"]).position(px + 2, py + 12)
                        // Legs
                        container.solidRect(4, 12, Colors["#4a3520"]).position(px + 4, py + 16)
                        container.solidRect(4, 12, Colors["#4a3520"]).position(px + 24, py + 16)
                    }
                    TileType.FENCE -> {
                        container.solidRect(TILE_SIZE, 4, Colors["#5a4a30"]).position(px, py + 14)
                        // Posts
                        container.solidRect(4, 20, Colors["#5a4a30"]).position(px + 2, py + 6)
                        container.solidRect(4, 20, Colors["#5a4a30"]).position(px + 26, py + 6)
                    }
                    TileType.WARDROBE -> {
                        container.solidRect(TILE_SIZE - 4, TILE_SIZE - 4, Colors["#7a5e3e"])
                            .position(px + 2, py + 2)
                        // Door line
                        container.solidRect(2, TILE_SIZE - 8, Colors["#5c4030"])
                            .position(px + TILE_SIZE / 2 - 1, py + 4)
                    }
                    TileType.KITCHEN_CABINET -> {
                        container.solidRect(TILE_SIZE - 4, TILE_SIZE - 4, Colors["#a08860"])
                            .position(px + 2, py + 2)
                        // Handles
                        container.solidRect(8, 3, Colors["#c0a870"]).position(px + 6, py + 10)
                        container.solidRect(8, 3, Colors["#c0a870"]).position(px + 18, py + 10)
                    }
                    TileType.MEDKIT -> {
                        container.solidRect(TILE_SIZE - 6, TILE_SIZE - 6, Colors["#e0e0e0"])
                            .position(px + 3, py + 3)
                        // Red cross
                        container.solidRect(14, 4, Colors["#cc3333"]).position(px + 9, py + 14)
                        container.solidRect(4, 14, Colors["#cc3333"]).position(px + 14, py + 9)
                    }
                    TileType.GARAGE_SHELF -> {
                        container.solidRect(TILE_SIZE - 4, TILE_SIZE - 4, Colors["#6a5a40"])
                            .position(px + 2, py + 2)
                        // Shelf lines
                        container.solidRect(TILE_SIZE - 8, 2, Colors["#8a7a5a"]).position(px + 4, py + 10)
                        container.solidRect(TILE_SIZE - 8, 2, Colors["#8a7a5a"]).position(px + 4, py + 20)
                    }
                    TileType.TABLE -> {
                        container.solidRect(TILE_SIZE - 6, TILE_SIZE - 6, Colors["#8a7a5e"])
                            .position(px + 3, py + 3)
                    }
                    TileType.BED -> {
                        container.solidRect(TILE_SIZE - 4, TILE_SIZE - 4, Colors["#5a4a7a"])
                            .position(px + 2, py + 2)
                        // Pillow
                        container.solidRect(12, 8, Colors["#e0d8c8"]).position(px + 10, py + 4)
                    }
                    TileType.CARPET -> {
                        // subtle pattern
                        container.solidRect(TILE_SIZE - 2, TILE_SIZE - 2, Colors["#7a3535"])
                            .position(px + 1, py + 1)
                    }
                    TileType.WINDOW -> {
                        // Wall base with glass pane
                        container.solidRect(TILE_SIZE - 2, TILE_SIZE - 2, Colors["#6b4e3d"])
                            .position(px + 1, py + 1)
                        // Glass (light blue, semi-transparent look)
                        container.solidRect(TILE_SIZE - 8, TILE_SIZE - 8, Colors["#8ec8e8"])
                            .position(px + 4, py + 4)
                        // Cross frame
                        container.solidRect(TILE_SIZE - 8, 2, Colors["#5c4033"])
                            .position(px + 4, py + TILE_SIZE / 2 - 1)
                        container.solidRect(2, TILE_SIZE - 8, Colors["#5c4033"])
                            .position(px + TILE_SIZE / 2 - 1, py + 4)
                    }
                    else -> {}
                }
            }
        }
    }

    fun getTile(x: Int, y: Int): TileType? {
        if (x < 0 || x >= MAP_WIDTH || y < 0 || y >= MAP_HEIGHT) return null
        return TileType.entries[mapData[y * MAP_WIDTH + x]]
    }

    fun isWalkable(tileX: Int, tileY: Int): Boolean {
        val tile = getTile(tileX, tileY) ?: return false
        return tile.walkable
    }

    fun blocksVision(tileX: Int, tileY: Int): Boolean {
        val tile = getTile(tileX, tileY) ?: return true
        return tile.blocksVision
    }
}
