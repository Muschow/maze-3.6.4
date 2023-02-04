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
    [Export] public int maxLives = 3;


    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        GD.Print("pacman ready");
        mazeTm = GetNode<MazeGenerator>("/root/Game/MazeContainer/Maze/MazeTilemap");
        raycasts = GetNode<RayCastScript>("RayCasts"); //maybe have a pacmanInit method with all this crap in
        game = GetNode<GameScript>("/root/Game");

        //put all the labels with initial values in a function like this and call the function in ready

        Position = new Vector2(1, mazeTm.mazeOriginY + mazeTm.height - 2) * 32 + new Vector2(16, 16);
        GD.Print("pacman mazeheight,", mazeTm.height);
        GD.Print("pman ps", Position);
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _PhysicsProcess(float delta)
    {
        GetInput();
        Vector2 masVector = Move(moveDir, speed);
        MoveAnimManager(masVector);

        speed = baseSpeed * GameScript.gameSpeed;

        UpdateTravelDistance();

        if (Input.IsActionJustPressed("ui_accept"))
        {
            GD.Print("pacman speed ", speed);
        }

    }

    private void GetInput()
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

    private bool invincible = false;
    public void _OnPacmanAreaEntered(Area area) //do more stuff with this
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
            GD.Print("pacman hit killwall, game over");
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


    private Vector2 oldPos = new Vector2(GameScript.INFINITY, GameScript.INFINITY);
    private void UpdateTravelDistance()
    {
        if ((int)((Position / 32).y) < (int)((oldPos / 32).y))
        {
            oldPos = Position;
            game.travelDist++;
        }
    }
}
