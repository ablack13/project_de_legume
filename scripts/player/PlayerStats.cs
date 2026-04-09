using System;

namespace ProiectDeLegume.Scripts.Player;

public class PlayerStats
{
    public double Hp { get; private set; } = 100.0;
    public double Hunger { get; private set; } = 80.0;
    public double Thirst { get; private set; } = 90.0;
    public double Fatigue { get; private set; } = 100.0;

    public bool IsDead => Hp <= 0;

    private double _hungerTimer;
    private double _thirstTimer;
    private double _hpDamageTimer;
    private double _regenTimer;
    private double _fatigueTimer;

    public void Update(double delta, bool isRunning, bool isMoving, double weightRatio)
    {
        // === HUNGER ===
        _hungerTimer += delta;
        const double hungerInterval = 30.0;
        double hungerMult = 1.0;
        if (isRunning && isMoving) hungerMult *= 2.0;
        if (weightRatio > 0.7) hungerMult *= 1.5;

        while (_hungerTimer >= hungerInterval / hungerMult)
        {
            _hungerTimer -= hungerInterval / hungerMult;
            Hunger = Math.Max(Hunger - 1, 0);
        }

        // === THIRST ===
        _thirstTimer += delta;
        const double thirstInterval = 20.0;
        double thirstMult = 1.0;
        if (isRunning && isMoving) thirstMult *= 2.5;

        while (_thirstTimer >= thirstInterval / thirstMult)
        {
            _thirstTimer -= thirstInterval / thirstMult;
            Thirst = Math.Max(Thirst - 1, 0);
        }

        // === FATIGUE ===
        _fatigueTimer += delta;
        const double fatigueInterval = 60.0;
        double fatigueDrain = 0.1;
        if (isMoving && !isRunning) fatigueDrain = 0.5;
        if (isMoving && isRunning) fatigueDrain = 3.0;
        if (Hunger < 30) fatigueDrain += 1.0;
        if (weightRatio > 0.7) fatigueDrain += 0.5;
        double fatigueRecovery = isMoving ? 0 : 0.3;

        while (_fatigueTimer >= fatigueInterval)
        {
            _fatigueTimer -= fatigueInterval;
            Fatigue = Math.Clamp(Fatigue - fatigueDrain + fatigueRecovery, 0, 100);
        }

        // === HP DAMAGE ===
        if (Hunger <= 0 || Thirst <= 0)
        {
            _hpDamageTimer += delta;
            while (_hpDamageTimer >= 10.0)
            {
                _hpDamageTimer -= 10.0;
                if (Hunger <= 0) Hp = Math.Max(Hp - 2, 0);
                if (Thirst <= 0) Hp = Math.Max(Hp - 3, 0);
            }
        }
        else
        {
            _hpDamageTimer = 0;
        }

        // === HP REGEN ===
        if (Hunger > 50 && Thirst > 50 && Hp < 100)
        {
            _regenTimer += delta;
            while (_regenTimer >= 120.0)
            {
                _regenTimer -= 120.0;
                Hp = Math.Min(Hp + 0.5, 100);
            }
        }
        else
        {
            _regenTimer = 0;
        }
    }

    public double SpeedMultiplier
    {
        get
        {
            double mult = 1.0;
            if (Hunger < 30) mult *= 0.8;
            if (Thirst < 30) mult *= 0.75;
            if (Fatigue < 20) mult *= 0.7;
            return mult;
        }
    }

    public bool CanRun => Fatigue >= 40;

    public double FovMultiplier
    {
        get
        {
            double mult = 1.0;
            if (Hunger < 15) mult *= 0.7;
            if (Fatigue < 10) mult *= 0.6;
            return mult;
        }
    }

    public void Eat(double hungerRestore)
    {
        Hunger = Math.Min(Hunger + hungerRestore, 100);
    }

    public void Drink(double thirstRestore, double hungerRestore = 0)
    {
        Thirst = Math.Min(Thirst + thirstRestore, 100);
        Hunger = Math.Min(Hunger + hungerRestore, 100);
    }

    public void Heal(double hpRestore)
    {
        Hp = Math.Min(Hp + hpRestore, 100);
    }
}
