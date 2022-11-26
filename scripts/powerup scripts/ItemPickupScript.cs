using Godot;
using System;

public abstract class ItemPickupScript : Sprite
{
    [Export] protected int baseScore = 100;
    protected GameScript game;


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


    public void _OnItemAreaEntered(Area area)
    {
        //GD.Print(area.Name);
        if (area.Name == "PacmanArea")
        {
            game.score += (int)(baseScore * game.scoreMultiplier);
            ItemAbility();
        }
    }

    //Must call QueueFree somewhere to delete the node

    public abstract void ItemAbility();
}
