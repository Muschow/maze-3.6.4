using Godot;
using System;

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
        if (IsOnNode(mazeTm.WorldToMap(pacman.Position))) //only update pacman positon when passed node
        {
            target = currTarget; //pinky travels to players last seen node
        }
    }
}
