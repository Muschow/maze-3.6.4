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

    // Called when the node enters the scene tree for the first time.
    // public override void _Ready()
    // {

    // }

    //  // Called every frame. 'delta' is the elapsed time since the previous frame.
    //  public override void _Process(float delta)
    //  {
    //      
    //  }

    public override void ItemAbility()
    {
        //GD.Print("PelletAbility active");
    }


}
