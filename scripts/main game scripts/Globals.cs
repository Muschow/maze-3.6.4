using Godot;
using System;

public class Globals : Node
{
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
