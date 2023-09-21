using System;
using UnityEngine;
using UnityEngine.UI;

public class PasswordManager : MonoBehaviour
{
    public InputField passwordInputField;
    public Text feedbackText;

    private string correctPassword = "Terra@12345";

    public Action OnCorrectPasswordEntered;

    public void CheckPassword()
    {
        string enteredPassword = passwordInputField.text;

        if (enteredPassword == correctPassword)
        {
            feedbackText.text = "Correct Password!";
            OnCorrectPasswordEntered?.Invoke();
        }
        else
        {
            feedbackText.text = "Incorrect Password. Try again.";
        }
    }
}
