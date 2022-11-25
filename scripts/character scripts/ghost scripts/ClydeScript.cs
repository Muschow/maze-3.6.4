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

        firstGhost = GetParent().GetChildOrNull<KinematicBody2D>(0);
        if (firstGhost == null)
        {
            patrolTimer.Paused = true;
        }
        else
        {
            target = FindClosestNodeTo(mazeTm.WorldToMap((firstGhost.Position + pacman.Position) / 2));
        }
    }

    //clyde constantly gets 1st child of enemycontainer (random ghost)
    //if there is a child and its not null,
    //clyde finds closest node at the midpoint between that child at ready



    // // Called when the node enters the scene tree for the first time.
    // public override void _Ready()
    // {
    //     base._Ready();
    //     

    // }

    //  // Called every frame. 'delta' is the elapsed time since the previous frame.
    //  public override void _Process(float delta)
    //  {
    //      
    //  }
}
