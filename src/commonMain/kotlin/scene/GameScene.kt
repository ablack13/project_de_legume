package scene

import korlibs.event.*
import korlibs.korge.input.*
import korlibs.korge.scene.*
import korlibs.korge.view.*
import korlibs.korge.view.camera.*
import korlibs.math.geom.*
import fov.FieldOfView
import inventory.Inventory
import inventory.ItemRegistry
import localization.Lang
import player.Player
import ui.ContainerUI
import ui.HUD
import ui.InventoryUI
import world.ContainerManager
import world.GameWorld

class GameScene : Scene() {
    override suspend fun SContainer.sceneMain() {
        Lang.load("uk")
        ItemRegistry.load()

        val world = GameWorld()
        val containerManager = ContainerManager(world)
        val playerInventory = Inventory()
        lateinit var player: Player
        lateinit var fov: FieldOfView

        val camera = cameraContainer(Size(1280, 720), clip = true) {
            world.buildMap(this)
            player = Player(world, this)
            player.spawn(37, 27)
            fov = FieldOfView(world, this)
        }

        camera.follow(player.view, setImmediately = true)

        // UI layer
        val uiLayer = container {}
        val hud = HUD(uiLayer)
        val containerUI = ContainerUI(uiLayer)
        val inventoryUI = InventoryUI(uiLayer)

        // Game loop
        camera.addUpdater { dt ->
            if (!containerUI.isOpen && !inventoryUI.isOpen) {
                player.update(dt, views.input, playerInventory)
            }

            // FOV with stats modifier
            val fovMult = player.stats.fovMultiplier
            fov.update(player.px, player.py, player.facingAngle, fovMult)

            // Update HUD bars
            hud.updateStats(player.stats)

            // Nearby container prompt
            val nearby = containerManager.findNearby(player.px, player.py)
            if (nearby != null && !containerUI.isOpen) {
                hud.showPrompt(Lang["prompt.search"])
            } else if (!containerUI.isOpen) {
                hud.hidePrompt()
            }

            // Death screen
            if (player.stats.isDead) {
                hud.showPrompt(Lang["prompt.died"])
            }
        }

        // Key events
        keys {
            down(Key.E) {
                if (player.stats.isDead) return@down
                val nearby = containerManager.findNearby(player.px, player.py)
                if (nearby != null) {
                    if (containerUI.isOpen) {
                        containerUI.close()
                        hud.hidePrompt()
                    } else {
                        val items = containerManager.open(nearby)
                        containerUI.open(nearby, items, playerInventory) {
                            if (inventoryUI.isOpen) inventoryUI.open(playerInventory, player.stats)
                        }
                    }
                }
            }
            down(Key.TAB) {
                inventoryUI.toggle(playerInventory, player.stats)
            }
            down(Key.ESCAPE) {
                if (containerUI.isOpen) containerUI.close()
                if (inventoryUI.isOpen) inventoryUI.close()
            }
        }
    }
}
