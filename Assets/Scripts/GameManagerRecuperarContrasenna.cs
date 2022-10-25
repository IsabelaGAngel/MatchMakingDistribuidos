using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Firebase.Auth;
using UnityEngine.SceneManagement;

public class GameManagerRecuperarContrasenna : MonoBehaviour
{
    [SerializeField] public GameObject Notification;
    [SerializeField] private Text _txtNotificacion;
    
    [SerializeField] private InputField _emailInputField;
    
    public void ClickCerrarNotificacion()
    {
        SceneManager.LoadScene(0);
    }
    public void ClickRestorePassword()
    {

        try
        {
            string emailAddress = _emailInputField.text;
            FirebaseAuth.DefaultInstance.SendPasswordResetEmailAsync(emailAddress).ContinueWith(task =>
            {
                if (task.IsCanceled)
                {
                    Debug.LogError("SendPasswordResetEmailAsync was canceled.");
                    return;
                }

                if (task.IsFaulted)
                {
                    Debug.LogError("SendPasswordResetEmailAsync encountered an error: " + task.Exception);
                    return;
                }

                Debug.Log("Password reset email sent successfully.");
                

            });
            _txtNotificacion.text = "Se ha enviado un correo de recuperacion.";
            Notification.SetActive(true);
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
    }
    
    // Start is called before the first frame update
    
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
