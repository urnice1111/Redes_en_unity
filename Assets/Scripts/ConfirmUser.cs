using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class ConfirmUser : MonoBehaviour
{
    [System.Serializable]
    public class LoginData
    {
        public string email;
        public string password;
    }
    
    [System.Serializable]
    public class LoginResponse
    {
        public string message;
        public UserData user;
    }

    [System.Serializable]
    public class UserData
    {
        public int id;
        public string email;
    }

    private TextField emailEntry;
    private TextField passwordEntry;
    private Label resultMessage;

    void OnEnable()
    {
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;
        emailEntry = root.Q<TextField>("EmailEntry");
        passwordEntry = root.Q<TextField>("PasswordEntry");
        resultMessage = root.Q<Label>("Response");

        Button loginButton = root.Q<Button>("LogInButton");
        loginButton.clicked += ConfirmCredentials;
    }

    private void ConfirmCredentials()
    {
        string email = emailEntry.value;
        string password = passwordEntry.value;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            ShowMessage("Please enter both email and password.", Color.red);
            return;
        }

        StartCoroutine(PostRequestLogin(email, password));
    }

    IEnumerator PostRequestLogin(string email, string password)
    {
        LoginData loginData = new LoginData
        {
            email = email,
            password = password
        };

        string jsonBody = JsonUtility.ToJson(loginData);

        using UnityWebRequest www = UnityWebRequest.Post("http://localhost:3000/login", jsonBody, "application/json");


        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Login request failed: " + www.error);
            ShowMessage("Unable to contact server. Try again later.", Color.red);
            yield break;
        }

        if (www.responseCode == 200)
        {
            ShowMessage("Login successful! Loading game...", Color.green);
            SceneManager.LoadScene("SampleScene");
            StartCoroutine(RegisterSessionInDB(response.user.id));
        }
        else if (www.responseCode == 401 || www.responseCode == 403)
        {
            ShowMessage("Email or password is incorrect.", Color.red);
        }
        else
        {
            Debug.LogWarning("Unexpected login response: " + www.responseCode + " - " + www.downloadHandler.text);
            ShowMessage("Login failed. Please try again.", Color.red);
        }
    }
    
    IEnumerator RegisterSessionInDB(int userId)
    {
        string jsonBody = "{\"userId\":" + userId + "}";

        using UnityWebRequest req = new UnityWebRequest("http://localhost:3000/set_login_user", "PUT");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
        req.uploadHandler = new UploadHandlerRaw(bodyRaw);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");
        req.timeout = 5;

        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error registering session: " + req.error);
        }
        else if (req.responseCode == 201)
        {
            Debug.Log("Session registered in DB: " + req.downloadHandler.text);
            SceneManager.LoadScene("SampleScene");
        }
        else
        {
            Debug.LogWarning("Unexpected response: " + req.responseCode + " - " + req.downloadHandler.text);
        }
    }
    
    private void ShowMessage(string text, Color color)
    {
        if (resultMessage != null)
        {
            resultMessage.text = text;
            resultMessage.style.color = color;
            resultMessage.style.opacity = 1;
        }
    }
}
