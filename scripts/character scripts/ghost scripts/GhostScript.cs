using Godot;
using System;
using System.Collections.Generic;

public class GhostScript : CharacterScript
{

    private Movement moveScr = new Movement();
    private List<Vector2> paths;
    protected PacmanScript pacman;
    private TileMap nodeTilemap;
    private Globals globals;
    private GameScript game;
    private int pathCounter = 0;
    private bool recalculate = false; //used for ghostchase and timer timeout
    private int baseScore = 500;
    Vector2 movementV;
    protected Timer chaseTimer;
    protected Timer patrolTimer;
    protected Timer vulnerableTimer;
    protected Timer resetChasePathTimer;
    private float speedModifier = 1;
    protected Vector2 target;
    private bool defaultTarget = true;
    public bool ghostIsVulnerable = false;
    protected Vector2 source;
    protected AnimatedSprite ghostBody;
    protected AnimatedSprite ghostEyes;
    protected Color ghostColour = Colors.White;
    protected enum states
    {
        patrol,
        chase
    }
    protected states ghostState = states.patrol; //initialise ghost state to patrol. Timer randomly switches states from patrol to chase and vice versa

    protected enum algo
    {
        dijkstras,
        astar,
        bfs,
    }
    protected algo searchingAlgo = algo.dijkstras;


    //As GhostScript is a base class, it will not be in the scene tree.
    // Called when the node enters the scene tree for the first time.

    public override void _Ready()
    {
        GD.Print("ghostscript ready");

        mazeTm = GetParent().GetParent().GetNode<MazeGenerator>("MazeTilemap");
        nodeTilemap = GetParent().GetParent().GetNode<TileMap>("NodeTilemap");
        pacman = GetNode<PacmanScript>("/root/Game/Pacman");
        globals = GetNode<Globals>("/root/Globals");
        game = GetNode<GameScript>("/root/Game");

        resetChasePathTimer = GetNode<Timer>("ResetChasePath");
        chaseTimer = GetNode<Timer>("ChaseTimer");
        patrolTimer = GetNode<Timer>("PatrolTimer");
        vulnerableTimer = GetNode<Timer>("VulnerableTimer");
        ghostBody = GetNode<AnimatedSprite>("AnimatedSprite");
        ghostEyes = GetNode<AnimatedSprite>("GhostEyes");

        moveScr.adjList = mazeTm.adjacencyList;
        moveScr.nodeList = mazeTm.nodeList;

        Position = new Vector2(1, mazeTm.mazeOriginY + 1) * MazeGenerator.CELLSIZE + MazeGenerator.halfCellSize; //spawn ghost on top left of current maze
        ghostBody.Modulate = ghostColour;

        FindNewPath(source, target);
        EnterState(ghostState); //initialise first ghostState (patrol);

        //connect powerup signals, if theres a bunch on these put it in a function and then sort things out later
        ConnectGhostSignals();

    }
    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        source = mazeTm.WorldToMap(Position); //always updates source pos

        if (defaultTarget)
        {
            UpdateTarget(); //only update target is defaultTarget true
        }

        speed = baseSpeed * GameScript.gameSpeed * speedModifier; //always update speed

        PlayAndPauseAnim(movementV);
        ProcessStates(delta);

        if (Input.IsActionJustPressed("ui_accept"))
        {
            GD.Print("ghost speed ", speed);
        }
    }

    private void ConnectGhostSignals()
    {
        game.Connect("PowerPelletActivated", this, "_OnPowerPelletActivated");
        game.Connect("DecoyPowerupActivated", this, "_OnDecoyPowerupActivated");
        game.Connect("RandomizerPowerupActivated", this, "_OnRandomizerPowerupActivated");
        game.Connect("ChangeSpeedModifier", this, "_OnChangeSpeedModifierActivated");
    }
    protected override void MoveAnimManager(Vector2 masVector)
    {
        masVector = masVector.Normalized().Round();

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
    private void EnterState(states newGhostState)
    {
        if (newGhostState == states.patrol)
        {
            patrolTimer.Start((float)GD.RandRange(7, 20));
        }
        else if (newGhostState == states.chase)
        {
            chaseTimer.Start((float)GD.RandRange(10, 30));
        }
        ghostState = newGhostState;


    }

    private void EnterGhostVulnerable()
    {
        ghostIsVulnerable = true;
        vulnerableTimer.Start(15);
        GD.Print("entered vulnerable state");
    }

    private void _OnResetChasePathTimeout() //recalculates pathfinding when timer timeouts
    {
        recalculate = true; //every x seconds, set recalculate to true
    }

    private void _OnChaseTimerTimeout()
    {
        defaultTarget = true; //resets back to default target if its been changed (eg from decoy)
        GD.Print("chaetimertimeout default target", defaultTarget);
        EnterState(states.patrol); //has to be in here as this is called once and not every frame
    }

    private void _OnPatrolTimerTimeout()
    {
        defaultTarget = true; //resets back to default target if its been changed (eg from decoy)
        GD.Print("patroltimertimeout default target", defaultTarget);
        EnterState(states.chase);

    }

    private void _OnVulnerableTimerTimeout()
    {
        ghostIsVulnerable = false;

        patrolTimer.Paused = false;
        ghostBody.Modulate = ghostColour;
        ghostBody.Play("walk");
        ghostEyes.Visible = true;

        EnterState(states.patrol);

    }

    protected bool IsOnNode(Vector2 pos) //make sure to pass in a worldtomap vector
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

    protected Vector2 FindClosestNodeTo(Vector2 targetVector) //finds closest node in nodelist to targetVector
    {

        Vector2 shortestNode = targetVector;

        if (!IsOnNode(targetVector))
        {
            //GD.Print("THIS IS GETTING CALLED!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
            //GD.Print("targetpos ", targetVector);
            //GD.Print("mazeorigin+height-1 ", mazeOrigin + mazeheight - 1);

            //the node must have the same x or y as targetPos
            int shortestInt = GameScript.INFINITY;
            shortestNode = new Vector2(GameScript.INFINITY, GameScript.INFINITY);

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


    private void FindNewPath(Vector2 sourcePos, Vector2 targetPos)
    {
        pathCounter = 0;

        //have targetPos = function and paths = moveScr.whatever in a new virtual function that can be overrided by the 

        GeneratePathListWithSearchingAlgo(sourcePos, targetPos);
        //pathfind to the new targetPos

    }

    private void MoveToAndValidatePos(float delta)
    {
        if (Position.IsEqualApprox(mazeTm.MapToWorld(paths[pathCounter]) + MazeGenerator.halfCellSize)) //must use IsEqualApprox with vectors due to floating point precision errors instead of ==
        {
            pathCounter++; //if ghost position == node position then increment
        }
        else
        {
            movementV = Position.MoveToward(mazeTm.MapToWorld(paths[pathCounter]) + MazeGenerator.halfCellSize, delta * speed); //if not, move toward node position
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
        else
        {
            EnterState(states.patrol);
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
        chaseTimer.Stop();          //stop chase and patrol timer just in case
        patrolTimer.Stop();
        //patrolTimer.Paused = true; //pauses patrol timer as scatter uses patrol mode for pathfinding

        //GD.Print("VULNERABLE STATE");

        ghostBody.Modulate = Colors.White;
        ghostEyes.Visible = false;


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

        if (ghostIsVulnerable)
        {
            GhostVulnerable(delta);
        }

    }

    public virtual void UpdateTarget()
    {

        target = FindClosestNodeTo(mazeTm.WorldToMap(pacman.Position));
    }

    //----------------------------These 2 signals are for if ghosts overlap each other. Gives them random speed increase so they dont overlap---------------------------------------
    private bool hasIntersectedBefore = false;
    protected float oldSpeed;
    private void _OnGhostAreaEntered(Area2D area) //if 2 ghosts are ontop of each other, randomly increase speed so they move away
    {
        if (hasIntersectedBefore == false)
        {
            oldSpeed = speed;
            speed = speed + (int)GD.RandRange(-20, 0);
        }
        hasIntersectedBefore = true;

        if ((area.Name == "PacmanArea") && (ghostIsVulnerable == true))
        {
            game.score += (int)(baseScore * game.scoreMultiplier); //add 100*mult to gamescript.score
            game.scoreMultiplier += 0.25f; //increase mult by 0.25
            QueueFree();
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
        EnterGhostVulnerable();
        pacman.EnableInvincibility(vulnerableTimer.TimeLeft);
    }

    private void _OnDecoyPowerupActivated(Vector2 newPosition, int decoyLengthTime)
    {

        defaultTarget = false;
        target = FindClosestNodeTo(mazeTm.WorldToMap(newPosition)); //change target to position of decoy
        EnterState(states.chase); //chase towards new target

        chaseTimer.Start(decoyLengthTime);
    }

    private void _OnRandomizerPowerupActivated()
    {
        Random rnd = new Random();
        int algoLength = Enum.GetNames(typeof(algo)).Length;
        searchingAlgo = (algo)rnd.Next(0, algoLength);
    }

    private void _OnChangeSpeedModifierActivated(float newSpeedModifier)
    {
        speedModifier = newSpeedModifier;
    }
}

