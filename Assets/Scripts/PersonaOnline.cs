using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using Firebase.Auth;
using Firebase.Database;
using UnityEngine;
using UnityEngine.UI;

public class PersonaOnline : MonoBehaviour
{
    DatabaseReference _mDatabase;
    [SerializeField]private GameObject buttonAdd;
    public Text txtPersona;
    public string nombre;
    public string remitente;
    void Start()
    {
        txtPersona.text = nombre;
        
        _mDatabase = FirebaseDatabase.DefaultInstance.RootReference;
    }

    public void SetBuzon()
    {
        try
        {
            UserData data = new UserData();
            data.username = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
            data.name = remitente;
            //Debug.Log(remitente);
            string json = JsonUtility.ToJson(data);
            _mDatabase.Child("buzones").Child(nombre).Child(remitente).SetRawJsonValueAsync(json);
            buttonAdd.SetActive(false);
            //Debug.Log(nombre);
            //Debug.Log(remitente);
            
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
    }
}
