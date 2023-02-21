using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
public class MazeGenerator : TileMap
{
    //---------------------------------------------statics and constants-----------------------------
    public const int CELLSIZE = 32;
    public const int NODE = 0; //on nodetilemap, nodetile has a value of 0
    public const int WALL = 1; //on mazetilemap, wall has a value of 1
    public const int PATH = 0; //on mazetilemap, path has a value of 0
    public static Vector2 halfCellSize = new Vector2(CELLSIZE / 2, CELLSIZE / 2);

    //-------------------------------------------------------------------------------------------------
    [Export] public int width = 31; //originally 31
    [Export] public int height = 37; //originally 37
    public int mazeOriginY = 0;
    private int backtrackCount = 0; //used to figure out when to join dead ends
    private bool generationComplete = false;

    Vector2[] directions = new Vector2[4] { Vector2.Up, Vector2.Right, Vector2.Down, Vector2.Left }; //used to loop through the directions

    List<Vector2> visited = new List<Vector2>();
    List<Vector2> wallEdgeList = new List<Vector2>(); //used to make sure wall edges arent removed when removing dead ends
    LLStack IDFSstack = new LLStack();

    //-----------------------------------------------------Adjacency Matrix/List properties---------------------------------------------------------------
    public List<Vector2> nodeList = new List<Vector2>(); //for nodes,maybe get rid of this to be honest
    public LinkedList<Tuple<Vector2, int>>[] adjacencyList;

    //-------------------------------------------------------------GetNodes------------------------------------------------------------------------------
    private Node2D powerupContainer; //parent node to hold powerups
    private Node2D powerupFactory; //scene that contains one of each powerup
    private TileMap nodeTilemap;
    private GameScript gameScr;

    //-----------------------------------------------------------PackedScenes---------------------------------------------------------------------------
    PackedScene pelletScene = GD.Load<PackedScene>("res://scenes/powerup scenes/Pellet.tscn");
    PackedScene powerupFactoryScene = GD.Load<PackedScene>("res://scenes/powerup scenes/PowerupFactory.tscn");

    //-------------------------------------------------------------Ready and Process--------------------------------------------------------------------
    public override void _EnterTree() //called before ready, when entered scene tree
    {
        CorrectMazeSize(); //makes width and height odd when maze enters scene tree (before ready is called)
    }
    //Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        //GD.Print("mazegen ready"); //debug

        GD.Randomize();
        GetNodes();

        powerupFactory = (Node2D)powerupFactoryScene.Instance(); //powerupfactory is a scene that contains all the powerups. used to spawn them in
        mazeOriginY = gameScr.mazeStartLoc; //just means the maze origin stays throughout

        IterativeDFSInit();
        GenerateAdjList();
        AddRandomPowerups();

        //GD.Print("nodeList Count: " + nodeList.Count); //debug
    }

    private void GetNodes()
    {
        gameScr = GetNode<GameScript>("/root/Game");
        nodeTilemap = GetParent().GetNode<TileMap>("NodeTilemap");
        powerupContainer = GetParent().GetNode<Node2D>("PowerupContainer");
    }

    //------------------------------------------------------Instancing Scenes Functions----------------------------------------------------------------
    private void AddPellet(Vector2 spawnPosition)
    {
        Sprite pelletInstance = (Sprite)pelletScene.Instance();
        GetParent().GetNode<Node2D>("PelletContainer").AddChild(pelletInstance);
        pelletInstance.Position = MapToWorld(spawnPosition) + halfCellSize; //CHANGED THIS MAKE SURE TO TEST IT!!!!
    }

    private void AddRandomPowerups() //adds powerups to random paths on maze by getting random child of powerupFactory and duplicating it
    {
        int numPowerupsToSpawn = (int)GD.RandRange(1, 5);
        for (int i = 0; i < numPowerupsToSpawn; i++)
        {
            Sprite powerup = (Sprite)powerupFactory.GetChild((int)GD.RandRange(0, powerupFactory.GetChildCount())).Duplicate();
            powerupContainer.AddChild(powerup);
            powerup.Position = SetRandomSpawnOnPath();
        }
    }

    //---------------------------------------------------Maze Generator Helper Functions-----------------------------------------------------------------
    private void CorrectMazeSize() //width and height must be odd as we generate the maze as a grid of path tiles, each surrounded by walls. Otherwise, edges wouldnt work
    {
        if (width % 2 != 1)
        {
            width -= 1;
        }
        if (height % 2 != 1)
        {
            height -= 1;
        }
        //GD.Print("width " + width); //debug
        //GD.Print("height " + height); //debug
    }

    private void CreateStartingGrid() //is a grid of path tiles, each surrounded by walls. top is empty, bottom has a wall edge.
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                //wall tile edges
                if ((i == 0 && j != 0) || (i == width - 1 && j != 0) || (j == height - 1)) //j != 0 stuff removes the entire top layer
                {
                    SetCell(i, j + mazeOriginY, WALL);

                    Vector2 wallEdge = new Vector2(i, j + mazeOriginY);
                    wallEdgeList.Add(wallEdge);
                }
                //alternating wall tiles
                else if ((i % 2 == 0 || j % 2 == 0) && (j != 0)) //again, j!=0 removes the top layer so that the next maze can slot into it
                {
                    SetCell(i, j + mazeOriginY, WALL);
                }
                //path tiles that go between those alternating wall tiles
                else if (j != 0)
                {
                    SetCell(i, j + mazeOriginY, PATH);
                    AddPellet(new Vector2(i, j + mazeOriginY));
                }
            }
        }
    }

    private void FixDeadEnds(Vector2 currentV) //removes dead ends so the maze is a braid maze
    {
        bool complete = false;

        for (int i = 0; i < directions.Length; i++)
        {
            if (!complete)
            {
                Vector2 newCell = new Vector2(currentV + directions[i]);
                if ((GetCellv(newCell) == WALL) && (!wallEdgeList.Contains(newCell)) && (!visited.Contains(newCell)))
                {
                    SetCellv(newCell, PATH);
                    AddPellet(newCell);

                    if (GetCellv(currentV + (directions[i] * 3)) != PATH)
                    {
                        AddNode(currentV + (directions[i] * 2));
                    }
                    if (GetCellv(currentV + (directions[i] * -1)) != PATH)
                    {
                        AddNode(currentV);
                    }
                    complete = true;
                }
            }
        }
    }

    private void PrepMazeForJoin(int numHoles) //removes walls on top and adds a couple paths to floor so that they can join seamlessly
    {
        Random rnd = new Random();
        int numUsedCells = 0;

        for (int i = 1; i < width - 1; i++) //this loop sets the top row of the maze into just paths so it can join with the bottom of another maze
        {
            Vector2 topWallCell = new Vector2(i, mazeOriginY);

            if (GetCellv(topWallCell + Vector2.Down) == WALL)
            {
                SetCellv(topWallCell + Vector2.Down, PATH);
                AddPellet(topWallCell + Vector2.Down);
                //GD.Print("set " + new Vector2(removeCell + south) + " path"); //debug
                //GD.Print("set cell+south path"); //debug
            }

            //on the top layer, if there isnt a node where there should be one due to removing the top wall, place one
            if (GetCellv(topWallCell + (Vector2.Down * 2)) == PATH && nodeTilemap.GetCellv(topWallCell + Vector2.Down) != NODE)
            {
                AddNode(topWallCell + Vector2.Down);
                //GD.Print("addNode " + new Vector2(topWallCell + south)); //debug
            }
        }

        //GD.Print(mazeOriginY); //debug



        if (gameScr.mazesOnTheScreen > 0) //If its not the first maze, Add paths to the floor so that you can join to the maze below
        {
            while (numUsedCells < numHoles)
            {
                int cellX = rnd.Next(1, width - 1);
                Vector2 cell = new Vector2(cellX, mazeOriginY + height - 1);
                if (GetCellv(cell) == WALL && GetCellv(cell + Vector2.Up) == PATH && GetCellv(cell + Vector2.Right) == WALL && GetCellv(cell + Vector2.Left) == WALL) //makes it so each hole has 2 walls either side
                {
                    SetCellv(cell, PATH);
                    AddPellet(cell);
                    numUsedCells++;
                    //I deliberately made it so there are no nodes joining the 2 mazes. This is as a ghost is instanced on its own maze; if pacman goes between mazes, 
                    //it switches to being chased by the ghosts on that maze. This way, ghosts arent going between mazes and leaving 1 maze empty and 1 maze full.
                }

            }
        }

    }

    private void AddNode(Vector2 nodeLocation) //adds a node making sure there are no duplicates while adding to nodelist for use in adjList
    {
        if (nodeTilemap.GetCellv(nodeLocation) != NODE) //makes sure theres no duplicates. I probably dont need this but just in case lol
        {
            nodeTilemap.SetCellv(nodeLocation, NODE); //turns it into an actual path node tile

            nodeList.Add(nodeLocation);

        }
        else
        {
            //GD.Print("found bad node"); //debug
        }
    }

    //------------------------------------------------------Actual Maze Generation functions---------------------------------------------------------------
    private void IterativeDFSInit()
    {
        generationComplete = false;


        CreateStartingGrid();

        //startVector x and y must be odd, between 1+mazeOriginX/Y & height-1 / width-1 
        Vector2 startVector = new Vector2(1, mazeOriginY + 1); //Choose the initial cell,
        //GD.Print("StartV: " + startVector); //debug

        visited.Add(startVector); //Mark initial cell as visited,
        //idfStack.Push(startVector); //and push it to the stack,
        IDFSstack.Push(startVector);

        IterativeDFSStep();
    }

    private void IterativeDFSStep()
    {
        Vector2 prev = new Vector2(0, 0);
        while (!generationComplete)
        {
            //Vector2 curr = idfStack.Pop(); //Pop a cell from the stack and make it a current cell.
            Vector2 curr = IDFSstack.Pop();
            Vector2 next = new Vector2(0, 0);

            bool found = false;

            //check neighbours in random order //N,E,S,W walls instead of their paths, so *2
            Random rnd = new Random();

            var rndDirections = directions.OrderBy(_ => rnd.Next()).ToList(); //randomly shuffle the list, create new list (rndDirections) and put values in there

            for (int i = 0; i < rndDirections.Count; i++)
            {
                next = 2 * rndDirections[i];
                if (GetCellv(curr + next) == PATH && (!visited.Contains(curr + next)))
                { //If the current cell has any neighbours which have not been visited,
                    found = true;
                    break; //Choose one of the unvisited neighbours (next),
                }
            }

            if (found)
            {
                if (prev != next)
                {
                    AddNode(curr);
                }
                prev = next;

                //idfStack.Push(curr); //Push the current cell to the stack,
                IDFSstack.Push(curr);
                SetCellv(curr + (next / 2), PATH); // Remove the wall between the current cell and the chosen cell,
                AddPellet(curr + (next / 2));
                visited.Add(curr + next); //Mark the chosen cell as visited,
                //idfStack.Push(curr + next); //and push it to the stack.  
                IDFSstack.Push(curr + next);

                backtrackCount = 0;
            }
            else
            {
                backtrackCount++;
                if (backtrackCount == 1) //if theres a dead end remove it to get braid maze
                {
                    FixDeadEnds(curr);
                }
            }
            //if idfStack.Count <= 0
            if (IDFSstack.isEmpty())
            { //If stack is empty
                FixDeadEnds(curr); //remove the dead end on the source tile
                PrepMazeForJoin(7); //originally 7 holes to travel between each maze

                generationComplete = true;

                //GD.Print("Maze Generation Complete!"); //debug
                return;
            }
        }
    }

    //------------------------------------------------------Adjacency Matrix/List stuff-------------------------------------------------------------------------

    private void PrintNodeList() //used for debugging
    {
        GD.Print("Printing NodeList: ");
        foreach (var thing in nodeList)
        {
            GD.Print(thing);
        }
    }

    private bool IfWallOrNodeBetween(Vector2 v1, Vector2 v2) //if there is a wall or node between the 2 vectors they are not neighbouring
    {
        Vector2 smallerV;
        Vector2 biggerV;
        //swaps so v1 is smaller and v2 is bigger
        if (v1 <= v2)
        {
            smallerV = v1;
            biggerV = v2;
        }
        else
        {
            smallerV = v2;
            biggerV = v1;
        }

        if (smallerV.x == biggerV.x) //if theyre on the same x
        {
            for (int y = (int)smallerV.y; (int)y < biggerV.y; y++) //scan vertically to see if theres wall or node between them
            {
                if ((GetCell((int)smallerV.x, y) == WALL) || (nodeTilemap.GetCell((int)smallerV.x, y) == NODE && y != smallerV.y && y != biggerV.y))
                {
                    //GD.Print("reached get cell x: " + smallerV.x + ",y: " + y); //debug
                    return true;
                }
            }
            return false;
        }
        else if (smallerV.y == biggerV.y) //if theyre on the same y
        {
            for (int x = (int)smallerV.x; (int)x < biggerV.x; x++) //scan horizontally to see if wall or node between them
            {
                if (GetCell(x, (int)smallerV.y) == WALL || (nodeTilemap.GetCell(x, (int)smallerV.y) == NODE && x != smallerV.x && x != biggerV.x))
                {
                    return true;
                }
            }
            return false;
        }
        else //if theyre not on the same x or y then their is always a wall or node between them
        {
            return true;
        }

    }

    private LinkedList<Tuple<Vector2, int>>[] GenerateAdjList()
    {
        int numFound = 0;

        //creates adjList which is a list where each tile has a list containing a tuple for each neighbour with (neighbour node, distance to node)
        adjacencyList = new LinkedList<Tuple<Vector2, int>>[nodeList.Count];
        for (int i = 0; i < adjacencyList.Length; i++)
        {
            adjacencyList[i] = new LinkedList<Tuple<Vector2, int>>();
        }

        for (int i = 0; i < nodeList.Count; i++)
        {
            numFound = 0;
            for (int j = 0; j < nodeList.Count; j++)
            {
                Vector2 v1 = nodeList[i];
                Vector2 v2 = nodeList[j];

                if (v1.x == v2.x || v1.y == v2.y) //vectors must be on same column/row to be neighbours
                {
                    int neighbourVal = Math.Abs(Globals.ConvertVectorToInt(v1 - v2)); //gets distance between the vectors
                    //if on wall, no edge, else put weight
                    if (numFound <= 3) //as theres only 4 direcitons there should be at most 3 neighbours to every tile
                    {
                        if ((!IfWallOrNodeBetween(v1, v2)) && (neighbourVal != 0))
                        {
                            adjacencyList[i].AddLast(new Tuple<Vector2, int>(nodeList[j], neighbourVal));
                            //adjList[i, numFound] = nodeList[j];
                            numFound++;
                            //GD.Print("numFound" + numFound);
                        }
                    }
                    else
                    {
                        break; //doesnt allow more than 3 neighbours
                    }

                }
            }
        }

        return adjacencyList;
    }

    private void PrintAdjList(LinkedList<Tuple<Vector2, int>>[] adjacencyList) //debug purposes
    {
        int i = 0;

        foreach (LinkedList<Tuple<Vector2, int>> list in adjacencyList)
        {
            Console.Write("adjacencyList[" + i + "] -> ");

            foreach (Tuple<Vector2, int> edge in list)
            {
                Console.Write(edge.Item1 + "(" + edge.Item2 + "), ");
            }

            ++i;
            Console.WriteLine();
        }
    }


    //--------------------------------------Other functions --------------------------------------------------------------------

    private Vector2 SetRandomSpawnOnPath() //used to spawn the powerups
    {
        Random rnd = new Random();

        int x = rnd.Next(1, width);
        int y = mazeOriginY + rnd.Next(1, height - 2);

        while (GetCell(x, y) == WALL)
        {
            x = rnd.Next(1, width);
            y = mazeOriginY + rnd.Next(1, height - 2);
        }

        Vector2 spawnLoc = new Vector2(x, y);
        //GD.Print("spawn" + spawnLoc); //debug

        spawnLoc = new Vector2(MapToWorld(spawnLoc) + halfCellSize);

        //GD.Print("MTWspawnLoc: " + spawnLoc); //debug
        return spawnLoc;
    }

    public override void _ExitTree() //built in godot method called when the scene is removed from scenetree, makes sure to delete everything
    {
        powerupFactory.QueueFree(); //powerupfactory is never added to scene tree so must be explcicity freed
        GetParent().QueueFree();
        //GD.Print("mazegen parent queuefree"); //debug
    }

}
