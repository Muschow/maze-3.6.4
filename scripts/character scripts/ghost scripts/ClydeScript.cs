using Godot;
using System;

public class ClydeScript : GhostScript
{
    public ClydeScript()
    {
        ghostColour = Colors.Orange;
        searchingAlgo = algo.astar;
    }

    private KinematicBody2D firstGhost;


    public override void UpdateSourceTarget()
    {
        source = mazeTm.WorldToMap(Position);
        target = FindClosestNodeTo(mazeTm.WorldToMap((firstGhost.Position + pacman.Position) / 2)); //clyde finds closest node at the midpoint between a ghost chosen randomly (first child of enemycontainer) at ready
    }
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        base._Ready();
        firstGhost = GetParent().GetChild<KinematicBody2D>(0); // get a reference to the first child of enemy container (random ghost)

    }

    //  // Called every frame. 'delta' is the elapsed time since the previous frame.
    //  public override void _Process(float delta)
    //  {
    //      
    //  }
}
