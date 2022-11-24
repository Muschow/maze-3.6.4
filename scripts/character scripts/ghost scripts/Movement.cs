using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class Movement : Node
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text"; 
    public LinkedList<Tuple<Vector2, int>>[] adjList;
    public List<Vector2> nodeList;

    //make a function to convert the source vector from a float to a actual vector by doing MapToWorld and WorldToMap etc
    public int ConvertVecToInt(Vector2 vector)
    {
        if (vector.x == 0)
        {
            return (int)vector.y;
        }
        else if (vector.y == 0)
        {
            return (int)vector.x;
        }
        else if (vector.x == 0 && vector.y == 0)
        {
            return 0;
        }
        else
        {
            return -1; //bascially error
        }
    }

    private int Heuristic(Vector2 source, Vector2 end, bool Astar)
    {
        if (Astar)
        {
            float x = Math.Abs(source.x - end.x);
            float y = Math.Abs(source.y - end.y);
            return (int)(x + y);
        }
        else
        {
            return 0;
        }
    }




    public List<Vector2> Dijkstras(Vector2 source, Vector2 target, bool Astar) //takes in graph (adjMatrix) and source (Pos) Ghost MUST spawn on node
    {
        List<Vector2> pathList = new List<Vector2>();

        //make all the adjList stuff static and then do nodeList = MazeGenerator.nodeList

        //to reset my changes, make the ajList properties static and then replace mazeG.nodeList with MazeGenerator.nodeList
        GD.Print("printing nodelist count in Movement...");
        GD.Print(nodeList.Count);

        GD.Print("source " + source);
        GD.Print("target " + target);
        if (nodeList.Contains(target))
        {
            GD.Print("target is a node");
        }
        else
        {
            GD.Print("target is not a node");
        }

        if (nodeList.Contains(source))
        {
            GD.Print("source is a node");
        }
        else
        {
            GD.Print("source is not a node");
        }
        //Have a method here that makes sure source and target are nice round Vectors and not decimals or something like that
        //Im thinking WorldToMap and then MapToWorld again

        if (!(nodeList.Contains(target) && nodeList.Contains(source)))
        {
            return pathList;
        }

        if (source == target)
        {
            pathList.Add(source);
            return pathList;
        }

        List<Vector2> unvisited = new List<Vector2>();

        // Previous nodes in optimal path from source
        Dictionary<Vector2, Vector2> previous = new Dictionary<Vector2, Vector2>();

        // The calculated distances, set all to Infinity at start, except the start Node
        Dictionary<Vector2, int> distances = new Dictionary<Vector2, int>();

        for (int i = 0; i < nodeList.Count; i++)
        {
            unvisited.Add(nodeList[i]);

            // Setting the node distance to Infinity (or in this case 9999 lol)
            distances.Add(nodeList[i], Globals.INFINITY);

            //previous.Add(nodeList[i], Vector2.Zero);
        }

        distances[source] = 0;

        while (unvisited.Count != 0)
        {
            //order unvisted list by distance.
            unvisited = unvisited.OrderBy(node => distances[node] + Heuristic(source, node, Astar)).ToList();

            Vector2 current = new Vector2(unvisited[0]); //get node with smallest distance
            unvisited.RemoveAt(0);

            if (current == target)
            {
                //Gd.Print("curr = " + current);
                //Gd.Print("target = " + target);

                GD.Print("curr == target");
                while (previous.ContainsKey(current))
                {
                    //Gd.Print("previous[current] " + previous[current]);
                    //insert the node onto the final result
                    pathList.Add(current);
                    current = previous[current];

                    //Gd.Print("current: " + current);

                }
                //insert the source onto the final result
                pathList.Add(current);
                pathList.Reverse(); //check if this reverse even works

                break; //maybe a return would be better here...
            }

            for (int i = 0; i < nodeList.Count; i++)
            {
                //GD.Print("current vec: " + current);
                int curIndex = nodeList.IndexOf(current);


                if (curIndex == -1)
                {
                    GD.Print("Could not find current node in nodeList");
                }

                int neighbourVal = 0;
                foreach (Tuple<Vector2, int> edge in adjList[curIndex])
                {
                    if (edge.Item1 == nodeList[i])
                    {
                        neighbourVal = edge.Item2;
                    }
                }

                if (neighbourVal != 0)
                {
                    int alt = distances[current] + neighbourVal;
                    Vector2 neighbourNode = nodeList[i]; //something to do with these lines

                    if (alt < distances[neighbourNode])
                    {
                        distances[neighbourNode] = alt;
                        previous[neighbourNode] = current;
                    }
                }

            }
        }

        if (Astar == false)
            GD.Print("dijkstras complete, pathlist count " + pathList.Count);
        else
            GD.Print("astar complete, pathlist count " + pathList.Count);

        return pathList;
    }

    public List<Vector2> BFS(Vector2 source, Vector2 target)
    {
        List<Vector2> pathList = new List<Vector2>();

        if (!(nodeList.Contains(target) && nodeList.Contains(source)))
        {
            return pathList;
        }

        if (source == target)
        {
            pathList.Add(source);
            return pathList;
        }

        Queue<Vector2> bfsQ = new Queue<Vector2>();
        bfsQ.Enqueue(source);

        List<Vector2> unvisited = new List<Vector2>();
        foreach (Vector2 node in nodeList)
        {
            unvisited.Add(node);
        }
        unvisited.Remove(source);

        Dictionary<Vector2, Vector2> previous = new Dictionary<Vector2, Vector2>();

        while (bfsQ.Count > 0)
        {
            Vector2 currNode = bfsQ.Dequeue();
            int curIndex = nodeList.IndexOf(currNode);

            foreach (Tuple<Vector2, int> edge in adjList[curIndex])
            {
                Vector2 neighbour = edge.Item1;
                if (unvisited.Contains(neighbour))
                {
                    bfsQ.Enqueue(neighbour);
                    unvisited.Remove(neighbour);
                    previous[neighbour] = currNode;
                }
            }

        }

        Vector2 current = target;
        while (current != source)
        {
            pathList.Add(current);
            current = previous[current];
        }
        pathList.Add(source);
        pathList.Reverse();

        GD.Print("bfs complete, pathlist count " + pathList.Count);

        return pathList;
    }


    //Called when the node enters the scene tree for the first time.
    // public override void _Ready()
    // {
    //     


    // }

    //  // Called every frame. 'delta' is the elapsed time since the previous frame.
    //  public override void _Process(float delta)
    //  {
    //      
    //  }
}
