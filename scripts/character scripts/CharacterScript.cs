using Godot;
using System;
using System.Collections.Generic;

public abstract class CharacterScript : KinematicBody2D
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";

    // Called when the node enters the scene tree for the first time.
    protected float speed;
    protected int baseSpeed = 100;
    protected MazeGenerator mazeTm;

    protected void PlayAndPauseAnim(Vector2 masVector) //requires AnimatedSprite reference
    {
        AnimatedSprite animatedSprite = GetNode<AnimatedSprite>("AnimatedSprite");
        //animatedSprite.SpeedScale = gameSpeed; not sure whether to get rid of this, it looks kind of weird.

        if (masVector == Vector2.Zero)
        {
            animatedSprite.Stop();
        }
        else if (masVector != Vector2.Zero)
        {
            animatedSprite.Play();
        }
    }

    protected virtual void MoveAnimManager(Vector2 masVector) //override this with swapping eye animation for ghosts
    {
        AnimatedSprite animatedSprite = GetNode<AnimatedSprite>("AnimatedSprite"); //not sure whether to put it in here for readabillity or in each ready so theres less calls
        masVector = masVector.Normalized().Round();


        if (masVector == Vector2.Up)
        {
            animatedSprite.RotationDegrees = -90;
        }
        else if (masVector == Vector2.Down)
        {
            animatedSprite.RotationDegrees = 90;
        }
        else if (masVector == Vector2.Right)
        {
            animatedSprite.RotationDegrees = 0; //this takes facing right as the default animation frame
        }
        else if (masVector == Vector2.Left)
        {
            animatedSprite.RotationDegrees = 180;
        }
    }

    //ready and process functions are useless here as Character Scene will never show up in the scene tree.
    // public override void _Ready()
    // {

    // }

    //  // Called every frame. 'delta' is the elapsed time since the previous frame.
    //  public override void _Process(float delta)
    //  {
    //      
    //  }
}
