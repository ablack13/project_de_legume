using Godot;
using System;
using ProiectDeLegume.Scripts.Inventory;

namespace ProiectDeLegume.Scripts.Player;

public partial class Player : CharacterBody2D
{
    private const float WalkSpeed = 120f;
    private const float RunSpeed = 200f;

    public PlayerStats Stats { get; } = new();
    public Inventory.Inventory PlayerInventory { get; } = new();

    public double FacingAngle { get; private set; }
    public bool IsRunning { get; private set; }
    public bool IsMoving { get; private set; }

    public override void _PhysicsProcess(double delta)
    {
        // Facing angle toward mouse
        var mousePos = GetGlobalMousePosition();
        float dx = mousePos.X - GlobalPosition.X;
        float dy = mousePos.Y - GlobalPosition.Y;
        FacingAngle = Math.Atan2(dy, dx);
        Rotation = (float)(FacingAngle + Math.PI / 2);

        // Movement input
        float forward = 0, strafe = 0;
        if (Input.IsActionPressed("move_forward")) forward += 1;
        if (Input.IsActionPressed("move_back")) forward -= 1;
        if (Input.IsActionPressed("move_left")) strafe -= 1;
        if (Input.IsActionPressed("move_right")) strafe += 1;

        float len = MathF.Sqrt(forward * forward + strafe * strafe);
        IsMoving = len > 0;
        IsRunning = Input.IsActionPressed("sprint") && Stats.CanRun;

        float baseSpeed = IsRunning ? RunSpeed : WalkSpeed;
        float speed = baseSpeed * (float)Stats.SpeedMultiplier;

        if (IsMoving)
        {
            float nf = forward / len;
            float ns = strafe / len;
            float cosA = MathF.Cos((float)FacingAngle);
            float sinA = MathF.Sin((float)FacingAngle);

            float vx = (nf * cosA + ns * -sinA) * speed;
            float vy = (nf * sinA + ns * cosA) * speed;
            Velocity = new Vector2(vx, vy);
        }
        else
        {
            Velocity = Vector2.Zero;
        }

        MoveAndSlide();

        // Update survival stats
        double weightRatio = PlayerInventory.CurrentWeight / PlayerInventory.MaxWeight;
        Stats.Update(delta, IsRunning, IsMoving, weightRatio);
    }
}
