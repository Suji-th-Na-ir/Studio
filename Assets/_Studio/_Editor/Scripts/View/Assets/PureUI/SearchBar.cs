using System;
using TMPro;
using UnityEngine;

namespace Terra.Studio
{
    public class SearchBar : MonoBehaviour
    {
        public TMP_InputField InputField;

        private Action<string> onSearchPressed;
        public void Init(Action<string> onSearch)
        {
            onSearchPressed = onSearch;
            InputField.onSubmit.AddListener(InputSubmitted);
        }

        private void InputSubmitted(string input)
        {
            onSearchPressed?.Invoke(input);
        }
    }
}