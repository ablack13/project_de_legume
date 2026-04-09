package ui

import inventory.Inventory
import korlibs.image.color.*
import korlibs.korge.input.*
import korlibs.korge.view.*
import localization.Lang
import player.PlayerStats

class InventoryUI(private val root: Container) {
    private var panel: Container? = null
    var isOpen: Boolean = false
        private set

    private var statsRef: PlayerStats? = null

    fun open(inventory: Inventory, stats: PlayerStats? = null) {
        close()
        isOpen = true
        if (stats != null) statsRef = stats

        panel = root.container {
            val panelW = 320.0
            val panelH = 400.0
            val px = 940.0
            val py = 20.0

            solidRect(panelW, panelH, RGBA(20, 20, 30, 220)).position(px, py)

            text(Lang["inventory.title"], textSize = 18.0).position(px + 10, py + 8)
            val cw = formatWeight(inventory.currentWeight)
            val mw = formatWeight(inventory.maxWeight)
            text("$cw / $mw ${Lang["unit.kg"]}", textSize = 13.0, color = Colors["#cccccc"])
                .position(px + panelW - 130, py + 10)

            solidRect(panelW - 20, 1.0, RGBA(100, 100, 100, 200)).position(px + 10, py + 32)

            if (inventory.items.isEmpty()) {
                text(Lang["inventory.empty"], textSize = 14.0, color = Colors["#666666"]).position(px + 10, py + 42)
            } else {
                for ((index, stack) in inventory.items.withIndex()) {
                    val yOff = py + 40 + index * 28.0

                    solidRect(panelW - 20, 24.0, RGBA(40, 40, 55, 200)).position(px + 10, yOff)

                    val catColor = when (stack.def.category) {
                        "food" -> Colors.GREEN
                        "medical" -> Colors.RED
                        "tool" -> Colors.YELLOW
                        "weapon" -> Colors.ORANGE
                        "clothing" -> Colors.CYAN
                        else -> Colors["#666666"]
                    }
                    solidRect(3.0, 20.0, catColor).position(px + 12, yOff + 2)

                    val countStr = if (stack.count > 1) " x${stack.count}" else ""
                    text("${stack.def.localizedName}$countStr", textSize = 13.0)
                        .position(px + 20, yOff + 4)

                    text("${formatWeight(stack.totalWeight)}${Lang["unit.kg"]}", textSize = 11.0, color = Colors["#cccccc"])
                        .position(px + panelW - 55, yOff + 5)

                    val capturedIndex = index

                    if (stack.def.isUsable) {
                        val useBtn = solidRect(50.0, 20.0, RGBA(50, 100, 130, 220))
                        useBtn.position(px + panelW - 178, yOff + 2)
                        text(stack.def.useLabel, textSize = 10.0).position(px + panelW - 175, yOff + 6)

                        useBtn.onClick {
                            val s = statsRef ?: return@onClick
                            val item = stack.def
                            when (item.useAction) {
                                "eat" -> s.eat(item.hungerRestore)
                                "drink" -> s.drink(item.thirstRestore, item.hungerRestore)
                                "heal" -> s.heal(item.hpRestore)
                            }
                            inventory.remove(capturedIndex)
                            open(inventory)
                        }
                    }

                    val btn = solidRect(50.0, 20.0, RGBA(120, 50, 50, 220))
                    btn.position(px + panelW - 120, yOff + 2)
                    text(Lang["inventory.drop"], textSize = 10.0).position(px + panelW - 117, yOff + 6)

                    btn.onClick {
                        inventory.remove(capturedIndex)
                        open(inventory)
                    }
                }
            }

            text(Lang["inventory.close"], textSize = 12.0, color = Colors["#666666"])
                .position(px + 10, py + panelH - 22)
        }
    }

    fun close() {
        panel?.removeFromParent()
        panel = null
        isOpen = false
    }

    fun toggle(inventory: Inventory, stats: PlayerStats? = null) {
        if (isOpen) close() else open(inventory, stats)
    }
}

private fun formatWeight(w: Double): String {
    val rounded = (w * 10).toInt() / 10.0
    return if (rounded == rounded.toInt().toDouble()) "${rounded.toInt()}.0" else "$rounded"
}
