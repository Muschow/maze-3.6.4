using Godot;
using System;
using System.Collections.Generic;

public abstract class CharacterScript : KinematicBody2D
{
    public float speed;
    protected float baseSpeed = 100;
    protected MazeGenerator mazeTm; //every character needs a reference to the maze for movement/collisions/spawning etc

    //plays animations when moving
    protected void PlayAndPauseAnim(Vector2 masVector, AnimatedSprite animatedSprite) //masVector is move and slide vector, basically any movement vector will do  
    {
        masVector = masVector.Round();

        if (masVector == Vector2.Zero)
        {
            animatedSprite.Stop();
        }
        else if (masVector != Vector2.Zero)
        {
            animatedSprite.Play();
        }
    }

    //plays the correct animation for whatever direction youre going in
    //each character can only move in 4 directions in my game so as long as they have seperate up/down/left/right animations
    //(which they do) this method can apply
    protected void MoveAnimManager(Vector2 masVector, AnimatedSprite animatedSprite)
    {
        masVector = masVector.Normalized().Round(); //just makes sure the movement vector is in the form 1,0 / -1,0 / 0,1 / 0,-1

        if (masVector == Vector2.Up)
        {
            animatedSprite.Play("up");
        }
        else if (masVector == Vector2.Down)
        {
            animatedSprite.Play("down");
        }
        else if (masVector == Vector2.Right)
        {
            animatedSprite.Play("right");
        }
        else if (masVector == Vector2.Left)
        {
            animatedSprite.Play("left");
        }
    }
}
