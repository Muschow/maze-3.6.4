using Godot;
using System;

public class LLStack : Node
{
    private LLNode top; //define the top pointer

    public LLStack()
    {
        top = null; //initialise top pointer to null
    }

    public void Push(Vector2 valueToBePushed)
    {
        LLNode newNode = new LLNode(valueToBePushed); //create new node to be added to linked list containing data

        if (isEmpty()) //if top is empty, therefore stack empty
        {
            newNode.next = null; //then this is the top so next doesnt exist
        }
        else
        {
            newNode.next = top; //if stack isnt empty, next = top
        }
        top = newNode; //however, as stack pushes to the top, top is this node
                       //therefore, the next of the newNode is pointing to the previous node

        GD.Print(valueToBePushed + " pushed to stack"); //debug
    }

    public Vector2 Pop()
    {
        if (isEmpty())
        {
            throw new InvalidOperationException("Stack empty, cannot pop");
        }

        GD.Print("popped " + top.data);

        Vector2 returnedV;
        returnedV = Peek();

        top = top.next; //if stack not empty increment top to point to next node
        //(the previous of the top that just got popped)

        return returnedV; //returns popped value
    }

    public Vector2 Peek()
    {
        if (!isEmpty())
        {
            GD.Print("peeked:" + top.data);
            return top.data;
        }
        else
        {
            throw new InvalidOperationException("Stack empty, cannot peek");
        }
    }

    public bool isEmpty()
    {
        if (top == null)
            return true;
        else
            return false;
    }
}
