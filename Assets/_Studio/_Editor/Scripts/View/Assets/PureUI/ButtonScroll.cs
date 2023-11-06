using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Terra.Studio
{
    public class ButtonScroll : MonoBehaviour
    {
        [SerializeField] private Button[] buttons;
        [SerializeField] private TextMeshProUGUI[] textInButtons;
        
        private int currentPage = 1;
        private int min = 1;
        private int max = 10;

        private Action<int> onPageChanged;
        public void Init(int max, Action<int> pageChanged)
        {
            this.max = max; 
            onPageChanged = pageChanged;
            currentPage = 1;
            SetButtonRange(1);
            PageChanged();
        }

        private void SetButtonRange(int lowestRange)
        {
            for (var i = 0; i < textInButtons.Length; i++)
            {
                textInButtons[i].text = (lowestRange + i).ToString();
            }

            if (currentPage > 2 && currentPage < max - 2)
            {
                SetActiveButton(2);
            }
            else if (currentPage == 1)
            {
                SetActiveButton(0);
            }
            else if (currentPage == 2)
            {
                SetActiveButton(1);
            }
            else if (currentPage == max - 1)
            {
                SetActiveButton(4);
            }
            else if (currentPage == max - 2)
            {
                SetActiveButton(3);
            }
        }
        
        private void SetActiveButton(int idx)
        {
            var txt = textInButtons[idx].text;
            textInButtons[idx].text = $"<b>{txt}</b>";
        }

        public void LeftClicked()
        {
            var newPage = Mathf.Max(1, currentPage - 1);
            if (newPage == currentPage)
            {
                return;
            }

            currentPage = newPage;
            SetButtonRange(Mathf.Max(currentPage-2,1));
            PageChanged();
        }

        public void RightClicked()
        {
            var newMin = Mathf.Min(max, currentPage + 1);
            if (newMin == currentPage)
            {
                return;
            }

            currentPage = newMin;
            SetButtonRange(Mathf.Min(Mathf.Max(currentPage-2,1),max-4));
            PageChanged();
        }

        private void PageChanged()
        {
            onPageChanged?.Invoke(currentPage);
        }
    }
}