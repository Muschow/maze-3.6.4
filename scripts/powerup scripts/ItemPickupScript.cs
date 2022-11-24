using Godot;
using System;

public class ItemPickupScript : Sprite
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";

    //VERY IMPORTANT INSTRUCTIONS
    //add itempickup to scene randomly to add powerups

    //create an instance of every powerup
    //have an array of all the powerups instances
    //randomly select a powerup instnace from the array
    //add that instance to the inventory

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {

    }

    //  // Called every frame. 'delta' is the elapsed time since the previous frame.
    //  public override void _Process(float delta)
    //  {
    //      
    //  }

    public void _OnItemAreaEntered()
    {
        //add to inventory if it can
        //if it cant add to inventory then add ammo or add score
        //QueueFree
    }
}
