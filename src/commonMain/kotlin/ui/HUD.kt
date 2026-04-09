package ui

import korlibs.image.color.*
import korlibs.korge.view.*
import localization.Lang
import player.PlayerStats

class HUD(private val root: Container) {
    private var promptText: Text? = null
    private var barsContainer: Container? = null

    // Bar views for updating
    private var hpFill: SolidRect? = null
    private var hungerFill: SolidRect? = null
    private var thirstFill: SolidRect? = null
    private var fatigueFill: SolidRect? = null

    private var hpLabel: Text? = null
    private var hungerLabel: Text? = null
    private var thirstLabel: Text? = null
    private var fatigueLabel: Text? = null

    companion object {
        const val BAR_WIDTH = 160.0
        const val BAR_HEIGHT = 14.0
        const val BAR_X = 16.0
        const val BAR_START_Y = 640.0
        const val BAR_SPACING = 18.0
    }

    init {
        promptText = root.text("", textSize = 14.0).apply {
            position(540, 680)
        }

        barsContainer = root.container {
            // HP
            createBar(this, 0, Lang["hud.hp"], Colors.RED)
            createBar(this, 1, Lang["hud.hunger"], Colors.ORANGE)
            createBar(this, 2, Lang["hud.thirst"], Colors.BLUE)
            createBar(this, 3, Lang["hud.stamina"], Colors.GREEN)
        }
    }

    private fun createBar(container: Container, index: Int, label: String, color: RGBA) {
        val y = BAR_START_Y + index * BAR_SPACING

        // Label
        val labelView = container.text(label, textSize = 11.0).apply {
            position(BAR_X, y - 1)
        }

        // Background
        container.solidRect(BAR_WIDTH, BAR_HEIGHT, RGBA(30, 30, 30, 200))
            .position(BAR_X + 52, y)

        // Fill
        val fill = container.solidRect(BAR_WIDTH, BAR_HEIGHT, color)
        fill.position(BAR_X + 52, y)

        // Value text
        val valText = container.text("100", textSize = 10.0).apply {
            position(BAR_X + 52 + BAR_WIDTH + 4, y + 1)
        }

        when (index) {
            0 -> { hpFill = fill; hpLabel = valText }
            1 -> { hungerFill = fill; hungerLabel = valText }
            2 -> { thirstFill = fill; thirstLabel = valText }
            3 -> { fatigueFill = fill; fatigueLabel = valText }
        }
    }

    fun updateStats(stats: PlayerStats) {
        updateBar(hpFill, hpLabel, stats.hp, Colors.RED)
        updateBar(hungerFill, hungerLabel, stats.hunger, Colors.ORANGE)
        updateBar(thirstFill, thirstLabel, stats.thirst, Colors.BLUE)
        updateBar(fatigueFill, fatigueLabel, stats.fatigue, Colors.GREEN)
    }

    private fun updateBar(fill: SolidRect?, label: Text?, value: Double, baseColor: RGBA) {
        val ratio = value / 100.0
        fill?.scaledWidth = BAR_WIDTH * ratio

        // Color changes based on value
        val color = when {
            value < 15 -> Colors.RED
            value < 30 -> Colors.YELLOW
            else -> baseColor
        }
        fill?.colorMul = color

        label?.text = "${value.toInt()}"
    }

    fun showPrompt(message: String) {
        promptText?.text = message
    }

    fun hidePrompt() {
        promptText?.text = ""
    }
}
