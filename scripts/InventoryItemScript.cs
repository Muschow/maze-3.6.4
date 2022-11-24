using Godot;
using System;

public class InventoryItemScript : TextureRect
{
    //https://www.youtube.com/watch?v=p-oLVHYzxrM

    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";
    private ReferenceRect border;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        border = GetNode<ReferenceRect>("ReferenceRect");
    }

    //  // Called every frame. 'delta' is the elapsed time since the previous frame.
    //  public override void _Process(float delta)
    //  {
    //      
    //  }

    private void InitializeTexture(Texture texture)
    {
        this.Texture = texture;
    }

    private void SelectItem()
    {
        border.Show();
    }

    private void DeselectItem()
    {
        border.Hide();
    }
}
