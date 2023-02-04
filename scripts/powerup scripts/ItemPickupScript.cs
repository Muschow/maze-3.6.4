using Godot;
using System;

public abstract class ItemPickupScript : Sprite
{
    [Export] public int baseScore = 100;
    protected GameScript game;


    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        game = GetNode<GameScript>("/root/Game");
    }

    public void _OnItemAreaEntered(Area area)
    {
        //GD.Print(area.Name);
        if (area.Name == "PacmanArea")
        {
            game.score += (int)(baseScore * game.scoreMultiplier);
            ItemAbility();
        }
    }
    public abstract void ItemAbility();
}
