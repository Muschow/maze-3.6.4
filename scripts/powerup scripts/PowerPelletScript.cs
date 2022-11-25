using Godot;
using System;

public class PowerPelletScript : ItemPickupScript
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";


    // Called when the node enters the scene tree for the first time.
    // public override void _Ready()
    // {
    //     //connect signal I think?
    // }

    //  // Called every frame. 'delta' is the elapsed time since the previous frame.
    //  public override void _Process(float delta)
    //  {
    //      
    //  }

    public override void ItemAbility()
    {
        GD.Print("POWERPELLET EATEN");
        //emitsignal change state to vunerable
        GetNode<Globals>("/root/Globals").EmitSignal("PowerPelletActivated");

    }


}
