package player

import korlibs.time.*
import inventory.Inventory

/**
 * Survival stats: HP, Hunger, Thirst, Fatigue.
 * All values 0–100.
 */
class PlayerStats {
    var hp: Double = 100.0
        private set
    var hunger: Double = 80.0
        private set
    var thirst: Double = 90.0
        private set
    var fatigue: Double = 100.0
        private set

    val isDead: Boolean get() = hp <= 0

    // Timers (accumulated seconds)
    private var hungerTimer = 0.0
    private var thirstTimer = 0.0
    private var hpDamageTimer = 0.0
    private var regenTimer = 0.0
    private var fatigueTimer = 0.0

    /**
     * Update stats each frame.
     * @param isRunning player is holding shift
     * @param isMoving player is pressing WASD
     * @param weightRatio current inventory weight / max weight (0.0–1.0)
     */
    fun update(dt: TimeSpan, isRunning: Boolean, isMoving: Boolean, weightRatio: Double) {
        val sec = dt / 1.seconds

        // === HUNGER ===
        hungerTimer += sec
        val hungerInterval = 30.0 // base: -1 every 30 sec
        var hungerMult = 1.0
        if (isRunning && isMoving) hungerMult *= 2.0
        if (weightRatio > 0.7) hungerMult *= 1.5

        while (hungerTimer >= hungerInterval / hungerMult) {
            hungerTimer -= hungerInterval / hungerMult
            hunger = (hunger - 1).coerceAtLeast(0.0)
        }

        // === THIRST ===
        thirstTimer += sec
        val thirstInterval = 20.0 // base: -1 every 20 sec
        var thirstMult = 1.0
        if (isRunning && isMoving) thirstMult *= 2.5

        while (thirstTimer >= thirstInterval / thirstMult) {
            thirstTimer -= thirstInterval / thirstMult
            thirst = (thirst - 1).coerceAtLeast(0.0)
        }

        // === FATIGUE ===
        fatigueTimer += sec
        val fatigueInterval = 60.0 // base: every 60 sec
        var fatigueDrain = 0.1 // standing still
        if (isMoving && !isRunning) fatigueDrain = 0.5
        if (isMoving && isRunning) fatigueDrain = 3.0
        if (hunger < 30) fatigueDrain += 1.0
        if (weightRatio > 0.7) fatigueDrain += 0.5

        // Recovery when standing still
        val fatigueRecovery = if (!isMoving) 0.3 else 0.0

        while (fatigueTimer >= fatigueInterval) {
            fatigueTimer -= fatigueInterval
            fatigue = (fatigue - fatigueDrain + fatigueRecovery).coerceIn(0.0, 100.0)
        }

        // === HP DAMAGE from starvation/dehydration ===
        if (hunger <= 0 || thirst <= 0) {
            hpDamageTimer += sec
            while (hpDamageTimer >= 10.0) {
                hpDamageTimer -= 10.0
                if (hunger <= 0) hp = (hp - 2).coerceAtLeast(0.0)
                if (thirst <= 0) hp = (hp - 3).coerceAtLeast(0.0)
            }
        } else {
            hpDamageTimer = 0.0
        }

        // === HP REGEN (if well-fed and hydrated) ===
        if (hunger > 50 && thirst > 50 && hp < 100) {
            regenTimer += sec
            while (regenTimer >= 120.0) { // +0.5 HP every 2 minutes
                regenTimer -= 120.0
                hp = (hp + 0.5).coerceAtMost(100.0)
            }
        } else {
            regenTimer = 0.0
        }
    }

    // === Speed/FOV modifiers ===

    /** Multiplier for movement speed (0.0–1.0) */
    val speedMultiplier: Double get() {
        var mult = 1.0
        if (hunger < 30) mult *= 0.8
        if (thirst < 30) mult *= 0.75
        if (fatigue < 20) mult *= 0.7
        return mult
    }

    /** Whether sprinting is allowed */
    val canRun: Boolean get() = fatigue >= 40

    /** Multiplier for FOV radius (0.0–1.0) */
    val fovMultiplier: Double get() {
        var mult = 1.0
        if (hunger < 15) mult *= 0.7
        if (fatigue < 10) mult *= 0.6
        return mult
    }

    // === Item effects ===

    fun eat(hungerRestore: Double) {
        hunger = (hunger + hungerRestore).coerceAtMost(100.0)
    }

    fun drink(thirstRestore: Double, hungerRestore: Double = 0.0) {
        thirst = (thirst + thirstRestore).coerceAtMost(100.0)
        hunger = (hunger + hungerRestore).coerceAtMost(100.0)
    }

    fun heal(hpRestore: Double) {
        hp = (hp + hpRestore).coerceAtMost(100.0)
    }
}
