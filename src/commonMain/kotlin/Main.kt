import korlibs.korge.*
import korlibs.korge.scene.*
import korlibs.image.color.*
import scene.GameScene

suspend fun main() = Korge(
    title = "Proiect de Legume",
    windowSize = korlibs.math.geom.Size(1280, 720),
    backgroundColor = Colors["#1a1a2e"]
) {
    val sc = sceneContainer()
    sc.changeTo { GameScene() }
}
