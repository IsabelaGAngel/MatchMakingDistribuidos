using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Firebase.Database;
using Firebase.Auth;
using Firebase.Extensions;
using JetBrains.Annotations;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

public class GameManagerRedSocial : MonoBehaviour
{
    DatabaseReference _mDatabase;
    
    [SerializeField] public GameObject Notification;
    [SerializeField] private Text _txtNotificacion;
    
    [SerializeField] private GameObject personaOnline;
    [SerializeField] private Transform padreOnline;
    
    [SerializeField] private GameObject personaBuzon;
    [SerializeField] private Transform padreBuzon;
    
    [SerializeField] private GameObject personaAmiga;
    [SerializeField] private Transform padreAmiga;
    private string _username;
    private string _salaName;
    private int countPlayers = 0;

    private int stateMatch = 0;
    // Start is called before the first frame update
    private void Update()
    {
        if (stateMatch == 1)
        {
            var _databaseMatchFounded = FirebaseDatabase.DefaultInstance.GetReference("salas/" + _salaName);
            _databaseMatchFounded.ChildAdded += HandleChildChangedMatch;
            stateMatch = 1;
        }
        if (stateMatch ==3)
        {
            Debug.Log("Cambiar de escena");
            SceneManager.LoadScene(3);
        }
    }
    

    public void ClickCerrarNotificacion()
    {
        Notification.SetActive(false);
    }
    private void GetName(object sender, EventArgs e)
    {
        FirebaseDatabase.DefaultInstance
            .GetReference("users/" + FirebaseAuth.DefaultInstance.CurrentUser.UserId+"/username" )
            .GetValueAsync().ContinueWithOnMainThread(task => {
                if (task.IsFaulted)
                {
                    Debug.Log(task.Exception);
                }
                else if (task.IsCompleted)
                {
                    DataSnapshot snapshot = task.Result;
                    
                    _username = (string)snapshot.Value;
                    //Debug.Log(_username);
                    if (_username != null)
                    {
                        SetOnline();
                    }
                }
            });

    }
    
    private void Awake()
    {
        _mDatabase = FirebaseDatabase.DefaultInstance.RootReference;
        var _databaseOnline = FirebaseDatabase.DefaultInstance.GetReference("/online/");
        _databaseOnline.ChildAdded += HandleChildAddedOnline;
        var _databaseBuzon = FirebaseDatabase.DefaultInstance.GetReference("/buzones/");
        _databaseBuzon.ChildAdded += HandleChildAddedBuzon;
        var _databaseAmigos = FirebaseDatabase.DefaultInstance.GetReference("users/" + FirebaseAuth.DefaultInstance.CurrentUser.UserId + "/amigos/");
        _databaseAmigos.ChildAdded += HandleChildAddedAmigos;
        
        var _databaseOnlineRemoved = FirebaseDatabase.DefaultInstance.GetReference("/online/");
        _databaseOnlineRemoved.ChildRemoved += HandleChildOnlineRemoved;
        
        FirebaseAuth.DefaultInstance.StateChanged += GetName;
        
    }

    public void SetOnline()
    {
        try
        {
            UserData data = new UserData();
            data.username = _username;
            string json = JsonUtility.ToJson(data);
            _mDatabase.Child("online").Child(FirebaseAuth.DefaultInstance.CurrentUser.UserId).SetRawJsonValueAsync(json);
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
    }
    public void CerrarSesion()
    {
        _mDatabase.Child("online").Child(FirebaseAuth.DefaultInstance.CurrentUser.UserId).SetRawJsonValueAsync(null);
        FirebaseAuth.DefaultInstance.SignOut();
        SceneManager.LoadScene(0);

    }
    /*-------------------------HAndles----------------------------------------------*/
    void HandleChildAddedOnline(object sender, ChildChangedEventArgs args) {
        if (args.DatabaseError != null) {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }
        // Do something with the data in args.Snapshot
        try
        {
            //Debug.Log(args.Snapshot);
            Dictionary<string, object> users = (Dictionary<string, object>)args.Snapshot.Value;
            //Debug.Log(users["username"]);
            personaOnline.GetComponent<PersonaOnline>().nombre = (string)users["username"];
            personaOnline.GetComponent<PersonaOnline>().remitente = _username;
            personaOnline.name = args.Snapshot.Key;
            Instantiate(personaOnline).transform.SetParent(padreOnline.transform); 
            
            
            
            _txtNotificacion.text = users["username"] + " se ha conectado.";
            Notification.SetActive(true);
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }

        
    }
    void HandleChildAddedBuzon(object sender, ChildChangedEventArgs args) {
        if (args.DatabaseError != null) {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }
        // Do something with the data in args.Snapshot
        try
        {
            //Debug.Log(args.Snapshot);
            //Debug.Log(args.Snapshot.Key);
            personaBuzon.GetComponent<PersonaBuzon>().nombre = (string)args.Snapshot.Key;
            personaBuzon.GetComponent<PersonaBuzon>().remitente = _username;
            Instantiate(personaBuzon).transform.SetParent(padreBuzon.transform); 
            
            
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }

        
    }void HandleChildAddedAmigos(object sender, ChildChangedEventArgs args) {
        if (args.DatabaseError != null) {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }
        // Do something with the data in args.Snapshot
        try
        {
            //Debug.Log(args.Snapshot);
            personaAmiga.GetComponent<PersonaAmiga>().nombre = args.Snapshot.Key;
            
            Instantiate(personaAmiga).transform.SetParent(padreAmiga.transform);
           

        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }

        
    }
    void HandleChildOnlineRemoved(object sender, ChildChangedEventArgs args) {
        if (args.DatabaseError != null) {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }

        try
        {
            Debug.Log(args.Snapshot);
            Dictionary<string, object> users = (Dictionary<string, object>)args.Snapshot.Value;
            var gameObject = GameObject.Find((string)args.Snapshot.Key+ "(Clone)");
            Debug.Log(args.Snapshot.Key);
            Destroy(gameObject);
            _txtNotificacion.text = users["username"] + " se ha desconectado";
            Notification.SetActive(true);
            // Do something with the data in args.Snapshot
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
        
    }void HandleChildChangedMatch(object sender, ChildChangedEventArgs args) {
        if (args.DatabaseError != null) {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }

        try
        {
            Debug.Log(args.Snapshot);
            
            
            countPlayers += 1;
            if (countPlayers ==2)
            {
                FirebaseAuth.DefaultInstance.StateChanged += SetSalaJuegos;
                
                stateMatch = 3;
            }

        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
        
    }
    public void BtnMatch()
    {
        if (stateMatch == 0)
        {
            FirebaseAuth.DefaultInstance.StateChanged += GetSalaMatch;
        }
    }
    private void GetSalaMatch(object sender, EventArgs e)
    {
        FirebaseDatabase.DefaultInstance
            .GetReference("salas/")
            .GetValueAsync().ContinueWithOnMainThread(task => {
                if (task.IsFaulted)
                {
                    Debug.Log(task.Exception);
                }
                else if (task.IsCompleted)
                {
                    DataSnapshot snapshot = task.Result;
                    long tmp = snapshot.ChildrenCount;
                    if (tmp <= 0)
                    {
                        UserData data = new UserData();
                        data.username = _username;
                        string json = JsonUtility.ToJson(data);
                        FirebaseDatabase.DefaultInstance.RootReference.Child("salas").Child("Sala " + _username).Child(_username)
                            .SetRawJsonValueAsync(json);
                        stateMatch = 1;
                    }
                    else
                    {
                        Dictionary<string, object> users = (Dictionary<string, object>)snapshot.Value;
                        foreach (var sala in users)
                        {
                            Debug.Log(sala.Key);
                            UserData data = new UserData();
                            data.username = _username;
                            string json = JsonUtility.ToJson(data);
                            FirebaseDatabase.DefaultInstance.RootReference.Child("salas/"+sala.Key).Child(_username)
                                .SetRawJsonValueAsync(json);
                            _salaName =sala.Key;
                            stateMatch = 1;

                        }
                    }

                }
            });

    }private void SetSalaJuegos(object sender, EventArgs e)
    {
        FirebaseDatabase.DefaultInstance
            .GetReference("salas/")
            .GetValueAsync().ContinueWithOnMainThread(task => {
                if (task.IsFaulted)
                {
                    Debug.Log(task.Exception);
                }
                else if (task.IsCompleted)
                {
                    DataSnapshot snapshot = task.Result;
                    long tmp = snapshot.ChildrenCount;
                    Dictionary<string, object> users = (Dictionary<string, object>)snapshot.Value;
                    foreach (var sala in users)
                    {
                        Debug.Log(sala.Key);
                        DataUserGame data = new DataUserGame();
                        data.username = _username;
                        data.score = 0;
                        string json = JsonUtility.ToJson(data);
                        FirebaseDatabase.DefaultInstance.RootReference.Child("juegos").Child(sala.Key).Child(_username)
                            .SetRawJsonValueAsync(json);
                        FirebaseDatabase.DefaultInstance.RootReference.Child("salas").Child(_salaName)
                            .SetRawJsonValueAsync(null);
                        
                    }

                }
            });

    }
    
}
public class DataUserGame
{
    public string username;
    public int score;
}

