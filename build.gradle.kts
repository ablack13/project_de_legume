import korlibs.korge.gradle.*

plugins {
    alias(libs.plugins.korge)
}

korge {
    id = "com.proiectdelegume"
    name = "Proiect de Legume"

    targetJvm()

    serializationJson()
}
