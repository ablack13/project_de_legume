package inventory

import kotlinx.serialization.Serializable

@Serializable
data class ItemDef(
    val id: String,
    val name: String,
    val description: String,
    val weight: Double,
    val category: String,
    val stackable: Boolean = false,
    val maxStack: Int = 1,
    val useAction: String? = null,    // "eat", "drink", "heal"
    val hungerRestore: Double = 0.0,
    val thirstRestore: Double = 0.0,
    val hpRestore: Double = 0.0
) {
    val isUsable: Boolean get() = useAction != null
    val localizedName: String get() = localization.Lang["item.$id"]
    val useLabel: String get() = when (useAction) {
        "eat" -> localization.Lang["action.eat"]
        "drink" -> localization.Lang["action.drink"]
        "heal" -> localization.Lang["action.use"]
        else -> localization.Lang["action.use"]
    }
}

/**
 * A concrete item instance in inventory or container.
 */
data class ItemStack(
    val def: ItemDef,
    var count: Int = 1
) {
    val totalWeight: Double get() = def.weight * count

    fun canAdd(): Boolean = def.stackable && count < def.maxStack
}
