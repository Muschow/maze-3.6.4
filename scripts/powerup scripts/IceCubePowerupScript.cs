using Godot;
using System;

public class IceCubePowerupScript : ItemPickupScript
{
    //[Export] private float changeSpeedVal = GameScript.gameSpeed / -2; //potentially make this static...???

    [Export] private float newSpeedModifier = 0.5f;
    [Export] private int icecubeWaitTime = 10;


    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        base._Ready();



    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    // public override void _Process(float delta)
    // {


    // }
    public override void ItemAbility()
    {

        this.Visible = false;

        GetNode<CollisionShape2D>("ItemArea/CollisionShape2D").SetDeferred("Disabled", true);
        GetNode<Timer>("PowerupTimer").Start(icecubeWaitTime);
        //EmitSignal change speedModifier
        game.EmitSignal("ChangeSpeedModifier", newSpeedModifier);

    }

    private void _OnPowerupTimerTimeout()    //on timer timeout, reset everything and delete powerup
    {
        GD.Print("onpoweruptimertimeout");
        //EmitSignal change speedModifier back
        newSpeedModifier = 1;
        game.EmitSignal("ChangeSpeedModifier", newSpeedModifier); //i think changespeedval changes here im not sure
        QueueFree();


    }
}
