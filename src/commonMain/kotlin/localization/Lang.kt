package localization

import korlibs.io.file.std.*
import kotlinx.serialization.json.*

object Lang {
    private var strings = mapOf<String, String>()

    suspend fun load(locale: String = "en") {
        val jsonText = resourcesVfs["data/lang_$locale.json"].readString()
        val jsonObj = Json.parseToJsonElement(jsonText).jsonObject
        strings = jsonObj.mapValues { it.value.jsonPrimitive.content }
    }

    operator fun get(key: String): String = strings[key] ?: key

    fun get(key: String, vararg args: Pair<String, String>): String {
        var result = strings[key] ?: return key
        for ((placeholder, value) in args) {
            result = result.replace("{$placeholder}", value)
        }
        return result
    }
}
