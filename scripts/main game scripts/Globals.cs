using Godot;
using System;

public class Globals : Node
{
    public const int INFINITY = 9999; //used in movement, killwall, generally to denote something thats not got a value yet eg initally, shortestval = infinity 
    public int userId; //used for database
    public int userScore; //used for highscore
    public bool gameWon = false; //used so gameOver scene knows whether to show you win or you lose

    //used to convert a (0,y) or (x,0) vector to an int. 
    //This is so I can get a distance from neighoburing nodes.
    public static int ConvertVectorToInt(Vector2 vector)
    {
        if (vector.x == 0)
        {
            return (int)vector.y;
        }
        else if (vector.y == 0)
        {
            return (int)vector.x;
        }
        else
        {
            return -1; //bascially error
        }
    }
}
