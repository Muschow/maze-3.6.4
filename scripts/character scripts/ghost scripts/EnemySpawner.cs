using Godot;
using System;

public class EnemySpawner : Node2D
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";

    // Called when the node enters the scene tree for the first time.

    private PackedScene RedGhost = GD.Load<PackedScene>("res://scenes/ghost scenes/RedGhost.tscn");
    private PackedScene BlueGhost = GD.Load<PackedScene>("res://scenes/ghost scenes/BlueGhost.tscn");
    private PackedScene OrangeGhost = GD.Load<PackedScene>("res://scenes/ghost scenes/OrangeGhost.tscn");
    private PackedScene PinkGhost = GD.Load<PackedScene>("res://scenes/ghost scenes/PinkGhost.tscn");
    private PackedScene[] ghostArray;
    [Export] private int numGhosts = 4; //export just makes it available to edit in the editor, like [SerializeField]

    public override void _Ready()
    {
        ghostArray = new PackedScene[4] { RedGhost, OrangeGhost, PinkGhost, BlueGhost };

        for (int i = 0; i < numGhosts; i++)
        {
            int randomIndex = (int)GD.RandRange(0, ghostArray.Length);
            AddChild(ghostArray[randomIndex].Instance()); //adds 4 random ghosts as child to enemycontainer
        }
    }


    //  // Called every frame. 'delta' is the elapsed time since the previous frame.
    //  public override void _Process(float delta)
    //  {
    //      
    //  }
}
