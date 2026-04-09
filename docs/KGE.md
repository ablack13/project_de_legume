# KorGE — Довідник розробника

**Сайт:** https://korge.org/  
**Документація:** https://docs.korge.org/  
**API (Dokka):** https://dokka.korge.org/  
**GitHub:** https://github.com/korlibs/korge  
**Версія:** 6.0.0 (Maven Central)  

> **Ризик:** Оригінальний автор (soywiz) залишив проєкт після 6.0 (квітень 2025).
> Проєкт open source, шукає нових мейнтейнерів. Код працює і білдиться.

---

## 1. Налаштування проєкту (Gradle)

```kotlin
// build.gradle.kts
import korlibs.korge.gradle.*

plugins {
    alias(libs.plugins.korge)
}

korge {
    id = "com.sample.demo"
    targetJvm()        // Desktop — основна платформа для розробки
    targetJs()         // Web (JS)
    targetWasm()       // Web (WASM)
    targetDesktop()    // Native Desktop
    targetAndroid()    // Android
    targetIos()        // iOS
    serializationJson() // kotlinx.serialization
}
```

**Команди запуску:**
- `./gradlew runJvm` — основний спосіб розробки
- `./gradlew runJvmAutoreload` — з hot-reload
- `./gradlew runJs` — запуск у браузері
- `./gradlew runAndroidRelease` — Android

**Ресурси:** `src/commonMain/resources/` — сюди кладемо спрайти, JSON, звуки.

---

## 2. Точка входу та сцени

```kotlin
fun main() = Korge {
    val sc = sceneContainer()
    sc.changeTo { GameScene() }
}

class GameScene : Scene() {
    override suspend fun SContainer.sceneMain() {
        // додаємо в'юхи, реєструємо івенти, вантажимо ассети
    }
}
```

**Для піксельарту** — використовувати `PixelatedScene(width, height)` або `ScaledScene(width, height)` 
для фіксованої віртуальної роздільності з автоматичним масштабуванням.

**Життєвий цикл сцени:**
`sceneInit()` → `sceneMain()` → `sceneAfterInit()` → `sceneBeforeLeaving()` → `sceneDestroy()`

**Переходи між сценами:**
```kotlin
sc.changeTo<MenuScene>(transition = MaskTransition(...), time = 0.5.seconds)
// Навігація стеком: pushTo, back(), forward()
```

---

## 3. Ієрархія View

`Stage` (корінь) → `Container` (містить дітей) → `View` (базовий елемент)

Ключові класи:
- `Image` — зображення
- `Sprite` / `SpriteAnimation` — анімовані спрайти
- `TileMap` / `TileSet` — тайлові карти
- `SolidRect`, `Circle`, `Graphics` — примітиви
- `Text`, `TextBlock` — текст
- `CameraContainer` — камера
- `SceneContainer` — контейнер сцен
- `FixedSizeContainer` — контейнер фіксованого розміру

Кожен View має: `x`, `y`, `scale`, `rotation`, `alpha`, `visible`, `colorMul`.

---

## 4. Input (WASD + миша)

### Клавіатура (polling — для руху):
```kotlin
addUpdater { dt ->
    val speed = 100.0 * (dt / 1.seconds)
    if (input.keys[Key.W]) player.y -= speed
    if (input.keys[Key.S]) player.y += speed
    if (input.keys[Key.A]) player.x -= speed
    if (input.keys[Key.D]) player.x += speed
}
```

### Клавіатура (event-based):
```kotlin
keys {
    down(Key.ESCAPE) { views.gameWindow.close(0) }
    justDown(Key.SPACE) { /* одноразова дія */ }
}
```

### Миша:
```kotlin
addUpdater {
    val mousePos: Point = input.mouse   // глобальні координати
    val buttons: Int = input.mouseButtons
}
view.mouse { click { /* обробка */ } }
view.onClick { /* shorthand */ }
```

---

## 5. Камера

```kotlin
val camera = cameraContainer(Size(800, 600), clip = true) {
    // весь ігровий світ додаємо сюди
    tileMap(...)
    player = sprite(...)
}
camera.follow(player, setImmediately = true)
camera.cameraZoom = 2.0
camera.cameraAngle = 0.degrees
```

**Властивості:** `cameraX`, `cameraY`, `cameraZoom`, `cameraAngle`, `cameraViewportBounds`, `clampToBounds`  
**Методи:** `follow(view)`, `getCameraToFit(rect)`, `getCameraToCover(rect)`

---

## 6. Tile Maps

KorGE має вбудовані `TileMap` і `TileSet`. TMX (Tiled) підтримка — через store extension.

```kotlin
val tileMap = tileMap(
    map = stackedIntArray,
    tileset = myTileSet,
    repeatX = TileMapRepeat.NONE,
    repeatY = TileMapRepeat.NONE,
    smoothing = true
)

// Оновити тайли в рантаймі:
tileMap.lock {
    stackedIntArray.push(x, y, TileInfo(tile = 42, flipX = true).data)
}
```

- Автоматичний occlusion culling (рендерить лише видимі тайли)
- `SparseChunkedStackedIntArray2` — для великих/нескінченних карт (chunk-based)
- Підтримує кілька шарів на клітинку

---

## 7. Спрайти та анімації

### Зі спрайт-листа:
```kotlin
val spriteMap = resourcesVfs["player.png"].readBitmap()
val walkAnim = SpriteAnimation(
    spriteMap = spriteMap,
    spriteWidth = 32, spriteHeight = 32,
    columns = 4, rows = 4
)
val player = sprite(walkAnim)
player.playAnimationLooped()
```

### З атласу:
```kotlin
val atlas = resourcesVfs["sprites.atlas.json"].readAtlas()
val runAnim = atlas.getSpriteAnimation(prefix = "run")
val idleAnim = atlas.getSpriteAnimation(prefix = "idle")
val player = sprite(idleAnim)
player.playAnimationLooped(spriteAnimation = runAnim)
```

**Методи:** `playAnimation(times)`, `playAnimationLooped()`, `playAnimationForDuration(duration)`, `stopAnimation()`, `setFrame(index)`

**Авто-атлас:** помістити папку `.atlas` у resources — плагін згенерує JSON автоматично.

---

## 8. Графіка та прозорість (для Fog of War)

### Напівпрозорі прямокутники:
```kotlin
solidRect(800, 600, Colors["#00000080"])  // fog overlay
```

### Складні форми (конус FOV):
```kotlin
graphics {
    fill(Colors["#FFFF0044"]) {
        moveTo(Point(0, 0))
        lineTo(Point(100, -50))
        arc(Point(0, 0), 100.0, (-30).degrees, 60.degrees)
        close()
    }
}
```

### Або через gpuShapeView для більшої продуктивності.

**Alpha:** кожен View має `alpha` (0.0–1.0)  
**colorMul:** RGBA множення  
**Blend modes:** NORMAL, ADD, MULTIPLY, SUBTRACT

---

## 9. Колізії

### Вбудовані:
```kotlin
if (player.collidesWithShape(wall)) { /* зупинити рух */ }
player.onCollisionShape(filter = { it != player }) { other ->
    // реакція на зіткнення
}
```

**Два режими:** `GLOBAL_RECT` (швидкий AABB) і `SHAPE` (використовує `View.hitShape` — векторний шлях)

**Box2D:** є порт як extension — для повної фізичної симуляції (нам не потрібен для v1).

---

## 10. UI система

Вбудовані віджети (не потрібна зовнішня бібліотека):

```kotlin
uiVerticalStack(padding = 4f) {
    uiButton("Інвентар") { onClick { /* відкрити */ } }
    uiText("HP: 100")
}
```

**Доступні віджети:**
`uiButton`, `UIText`, `TextBlock` (rich text, wrapping), `UITextInput`, `UICheckBox`,
`UIRadioButton`, `UIComboBox`, `UISlider`, `UIProgressBar`, `UIWindow` (перетягування/ресайз),
`UIVerticalStack`, `UIHorizontalStack`, `UIGridFill`, `UIScrollableContainer`, `UIVerticalList`,
`UIImage`, `UITreeView`

**Стилізація:** `styles { textColor = Colors.RED; textSize = 32f }`

---

## 11. Завантаження ресурсів

```kotlin
val bitmap = resourcesVfs["player.png"].readBitmap()
val atlas = resourcesVfs["sprites.atlas.json"].readAtlas()
val sound = resourcesVfs["shoot.mp3"].readSound()      // декодується в пам'ять
val music = resourcesVfs["bgm.mp3"].readMusic()         // стрімиться
val json = resourcesVfs["items.json"].readString()       // потім парсити kotlinx.serialization
```

---

## 12. Game Loop

```kotlin
// Змінний timestep (кожен кадр):
view.addUpdater { dt: TimeSpan ->
    val scale = dt / 16.666.milliseconds
    x += speed * scale
}

// Фіксований timestep (детерміністичний):
view.addFixedUpdater(60.timesPerSecond) {
    x += 1  // викликається рівно 60 разів/сек
}
```

> `addFixedUpdater` може спричинити візуальне заїкання без інтерполяції.
> Для руху краще використовувати `addUpdater` з delta time.

---

## 13. Важливі обмеження

| Обмеження | Деталі |
|-----------|--------|
| Мейнтейнер | Автор залишив проєкт після 6.0 — ризик відсутності фіксів |
| Kotlin/Native | Повільна компіляція (~7GB RAM), використовувати JVM для розробки |
| Спільнота | Менше туторіалів і відповідей на SO ніж у libGDX |
| Без рефлексії | Multiplatform не підтримує runtime reflection |
| Gradle магія | Плагін робить багато під капотом — дебаг білд-проблем може бути складним |
| TMX | Підтримка Tiled — через extension, не в core |
