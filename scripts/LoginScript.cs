using Godot;
using System;

public class LoginScript : Control
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";
    private LineEdit usernameInput;
    private LineEdit passwordInput;
    private Label errorMessage;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        usernameInput = GetNode<LineEdit>("CanvasLayer/Panel/UsernameInput");
        passwordInput = GetNode<LineEdit>("CanvasLayer/Panel/PasswordInput");
        errorMessage = GetNode<Label>("CanvasLayer/Panel/ErrorMessage");
    }

    //  // Called every frame. 'delta' is the elapsed time since the previous frame.
    //  public override void _Process(float delta)
    //  {
    //      
    //  }

    private void _OnLoginButtonPressed()
    {
        GD.Print("login button pressed");
        GD.Print(usernameInput.Text);
        GD.Print(passwordInput.Text);
        //put code here
    }

    private void _OnCreateAccountButtonPressed()
    {
        GD.Print("create account button pressed");
        //put code here
    }
}
