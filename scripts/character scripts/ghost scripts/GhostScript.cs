using Godot;
using System;
using System.Collections.Generic;

public class GhostScript : CharacterScript
{
    public GhostScript()
    {
        //speed = 100 * Globals.gameSpeed;
        speed = baseSpeed * (Globals.gameSpeed + 0.28f);
    }
    private Movement moveScr = new Movement();
    private List<Vector2> paths;
    protected PacmanScript pacman;
    private TileMap nodeTilemap;
    //private int Gspeed = 100; //maybe just have a constructor with speed = 100 instead of this Gspeed crap
    private int pathCounter = 0;
    private bool recalculate = false; //used for ghostchase and timer timeout
    Vector2 movementV;
    protected Timer chaseTimer;
    protected Timer patrolTimer;
    protected Timer vulnerableTimer;
    protected Timer resetChasePathTimer;

    protected Vector2 target;
    protected Vector2 source;
    protected AnimatedSprite ghostBody;
    protected Color ghostColour = Colors.White;
    protected enum states
    {
        patrol,
        chase,
        vulnerable
    }
    protected states ghostState = states.patrol; //initialise ghost state to patrol. Timer randomly switches states from patrol to chase and vice versa

    protected enum algo
    {
        dijkstras,
        astar,
        bfs,
    }
    protected algo searchingAlgo = algo.dijkstras;

    protected override void MoveAnimManager(Vector2 masVector)
    {
        AnimatedSprite ghostEyes = GetNode<AnimatedSprite>("GhostEyes"); //not sure whether to put it in here for readabillity or in each ready so theres less calls

        masVector = masVector.Normalized().Round();


        //GD.Print(masVector);
        if (masVector == Vector2.Up)
        {
            ghostEyes.Play("up");
        }
        else if (masVector == Vector2.Down)
        {
            ghostEyes.Play("down");
        }
        else if (masVector == Vector2.Right)
        {
            ghostEyes.Play("right");
        }
        else if (masVector == Vector2.Left)
        {
            ghostEyes.Play("left");
        }
    }
    //As GhostScript is a base class, it will not be in the scene tree.
    // Called when the node enters the scene tree for the first time.

    public override void _Ready()
    {
        GD.Print("ghostscript ready");

        mazeTm = GetParent().GetParent().GetNode<MazeGenerator>("MazeTilemap");
        nodeTilemap = GetParent().GetParent().GetNode<TileMap>("NodeTilemap");
        pacman = GetNode<PacmanScript>("/root/Game/Pacman");

        resetChasePathTimer = GetNode<Timer>("ResetChasePath");
        chaseTimer = GetNode<Timer>("ChaseTimer");
        patrolTimer = GetNode<Timer>("PatrolTimer");
        vulnerableTimer = GetNode<Timer>("VulnerableTimer");
        ghostBody = GetNode<AnimatedSprite>("AnimatedSprite");

        //turns out i CAN actually just use a list and adjacency list... ffs
        moveScr.adjList = mazeTm.adjacencyList;
        moveScr.nodeList = mazeTm.nodeList;

        Position = new Vector2(1, mazeTm.mazeOriginY + 1) * 32 + new Vector2(16, 16); //spawn ghost on top left of current maze
        ghostBody.Modulate = ghostColour;

        FindNewPath(source, target);
        EnterState(ghostState); //initialise first ghostState (patrol);
    }

    private states EnterState(states ghostState)
    {
        if (ghostState == states.patrol)
        {
            patrolTimer.Start((float)GD.RandRange(7, 20));
        }
        else if (ghostState == states.chase)
        {
            chaseTimer.Start((float)GD.RandRange(10, 30));
        }
        else if (ghostState == states.vulnerable)
        {
            vulnerableTimer.Start(15);
        }

        return ghostState;
    }

    private void _OnResetChasePathTimeout() //recalculates pathfinding when timer timeouts
    {
        recalculate = true; //every x seconds, set recalculate to true
    }

    private void _OnChaseTimerTimeout()
    {
        ghostState = EnterState(states.patrol); //has to be in here as this is called once and not every frame
    }

    private void _OnPatrolTimerTimeout()
    {
        ghostState = EnterState(states.chase);

    }

    private void _OnVulnerableTimerTimeout()
    {
        patrolTimer.Paused = false;
        ghostState = EnterState(states.patrol);

    }

    protected bool IsOnNode(Vector2 pos) //make sure to pass in a worldtomap vector
    {

        if (nodeTilemap.GetCellv(pos) == Globals.NODE)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    protected Vector2 FindClosestNodeTo(Vector2 targetVector) //finds closest node in nodelist to targetVector
    {
        Vector2 shortestNode = targetVector;

        if (!IsOnNode(targetVector))
        {
            //GD.Print("THIS IS GETTING CALLED!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
            //GD.Print("targetpos ", targetVector);
            //GD.Print("mazeorigin+height-1 ", mazeOrigin + mazeheight - 1);

            //the node must have the same x or y as targetPos
            int shortestInt = Globals.INFINITY;
            shortestNode = Vector2.Inf;

            foreach (Vector2 node in moveScr.nodeList)
            {
                if ((node.y == targetVector.y || node.x == targetVector.x) && (node != targetVector))
                {
                    int currShortestInt = Math.Abs(moveScr.ConvertVecToInt(targetVector - node));
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
    // private List<Vector2> GetAvailableDir()
    // {
    //     Vector2[] directions = new Vector2[4] { Vector2.Up, Vector2.Right, Vector2.Down, Vector2.Left };
    //     List<Vector2> availableDir = new List<Vector2>();

    //     TileMap mazeTm = this.GetParent().GetNode<TileMap>("MazeTilemap");

    //     //checks for available directions on ghost curr position
    //     foreach (Vector2 dir in directions)
    //     {
    //         if (mazeTm.GetCellv(source + dir) != Globals.WALL)
    //         {
    //             availableDir.Add(dir);
    //         }
    //     }

    //     return availableDir;
    // }
    private void GeneratePath(Vector2 sourcePos, Vector2 targetPos)
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


    private void FindNewPath(Vector2 sourcePos, Vector2 targetPos)
    {
        pathCounter = 0;

        //have targetPos = function and paths = moveScr.whatever in a new virtual function that can be overrided by the 

        GeneratePath(sourcePos, targetPos);
        //pathfind to the new targetPos

    }

    private void MoveToAndValidatePos(float delta)
    {
        if (Position.IsEqualApprox(mazeTm.MapToWorld(paths[pathCounter]) + new Vector2(16, 16))) //must use IsEqualApprox with vectors due to floating point precision errors instead of ==
        {
            pathCounter++; //if ghost position == node position then increment
        }
        else
        {
            movementV = Position.MoveToward(mazeTm.MapToWorld(paths[pathCounter]) + new Vector2(16, 16), delta * speed); //if not, move toward node position
            Position = movementV;
            MoveAnimManager(paths[pathCounter] - mazeTm.WorldToMap(Position));
            // GD.Print("Position ", Position);
        }
    }

    private void GhostChase(float delta)
    {


        if (mazeTm.WorldToMap(pacman.Position).y < (mazeTm.mazeOriginY + mazeTm.height - 1))
        {
            if (IsOnNode(source) && recalculate) //every x seconds, if pacman and ghost is on a node, it recalulates shortest path.
            {
                recalculate = false;
                FindNewPath(source, target);
            }

            if (pathCounter < paths.Count)
            {
                MoveToAndValidatePos(delta);
                //GD.Print(pathCounter);
            }
            else if (pathCounter >= paths.Count) //if its reached the end of its path, calculate new path
            {
                FindNewPath(source, target);
            }
        }

    }


    private void GhostPatrol(float delta)
    {

        //GD.Print(patrolTimer.WaitTime);

        if (pathCounter < paths.Count)
        {
            MoveToAndValidatePos(delta);
            //GD.Print(pathCounter);
        }
        else if (pathCounter >= paths.Count) //if its reached the end of its path, calculate new path
        {

            int randNodeIndex = (int)GD.RandRange(0, moveScr.nodeList.Count);
            FindNewPath(source, moveScr.nodeList[randNodeIndex]); //target is instead a random node
        }
    }

    private void GhostVulnerable(float delta)
    {

        GhostPatrol(delta);
        ghostBody.Play("vulnerable");
        //if ghost collides with pacman, kill ghost, give pacman like 100 points and increase multiplier

        //on leaving scatter, play the normal one again. On ready, play the normal one to intitialise.
    }

    private void ProcessStates(float delta)
    {
        if (ghostState == states.patrol)
        {
            //GD.Print("PATROL STATE-----------------------------------------");
            GhostPatrol(delta);
        }
        else if (ghostState == states.chase)
        {
            //GD.Print("CHASE STATE-----------------------------------------");
            GhostChase(delta);
        }
        else if (ghostState == states.vulnerable)
        {
            //GD.Print("VULNERABLE STATE-----------------------------------------");
            chaseTimer.Stop();          //stop chase and patrol timer just in case
            patrolTimer.Stop();
            patrolTimer.Paused = true; //pauses patrol timer as scatter uses patrol mode for pathfinding

            GhostVulnerable(delta);
        }
    }

    public virtual void UpdateSourceTarget()
    {
        source = mazeTm.WorldToMap(Position);
        target = FindClosestNodeTo(mazeTm.WorldToMap(pacman.Position));
    }

    //----------------------------These 2 signals are for if ghosts overlap each other. Gives them random speed increase so they dont overlap---------------------------------------
    private bool hasIntersectedBefore = false;
    protected float oldSpeed;
    public void _OnGhostAreaEntered(Area2D area) //if 2 ghosts are ontop of each other, randomly increase speed so they move away
    {
        if (hasIntersectedBefore == false)
        {
            oldSpeed = speed;
            speed = speed + (int)GD.RandRange(-20, 20);
        }
        hasIntersectedBefore = true;
    }

    public void _OnGhostAreaExited(Area2D area) //when 2 ghosts are not ontop of each other reset speed back to normal.
    {
        speed = oldSpeed;
        hasIntersectedBefore = false;
    }
    //-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        UpdateSourceTarget();

        PlayAndPauseAnim(movementV);
        ProcessStates(delta);
        //GD.Print("ghostspeed", speed);

    }
}

