using Godot;
using System;

public abstract class ItemPickupScript : Sprite
{
    [Export] protected int baseScore = 100;
    private GameScript game;

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
        game.score += (int)(baseScore * game.scoreMultiplier);
        ItemAbility();
        QueueFree();
    }

    public abstract void ItemAbility();
}
