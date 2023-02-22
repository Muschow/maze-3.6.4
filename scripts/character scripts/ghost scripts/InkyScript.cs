using Godot;
using System;

public class InkyScript : GhostScript
{
    public InkyScript() //inky should move randomly, essentially, constantly be in patrol mode
    {
        ghostColour = Colors.Cyan;

        Array values = Enum.GetValues(typeof(algo));
        Random random = new Random();
        algo randomAlgo = (algo)values.GetValue(random.Next(values.Length)); //puts all the values in algo enum into an array and chooses a random one
        //https://stackoverflow.com/questions/3132126/how-do-i-select-a-random-value-from-an-enumeration
        //adapted code to get random value from enum from here

        searchingAlgo = randomAlgo; //blue uses a random algorithm to pathfind
    }

    public override void _Ready()
    {
        base._Ready();
        patrolTimer.Paused = true; //a timer is a built in godot node so we must wait for ready to be called to edit it
        //by pausing the patrol timer, it means inky never goes to chase mode

    }
}
