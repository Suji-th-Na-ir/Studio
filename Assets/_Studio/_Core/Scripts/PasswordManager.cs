using System;
using UnityEngine;
using UnityEngine.UI;

public class PasswordManager : MonoBehaviour {
    public InputField passwordInputField;
    public Text feedbackText;

    private const string correctPassword = "Terra@12345";

    public Action OnCorrectPasswordEntered;

    private void Update () {
        if (Input.GetKeyDown (KeyCode.Return))
            CheckPassword ();
    }

    public void CheckPassword()
    {
        string enteredPassword = passwordInputField.text;

        if (enteredPassword == correctPassword)
        {
            feedbackText.text = "<color=green>Correct Password!</color>";
            OnCorrectPasswordEntered?.Invoke();
        }
        else if (string.IsNullOrEmpty(enteredPassword))
        {
            feedbackText.text = "<color=red>Password Is Empty.</color>";
        }
        else
        {
            feedbackText.text = "<color=red>Incorrect Password. Try again.</color>";
        }
    }
}
