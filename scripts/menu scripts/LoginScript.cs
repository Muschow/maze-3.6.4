using Godot;
using System;
using System.Linq;
using System.Collections.Generic;

public class LoginScript : Control
{
    private const string PEPPER = "VBU^(v9Vum#$/04a14V>wS(^0EfQRs6#o<"; //32 char
    private LineEdit usernameInput;
    private LineEdit passwordInput;
    private Label notification;
    private Node database; //this is in gdscript
    private Globals global;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        GetNodes();
    }

    private void GetNodes()
    {
        usernameInput = GetNode<LineEdit>("CanvasLayer/Panel/UsernameInput");
        passwordInput = GetNode<LineEdit>("CanvasLayer/Panel/PasswordInput");
        notification = GetNode<Label>("CanvasLayer/Panel/Notification");

        database = GetNode<Node>("/root/Database");
        global = GetNode<Globals>("/root/Globals");
    }
    private void _OnLoginButtonPressed()
    {
        //login: if username and password not empty and not sql injection, look in database 
        //for username and grab salt.
        //add salt and pepper to password and hash, check if hash = hash in database. 
        //If so, login, else error

        //GD.Print("login button pressed"); //remember to commect these out
        // GD.Print(usernameInput.Text);   //debug
        // GD.Print(passwordInput.Text);

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
                //this var above is a godot array of dictoinaries

                Godot.Collections.Array returnedSaltArray = (Godot.Collections.Array)database.Call("queryValue", saltQuery); //returns an object, must be cast to array
                string userSalt = (string)returnedSaltArray[0];
                GD.Print("userSalt from db: " + userSalt); //testing

                string password = passwordInput.Text + userSalt + PEPPER;
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
                    GD.Print("user hashed password: " + password); //testing
                    Godot.Collections.Array returnedIdArray = (Godot.Collections.Array)database.Call("queryValue", idQuery);
                    global.userId = (int)returnedIdArray[0];

                    GetTree().ChangeScene("res://scenes/main game scenes/Game.tscn");
                    //once logged in, switch scene to main game
                }
            }
        }
    }

    private void _OnCreateAccountButtonPressed()
    {
        //GD.Print("create account button pressed"); //debug

        if (IsInputValid(usernameInput.Text, passwordInput.Text)) //does input have sql injection/empty
        {
            if (ExistsInDB(IsUsernameInDBQuery())) //does username exist in db
            {
                notification.Text = "username already exists";
            }
            else
            {
                string newSalt = GenerateSalt(32);
                string password = passwordInput.Text + newSalt + PEPPER;
                password = password.SHA256Text(); //secures passwords so theyre not stored in plaintext in database
                GD.Print("salt+pepper+hashed password: " + password);

                string[] newRecordParam = new string[] { usernameInput.Text, password, newSalt };
                database.Call("queryDBwithParameters", "INSERT INTO PlayerInfo (Username, Password, Salt) VALUES (?, ?, ?);", newRecordParam);
                //adds new record to playerInfo, with username, password and salt
                notification.Text = "account created";
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

        Godot.Collections.Array returnedCount1Value = (Godot.Collections.Array)database.Call("queryValue", count1QueryResult);
        if ((int)returnedCount1Value[0] == 1) //returned count1value is either a 0 or 1, essentially a bool
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private bool IsInputValid(string username, string password) //validates input
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

    private string GenerateSalt(int saltLength)
    {
        string salt = null;

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

        GD.Print("generated salt: ", salt); //testing
        return salt;
    }

    private bool ContainsInvalidWords(string input) //used to prevent SQL injection
    {
        string[] invalidWords = new string[] { "create", "drop", "table", "index", "alter", "insert", "select", "update", "delete", "database", "into" };

        for (int i = 0; i < invalidWords.Length; i++)
        {
            if (input.ToLower().Contains(invalidWords[i]))
            {
                //GD.Print("use of invalid word"); //debug
                return true;
            }
        }

        return false;

    }
    //when user clicks login, check their username and grab their salt from databse,
    //then add salt + pepper to password, then check if hashed password == database password
    //if so, then login, otherwise error
}
