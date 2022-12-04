using Godot;
using System;
using System.Linq;
using System.Collections.Generic;

public class LoginScript : Control
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";
    //have really long string here as a private const for the pepper (potentially?)
    private const string pepper = "VBU^(v9Vum#$/04a14V>wS"; //20 char
    private LineEdit usernameInput;
    private LineEdit passwordInput;
    private Label notification;
    private Node database; //this is in gdscript

    private Globals global;

    private string salt;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        usernameInput = GetNode<LineEdit>("CanvasLayer/Panel/UsernameInput");
        passwordInput = GetNode<LineEdit>("CanvasLayer/Panel/PasswordInput");
        notification = GetNode<Label>("CanvasLayer/Panel/Notification");

        database = GetNode<Node>("/root/Database");
        global = GetNode<Globals>("/root/Globals");


    }

    //  // Called every frame. 'delta' is the elapsed time since the previous frame.
    //  public override void _Process(float delta)
    //  {
    //      
    //  }

    private void _OnLoginButtonPressed()
    {
        //login: if username and password not empty and not sql injection, look in database 
        //for username and grab salt.
        //add salt and pepper to password and hash, check if hash = hash in database. 
        //If so, login, else error

        GD.Print("login button pressed");
        GD.Print(usernameInput.Text);
        GD.Print(passwordInput.Text);
        //put code here

        if (IsInputValid(usernameInput.Text, passwordInput.Text))
        {
            if (!ExistsInDB(IsUsernameInDBQuery()))
            {
                notification.Text = "username doesnt exist, try creating account";
            }
            else
            {
                string[] usernameParam = new string[] { usernameInput.Text };
                var saltQuery = database.Call("queryDBwithParameters", "SELECT Salt FROM PlayerInfo WHERE Username = ?;", usernameParam);
                //this var above is an array of dictoinaries
                string userSalt = (string)database.Call("returnSalt", saltQuery);

                string password = passwordInput.Text + userSalt + pepper;
                password = password.SHA256Text();

                string[] userParams = new string[] { usernameInput.Text, password };
                var passwordExistsQuery = database.Call("queryDBwithParameters", "SELECT COUNT(1) FROM PlayerInfo WHERE Username = ? AND Password = ?;", userParams);

                if (!ExistsInDB(passwordExistsQuery))
                {
                    notification.Text = "invalid password";
                }
                else
                {
                    var idQuery = database.Call("queryDBwithParameters", "SELECT ID FROM PlayerInfo WHERE Username = ? AND Password = ?;", userParams);
                    global.userId = (int)database.Call("queryValue", idQuery); //grabs the userId

                    //once logged in, switch scene to main game
                }
            }
        }
    }

    private void _OnCreateAccountButtonPressed()
    {
        GD.Print("create account button pressed");
        //put code here
        if (IsInputValid(usernameInput.Text, passwordInput.Text)) //does input have sql injection/empty
        {
            if (ExistsInDB(IsUsernameInDBQuery())) //does username exist in db
            {
                notification.Text = "username already exists";
            }
            else
            {
                GenerateSalt(20); //if not, generate salt and do password + salt + pepper & hash it
                string password = passwordInput.Text + salt + pepper;
                password = password.SHA256Text();


                string[] newRecordParam = new string[] { usernameInput.Text, password, salt };
                database.Call("queryDBwithParameters", "INSERT INTO PlayerInfo (Username, Password, Salt) VALUES (?, ?, ?);", newRecordParam);
                //add new record to playerInfo, with username, password and salt
            }



        }

    }

    private object IsUsernameInDBQuery() //combine with exists in db to get useful output
    {
        string[] usernameParam = new string[1] { usernameInput.Text };
        var result = database.Call("queryDBwithParameters", "SELECT COUNT(1) FROM PlayerInfo WHERE Username = ?;", usernameParam);
        //returns either {COUNT(1):1} for does exist or {COUNT(1):0} for doesnt exist //array of dictionaries
        return result;

    }

    private bool ExistsInDB(object count1QueryResult) //use with COUNT(1) query
    {
        database.Call("printFromQuery", count1QueryResult);

        if ((bool)database.Call("existsInDB", count1QueryResult))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private bool IsInputValid(string username, string password)
    {
        if (username.Empty() || password.Empty())
        {
            notification.Text = "username or password is empty";
        }
        else if (ContainsInvalidWords(username) || ContainsInvalidWords(password))
        {
            notification.Text = "username/password contains invalid words";
        }
        else
        {
            return true;
        }

        return false;
    }

    private void GenerateSalt(int saltLength)
    {
        string lowerLetters = "abcdefghijklmnopqrstuvwxyz";
        string upperLetters = lowerLetters.ToUpper();
        string numbers = "0123456789";
        string symbols = "¬`!£$%^&*()_+}{~#@:?><m,./;[]";


        string valid = lowerLetters + upperLetters + numbers + symbols;

        Random rnd = new Random();

        for (int i = 0; i < saltLength; i++)
        {
            salt = salt + valid[rnd.Next(0, valid.Length)];
        }

        GD.Print("salt: ", salt);
    }

    private bool ContainsInvalidWords(string input)
    {
        string[] invalidWords = new string[] { "create", "drop", "table", "index", "alter", "insert", "select", "update", "delete", "database", "into" };

        for (int i = 0; i < invalidWords.Length; i++)
        {
            if (input.ToLower().Contains(invalidWords[i]))
            {
                GD.Print("use of invalid word");
                return true;
            }
        }

        return false;

    }





    //when user clicks login, check their username and grab their salt from databse,
    //then add salt + pepper to password, then check if hashed password == database password
    //if so, then login, otherwise error
}
