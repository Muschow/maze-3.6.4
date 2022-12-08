using Godot;
using System;

public class DecoyPowerupScript : ItemPickupScript
{


    private int decoyLength = 15;

    public override void ItemAbility() //changes target of ghosts to the closest node of decoy
    {

        game.EmitSignal("DecoyPowerupActivated", this.Position, decoyLength);
        QueueFree();

    }

}
