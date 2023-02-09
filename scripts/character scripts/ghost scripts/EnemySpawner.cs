using Godot;


public class EnemySpawner : Node2D
{
    private PackedScene RedGhost = GD.Load<PackedScene>("res://scenes/character scenes/ghost scenes/RedGhost.tscn");
    private PackedScene BlueGhost = GD.Load<PackedScene>("res://scenes/character scenes/ghost scenes/BlueGhost.tscn");
    private PackedScene OrangeGhost = GD.Load<PackedScene>("res://scenes/character scenes/ghost scenes/OrangeGhost.tscn");
    private PackedScene PinkGhost = GD.Load<PackedScene>("res://scenes/character scenes/ghost scenes/PinkGhost.tscn");
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
}
