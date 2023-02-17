using Godot;
using System;

public class LLQueue : Node
{
    private LLNode front; //frontpointer of q
    private LLNode rear; //rearpointer of q

    public LLQueue()
    {
        front = null; //initialise both to null
        rear = null;
    }

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
            return front.data; //get value from top of queue
        }
        else
        {
            throw new InvalidOperationException("Queue empty, cannot peek");
        }
    }

    public bool isEmpty()
    {
        if (rear == null) //if there is no node at the back then obviously its empty as a node gets added to the back
            return true;
        else
            return false;
    }
}
