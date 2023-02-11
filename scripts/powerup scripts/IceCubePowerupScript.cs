using Godot;
using System;

public class IceCubePowerupScript : ItemPickupScript
{
    [Export] private float newSpeedModifier = 0.5f;
    [Export] private int icecubeWaitTime = 10;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        base._Ready();
    }
    public override void ItemAbility()
    {
        this.Visible = false; //instead of deleting the ice cube when picked up, remove collision and make it invisible

        GetNode<CollisionShape2D>("ItemArea/CollisionShape2D").SetDeferred("Disabled", true);
        GetNode<Timer>("PowerupTimer").Start(icecubeWaitTime);
        //EmitSignal change speedModifier
        game.EmitSignal("ChangeSpeedModifier", newSpeedModifier);

    }

    private void _OnPowerupTimerTimeout()    //on timer timeout, reset everything and delete powerup
    {
        //GD.Print("onpoweruptimertimeout"); //debug
        //EmitSignal change speedModifier back
        newSpeedModifier = 1;
        game.EmitSignal("ChangeSpeedModifier", newSpeedModifier);
        QueueFree(); //delete the icecube when the duration has been reached
    }
}
