using System.Collections;
using System.Collections.Generic;
using Firebase.Database;
using UnityEngine;
using UnityEngine.UI;

public class PersonaAmiga : MonoBehaviour
{
    // Start is called before the first frame update
    DatabaseReference _mDatabase;
    public Text txtPersona;
    public string nombre;
    void Start()
    {
        txtPersona.text = nombre;
    }
}
