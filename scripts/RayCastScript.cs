using Godot;
using System;
using System.Collections.Generic;
public class RayCastScript : Node2D
{
    IDictionary<Vector2, RayCast2D[]> rayDict = new Dictionary<Vector2, RayCast2D[]>();
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        AddRaystoDict(); //initialise rayDict
    }

    //  // Called every frame. 'delta' is the elapsed time since the previous frame.
    //  public override void _Process(float delta)
    //  {
    //      
    //  }

    public void AddRaystoDict()
    {
        Godot.Collections.Array rays = GetChildren(); //gets all the rays, stores it in an array

        RayCast2D[] upRays = new RayCast2D[2];      //initialises arrays for diff directions
        RayCast2D[] downRays = new RayCast2D[2];
        RayCast2D[] rightRays = new RayCast2D[2];
        RayCast2D[] leftRays = new RayCast2D[2];

        int dictItem = -1;
        for (int i = 0; i < rays.Count; i++)
        {

            if (i % 2 == 0)
                dictItem++;

            if (dictItem == 0)
                upRays[i % 2] = (RayCast2D)rays[i];
            else if (dictItem == 1)
                downRays[i % 2] = (RayCast2D)rays[i];
            else if (dictItem == 2)
                rightRays[i % 2] = (RayCast2D)rays[i];
            else if (dictItem == 3)
                leftRays[i % 2] = (RayCast2D)rays[i];
        }

        rayDict.Add(Vector2.Up, upRays);    //add rays arrays to dict
        rayDict.Add(Vector2.Down, downRays);
        rayDict.Add(Vector2.Right, rightRays);
        rayDict.Add(Vector2.Left, leftRays);
    }

    public bool RaysAreColliding(Vector2 nextDir)
    {
        //GD.Print("rays colldiing getting called");
        if (nextDir == Vector2.Zero) //initial
        {
            return true;
        }

        int noCollision = 0;
        for (int i = 0; i < rayDict[nextDir].Length; i++)
        {
            if ((rayDict[nextDir])[i].IsColliding())
            {
                return true;
            }
            else
            {
                noCollision++;
            }
        }

        if (noCollision == rayDict[nextDir].Length) //length is 2
        {
            return false;
        }

        return false;
    }
}
