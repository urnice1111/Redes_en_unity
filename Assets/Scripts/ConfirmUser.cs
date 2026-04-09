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
        string email = emailEntry.value.Trim();
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

        using UnityWebRequest www = new UnityWebRequest("http://localhost:3000/login", "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
        www.uploadHandler = new UploadHandlerRaw(bodyRaw);
        www.downloadHandler = new DownloadHandlerBuffer();
        www.SetRequestHeader("Content-Type", "application/json");
        www.timeout = 5;

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
