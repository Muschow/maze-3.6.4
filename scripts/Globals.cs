using Godot;
using System;

public class Globals : Node
{
    public const int NODE = 0;
    public const int WALL = 1;
    public const int PATH = 0;
    public const int INFINITY = 9999;

    public const int CELLSIZE = 32;
    public static Vector2 halfCellSize = new Vector2(CELLSIZE / 2, CELLSIZE / 2);

    public static float gameSpeed = 0.72f;


    //---------------global signals -------------
    [Signal] public delegate void PowerPelletActivated();


}
