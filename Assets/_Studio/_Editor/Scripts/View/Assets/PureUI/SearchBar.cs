using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Terra.Studio
{
    public class SearchBar : MonoBehaviour
    {
        [SerializeField] private Button cancelSearchButton;
        [SerializeField] private TMP_InputField inputField;
        
        private Action<string> onSearchPressed;
        
        public void Init(Action<string> onSearch)
        {
            onSearchPressed = onSearch;
            inputField.onValueChanged.AddListener(InputValueChanged);
            inputField.onSubmit.AddListener(InputSubmitted);
        }

        private void InputValueChanged(string input)
        {
            
        }
        
        private void InputSubmitted(string input)
        {
            onSearchPressed?.Invoke(input);
        }
    }
}