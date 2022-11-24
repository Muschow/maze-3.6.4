using Godot;
using System;

public class PinkyScript : GhostScript
{
    public PinkyScript()
    {
        ghostColour = Colors.Pink;
        searchingAlgo = algo.bfs;
        baseSpeed = 110;
    }


    public override void UpdateSourceTarget()
    {
        source = mazeTm.WorldToMap(Position);

        Vector2 currTarget = mazeTm.WorldToMap(pacman.Position);
        if (IsOnNode(mazeTm.WorldToMap(pacman.Position))) //only update pacman positon when passed node
        {
            target = currTarget; //pinky travels to players last seen node
        }
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
