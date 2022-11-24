using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class PacmanScript : CharacterScript
{
    private RayCastScript raycasts;
    private Vector2 nextDir = Vector2.Zero;
    private Vector2 moveDir = Vector2.Zero;
    private GameScript game;
    [Export] public int lives = 3;


    // Called when the node enters the scene tree for the first time.
    public PacmanScript()
    {
        //speed = 100; //originally 100 speed
        //speed = 100 * Globals.gameSpeed; //need to figure out a way to have this inside of the character class/globals or something
    }

    public void GetInput()
    {
        if (Input.IsActionJustPressed("move_up"))
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

        if (raycasts.RaysAreColliding(nextDir) == false)
        {
            moveDir = nextDir;
        }
        //CheckCollision(); //merge checkCollision code with GetInput
        //moveVelocity = moveDir * speed;



    }


    private Vector2 Move(Vector2 moveDir, float speed) //change moveDir and speed
    {
        Vector2 moveVelocity = moveDir * speed;

        Vector2 masVector = MoveAndSlide(moveVelocity, Vector2.Up);

        PlayAndPauseAnim(masVector);

        return masVector;
    }

    bool invincible = false;
    public void _OnPacmanAreaEntered(Area area) //do more stuff with this
    {
        GD.Print(area.Name);
        if (area.Name == "GhostArea" && !invincible)
        {
            lives--;
            CallDeferred("EnableInvincibility", 3);
        }


    }

    private void EnableInvincibility(float time)
    {
        invincible = true;
        GetNode<Timer>("PacmanArea/InvincibleTimer").Start(time);
    }

    public void _OnInvincibleTimerTimeout()
    {
        invincible = false; //disable invincibility
    }


    Vector2 oldPos = new Vector2(Globals.INFINITY, Globals.INFINITY);
    public void UpdateTravelDistance()
    {
        if ((int)((Position / 32).y) < (int)((oldPos / 32).y))
        {
            oldPos = Position;
            game.travelDist++;
        }
    }



    public override void _Ready()
    {
        GD.Print("pacman ready");
        mazeTm = GetNode<MazeGenerator>("/root/Game/MazeContainer/Maze/MazeTilemap");
        raycasts = GetNode<RayCastScript>("RayCasts"); //maybe have a pacmanInit method with all this crap in
        game = GetNode<GameScript>("/root/Game");



        //put all the labels with initial values in a function like this and call the function in ready

        Position = new Vector2(1, mazeTm.mazeOriginY + mazeTm.height - 3) * 32 + new Vector2(16, 16);

        GD.Print("pman ps", Position);
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _PhysicsProcess(float delta)
    {
        GetInput();
        Vector2 masVector = Move(moveDir, speed);
        MoveAnimManager(masVector);

        speed = baseSpeed * Globals.gameSpeed;

        UpdateTravelDistance();
    }
}
