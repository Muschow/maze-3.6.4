using Godot;
using System;

public class MainMenuScript : Control
{

    private void _OnPlayButtonPressed()
    {
        GetTree().ChangeScene("res://scenes/LoginScene.tscn");
    }

    private void _OnQuitButtonPressed()
    {
        GetTree().Quit();
    }
}
