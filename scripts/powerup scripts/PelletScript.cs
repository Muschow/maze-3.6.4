using Godot;
using System;

public class PelletScript : ItemPickupScript
{

    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";

    PelletScript()
    {
        baseScore = 10;
    }

    public override void ItemAbility()
    {
        QueueFree();
    }


}
