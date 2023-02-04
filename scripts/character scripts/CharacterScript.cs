using Godot;
using System;
using System.Collections.Generic;

public abstract class CharacterScript : KinematicBody2D
{
    public float speed;
    protected float baseSpeed = 100;
    protected MazeGenerator mazeTm;
    protected AnimatedSprite animatedSprite;

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
}
