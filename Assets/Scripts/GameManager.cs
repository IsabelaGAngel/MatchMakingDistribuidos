using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;



public class GameManager : MonoBehaviour
{

    public GameObject gameOverCanvas;
    [SerializeField] private Text _label;
    [SerializeField] private Text TableData;
    [SerializeField] public GameObject TableScores;
    private long _score;
    private string _username;
    private string _userId;
    DatabaseReference _mDatabase;
    Dictionary<string, object> TopLeaders;
    List<object> myListScores = new List<object>();
    void Start()
    {
        _mDatabase = FirebaseDatabase.DefaultInstance.RootReference;
        _userId = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
        Time.timeScale = 1;
        FirebaseAuth.DefaultInstance.StateChanged += GetName;
        FirebaseAuth.DefaultInstance.StateChanged += GetUserScore;

    }
    private void GetName(object sender, EventArgs e)
    {
        FirebaseDatabase.DefaultInstance
            .GetReference("users/" + _userId+"/username" )
            .GetValueAsync().ContinueWithOnMainThread(task => {
                if (task.IsFaulted)
                {
                    Debug.Log(task.Exception);
                    _label.text = "NULL";
                }
                else if (task.IsCompleted)
                {
                    DataSnapshot snapshot = task.Result;
                    
                    _username = (string)snapshot.Value;
                    _label.text = _username;
                }
            });

    }
    private void GetUserScore(object sender, EventArgs e)
    {

            FirebaseDatabase.DefaultInstance
            .GetReference("users/"+_userId+"/score")
            .GetValueAsync().ContinueWithOnMainThread(task => {
                if (task.IsFaulted)
                {
                    Debug.Log(task.Exception);
                }
                else if (task.IsCompleted)
                {
                    DataSnapshot snapshot = task.Result;

                    _score = (long)snapshot.Value;
  
                }
            });
    }

    public void GameOver()
    {
        //SetNewScore();
        gameOverCanvas.SetActive(true);
        Time.timeScale = 0;

    }

    public void Replay()
    {
        SceneManager.LoadScene(1);
    }

    public void ClickScores()
    {
        try
        {
            FirebaseDatabase.DefaultInstance
                .GetReference("users").OrderByChild("score").LimitToLast(5)
                .ValueChanged += HandleValueChanged;
           
        }

        catch (Exception  e)
        {
            Debug.Log(e.Message);
        }

        
    }

    public void ClickSignOut()
    {

        FirebaseAuth.DefaultInstance.SignOut();
        SceneManager.LoadScene(0);
    }

    /*public void SetNewScore()
    {
        try
        {
            if ((int)_score < Score.score)
                    {
                        UserData data = new UserData();
                        data.score = Score.score;   
                        data.username = _username;
                        Debug.Log(_username + "     ======     " + _score + "     ======     " + Score.score);
                        string json = JsonUtility.ToJson(data);
                        _mDatabase.Child("users").Child(_userId).SetRawJsonValueAsync(json);
                        
                    }
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
    }*/
    void HandleValueChanged(object sender, ValueChangedEventArgs args) {
        if (args.DatabaseError != null) {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }
        // Do something with the data in args.Snapshot
        TableScores.SetActive(true);

        Debug.Log(args.Snapshot);

        var users = (Dictionary<string, object>) args.Snapshot.Value;
        var OrderedScoresList = users.Values.OrderByDescending(x => ((Dictionary<string, object>)x)["score"]);
        
        foreach (var userData in OrderedScoresList)
        {
            Dictionary<string, object> userObject = (Dictionary<string, object>)userData;
            TableData.text += userObject["username"] + ":  " + userObject["score"] + "\n";
            Debug.Log(userObject["username"]+":"+userObject["score"]);
        }


        //TopLeaders = (Dictionary<string, object>) args.Snapshot;
        /*foreach (var userDoc in ((Dictionary<string, object>)args.Snapshot.Value))
        {
            //Debug.Log(userDoc);
            /*Dictionary<string, object> userObject = (Dictionary<string, object>)userDoc.Value;

            TableData.text += userObject["username"] + ":  " + userObject["score"] + "\n";
            Debug.Log(userObject["username"]+":"+userObject["score"]);#1#
        
        }
        */

    }
}