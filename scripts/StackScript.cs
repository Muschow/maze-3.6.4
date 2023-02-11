using Godot;
using System;
using System.Collections.Generic;

public class StackScript : Node
{
    private List<Vector2> stackList = new List<Vector2>();
    private int sp = -1;
    public bool isEmpty()
    {
        if (sp == -1)
            return true;
        else
            return false;
    }

    //normally, if we were using an array we would need an isfull function
    //for all intents and purposes the list wont ever get full so not really any need for isfull

    public Vector2 Peek()
    {
        return stackList[sp];
    }

    public void Push(Vector2 vector)
    {
        //normally if(!isFull()) would go here but lists dont really get full (unless you put loads of items in it) so its not needed
        sp++;
        stackList.Insert(sp, vector);
    }

    public Vector2 Pop()
    {
        Vector2 returnedV = Peek();
        //normally if(!isEmpty()) would go here
        //but Peek will error if empty so its not needed
        stackList.RemoveAt(sp);
        sp--;
        return returnedV;
    }
}
