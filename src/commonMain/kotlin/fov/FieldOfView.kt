package fov

import korlibs.image.bitmap.*
import korlibs.image.color.*
import korlibs.korge.view.*
import korlibs.math.geom.*
import world.GameWorld
import kotlin.math.*

class FieldOfView(
    private val world: GameWorld,
    private val container: Container
) {
    companion object {
        const val FOV_ANGLE = 120.0          // degrees
        const val FOV_RADIUS = 12            // tiles
        const val RAY_STEP = 0.5             // degrees between rays
        const val BEHIND_RADIUS = 3          // small radius behind player
    }

    private val mapW = GameWorld.MAP_WIDTH
    private val mapH = GameWorld.MAP_HEIGHT
    private val tileSize = GameWorld.TILE_SIZE

    // Visibility states per tile
    // 0 = unknown (black), 1 = explored (dark grey), 2 = visible (clear)
    private val visibility = IntArray(mapW * mapH) { 0 }

    // Bitmap-based fog overlay: one pixel per tile, scaled up
    private val fogBitmap = Bitmap32(mapW, mapH, premultiplied = false)
    private val fogImage: Image

    init {
        // Fill fog completely black (opaque)
        for (i in 0 until mapW * mapH) {
            fogBitmap.setRgba(i % mapW, i / mapW, RGBA(0, 0, 0, 255))
        }
        fogImage = container.image(fogBitmap) {
            smoothing = false
            size(Size((mapW * tileSize).toFloat(), (mapH * tileSize).toFloat()))
        }
    }

    fun update(playerX: Double, playerY: Double, facingAngle: Double, fovMultiplier: Double = 1.0) {
        val effectiveRadius = (FOV_RADIUS * fovMultiplier).toInt().coerceAtLeast(3)
        val playerTileX = (playerX / tileSize).toInt()
        val playerTileY = (playerY / tileSize).toInt()

        // Demote all currently visible tiles to explored
        for (i in 0 until mapW * mapH) {
            if (visibility[i] == 2) {
                visibility[i] = 1
            }
        }

        // Small circle around player — wall-aware peripheral vision
        // Cast short rays in all directions to fill the close area
        for (deg in 0 until 360 step 2) {
            castRay(playerX, playerY, Math.toRadians(deg.toDouble()), maxTiles = BEHIND_RADIUS)
        }

        // Cast rays within the FOV cone
        val halfFov = FOV_ANGLE / 2.0
        val startAngle = Math.toDegrees(facingAngle) - halfFov
        val endAngle = Math.toDegrees(facingAngle) + halfFov

        var angle = startAngle
        while (angle <= endAngle) {
            castRay(playerX, playerY, Math.toRadians(angle), maxTiles = effectiveRadius)
            angle += RAY_STEP
        }

        // Update fog bitmap
        updateFogBitmap()
    }

    /**
     * DDA raycasting — visits every tile the ray passes through,
     * so it never skips over a 1-tile wall.
     */
    private fun castRay(originX: Double, originY: Double, angleRad: Double, maxTiles: Int = FOV_RADIUS) {
        val dirX = cos(angleRad)
        val dirY = sin(angleRad)
        val ts = tileSize.toDouble()
        val maxDistSq = (maxTiles * ts) * (maxTiles * ts)

        // Current tile
        var tileX = (originX / ts).toInt()
        var tileY = (originY / ts).toInt()

        // Step direction (+1 or -1)
        val stepX = if (dirX >= 0) 1 else -1
        val stepY = if (dirY >= 0) 1 else -1

        // Distance along ray to next vertical/horizontal tile boundary
        val tDeltaX = if (dirX != 0.0) abs(ts / dirX) else Double.MAX_VALUE
        val tDeltaY = if (dirY != 0.0) abs(ts / dirY) else Double.MAX_VALUE

        // Initial distance to first boundary
        var tMaxX = if (dirX != 0.0) {
            val boundary = if (dirX > 0) (tileX + 1) * ts else tileX * ts
            abs((boundary - originX) / dirX)
        } else Double.MAX_VALUE

        var tMaxY = if (dirY != 0.0) {
            val boundary = if (dirY > 0) (tileY + 1) * ts else tileY * ts
            abs((boundary - originY) / dirY)
        } else Double.MAX_VALUE

        var steps = 0
        val maxSteps = maxTiles * 3 // safety limit

        while (steps < maxSteps) {
            if (tileX < 0 || tileX >= mapW || tileY < 0 || tileY >= mapH) break

            // Check distance from origin
            val cx = tileX * ts + ts / 2 - originX
            val cy = tileY * ts + ts / 2 - originY
            if (cx * cx + cy * cy > maxDistSq) break

            visibility[tileY * mapW + tileX] = 2

            // Stop at vision-blocking tiles (walls, trees) but mark them visible
            if (world.blocksVision(tileX, tileY)) break

            // Step to next tile
            if (tMaxX < tMaxY) {
                tMaxX += tDeltaX
                tileX += stepX
            } else {
                tMaxY += tDeltaY
                tileY += stepY
            }
            steps++
        }
    }

    private fun updateFogBitmap() {
        fogBitmap.lock()
        for (y in 0 until mapH) {
            for (x in 0 until mapW) {
                val v = visibility[y * mapW + x]
                val color = when (v) {
                    2 -> RGBA(0, 0, 0, 0)         // visible — fully clear
                    1 -> RGBA(0, 0, 0, 140)       // explored — dimmed but visible
                    else -> RGBA(0, 0, 0, 245)    // unknown — almost black
                }
                fogBitmap.setRgba(x, y, color)
            }
        }
        fogBitmap.unlock()
        fogImage.bitmap = fogBitmap.slice()
    }
}
