using Godot;
using System;

public class KillWallScript : Sprite
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";
    private MazeGenerator maze;
    private int speed = 20;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        maze = GetNode<MazeGenerator>("/root/Game/MazeContainer/Maze/MazeTilemap");
        Scale = new Vector2(maze.width + 1, 1);
        Position = new Vector2((maze.width / 2) * 32, (maze.mazeOriginY + maze.height + 10) * 32); //centers
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        Position = Position.MoveToward(new Vector2(Position.x, -Globals.INFINITY), delta * (speed * Globals.gameSpeed));
    }
}
