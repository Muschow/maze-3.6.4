using Godot;

public class PinkyScript : GhostScript
{
    public PinkyScript()
    {
        ghostColour = Colors.Pink;
        searchingAlgo = algo.bfs;
        baseSpeed = 110;
    }


    public override void UpdateTarget()
    {
        Vector2 currTarget = mazeTm.WorldToMap(pacman.Position);
        if (IsOnNode(currTarget)) //only update pacman positon when passed node mazeTm.WorldToMap(pacman.Position)
        {
            target = currTarget; //pinky travels to players last seen node
        }
    }
}
