using Godot;
using System;
using System.Collections.Generic;

public class QueueScript : Node
{
    private List<Vector2> queueList = new List<Vector2>();
    private int fp = 0;
    public int rp = -1;

    public bool isEmpty()
    {
        if (fp > rp)
            return true;
        else
            return false;
    }

    //normally isFull goes here but lists dont really get full so not needed

    public void Enqueue(Vector2 vector) //the error here is that to put an item in list[1], there must be something in list[0] but sometimes there isnt
    {
        //if isFull false would normally be called here but lists dont really get full
        rp++;
        GD.Print("rearpointer", rp);
        queueList.Insert(rp, vector);

    }

    public Vector2 Dequeue()
    {
        //normally if isEmpty false would be herem but the Peek would error if empty so no need
        Vector2 returnedV = Peek();
        if (!isEmpty())
        {
            queueList.RemoveAt(fp);
            fp++;
        }

        return returnedV;
    }

    public Vector2 Peek()
    {
        return queueList[fp];
    }
}
