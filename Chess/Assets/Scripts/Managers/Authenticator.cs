using SimpleJSON;
using System.Collections;
using System.Net;
using TMPro;
using UnityEngine;
using UnityHelper.Utilities;

public class Authenticator : MonoBehaviour
{
    [SerializeField] private bool isCreator;

    [SerializeField] private GameObject loginWindow;
    [SerializeField] private GameObject signupWindow;
    [SerializeField] private GameObject puzzleWindow;

    private TMP_InputField usernameInput;
    private TMP_InputField emailInput;
    private TMP_InputField passwordInput;

    private IEnumerator LoginRequest()
    {
        if (IsEmptyOrWhiteSpace(emailInput.text))
        {
            Debug.LogError("Email is not valid!");
            yield break;
        }
        if (IsEmptyOrWhiteSpace(passwordInput.text))
        {
            Debug.LogError("Password is not valid!");
            yield break;
        }

        var body = new JSONObject();
        body.Add("email", emailInput.text);
        body.Add("password", passwordInput.text);
        body.Add("role", isCreator ? "Creator" : "Player");


        JSONObject response = null;

        yield return APIHandler.SendRequest(
            "http://localhost:3000/api/auth/login",
            "POST", body: body.ToString(),
            result: res => response = (JSONObject)res);

        if (response["status"] == (int)HttpStatusCode.BadRequest)
        {
            Debug.LogError(response["message"]);
            yield break;
        }

        User.CreateUser(response);

        gameObject.SetActive(false);
        puzzleWindow.SetActive(true);
    }

    private IEnumerator SignupRequest()
    {
        if (IsEmptyOrWhiteSpace(usernameInput.text))
        {
            Debug.LogError("Username is not valid!");
            yield break;
        }

        if (IsEmptyOrWhiteSpace(emailInput.text))
        {
            Debug.LogError("Email is not valid!");
            yield break;
        }

        if (IsEmptyOrWhiteSpace(passwordInput.text))
        {
            Debug.LogError("Password is not valid!");
            yield break;
        }

        var body = new JSONObject();
        body.Add("username", usernameInput.text);
        body.Add("email", emailInput.text);
        body.Add("password", passwordInput.text);
        body.Add("role", isCreator ? "Creator" : "Player");

        JSONObject response = null;

        yield return APIHandler.SendRequest(
            "http://localhost:3000/api/auth/signup",
            "POST", body: body.ToString(),
            result: res => response = (JSONObject)res);

        if (response["status"] == (int)HttpStatusCode.BadRequest)
        {
            Debug.LogError(response["message"]);
            yield break;
        }

        User.CreateUser(response);

        gameObject.SetActive(false);
        puzzleWindow.SetActive(true);
    }
    private bool IsEmptyOrWhiteSpace(string str)
    {
        return str == string.Empty || string.IsNullOrWhiteSpace(str);
    }

    public void Login()
    {

        StartCoroutine(LoginRequest());
    }

    public void Signup()
    {
        StartCoroutine(SignupRequest());
    }

    public void OpenLoginWindow()
    {
        loginWindow.SetActive(true);
        signupWindow.SetActive(false);
    }

    public void OpenSignupWindow()
    {
        signupWindow.SetActive(true);
        loginWindow.SetActive(false);
    }

    public void IdentifyInputField(string name)
    {
        switch (name)
        {
            case "username":
                usernameInput = UnityUtils.GetPressedUI().GetComponent<TMP_InputField>();
                break;
            case "email":
                emailInput = UnityUtils.GetPressedUI().GetComponent<TMP_InputField>();
                break;
            case "password":
                passwordInput = UnityUtils.GetPressedUI().GetComponent<TMP_InputField>();
                break;
        }
    }

}
