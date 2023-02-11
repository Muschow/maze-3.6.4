using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class Movement : Node
{
    public LinkedList<Tuple<Vector2, int>>[] adjList;
    public List<Vector2> nodeList;

    //used to convert a (0,y) or (x,0) vector to an int. 
    //This is so I can get a distance from neighoburing nodes.
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

    //Heuristic is only used in A* algorithm so thats what the bool is for
    //It is used to make more accurate predictions on shortest path so not every path needs to be checked
    //I am using manhattan distance which is end coord - source added together. 
    //Manhattan distance is used for A* when you can move only in 4 different directions, which in my pacman game you can.
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

        //if the target and source arent in the nodelist, return. It is likely
        //the ghost is not on the same maze as the player and in my game the ghosts intentionally cant travel between mazes
        //so no point doing any calculations.
        if (!(nodeList.Contains(target) && nodeList.Contains(source)))
        {
            return pathList;
        }

        if (source == target) //this is as there is nothing to pathfind here, youve reached your destination. Adds source so it stays to that position.
        {
            pathList.Add(source);
            return pathList;
        }

        List<Vector2> unvisited = new List<Vector2>(); //stores unvisited nodes so we dont waste time visiting visited ones

        Dictionary<Vector2, Vector2> previous = new Dictionary<Vector2, Vector2>(); //stores the previous nodes in the shortest path from the source

        //stores the calculated distances which are all set to infinity at the start as we dont know their sizes yet apart from start node.
        Dictionary<Vector2, int> distances = new Dictionary<Vector2, int>(); //stores coordinate and its distance from source

        for (int i = 0; i < nodeList.Count; i++)
        {
            unvisited.Add(nodeList[i]); //initially every node is unvisited.
            distances.Add(nodeList[i], GameScript.INFINITY); //sets the node distance to infinity (or in this case 9999 lol)
        }

        distances[source] = 0; //sets distance from source to 0 as it starts at source

        while (unvisited.Count > 0)
        {
            //order unvisted list by distance from source. Heuristic is only used in A*, otherwise it adds 0
            unvisited = unvisited.OrderBy(node => distances[node] + Heuristic(source, node, Astar)).ToList();

            Vector2 current = new Vector2(unvisited[0]); //get unvisited node with smallest distance
            unvisited.RemoveAt(0); //removes it from unvisited list, essentially marking it as visited

            if (current == target) //if current is target, we've gone through all possibilities and found our shortest path
            {
                while (previous.ContainsKey(current)) //work backwards from target node to find previous nodes we must go to to get to it
                {
                    pathList.Add(current);
                    current = previous[current];
                }
                pathList.Add(current); //insert the source onto the final result
                pathList.Reverse(); //right now the list goes from target --> source, reverse the list to get source --> target

                break;
            }


            int curIndex = nodeList.IndexOf(current);

            if (curIndex == -1) //debug purposes only, can get rid of it
            {
                //GD.Print("Could not find current node in nodeList"); //debug
            }

            int neighbourVal = 0;
            foreach (Tuple<Vector2, int> edge in adjList[curIndex]) //for each neighbour coordinate for current node
            {
                neighbourVal = edge.Item2; //edge.Item2 is the distance from neighbour to current
                int alt = distances[current] + neighbourVal; //alternate distance is distance from source of current + distance to neighbour
                Vector2 neighbourNode = edge.Item1; //edge.Item1 is the neighbour node coordinates

                if (alt < distances[neighbourNode]) //if the distance from source is shorter when going through current, 
                {
                    distances[neighbourNode] = alt; //update dist from source for neighbour node
                    previous[neighbourNode] = current; //show that you must go through current by putting it as previous to neighbour
                }
            }
        }

        return pathList;
    }

    public List<Vector2> BFS(Vector2 source, Vector2 target)
    {
        List<Vector2> pathList = new List<Vector2>();

        //if the target and source arent in the nodelist, return. It is likely
        //the ghost is not on the same maze as the player and in my game the ghosts intentionally cant travel between mazes
        //so no point doing any calculations.
        if (!(nodeList.Contains(target) && nodeList.Contains(source)))
        {
            return pathList;
        }

        if (source == target) //this is as there is nothing to pathfind here, youve reached your destination. Adds source so it stays to that position.
        {
            pathList.Add(source);
            return pathList;
        }

        Queue<Vector2> bfsQ = new Queue<Vector2>(); //breadth first search uses a queue to search the nodes
        bfsQ.Enqueue(source);

        List<Vector2> unvisited = new List<Vector2>();
        foreach (Vector2 node in nodeList)
        {
            unvisited.Add(node);
        }
        unvisited.Remove(source);

        Dictionary<Vector2, Vector2> previous = new Dictionary<Vector2, Vector2>(); //stores the previous nodes in the shortest path from the source
        //bfs is used for unweighted graph so the distances from start arent needed, just finds the shortest paths in terms of number of edges.

        while (bfsQ.Count > 0) //while bfsQ is not empty
        {
            Vector2 currNode = bfsQ.Dequeue();
            int curIndex = nodeList.IndexOf(currNode);

            foreach (Tuple<Vector2, int> edge in adjList[curIndex]) //for each neighbour node of current node
            {
                Vector2 neighbour = edge.Item1; //edge.Item1 is coordinate of neighbour
                if (unvisited.Contains(neighbour))
                {
                    bfsQ.Enqueue(neighbour);
                    unvisited.Remove(neighbour);
                    previous[neighbour] = currNode;
                }
            }

        }

        Vector2 current = target;
        while (previous.ContainsKey(current)) //work backwards from target to get to source
        {
            pathList.Add(current);
            current = previous[current];
        }
        pathList.Add(source); //as source was the first thing, it was never added to previous so we add it now
        pathList.Reverse(); //from target to source --> source to target

        //GD.Print("bfs complete, pathlist count " + pathList.Count);

        return pathList;
    }
}
