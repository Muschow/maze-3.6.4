using Godot;
using System;

public class RandomizerPowerupScript : ItemPickupScript
{
    //Randomizer randomises all the ghost's searching algorithms for the rest of the game so you have to relearn which ones they use
    public override void ItemAbility()
    {
        game.EmitSignal("RandomizerPowerupActivated");
        QueueFree();
    }
}
