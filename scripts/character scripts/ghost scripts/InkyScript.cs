using Godot;
using System;

public class InkyScript : GhostScript
{
    public InkyScript() //inky should move randomly, essentially, constantly be in patrol mode
    {
        ghostColour = Colors.Cyan;

        Array values = Enum.GetValues(typeof(algo));
        Random random = new Random();
        algo randomAlgo = (algo)values.GetValue(random.Next(values.Length));

        searchingAlgo = randomAlgo; //blue, although it moves randomly, uses astar as thats generally the fastest algorithm
    }

    public override void _Ready()
    {
        base._Ready();
        patrolTimer.Paused = true; //patrol timer is a built in godot node so we must wait for ready to be called to edit it
        //by pausing the patrol timer, it means inky never goes to chase mode

    }
}
