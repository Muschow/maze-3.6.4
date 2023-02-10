using Godot;
using System.Collections.Generic;
public class RayCastScript : Node2D
{
    //raydict maps vector directions to the corresponding rays attatched to pacman
    IDictionary<Vector2, RayCast2D[]> rayDict = new Dictionary<Vector2, RayCast2D[]>();
    
    public override void _Ready()
    {
        AddRaystoDict(); //initialise rayDict
    }

    public void AddRaystoDict()
    {
        //gets all the ray nodes attatched to parent node, stores it in an array
        //GetChildren is a in built godot node that returns a Godot Array.
        Godot.Collections.Array rays = GetChildren(); 

        //initialises arrays for diff directions. 
        //There are 2 rays for each corner and each direction so that
        //an object of size tile (ie pacman or a ghost) can detect when its fully in the gap of a path tile
        //this is used to know when you can change directions so you dont collide with a wall and just stop.
        RayCast2D[] upRays = new RayCast2D[2];      
        RayCast2D[] downRays = new RayCast2D[2];
        RayCast2D[] rightRays = new RayCast2D[2];
        RayCast2D[] leftRays = new RayCast2D[2];

        int dictItem = -1;
        for (int i = 0; i < rays.Count; i++)
        {

            if (i % 2 == 0)
                dictItem++; //as there are 2 rays for each direction

            if (dictItem == 0) //the child rays of the parent node are in the order up,down,right,left so theyre in that order in rays[]
                upRays[i % 2] = (RayCast2D)rays[i]; 
            else if (dictItem == 1)
                downRays[i % 2] = (RayCast2D)rays[i]; //eg if rays[i] is 2,3, it will put it in downrays[0], downrays[1]
            else if (dictItem == 2)
                rightRays[i % 2] = (RayCast2D)rays[i];
            else if (dictItem == 3)
                leftRays[i % 2] = (RayCast2D)rays[i];
        }

        rayDict.Add(Vector2.Up, upRays);    //add rays arrays to dict with the corresponding direction vector
        rayDict.Add(Vector2.Down, downRays);
        rayDict.Add(Vector2.Right, rightRays);
        rayDict.Add(Vector2.Left, leftRays);
    }

    public bool RaysAreColliding(Vector2 nextDir) //next direction of object, eg WASD input for pacman or next dir for ghost
    {
        //GD.Print("rays colldiing getting called");
        if (nextDir == Vector2.Zero) //initial
        {
            return true;
        }

        int noCollision = 0; //essentially if a ray for 1 direction is colliding return true, else if 2 are not return false
        for (int i = 0; i < rayDict[nextDir].Length; i++) //length is 2 as each vector stores up1 up2 etc
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

        return false; //i believe this is just here as the code needs a return
    }
}
