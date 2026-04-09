package inventory

import korlibs.io.file.std.*
import kotlinx.serialization.json.Json

object ItemRegistry {
    private val items = mutableMapOf<String, ItemDef>()

    suspend fun load() {
        val jsonText = resourcesVfs["data/items.json"].readString()
        val itemList = Json.decodeFromString<List<ItemDef>>(jsonText)
        for (item in itemList) {
            items[item.id] = item
        }
    }

    fun get(id: String): ItemDef = items[id] ?: error("Unknown item: $id")

    fun all(): Collection<ItemDef> = items.values
}
