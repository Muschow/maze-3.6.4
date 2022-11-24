using Godot;
using System;

public class BlinkyScript : GhostScript
{
    public BlinkyScript()
    {
        ghostColour = Colors.Red;
        searchingAlgo = algo.dijkstras;
    }

    public override void UpdateSourceTarget()
    {
        source = mazeTm.WorldToMap(Position);
        target = FindClosestNodeTo(mazeTm.WorldToMap(pacman.Position)); //blinky finds closest node to player
    }


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





}
