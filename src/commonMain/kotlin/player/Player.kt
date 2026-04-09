package player

import korlibs.event.*
import korlibs.image.color.*
import korlibs.korge.input.*
import korlibs.korge.view.*
import korlibs.math.geom.*
import korlibs.time.*
import inventory.Inventory
import world.GameWorld
import kotlin.math.*

class Player(
    private val world: GameWorld,
    private val container: Container
) {
    companion object {
        const val SIZE = 20.0
        const val WALK_SPEED = 120.0
        const val RUN_SPEED = 200.0
    }

    val view: Container = Container()
    val stats = PlayerStats()

    var px: Double = 0.0
    var py: Double = 0.0
    var facingAngle: Double = 0.0

    // Exposed for HUD / stats
    var isRunning: Boolean = false
        private set
    var isMoving: Boolean = false
        private set

    init {
        buildSprite()
    }

    private fun buildSprite() {
        val body = view.solidRect(SIZE, SIZE, Colors["#2d5a3d"])
        body.anchor(Anchor.CENTER)

        val indicator = view.solidRect(SIZE * 0.4, SIZE * 0.8, Colors["#8fbc8f"])
        indicator.anchor(Anchor(0.5, 1.0))

        container.addChild(view)
    }

    fun spawn(tileX: Int, tileY: Int) {
        px = tileX * GameWorld.TILE_SIZE + GameWorld.TILE_SIZE / 2.0
        py = tileY * GameWorld.TILE_SIZE + GameWorld.TILE_SIZE / 2.0
        updateViewPosition()
    }

    fun update(dt: TimeSpan, input: korlibs.korge.input.Input, inventory: Inventory) {
        val wantRun = input.keys[Key.LEFT_SHIFT]
        isRunning = wantRun && stats.canRun

        val baseSpeed = if (isRunning) RUN_SPEED else WALK_SPEED
        val speed = baseSpeed * stats.speedMultiplier

        // Facing angle toward mouse cursor
        val mousePos = input.mousePos
        val viewPos = view.globalPos
        val mdx = mousePos.x - viewPos.x
        val mdy = mousePos.y - viewPos.y
        facingAngle = atan2(mdy, mdx)

        // Movement relative to facing direction
        var forward = 0.0
        var strafe = 0.0
        if (input.keys[Key.W] || input.keys[Key.UP]) forward += 1.0
        if (input.keys[Key.S] || input.keys[Key.DOWN]) forward -= 1.0
        if (input.keys[Key.A] || input.keys[Key.LEFT]) strafe -= 1.0
        if (input.keys[Key.D] || input.keys[Key.RIGHT]) strafe += 1.0

        val len = sqrt(forward * forward + strafe * strafe)
        isMoving = len > 0

        if (isMoving) {
            val nf = forward / len
            val ns = strafe / len

            val cosA = cos(facingAngle)
            val sinA = sin(facingAngle)

            val dx = (nf * cosA + ns * -sinA) * speed * (dt / 1.seconds)
            val dy = (nf * sinA + ns * cosA) * speed * (dt / 1.seconds)

            val newPx = px + dx
            if (canMoveTo(newPx, py)) px = newPx
            val newPy = py + dy
            if (canMoveTo(px, newPy)) py = newPy
        }

        view.rotation = Angle.fromRadians(facingAngle + PI / 2)
        updateViewPosition()

        // Update survival stats
        val weightRatio = inventory.currentWeight / inventory.maxWeight
        stats.update(dt, isRunning, isMoving, weightRatio)
    }

    private fun canMoveTo(newX: Double, newY: Double): Boolean {
        val halfSize = SIZE / 2.0 - 2
        val corners = arrayOf(
            newX - halfSize to newY - halfSize,
            newX + halfSize to newY - halfSize,
            newX - halfSize to newY + halfSize,
            newX + halfSize to newY + halfSize
        )
        for ((cx, cy) in corners) {
            val tileX = (cx / GameWorld.TILE_SIZE).toInt()
            val tileY = (cy / GameWorld.TILE_SIZE).toInt()
            if (!world.isWalkable(tileX, tileY)) return false
        }
        return true
    }

    private fun updateViewPosition() {
        view.position(px, py)
    }
}
