package inventory

class Inventory(val maxWeight: Double = 15.0) {
    val items = mutableListOf<ItemStack>()

    val currentWeight: Double get() = items.sumOf { it.totalWeight }
    val freeWeight: Double get() = maxWeight - currentWeight

    fun canAdd(itemDef: ItemDef, count: Int = 1): Boolean {
        return currentWeight + itemDef.weight * count <= maxWeight
    }

    /**
     * Add item to inventory. Returns true if successful.
     */
    fun add(itemDef: ItemDef, count: Int = 1): Boolean {
        if (!canAdd(itemDef, count)) return false

        if (itemDef.stackable) {
            val existing = items.find { it.def.id == itemDef.id && it.canAdd() }
            if (existing != null) {
                val canFit = (itemDef.maxStack - existing.count).coerceAtMost(count)
                existing.count += canFit
                val remainder = count - canFit
                if (remainder > 0) {
                    items.add(ItemStack(itemDef, remainder))
                }
                return true
            }
        }

        items.add(ItemStack(itemDef, count))
        return true
    }

    /**
     * Remove one item from a stack. Returns true if successful.
     */
    fun remove(index: Int): Boolean {
        if (index < 0 || index >= items.size) return false
        val stack = items[index]
        if (stack.count > 1) {
            stack.count--
        } else {
            items.removeAt(index)
        }
        return true
    }
}
