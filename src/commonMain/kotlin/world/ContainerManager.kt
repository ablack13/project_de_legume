package world

import inventory.ItemStack
import inventory.LootTables

/**
 * Manages lootable containers on the map.
 * Loot is generated on first open, then persisted.
 */
class ContainerManager(private val world: GameWorld) {
    data class ContainerData(
        val tileX: Int,
        val tileY: Int,
        val type: String,
        val items: MutableList<ItemStack>,
        var opened: Boolean = false
    ) {
        val displayName: String get() = localization.Lang["container.name.$type"]
    }

    private val containers = mutableMapOf<Long, ContainerData>()

    init {
        // Scan entire map for any tile with a containerType
        for (y in 0 until GameWorld.MAP_HEIGHT) {
            for (x in 0 until GameWorld.MAP_WIDTH) {
                val tile = world.getTile(x, y)
                if (tile != null && tile.containerType != null) {
                    val key = posKey(x, y)
                    containers[key] = ContainerData(x, y, tile.containerType, mutableListOf())
                }
            }
        }
    }

    private fun posKey(x: Int, y: Int): Long = y.toLong() * GameWorld.MAP_WIDTH + x

    fun findNearby(px: Double, py: Double, range: Double = 48.0): ContainerData? {
        val ts = GameWorld.TILE_SIZE
        for (container in containers.values) {
            val cx = container.tileX * ts + ts / 2.0
            val cy = container.tileY * ts + ts / 2.0
            val dx = px - cx
            val dy = py - cy
            if (dx * dx + dy * dy <= range * range) {
                return container
            }
        }
        return null
    }

    fun open(container: ContainerData): List<ItemStack> {
        if (!container.opened) {
            container.items.addAll(LootTables.generate(container.type))
            container.opened = true
        }
        return container.items
    }

    fun takeItem(container: ContainerData, index: Int): ItemStack? {
        if (index < 0 || index >= container.items.size) return null
        val stack = container.items[index]
        if (stack.count > 1) {
            stack.count--
            return ItemStack(stack.def, 1)
        } else {
            return container.items.removeAt(index)
        }
    }
}
