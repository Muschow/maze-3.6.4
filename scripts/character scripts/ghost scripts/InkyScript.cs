using Godot;
using System;

public class InkyScript : GhostScript
{
    public InkyScript() //inky should move randomly, essentially, constantly be in patrol mode
    {
        ghostColour = Colors.Cyan;
        searchingAlgo = algo.astar; //blue, although it moves randomly, uses astar as thats generally the fastest algorithm
    }
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        base._Ready();
        patrolTimer.Paused = true; //patrol timer is a built in godot node so we must wait for ready to be called to edit it
        //by pausing the patrol timer, it means inky never goes to chase mode

    }

    //  // Called every frame. 'delta' is the elapsed time since the previous frame.
    //  public override void _Process(float delta)
    //  {
    //      
    //  }
}
