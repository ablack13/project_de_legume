package ui

import inventory.Inventory
import inventory.ItemStack
import korlibs.image.color.*
import korlibs.korge.input.*
import korlibs.korge.view.*
import localization.Lang
import world.ContainerManager

class ContainerUI(private val root: Container) {
    private var panel: Container? = null
    private var currentContainer: ContainerManager.ContainerData? = null
    var isOpen: Boolean = false
        private set

    fun open(container: ContainerManager.ContainerData, items: List<ItemStack>, inventory: Inventory, onChanged: () -> Unit) {
        close()
        currentContainer = container
        isOpen = true

        panel = root.container {
            val panelW = 320.0
            val panelH = 300.0
            val px = 20.0
            val py = 20.0

            solidRect(panelW, panelH, RGBA(20, 20, 30, 220)).position(px, py)

            text(container.displayName, textSize = 18.0).position(px + 10, py + 8)

            solidRect(panelW - 20, 1.0, RGBA(100, 100, 100, 200)).position(px + 10, py + 32)

            if (items.isEmpty()) {
                text(Lang["container.empty"], textSize = 14.0, color = Colors["#666666"]).position(px + 10, py + 42)
            } else {
                for ((index, stack) in items.withIndex()) {
                    val yOff = py + 40 + index * 28.0

                    solidRect(panelW - 20, 24.0, RGBA(40, 40, 55, 200)).position(px + 10, yOff)

                    val countStr = if (stack.count > 1) " x${stack.count}" else ""
                    text("${stack.def.localizedName}$countStr", textSize = 13.0)
                        .position(px + 14, yOff + 4)

                    text("${formatWeight(stack.totalWeight)}${Lang["unit.kg"]}", textSize = 11.0, color = Colors["#cccccc"])
                        .position(px + panelW - 60, yOff + 5)

                    val btn = solidRect(44.0, 20.0, RGBA(60, 120, 60, 220))
                    btn.position(px + panelW - 115, yOff + 2)
                    text(Lang["container.take"], textSize = 10.0).position(px + panelW - 112, yOff + 6)

                    btn.onClick {
                        if (inventory.canAdd(stack.def)) {
                            inventory.add(stack.def)
                            if (stack.count > 1) {
                                stack.count--
                            } else {
                                container.items.remove(stack)
                            }
                            open(container, container.items, inventory, onChanged)
                            onChanged()
                        }
                    }
                }
            }

            text(Lang["container.close"], textSize = 12.0, color = Colors["#666666"])
                .position(px + 10, py + panelH - 22)
        }
    }

    fun close() {
        panel?.removeFromParent()
        panel = null
        currentContainer = null
        isOpen = false
    }
}

private fun formatWeight(w: Double): String {
    val rounded = (w * 10).toInt() / 10.0
    return if (rounded == rounded.toInt().toDouble()) "${rounded.toInt()}.0" else "$rounded"
}
