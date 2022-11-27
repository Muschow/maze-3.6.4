using Godot;
using System;

public class IceCubePowerupScript : ItemPickupScript
{
    [Export] private float changeSpeedVal = -0.5f; //upgrades make this slower by -=0.1 or something
    [Export] private int icecubeWaitTime;
    private PacmanScript pacman;
    private bool fixPacmanSpeed = false;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        base._Ready();
        pacman = GetNode<PacmanScript>("/root/Game/Pacman");
        icecubeWaitTime = 10;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        if (fixPacmanSpeed)
        {
            pacman.speed = pacman.speed / GameScript.gameSpeed * (GameScript.gameSpeed - changeSpeedVal);
        }
    }
    public override void ItemAbility()
    {
        Visible = false;

        GetNode<CollisionShape2D>("ItemArea/CollisionShape2D").SetDeferred("Disabled", true);
        GetNode<Timer>("PowerupTimer").Start(icecubeWaitTime);

        fixPacmanSpeed = true;

        if (GameScript.gameSpeed + changeSpeedVal > 0)
        {
            GD.Print("before ", GameScript.gameSpeed);
            GameScript.gameSpeed += changeSpeedVal; //lowers ghost and wall speed by changespeedval and keeps pacmans the same
            GD.Print("after ", GameScript.gameSpeed);
        }
    }

    public void _OnPowerupTimerTimeout()    //on timer timeout, reset everything and delete powerup
    {
        fixPacmanSpeed = false;
        GameScript.gameSpeed -= changeSpeedVal;
        QueueFree();
    }
}
