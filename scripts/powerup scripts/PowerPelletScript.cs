using Godot;
using System;

public class PowerPelletScript : ItemPickupScript
{
    public override void ItemAbility()
    {
        GD.Print("POWERPELLET EATEN");
        game.EmitSignal("PowerPelletActivated");
        QueueFree();
    }
}
