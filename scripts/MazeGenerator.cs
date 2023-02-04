using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
public class MazeGenerator : TileMap
{
    //---------------------------------------------statics and constants-----------------------------
    public const int CELLSIZE = 32;
    public const int NODE = 0;
    public const int WALL = 1;
    public const int PATH = 0;
    public static Vector2 halfCellSize = new Vector2(CELLSIZE / 2, CELLSIZE / 2);

    //-------------------------------------------------------------------------------------------------
    [Export] public int width = 31; //originally 31
    [Export] public int height = 38; //was originally 19, then 38, now random
    public int mazeOriginY = 0; //maybe make this or mazeStartLoc a global variable/static or something
    private int backtrackCount = 0; //used to figure out when to join dead ends
    private bool generationComplete = false;

    Vector2[] directions = new Vector2[4] { Vector2.Up, Vector2.Right, Vector2.Down, Vector2.Left };

    List<Vector2> visited = new List<Vector2>();
    List<Vector2> wallEdgeList = new List<Vector2>();
    Stack<Vector2> rdfStack = new Stack<Vector2>();

    //-----------------------------------------------------Adjacency Matrix/List properties---------------------------------------------------------------
    public List<Vector2> nodeList = new List<Vector2>(); //for nodes,maybe get rid of this to be honest
    public LinkedList<Tuple<Vector2, int>>[] adjacencyList;

    //-------------------------------------------------------------GetNodes------------------------------------------------------------------------------
    private Node2D powerupContainer;
    private Node2D powerupFactory;
    private TileMap nodeTilemap;
    private GameScript gameScr;

    //-----------------------------------------------------------PackedScenes---------------------------------------------------------------------------
    PackedScene pelletScene = GD.Load<PackedScene>("res://scenes/Pellet.tscn");
    PackedScene powerupFactoryScene = GD.Load<PackedScene>("res://scenes/powerup scenes/PowerupFactory.tscn");

    //-------------------------------------------------------------Ready and Process--------------------------------------------------------------------
    public override void _EnterTree()
    {
        CorrectMazeSize(); //makes width and height odd when maze enters scene tree (before ready is called)
    }
    //Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        GD.Print("mazegen ready");

        GD.Randomize();

        gameScr = GetNode<GameScript>("/root/Game");
        nodeTilemap = GetParent().GetNode<TileMap>("NodeTilemap");
        powerupContainer = GetParent().GetNode<Node2D>("PowerupContainer");
        powerupFactory = (Node2D)powerupFactoryScene.Instance();


        mazeOriginY = gameScr.mazeStartLoc; //just means the maze origin stays throughout //maybe this would work if it was static and we just deleted mazestartloc

        IterativeDFSInit();
        //UpdateDirtyQuadrants(); //maybe get rid of this tbh, not sure if its doing anything. Supposed to force and update to the tilemap if tiles arent updating

        GenerateAdjList();

        AddRandomPowerups();

        GD.Print("nodeList Count: " + nodeList.Count);
    }

    //------------------------------------------------------Instancing Scenes Functions----------------------------------------------------------------
    private void AddPellet(Vector2 spawnPosition)
    {
        Sprite pelletInstance = (Sprite)pelletScene.Instance();
        GetParent().GetNode<Node2D>("PelletContainer").AddChild(pelletInstance);
        pelletInstance.Position = MapToWorld(spawnPosition) + new Vector2(16, 16);
    }

    private void AddRandomPowerups()
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
    private void CorrectMazeSize()
    {
        if (width % 2 != 1)
        {
            width -= 1;
        }
        if (height % 2 != 1)
        {
            height -= 1;
        }
        GD.Print("width " + width);
        GD.Print("height " + height);
    }

    private void CreateStartingGrid()
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

    private void FixDeadEnds(Vector2 currentV)
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

    private void PrepMazeForJoin(int numHoles) //dependancy on gameScr.Get(mazesOnTheScreen)
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
                //GD.Print("set " + new Vector2(removeCell + south) + " path");
                //GD.Print("set cell+south path");
            }

            //on the top layer, if there isnt a node where there should be one due to removing the top wall, place one
            if (GetCellv(topWallCell + (Vector2.Down * 2)) == PATH && nodeTilemap.GetCellv(topWallCell + Vector2.Down) != NODE)
            {
                AddNode(topWallCell + Vector2.Down);
                //GD.Print("addNode " + new Vector2(topWallCell + south));
            }
        }

        GD.Print(mazeOriginY); //debug



        if (gameScr.mazesOnTheScreen > 0) //If its not the first maze, Add paths to the floor so that you can join to the maze below
        {
            while (numUsedCells < numHoles) //Maybe change to Math.Round(width/4) <-- [must be >3]
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
                    //This also means pacman could exploit the game by just staying in between mazes, however, the ghost maze wall will stop that, forcing pacman to move up.
                }

            }
        }

    }

    private void AddNode(Vector2 nodeLocation)
    {
        if (nodeTilemap.GetCellv(nodeLocation) != NODE) //makes sure theres no duplicates... in a perfect world i would not need this
        {
            //SetCellv(nodeLocation, -1); //deletes tile so will remove wall node that collides (probably dont actually need this but just in case lol)
            nodeTilemap.SetCellv(nodeLocation, NODE); //turns it into an actual path node tile

            nodeList.Add(nodeLocation);

        }
        else
        {
            GD.Print("found bad node");
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
        rdfStack.Push(startVector); //and push it to the stack,

        IterativeDFSStep();
    }

    private void IterativeDFSStep()
    {
        Vector2 prev = new Vector2(0, 0);
        while (!generationComplete)
        {
            Vector2 curr = rdfStack.Pop(); //Pop a cell from the stack and make it a current cell.
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

                rdfStack.Push(curr); //Push the current cell to the stack,
                SetCellv(curr + (next / 2), PATH); // Remove the wall between the current cell and the chosen cell,
                AddPellet(curr + (next / 2));
                visited.Add(curr + next); //Mark the chosen cell as visited,
                rdfStack.Push(curr + next); //and push it to the stack.  

                backtrackCount = 0;
            }
            else
            {
                backtrackCount++;
                if (backtrackCount == 1)
                {
                    FixDeadEnds(curr);
                }
            }

            if (rdfStack.Count <= 0)
            { //While stack is not empty, (if stack is empty)
                FixDeadEnds(curr);
                PrepMazeForJoin(7); //dependancy on gameScr.Get(mazesOnTheScreen)

                generationComplete = true;

                GD.Print("Maze Generation Complete!"); //debug
                return;
            }
        }
    }

    //------------------------------------------------------Adjacency Matrix/List stuff-------------------------------------------------------------------------

    private int ConvertVectorToInt(Vector2 temp)
    {

        if (temp.x == 0)
        {
            return (int)Math.Abs(temp.y);
        }
        else
        {
            return (int)Math.Abs(temp.x);
        }
    }

    private void PrintNodeList()
    {
        GD.Print("Printing NodeList: ");
        foreach (var thing in nodeList)
        {
            GD.Print(thing);
        }
    }

    private void GenerateNodeList() //currently not using this, if i find out the AddNode stuff doesnt work then use this instead
    {

        for (int i = 1; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                if (nodeTilemap.GetCell(j, i) == NODE)
                {
                    nodeList.Add(new Vector2(j, i));
                }
            }
        }
        PrintNodeList();
    }

    private bool IfWallOrNodeBetween(Vector2 vec1, Vector2 vec2)
    {
        //vec1 should be the smaller vector
        //GD.Print("IfOnWall: " + " Vec1: " + vec1 + ", Vec2: " + vec2);
        //TileMap mazeTilemap = GetNode<TileMap>("MazeTilemap");

        if (vec1.x == vec2.x)
        {
            for (int y = (int)vec1.y; (int)y < vec2.y; y++)
            {
                if ((GetCell((int)vec1.x, y) == WALL) || (nodeTilemap.GetCell((int)vec1.x, y) == NODE && y != vec1.y && y != vec2.y))
                {
                    //GD.Print("reached get cell x: " + vec1.x + ",y: " + y);
                    return true;
                }
            }
            return false;
        }
        else if (vec1.y == vec2.y)
        {
            for (int x = (int)vec1.x; (int)x < vec2.x; x++)
            {
                if (GetCell(x, (int)vec1.y) == WALL || (nodeTilemap.GetCell(x, (int)vec1.y) == NODE && x != vec1.x && x != vec2.x))
                {
                    return true;
                }
            }
            return false;
        }
        else
        {
            return true;
        }

    }

    private LinkedList<Tuple<Vector2, int>>[] GenerateAdjList()
    {
        int numFound = 0;

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
                if (v1.x == v2.x || v1.y == v2.y)
                {
                    Vector2 vec1;
                    Vector2 vec2;
                    //swaps so v1 is smaller (vec1) and v2 is bigger (vec2)
                    if (v1 <= v2)
                    {
                        vec1 = v1;
                        vec2 = v2;
                    }
                    else
                    {
                        vec1 = v2;
                        vec2 = v1;
                    }

                    int neighbourVal = ConvertVectorToInt(vec2 - vec1);
                    //if on wall, no edge, else put weight
                    if (numFound <= 3)
                    {
                        if ((!IfWallOrNodeBetween(vec1, vec2)) && (neighbourVal != 0))
                        {
                            adjacencyList[i].AddLast(new Tuple<Vector2, int>(nodeList[j], neighbourVal));
                            //adjList[i, numFound] = nodeList[j];
                            numFound++;
                            //GD.Print("numFound" + numFound);
                        }
                    }
                    else
                    {
                        break; //breaks out of for loop if >=5 i hope
                    }

                }
            }
        }

        return adjacencyList;
    }

    private void PrintAdjList(LinkedList<Tuple<Vector2, int>>[] adjacencyList)
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

    private Vector2 SetRandomSpawnOnPath() //probably place this somewhere else or make global idk
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
        GD.Print("spawn" + spawnLoc); //debug

        spawnLoc = new Vector2(MapToWorld(spawnLoc) + halfCellSize);

        GD.Print("MTWspawnLoc: " + spawnLoc); //debug
        return spawnLoc;
    }

    //--------------------------------------------End of Other Functions--------------------------------------------------------------

    public override void _ExitTree()
    {
        powerupFactory.QueueFree(); //powerupfactory is never added to scene tree so must be explcicity freed
        GetParent().QueueFree();
        GD.Print("mazegen parent queuefree");
    }

}
