using Godot;
using System;

public class MainMenuScript : Control
{
    private void _OnPlayButtonPressed()
    {
        GetTree().ChangeScene("res://scenes/menu scenes/LoginScene.tscn");
    }

    private void _OnQuitButtonPressed()
    {
        PrintStrayNodes();
        GetTree().Quit();
    }
}
