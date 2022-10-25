using Firebase.Auth;
using System;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using Firebase.Database;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class RestApiManager : MonoBehaviour
{
    // Start is called before the first frame update
    
    [SerializeField] private InputField _usernameInputField;
    [SerializeField] private InputField _emailInputField;
    [SerializeField] private InputField _passwordInputField;

    [SerializeField] public GameObject Notification;
    private string _username;

    
    private Coroutine _registrationCoroutine;
    private Coroutine _loginCoroutine;
    
    public event Action<FirebaseUser> OnUserRegistered;
    public event Action<string> OnUserResgistrationFailed;
    public event Action<FirebaseUser> OnLoginSucceeded;
    public event Action<string> OnLoginFailed;
    

    void Start()
    {
        FirebaseAuth.DefaultInstance.StateChanged += HandleAuthStateChange;
    }
    public void ClickSignUp()
    {

        HandleRegistrationButtonClicked();
    }
    public void ClickLogIn()
    {
       HandleLoginButtonClicked();
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
            Notification.SetActive(true);
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
        
        

    }

    public void ClickClose()
    {
        Notification.SetActive(false);
    }
    ///////////////////////////////////////////////////////////////////////
    private void HandleAuthStateChange(object sender, EventArgs e)
    {
        if (FirebaseAuth.DefaultInstance.CurrentUser != null)
        {
            SceneManager.LoadScene(1);
        }
    }
    private void HandleRegistrationButtonClicked()
    {
        string email = _emailInputField.text;
        string password = _passwordInputField.text;
        _username = _usernameInputField.text;

        _registrationCoroutine = StartCoroutine(RegisterUser(email, password, _username));
    }
    private void HandleLoginButtonClicked()
    {
        if(_loginCoroutine == null)
        {
            _loginCoroutine = StartCoroutine(LoginCoroutine(_emailInputField.text, _passwordInputField.text));
        } 
    }
    private IEnumerator RegisterUser(string email, string password, string username)
    {
        
        var auth = FirebaseAuth.DefaultInstance;
        var registerTask = auth.CreateUserWithEmailAndPasswordAsync(email, password);

        yield return new WaitUntil(() => registerTask.IsCompleted);

        if (registerTask.Exception != null)
        {
            Debug.LogWarning($"Failed to register task {registerTask.Exception}");
            OnUserResgistrationFailed?.Invoke($"Failed to register task {registerTask.Exception}");
        }
        else
        {
            Debug.Log($"Succesfully registered user {registerTask.Result.Email}");
            try
            {
                UserData data = new UserData();
                //data.score = 0;
                data.username = username;
                string json = JsonUtility.ToJson(data);
                FirebaseDatabase.DefaultInstance.RootReference.Child("users").Child(registerTask.Result.UserId)
                    .SetRawJsonValueAsync(json);
                OnUserRegistered?.Invoke(registerTask.Result);
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
            }
            
            HandleLoginButtonClicked();
        }

        _registrationCoroutine = null;
    }
    private IEnumerator LoginCoroutine(string email, string password)
    {
        var auth = FirebaseAuth.DefaultInstance;
        var loginTask = auth.SignInWithEmailAndPasswordAsync(email,password);

        yield return new WaitUntil(() => loginTask.IsCompleted);

        if(loginTask.Exception != null)
        {
            Debug.LogWarning($"Login Failed with {loginTask.Exception}");
            OnLoginFailed?.Invoke($"Login Failed with {loginTask.Exception}");
            _loginCoroutine = null;
        }
        else
        {
            Debug.Log($"Login succeeded with {loginTask.Result}");
            OnLoginSucceeded?.Invoke(loginTask.Result);
            SceneManager.LoadScene(1);
        }
    }
    
    //////////////////////////////////////////////////////////////////////////
    



}


