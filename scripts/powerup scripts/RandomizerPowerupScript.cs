using Godot;
using System;

public class RandomizerPowerupScript : ItemPickupScript
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";

    // Called when the node enters the scene tree for the first time.
    // public override void _Ready()
    // {

    // }

    //  // Called every frame. 'delta' is the elapsed time since the previous frame.
    //  public override void _Process(float delta)
    //  {
    //      
    //  }

    //Randomizer randomises all the ghost's searching algorithms for the rest of the game so you have to relearn which ones they use
    public override void ItemAbility()
    {
        game.EmitSignal("RandomizerPowerupActivated");
        QueueFree();
    }
}
