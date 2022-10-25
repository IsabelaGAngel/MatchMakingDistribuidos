using Firebase.Auth;
using System;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using Firebase.Database;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManagerHome : MonoBehaviour
{
    [SerializeField] private InputField _usernameInputField;
    [SerializeField] private InputField _emailInputField;
    [SerializeField] private InputField _passwordInputField;
    
    [SerializeField] public GameObject Notification;
    [SerializeField] private Text _txtNotificacion;
    
    private Coroutine _registrationCoroutine;
    private Coroutine _loginCoroutine;
    
    public event Action<FirebaseUser> OnUserRegistered;
    public event Action<string> OnUserResgistrationFailed;
    public event Action<FirebaseUser> OnLoginSucceeded;
    public event Action<string> OnLoginFailed;
    
    
    
    // Start is called before the first frame update
    void Start()
    {
        
        FirebaseAuth.DefaultInstance.StateChanged += HandleAuthStateChange;
    }
    public void ClickRegistrarse()
    {

        HandleRegistrationButtonClicked();
    }
    public void ClickIniciarSesion()
    {
        HandleLoginButtonClicked();
    }
    public void ClickCerrarNotificacion()
    {
        Notification.SetActive(false);
    }
    public void ClickOlvideMiContrasenna()
    {
        SceneManager.LoadScene(1);
    }
    /*------------------------------Handles----------------------------------------*/
    private void HandleAuthStateChange(object sender, EventArgs e)
    {
        //FirebaseAuth.DefaultInstance.SignOut();
        if (FirebaseAuth.DefaultInstance.CurrentUser != null)
        {
            SceneManager.LoadScene(2);
        }
    }
    private void HandleRegistrationButtonClicked()
    {
        string email = _emailInputField.text;
        string password = _passwordInputField.text;
        string username = _usernameInputField.text;

        _registrationCoroutine = StartCoroutine(RegisterUser(email, password, username));
    }
    private void HandleLoginButtonClicked()
    {
        if(_loginCoroutine == null)
        {
            _loginCoroutine = StartCoroutine(LoginCoroutine(_emailInputField.text, _passwordInputField.text));
        } 
    }
    
    
    
    /*---------------------------------Coroutines-------------------------------------------------*/
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
            _txtNotificacion.text = "Usuario no registrado, favor registrarse primero.";
            Notification.SetActive(true);
        }
        else
        {
            Debug.Log($"Login succeeded with {loginTask.Result}");
            OnLoginSucceeded?.Invoke(loginTask.Result);
            SceneManager.LoadScene(2);
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
            _txtNotificacion.text = "Este usuario ya se encuentra registrado. Cambie sus datos e intentelo nuevamente";
            Notification.SetActive(true);
        }
        else
        {
            Debug.Log($"Succesfully registered user {registerTask.Result.Email}");
            try
            {
                UserData data = new UserData();
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
}
public class UserData
{
    public string username;
    public string name;
}
