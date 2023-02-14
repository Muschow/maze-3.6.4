using Godot;
using System;
using System.Collections.Generic;

public class GameOverScript : Control
{
    private Globals global;
    private Node database;
    private Label scoreLabel;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        global = GetNode<Globals>("/root/Globals");
        database = GetNode<Node>("/root/Database");

        GetNode<Label>("CanvasLayer/Panel/YourScore").Text = "Your Score:\n" + global.userScore;

        if (global.gameWon)
        {
            GetNode<Label>("CanvasLayer/Panel/Heading").Text = "YOU WIN!";

            int[] dbParams = new int[] { global.userId, global.userScore };
            database.Call("queryDBwithParameters", "INSERT INTO HighScores (PlayerID, Score) VALUES (?, ?);", dbParams); //adds new highscore to db
            database.Call("queryDB", "DELETE FROM HighScores WHERE ID NOT IN (SELECT ID FROM HighScores ORDER BY Score DESC LIMIT 10);"); //removes rows below top 10 scores
        }

        //gets name and score of top 10 scores
        var scoreQueryResult = database.Call("queryDB", "SELECT PlayerInfo.Username, HighScores.Score FROM PlayerInfo INNER JOIN HighScores ON PlayerInfo.Id = HighScores.PlayerId ORDER BY Score DESC LIMIT 10;");
        Godot.Collections.Array returnedScoreValues = (Godot.Collections.Array)database.Call("queryValue", scoreQueryResult); //returns array of username, score, username, score etc...

        //sorts the godot.collections.array of values returned into more usable, 2 lists: nameList and scoreList
        List<string> nameList = new List<string>();
        List<string> scoreList = new List<string>();
        for (int i = 0; i < returnedScoreValues.Count; i++)
        {
            if (i % 2 == 0)
            {
                nameList.Add((string)returnedScoreValues[i]);
            }
            else
            {
                int score = (int)returnedScoreValues[i];
                scoreList.Add(Convert.ToString(score));
            }
        }

        //displays scores
        for (int i = 0; i < nameList.Count; i++)
        {
            GetNode<Label>("CanvasLayer/Panel/Heading/SubHeading/Control/Name" + i).Text = nameList[i];
            GetNode<Label>("CanvasLayer/Panel/Heading/SubHeading/Control/Score" + i).Text = scoreList[i];
        }
    }

    private void _OnQuitToMainMenuButtonPressed()
    {
        GetTree().ChangeScene("res://scenes/menu scenes/MainMenuScene.tscn");
    }

    private void _OnRestartButtonPressed()
    {
        GetTree().ChangeScene("res://scenes/main game scenes/Game.tscn");
    }
}
