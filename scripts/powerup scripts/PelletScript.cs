using Godot;
using System;

public class PelletScript : ItemPickupScript
{
    public PelletScript()
    {
        baseScore = 10;
    }

    public override void ItemAbility()
    {
        QueueFree();
    }
}
