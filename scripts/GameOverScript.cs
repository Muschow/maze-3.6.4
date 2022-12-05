using Godot;
using System;

public class GameOverScript : Control
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";
    private Globals global;
    private Node database;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        global = GetNode<Globals>("/root/Globals");
        database = GetNode<Node>("/root/Database");

        int[] dbParams = new int[] { global.userId, global.userScore };
        database.Call("queryDBwithParameters", "INSERT INTO HighScores (PlayerID, Score) VALUES (?, ?);", dbParams);
        //database.Call("queryDB", "DELETE FROM HighScores WHERE Score NOT IN (SELECT * FROM HighScores ORDER BY Score LIMIT 10);");


    }
    //DELETE FROM Table WHERE ID NOT IN (SELECT TOP 10 ID FROM Table)

    //  // Called every frame. 'delta' is the elapsed time since the previous frame.
    //  public override void _Process(float delta)
    //  {
    //      
    //  }

    private void _OnQuitToMainMenuButtonPressed()
    {
        //queue free game and this
        //switch scene to main menu

    }

    private void _OnRestartButtonPressed()
    {
        GetTree().ChangeScene("res://scenes/Game.tscn");

        //reload game scene
        //queuefree this
    }
}
