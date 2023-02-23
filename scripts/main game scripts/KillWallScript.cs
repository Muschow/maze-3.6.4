using Godot;
using System;

public class KillWallScript : Sprite
{
    private MazeGenerator maze;
    private GameScript game;
    private int speed = 20;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        GetNodes();
        GD.Print("killwall basespeed" + speed);
        Scale = new Vector2(maze.width + 1, 1); //the killwall is just a bit larger than the width of maze
        Position = new Vector2((maze.width / 2), (maze.mazeOriginY + maze.height + 10)) * MazeGenerator.CELLSIZE; //centers killwall
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

    private void MoveKillWall(float delta) //killwall tps and is always moving so the player is forced to move up and is always being chased
    {
        int deletedMazeY = (game.mazeStartLoc + (maze.height * 3) - 2) * MazeGenerator.CELLSIZE; //y coord of bottom of 1 maze behind new maze player is on
        //int deletedMazeY = (game.mazeStartLoc + (maze.height * 3) - maze.height / 2) * Globals.CELLSIZE; //this one is much harder, spawns wall halfway through deleted maze
        if (Position.y > deletedMazeY)
        {
            Position = new Vector2(Position.x, deletedMazeY); //if the player is on a new maze and the wall is 2 mazes behind, teleport the wall bottom of 1 maze behind
        }
        else //otherwise move wall upwards constantly with speed of game
        {
            Position = Position.MoveToward(new Vector2(Position.x, -Globals.INFINITY), delta * (speed * GameScript.gameSpeed));
        }
    }
}
