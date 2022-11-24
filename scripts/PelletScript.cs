using Godot;
using System;

public class PelletScript : Sprite
{

    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";
    private GameScript game;
    [Export] private int pelletScore = 10;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        game = GetNode<GameScript>("/root/Game");
    }

    //  // Called every frame. 'delta' is the elapsed time since the previous frame.
    //  public override void _Process(float delta)
    //  {
    //      
    //  }

    public void _OnPelletAreaEntered(Area area)
    {
        game.score += (int)(pelletScore * game.scoreMultiplier); //add 100*mult to gamescript.score
        QueueFree();
    }
}
