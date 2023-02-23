using Godot;

public class PacmanScript : CharacterScript
{
    private RayCastScript raycasts;
    private AnimatedSprite pacmanBody;
    private Vector2 nextDir = Vector2.Zero;
    private Vector2 moveDir = Vector2.Zero;
    private GameScript game;
    [Export] public int lives = 3;
    [Export] public int maxLives = 3;


    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        GD.Print("pacman ready, basespeed: " + baseSpeed);

        GetNodes();

        //spawns pacman on bottom left of maze. * celllsize to convert from map to world, + halfCellSize so spawns in centre of tile
        Position = new Vector2(1, mazeTm.mazeOriginY + mazeTm.height - 2) * MazeGenerator.CELLSIZE + MazeGenerator.halfCellSize;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _PhysicsProcess(float delta)
    {
        GetInput(); //gets wasd/arrow keys inputs

        Move(moveDir, speed); //handles movement, animations etc

        speed = baseSpeed * GameScript.gameSpeed;

        UpdateTravelDistance(); //moving vertically increases dist for win condition
    }

    private void GetNodes()
    {
        mazeTm = GetNode<MazeGenerator>("/root/Game/MazeContainer/Maze/MazeTilemap");
        raycasts = GetNode<RayCastScript>("RayCasts");
        game = GetNode<GameScript>("/root/Game");
        pacmanBody = GetNode<AnimatedSprite>("AnimatedSprite");
    }

    private void GetInput() //translate WASD/arrow key presses to movement vector
    {
        if (Input.IsActionJustPressed("move_up")) //in project settings i have set WASD and arrow keys to "move_up" etc.
        {
            nextDir = Vector2.Up;
        }
        else if (Input.IsActionJustPressed("move_down"))
        {
            nextDir = Vector2.Down;
        }
        else if (Input.IsActionJustPressed("move_right"))
        {
            nextDir = Vector2.Right;
        }
        else if (Input.IsActionJustPressed("move_left"))
        {
            nextDir = Vector2.Left;
        }

        //this is so that you can queue a turn before you actually reach it, and then it executes the turn when you reach it
        //via rays detecting free space to turn into
        if (raycasts.RaysAreColliding(nextDir) == false)
        {
            moveDir = nextDir;
        }
    }

    private void Move(Vector2 moveDir, float speed) //change moveDir and speed
    {
        Vector2 moveVelocity = moveDir * speed;

        Vector2 masVector = MoveAndSlide(moveVelocity, Vector2.Up);

        PlayAndPauseAnim(masVector, pacmanBody);
        MoveAnimManager(masVector, pacmanBody);
    }

    private bool invincible = false;

    //signal the area2d node emits whenever something enters its collision shape
    //so ghosts dont kill you when you get power pellet and so kill wall kills you
    public void _OnPacmanAreaEntered(Area area)
    {
        //GD.Print(area.Name);
        if (area.Name == "GhostArea" && !invincible)
        {
            lives--;
            game.scoreMultiplier = 1.0f; //if you lose a life reset the score multiplier
            CallDeferred("EnableInvincibility", 3);
        }

        if (area.Name == "KillArea")
        {
            //GD.Print("pacman hit killwall, game over"); //debug
            game.GameOver();
        }
    }

    public void EnableInvincibility(float time)
    {
        invincible = true;
        GetNode<Timer>("PacmanArea/InvincibleTimer").Start(time);
    }

    private void _OnInvincibleTimerTimeout()
    {
        invincible = false; //disable invincibility
    }


    private Vector2 oldPos = new Vector2(Globals.INFINITY, Globals.INFINITY);

    //if new pos is higher than old pos increase distance... 
    //except in godot +y goes downwards so thats why its less than
    private void UpdateTravelDistance()
    {
        if ((int)((Position / MazeGenerator.CELLSIZE).y) < (int)((oldPos / MazeGenerator.CELLSIZE).y))
        {
            oldPos = Position;
            game.travelDist++;
        }
    }
}
