using Godot;
using System;
using System.Collections.Generic;

public class GhostScript : CharacterScript
{
    //Timer times
    private const int VULNERABLE_TIME = 15;
    private const int PATROLTIMER_MIN = 7;
    private const int PATROLTIMER_MAX = 20;
    private const int CHASETIMER_MIN = 10;
    private const int CHASETIMER_MAX = 30;

    //-----------------------ghost properties----------------------
    private int baseScore = 500;
    private float speedModifier = 1;

    //-----------------------ghost bools----------------------------
    private bool recalculate = false; //used for ghostchase and timer timeout
    private bool defaultTarget = true;
    public bool ghostIsVulnerable = false;

    //----------------------------------ghost components------------
    protected AnimatedSprite ghostBody;
    protected AnimatedSprite ghostEyes;
    protected Color ghostColour = Colors.White;

    //----------------------------------ghost states----------------
    protected enum states
    {
        patrol,
        chase
    }
    protected enum algo
    {
        dijkstras,
        astar,
        bfs,
    }
    protected algo searchingAlgo = algo.dijkstras;
    protected states ghostState = states.patrol; //initialise ghost state to patrol. Timer randomly switches states from patrol to chase and vice versa

    //-------------------------movement---------------------------------
    private Movement moveScr = new Movement();
    private List<Vector2> paths;
    protected Vector2 target;
    protected Vector2 source;
    private int pathCounter = 0;

    //---------------------------------Timers----------------------------
    protected Timer chaseTimer;
    protected Timer patrolTimer;
    protected Timer vulnerableTimer;

    //------------------------other scenes--------------------------------
    protected PacmanScript pacman;
    private TileMap nodeTilemap;
    private GameScript game;

    //----------------------------------------------------Ready and Process----------------------------------------------------------------------------

    //As GhostScript is a base class, it will not be in the scene tree.
    public override void _Ready()
    {
        //GD.Print("ghostscript ready"); //debug
        //GD.Print("ghostcolour: " + ghostColour + " new searching algo: " + searchingAlgo); //testing, godot stores a colour as 4 values from 0-1. Theres no function to turn these back into the colour constants for some reason
        //GD.Print("ghostcolour: " + ghostColour + " baseSpeed: " + baseSpeed); //testing
        GetNodes();

        moveScr.adjList = mazeTm.adjacencyList; //make sure movement has the adjacency list and nodelist of the maze the ghost is on
        moveScr.nodeList = mazeTm.nodeList;

        //spawn ghost on top left tile of current maze * cellsize so map to world and + halfcellsize so they spawn in the centre
        Position = new Vector2(1, mazeTm.mazeOriginY + 1) * MazeGenerator.CELLSIZE + MazeGenerator.halfCellSize;
        ghostBody.Modulate = ghostColour; //initialise ghost colour

        FindNewPath(source, target);
        EnterState(ghostState); //initialise first ghostState (patrol);

        ConnectGhostSignals(); //connect powerup signals

    }
    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        source = mazeTm.WorldToMap(Position); //always updates source pos (current pos of ghost)

        if (defaultTarget)
        {
            UpdateTarget(); //only update target if defaultTarget true
        }

        speed = baseSpeed * GameScript.gameSpeed * speedModifier; //always update speed

        ProcessStates(delta); //does everything in regards to ghosts states, eg chase, patrol
    }

    //-------------------------------------------------------GetNodes and connecting signals functions--------------------------------------

    private void ConnectGhostSignals() //connect the signals from game script to the methods in this script
    {
        game.Connect("PowerPelletActivated", this, "_OnPowerPelletActivated");
        game.Connect("DecoyPowerupActivated", this, "_OnDecoyPowerupActivated");
        game.Connect("RandomizerPowerupActivated", this, "_OnRandomizerPowerupActivated");
        game.Connect("ChangeSpeedModifier", this, "_OnChangeSpeedModifierActivated");
    }

    private void GetNodes()
    {
        //Other scenes--------
        mazeTm = GetParent().GetParent().GetNode<MazeGenerator>("MazeTilemap");
        nodeTilemap = GetParent().GetParent().GetNode<TileMap>("NodeTilemap");
        pacman = GetNode<PacmanScript>("/root/Game/Pacman");
        game = GetNode<GameScript>("/root/Game");

        //Timers--------------
        chaseTimer = GetNode<Timer>("ChaseTimer");
        patrolTimer = GetNode<Timer>("PatrolTimer");
        vulnerableTimer = GetNode<Timer>("VulnerableTimer");

        //Ghost components----
        ghostBody = GetNode<AnimatedSprite>("AnimatedSprite");
        ghostEyes = GetNode<AnimatedSprite>("GhostEyes");
    }

    //---------------------------------------------------processing/entering state functions---------------------------------------

    private void EnterState(states newGhostState) //whenever a ghost enters a new state, start timers for specific state. Used in Finite State Machine for ghost behaviour
    {
        if (newGhostState == states.patrol)
        {
            patrolTimer.Start((float)GD.RandRange(PATROLTIMER_MIN, PATROLTIMER_MAX));
        }
        else if (newGhostState == states.chase)
        {
            chaseTimer.Start((float)GD.RandRange(CHASETIMER_MIN, CHASETIMER_MAX));
        }
        ghostState = newGhostState;
    }

    private void ProcessStates(float delta)
    {
        if (ghostState == states.patrol)
        {
            //GD.Print("PATROL STATE-----------------------------------------"); //debug
            GhostPatrol(delta);
        }
        else if (ghostState == states.chase)
        {
            //GD.Print("CHASE STATE-----------------------------------------"); //debug
            GhostChase(delta);
        }

    }

    //--------------------------------------Ghost 'states' although vulnerable is seperate------------------------------------------

    private void GhostVulnerable() //vulnerable allows ghost to be killed. Vulnerable is not in the FSM as when vulnerable, it can still be in patrol/chase
    {
        ghostIsVulnerable = true;
        vulnerableTimer.Start(VULNERABLE_TIME); //vulnerable mode lasts 15 seconds

        ghostBody.Modulate = Colors.White; //all ghosts body is white so the vulnerable skin isnt some wierd hue
        ghostEyes.Visible = false; //the eyes are turned off as the vulnerable ghost skin has built in eyes.
        ghostBody.Play("vulnerable"); //applies the vulnerable skin
        //GD.Print("entered vulnerable state"); //debug
    }

    //determines the ghosts behaviour in chase mode.
    //checks if on the same maze as pacman, if not go in patrol. This is so lots of more intensive movement calculations dont have to be done
    //and the ghosts can be well spread out across mazes so every single ghost in the game doesnt chase after you
    //if on the same maze, move towards pacman until path is complete, then recalculate
    private void GhostChase(float delta)
    {
        if (mazeTm.WorldToMap(pacman.Position).y < (mazeTm.mazeOriginY + mazeTm.height - 1))
        {
            if (IsOnNode(source) && recalculate) //every x seconds, if ghost is on a node, it recalulates shortest path.
            {
                recalculate = false;
                FindNewPath(source, target);
            }

            if (pathCounter < paths.Count)
            {
                MoveToAndValidatePos(delta);
                //GD.Print(pathCounter); //debug
            }
            else if (pathCounter >= paths.Count) //if its reached the end of its path, calculate new path
            {
                FindNewPath(source, target);
            }
        }
        else
        {
            EnterState(states.patrol);
        }

    }

    //move to a random position on the maze its on and recalculate once path is complete. done to spread out ghosts and so player isnt always chased.
    private void GhostPatrol(float delta)
    {
        //GD.Print(patrolTimer.WaitTime); //debug

        if (pathCounter < paths.Count)
        {
            MoveToAndValidatePos(delta);
            //GD.Print(pathCounter); //debug
        }
        else if (pathCounter >= paths.Count) //if its reached the end of its path, calculate new path
        {

            int randNodeIndex = (int)GD.RandRange(0, moveScr.nodeList.Count);
            FindNewPath(source, moveScr.nodeList[randNodeIndex]); //target is instead a random node
        }
    }

    //---------------------------------------------------------Timer functions-----------------------------------------------
    private void _OnResetChasePathTimeout() //recalculates pathfinding when timer timeouts
    {
        recalculate = true; //every x seconds, set recalculate to true
    }

    private void _OnChaseTimerTimeout()
    {
        defaultTarget = true; //resets back to default target if its been changed (eg from decoy)
        //GD.Print("chasetimertimeout default target", defaultTarget); //debug
        EnterState(states.patrol); //has to be in here as this is called once and not every frame. When chase is done, switch to patrol
    }

    private void _OnPatrolTimerTimeout()
    {
        defaultTarget = true; //resets back to default target if its been changed (eg from decoy)
        //GD.Print("patroltimertimeout default target", defaultTarget); //debug
        EnterState(states.chase); //when patrol is done switch to chase

    }

    private void _OnVulnerableTimerTimeout() //resets ghost back to normal state when vulnerable (power pellet) ran out
    {
        ghostIsVulnerable = false;
        ghostBody.Modulate = ghostColour;
        ghostBody.Play("walk");
        ghostEyes.Visible = true;
    }

    //------------------------------------------------Movement and position functions--------------------------------------------

    //isonnode used to check if can turn and if valid target vector for the movement script as the adjacency list used for movement only contains nodes
    protected bool IsOnNode(Vector2 pos) //make sure to pass in a worldtomap vector. 
    {
        if (nodeTilemap.GetCellv(pos) == MazeGenerator.NODE)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    //if pacman is between nodes, find closest one in nodelist so ghosts can still pathfind
    //goes through each node in nodelist, calculates distance and keeps the shortest. If new shortest, replaces shortest
    protected Vector2 FindClosestNodeTo(Vector2 targetVector) //finds closest node in nodelist to targetVector.
    {
        Vector2 shortestNode = targetVector;

        if (!IsOnNode(targetVector))
        {
            //the node must have the same x or y as targetPos
            int shortestInt = Globals.INFINITY;
            shortestNode = new Vector2(Globals.INFINITY, Globals.INFINITY);

            foreach (Vector2 node in moveScr.nodeList)
            {
                if ((node.y == targetVector.y || node.x == targetVector.x) && (node != targetVector))
                {
                    int currShortestInt = Math.Abs(Globals.ConvertVectorToInt(targetVector - node));
                    if (currShortestInt < shortestInt)
                    {
                        shortestInt = currShortestInt;
                        shortestNode = node;
                    }

                }
            }
        }

        return shortestNode;
    }

    //chooses algorithm for pathfinding, used for different ghosts as they have different algos
    private void GeneratePathListWithSearchingAlgo(Vector2 sourcePos, Vector2 targetPos)
    {
        if (searchingAlgo == algo.dijkstras)
        {
            paths = moveScr.Dijkstras(sourcePos, targetPos, false);
        }
        else if (searchingAlgo == algo.astar)
        {
            paths = moveScr.Dijkstras(sourcePos, targetPos, true);
        }
        else if (searchingAlgo == algo.bfs)
        {
            paths = moveScr.BFS(sourcePos, targetPos);
        }

    }

    //resets the pathfinding for the ghosts and generates a new path list for them to follow
    private void FindNewPath(Vector2 sourcePos, Vector2 targetPos)
    {
        pathCounter = 0;
        GeneratePathListWithSearchingAlgo(sourcePos, targetPos);
        //pathfind to the new targetPos
    }

    //used to move the ghosts to the correct position. 
    //The map to world returns the coordinate of the corner of the tile, so halfCellSize must be added to get coordinate of centre
    //ghosts follow an array with coordinates of their path. They go to paths[0], then paths[1] etc.
    private void MoveToAndValidatePos(float delta)
    {
        if (Position.IsEqualApprox(mazeTm.MapToWorld(paths[pathCounter]) + MazeGenerator.halfCellSize)) //must use IsEqualApprox with vectors due to floating point precision errors instead of ==
        {
            pathCounter++; //if ghost position == node position then increment
        }
        else //if not, move toward node position
        {
            Position = Position.MoveToward(mazeTm.MapToWorld(paths[pathCounter]) + MazeGenerator.halfCellSize, delta * speed); //delta*speed makes it so speed is consistent regardless of frame rate
            MoveAnimManager(paths[pathCounter] - mazeTm.WorldToMap(Position), ghostEyes);
            // GD.Print("Position ", Position); //debug
        }
    }

    public virtual void UpdateTarget() //default target is pacman
    {

        target = FindClosestNodeTo(mazeTm.WorldToMap(pacman.Position));
    }

    //----------------------------ghost collision functions (either with other ghosts or pacman)---------------------------------------
    private bool hasIntersectedBefore = false;
    protected float oldSpeed;
    private void _OnGhostAreaEntered(Area2D area) //if 2 ghosts are ontop of each other, randomly decrease speed so they move away
    {
        if (hasIntersectedBefore == false)
        {
            oldSpeed = speed;
            speed = speed + (int)GD.RandRange(-20, 0);
        }
        hasIntersectedBefore = true;

        if ((area.Name == "PacmanArea") && (ghostIsVulnerable == true)) //allows ghost to be killed when vulnerable
        {
            game.score += (int)(baseScore * game.scoreMultiplier); //add base*mult to gamescript.score
            game.scoreMultiplier += 0.25f; //increase mult by 0.25
            QueueFree(); //delete ghost
        }
    }

    private void _OnGhostAreaExited(Area2D area) //when 2 ghosts are not ontop of each other reset speed back to normal.
    {
        speed = oldSpeed;
        hasIntersectedBefore = false;
    }
    //------------------------------------------------------------------------Powerup Signals----------------------------------------------------------------------------------------------

    private void _OnPowerPelletActivated()
    {
        GhostVulnerable();
        pacman.EnableInvincibility(vulnerableTimer.TimeLeft); //pacman is invincible for the duration of vulnerable mode so he can collide and kill ghosts without losing health.
    }

    private void _OnDecoyPowerupActivated(Vector2 newPosition, int decoyLengthTime) //overrrides target of ghosts to new target of decoy
    {
        defaultTarget = false;
        target = FindClosestNodeTo(mazeTm.WorldToMap(newPosition)); //change target to position of decoy
        EnterState(states.chase); //chase towards new target

        chaseTimer.Start(decoyLengthTime);
    }

    private void _OnRandomizerPowerupActivated() //randomises the ghosts pathfinding algorithms
    {
        Random rnd = new Random();
        int algoLength = Enum.GetNames(typeof(algo)).Length;
        searchingAlgo = (algo)rnd.Next(0, algoLength);
        GD.Print("ghostcolour: " + ghostColour + " new searching algo :" + searchingAlgo); //godot stores a colour as 4 values from 0-1. Theres no function to turn these back into the colour constants for some reason
    }

    private void _OnChangeSpeedModifierActivated(float newSpeedModifier) //allows me to change speed via a signal
    {
        speedModifier = newSpeedModifier;
    }
}

