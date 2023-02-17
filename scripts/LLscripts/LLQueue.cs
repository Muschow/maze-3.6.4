using Godot;
using System;

public class LLQueue
{
    private LLNode front = null; //frontpointer of q, initialise to null
    private LLNode rear = null; //rearpointer of q, initialise to null

    public void Enqueue(Vector2 valueToBeAdded)
    {
        LLNode newNode = new LLNode(valueToBeAdded); //create new node to be added to linked list containing data

        if (isEmpty())
        {
            front = newNode; //if empty, the newNode is at front and back of queue
            rear = newNode;
        }
        else
        {
            rear.next = newNode; //rear next points to newNode (same as incrementing rearpointer)
            rear = newNode; //add newNode to back of queue
        }

        //GD.Print(valueToBeAdded + " enqueued to queue");
    }

    public Vector2 Dequeue()
    {
        Vector2 returnedV;

        if (isEmpty())
        {
            throw new InvalidOperationException("Queue empty, cannot dequeue");
        }
        else
        {
            returnedV = Peek(); //get value from dequeue
            front = front.next; //point front to next element (same as incrementing frontpointer)
            return returnedV; //return dequeued value
        }

    }

    public Vector2 Peek()
    {
        if (!isEmpty())
        {
            //GD.Print(front.data + "peeked");
            return front.data; //get value from top of queue
        }
        else
        {
            throw new InvalidOperationException("Queue empty, cannot peek");
        }
    }

    public bool isEmpty()
    {
        if (rear == null || front == null) //if there is no node at the back/front then its empty
            return true;
        else
            return false;
    }
}
