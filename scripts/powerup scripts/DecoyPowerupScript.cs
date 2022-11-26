using Godot;
using System;

public class DecoyPowerupScript : ItemPickupScript
{

    private int decoyLength = 15;

    public override void ItemAbility()
    {

        game.EmitSignal("DecoyPowerupActivated", this.Position, decoyLength);
        QueueFree();

    }

}
