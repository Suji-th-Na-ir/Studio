using System;
using UnityEngine;
using UnityEngine.UI;

public class PasswordManager : MonoBehaviour
{
    public InputField passwordInputField;
    public Text feedbackText;

    private string correctPassword = "Terra@12345";

    public Action OnCorrectPasswordEntered;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
            CheckPassword();
    }
    public void CheckPassword()
    {
        string enteredPassword = passwordInputField.text;

        if (enteredPassword == correctPassword)
        {
            feedbackText.text = "Correct Password!";
            OnCorrectPasswordEntered?.Invoke();
        }
        else if(string.IsNullOrEmpty(enteredPassword))
        {
            feedbackText.text = "Password Is Empty.";
        }
        else
        {
            feedbackText.text = "Incorrect Password. Try again.";
        }
    }
}
