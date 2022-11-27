using Godot;
using System;

public class GameScript : Node2D
{

    public const int INFINITY = 9999;
    public static float gameSpeed = 0.72f;


    //---------------'global' signals -------------
    [Signal] public delegate void PowerPelletActivated();
    [Signal] public delegate void DecoyPowerupActivated(Vector2 newPosition, int decoyLengthTime);
    [Signal] public delegate void RandomizerPowerupActivated();

    //-----------------------------------------------------------------------------------------------------------
    public int mazeStartLoc = 0;
    public int mazesOnTheScreen = 0; //this is literally used for 1 thing to make the walls black on the bottom of the first maze. Probably a better way to do this...

    public int score = 0;
    public int travelDist = 0;
    public float scoreMultiplier = 1.0f;

    private PacmanScript pacman;
    private MazeGenerator mazeTm;
    private Node2D mazeContainer;
    private HBoxContainer labelContainer;

    //dfnksjdjfsk


    PackedScene mazeScene = GD.Load<PackedScene>("res://scenes/Maze.tscn");

    // Called when the node enters the scene tree for the first time.
    private void InstanceAndRemoveMazes()
    {
        //store all the getnodes, MazeGenerator mazeG = new MazeGenerator() stuff in here instead of in the ready function for better encapsulation

        mazeStartLoc -= (mazeTm.height - 1); //this was -=

        Node2D mazeInstance = (Node2D)mazeScene.Instance();
        mazeContainer.AddChild(mazeInstance, true);

        mazesOnTheScreen++;
        GD.Print("instanced maze!");

        if (mazeContainer.GetChildCount() > 3) //if more than 3 mazes, remove maze
        {
            mazeContainer.GetChild(0).QueueFree(); //get and delete the first child of the MazeContainer node. The first child will be the oldest Maze node instance
            mazesOnTheScreen--;
            GD.Print("removed maze!");

        }
        //PrintTreePretty(); //debug, remove later

        //REMEMBER! This prnt tree is literally lying, if you change the pacman camera to 2x9, you can clearly see only 3 mazes are spawned in at a time!
        //I love how godot mono is trash and buggy and a broken mess :)
        //GD.Print("mazesOnTheScreen: " + mazesOnTheScreen); //yeah so this is always on 3 pretty much so idk whats going on lol

    }


    private void UpdateLabels()
    {
        labelContainer.GetNode<Label>("LifeCounter").Text = "Lives:" + pacman.lives + " ";
        labelContainer.GetNode<Label>("DistCounter").Text = "Dist:" + travelDist + " ";
        labelContainer.GetNode<Label>("ScoreCounter").Text = "Score:" + score + " ";
        labelContainer.GetNode<Label>("MultiplierCounter").Text = "Mult:" + scoreMultiplier + "x ";
    }
    public override void _Ready()
    {
        GD.Print("Game ready");
        //put all the maze crap in a mazeInit function
        mazeTm = GetNode<MazeGenerator>("MazeContainer/Maze/MazeTilemap");
        pacman = GetNode<PacmanScript>("Pacman");
        mazeContainer = GetNode<Node2D>("MazeContainer");
        labelContainer = GetNode<HBoxContainer>("CanvasLayer/HBoxContainer");

        mazesOnTheScreen++; //remove this if you instance the first maze

        //PrintTreePretty(); //debug, also you cant really trust this for instances anyway so...




        //maybe have a function with alll the intial label states and put function in ready
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        if (Math.Floor(pacman.Position.y / 32) == mazeStartLoc + mazeTm.height - 2) //if pacman goes on the join between 2 mazes, instance new and remove old maze
        {
            GameScript.gameSpeed += 0.28f; //max speed that i want is 400, 512 dist = 14 and a bit mazes, 400/14 mazes = 28.57 increase per maze
            InstanceAndRemoveMazes();
        }
        UpdateLabels();

        //GD.Print("stray nodes");
        //PrintStrayNodes(); //put a post on godot forums on how to fix this and the objects leaks errors
    }


}
