using Godot;

public class BlinkyScript : GhostScript
{
    public BlinkyScript()
    {
        ghostColour = Colors.Red;
        searchingAlgo = algo.dijkstras;
    }

    public override void UpdateTarget()
    {
        target = FindClosestNodeTo(mazeTm.WorldToMap(pacman.Position)); //blinky finds closest node to player
        //GD.Print("blinky speed" + speed); //debug
    }
}
