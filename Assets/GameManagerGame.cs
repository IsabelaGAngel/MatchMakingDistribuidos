using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Mime;
using Firebase.Database;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManagerGame : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private Text _txtPlayers;
    private void Awake()
    {
        var _databaseGame = FirebaseDatabase.DefaultInstance.GetReference("/juegos/");
        _databaseGame.ChildAdded += HandleChildAddedGame;
        var _databaseRemoved = FirebaseDatabase.DefaultInstance.GetReference("/juegos/");
        _databaseGame.ChildRemoved += HandleChildRemovedGame;
    }

    // Update is called once per frame
    void HandleChildAddedGame(object sender, ChildChangedEventArgs args) {
        if (args.DatabaseError != null) {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }
        // Do something with the data in args.Snapshot
        try
        {
            //Debug.Log(args.Snapshot);
            Dictionary<string, object> users = (Dictionary<string, object>)args.Snapshot.Value;
            Debug.Log(args.Snapshot.Value);
            
            foreach (var user in users)
            {
                Debug.Log(user.Key);
                _txtPlayers.text += (string)user.Key;
            }

        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }

        
    }
    void HandleChildRemovedGame(object sender, ChildChangedEventArgs args) {
        if (args.DatabaseError != null) {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }
        // Do something with the data in args.Snapshot
        try
        {
            //Debug.Log(args.Snapshot);
            SceneManager.LoadScene(2);

        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }

        
    }

    public void ExitGame()
    {
        FirebaseDatabase.DefaultInstance.RootReference.Child("juegos")
            .SetRawJsonValueAsync(null);
        SceneManager.LoadScene(2);
    }
}
