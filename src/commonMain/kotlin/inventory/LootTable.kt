package inventory

import kotlin.random.Random

/**
 * Loot table entry: item id + weight (probability).
 */
data class LootEntry(val itemId: String, val weight: Int)

/**
 * Loot tables for different container types.
 */
object LootTables {
    private val tables = mapOf(
        "trash_can" to listOf(
            LootEntry("newspaper", 30),
            LootEntry("empty_can", 25),
            LootEntry("old_shirt", 10),
            LootEntry("canned_beans", 8),
            LootEntry("water_bottle", 5),
            LootEntry("screwdriver", 5),
            LootEntry("flashlight", 2)
        ),
        "kitchen" to listOf(
            LootEntry("canned_beans", 25),
            LootEntry("kitchen_knife", 15),
            LootEntry("matches", 15),
            LootEntry("water_bottle", 10),
            LootEntry("empty_can", 10)
        ),
        "wardrobe" to listOf(
            LootEntry("old_shirt", 40),
            LootEntry("flashlight", 5),
            LootEntry("matches", 5)
        ),
        "medkit" to listOf(
            LootEntry("bandage", 60),
            LootEntry("water_bottle", 10)
        ),
        "garage" to listOf(
            LootEntry("screwdriver", 30),
            LootEntry("flashlight", 15),
            LootEntry("matches", 10)
        )
    )

    /**
     * Generate loot for a container type. Returns 1-4 random items.
     */
    fun generate(containerType: String): List<ItemStack> {
        val table = tables[containerType] ?: return emptyList()
        val totalWeight = table.sumOf { it.weight }
        val itemCount = Random.nextInt(1, 5) // 1 to 4 items
        val result = mutableListOf<ItemStack>()

        repeat(itemCount) {
            val roll = Random.nextInt(totalWeight)
            var cumulative = 0
            for (entry in table) {
                cumulative += entry.weight
                if (roll < cumulative) {
                    // Try to stack with existing
                    val existing = result.find { it.def.id == entry.itemId && it.canAdd() }
                    if (existing != null) {
                        existing.count++
                    } else {
                        result.add(ItemStack(ItemRegistry.get(entry.itemId)))
                    }
                    break
                }
            }
        }

        return result
    }
}
