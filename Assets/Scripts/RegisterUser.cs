using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;

public class RegisterUser : MonoBehaviour
{

    [System.Serializable]
    public class RegisterData
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
        Button submitButton = root.Q<Button>("SignInButton");
        submitButton.clicked += RegistrarUsuario;
   
    }

    private void RegistrarUsuario()
    {
        StartCoroutine(PostRequestRegister());
    }

    IEnumerator PostRequestRegister()
    {
        RegisterData registerData = new RegisterData();
        registerData.email = emailEntry.value;
        registerData.password = passwordEntry.value;

        string jsonBody = JsonUtility.ToJson(registerData);

        using UnityWebRequest www = UnityWebRequest.Post("http://localhost:3000/register", jsonBody, "application/json");
        www.timeout = 5;

        
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error de request: " + www.error);
            resultMessage.style.opacity = 1;
            resultMessage.text = "Error al registrar usuario";
        } else if (www.responseCode == 201)
        {
            resultMessage.style.opacity = 100;
            resultMessage.text = "¡Usuario creado exitosamente!";
            resultMessage.style.color = Color.green;
        } else
        {
            Debug.Log(www.responseCode);
        } 
    }
}
