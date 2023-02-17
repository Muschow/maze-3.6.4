using Godot;
using System;

public class LLNode : Node
{
    public Vector2 data;   //actual data in node in linked list
    public LLNode next; //pointer to the next node

    public LLNode(Vector2 dataToBeAdded) //constructor
    {
        data = dataToBeAdded;
        next = null; //default is null
    }
}
