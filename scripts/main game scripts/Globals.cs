using Godot;
using System;

public class Globals : Node
{
    public int userId; //used for database
    public int userScore; //used for highscore
    public bool gameWon = false; //used so gameOver scene knows whether to show you win or you lose
}
