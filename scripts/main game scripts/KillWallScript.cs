using Godot;
using System;

public class KillWallScript : Sprite
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";
    private MazeGenerator maze;
    private GameScript game;
    private int speed = 20;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        GetNodes();
        Scale = new Vector2(maze.width + 1, 1);
        Position = new Vector2((maze.width / 2) * MazeGenerator.CELLSIZE, (maze.mazeOriginY + maze.height + 10) * MazeGenerator.CELLSIZE); //centers

    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        MoveKillWall(delta);
    }

    private void GetNodes()
    {
        maze = GetNode<MazeGenerator>("/root/Game/MazeContainer/Maze/MazeTilemap");
        game = GetNode<GameScript>("/root/Game/");
    }

    private void MoveKillWall(float delta)
    {
        int deletedMazeY = (game.mazeStartLoc + (maze.height * 3) - 2) * MazeGenerator.CELLSIZE;
        //int deletedMazeY = (game.mazeStartLoc + (maze.height * 3) - maze.height / 2) * Globals.CELLSIZE; //this one is much harder, spawns wall halfway through deleted maze
        if (Position.y > deletedMazeY)
        {
            Position = new Vector2(Position.x, deletedMazeY);
        }
        else
        {
            Position = Position.MoveToward(new Vector2(Position.x, -GameScript.INFINITY), delta * (speed * GameScript.gameSpeed));
        }
    }
}
