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
        
        private int _currentPage = 1;
        private int _max = 10;

        private Action<int> _onPageChanged;
        public void Init(int currentMax, Action<int> pageChanged)
        {
            _max = currentMax; 
            _onPageChanged = pageChanged;
            _currentPage = 1;
            SetButtonRange(1);
            PageChanged();

            for (var i = 0; i < buttons.Length; i++)
            {
                var button = buttons[i];
                button.onClick.RemoveAllListeners();
                var i1 = i;
                button.onClick.AddListener(() =>
                {
                    var pageToGoTo = textInButtons[i1];
                    var str = pageToGoTo.text.Replace("<b>", "").Replace("</b>", "");
                    var x = int.Parse(str);

                    if (x==_currentPage)
                    {
                        return;
                    }
                    if (x < _currentPage)
                    {
                        _currentPage = x;
                        SetButtonRange(Mathf.Max(_currentPage-2,1));
                    }
                    else if (x > _currentPage)
                    {
                        _currentPage = x;
                        SetButtonRange(Mathf.Min(Mathf.Max(_currentPage - 2, 1), _max - 4));
                    }
                    PageChanged();
                });
            }

            this.GetComponent<HorizontalLayoutGroup> ().enabled = false;
            this.GetComponent<HorizontalLayoutGroup> ().enabled = true;
        }

        private void SetButtonRange(int lowestRange)
        {
            for (var i = 0; i < textInButtons.Length; i++)
            {
                textInButtons[i].text = (lowestRange + i).ToString();
                textInButtons [i].color = Color.white;
                buttons [i].gameObject.SetActive(lowestRange + i <= _max && lowestRange+i>0);
            }

            if (_currentPage > 2 && _currentPage < _max - 2)
            {
                SetActiveButton(2);
            }
            else if (_currentPage == 1)
            {
                SetActiveButton(0);
            }
            else if (_currentPage == 2)
            {
                SetActiveButton(1);
            }
            else if (_currentPage == _max - 1)
            {
                SetActiveButton(4);
            }
            else if (_currentPage == _max - 2)
            {
                SetActiveButton(3);
            }
        }
        
        private void SetActiveButton(int idx)
        {
            var txt = textInButtons[idx].text;
            textInButtons[idx].text = $"<b>{txt}</b>";
            textInButtons [idx].color = PlayShifu.Terra.Helper.GetColorFromHex ("#7063FF");
        }

        public void LeftClicked()
        {
            var newPage = Mathf.Max(1, _currentPage - 1);
            if (newPage == _currentPage)
            {
                return;
            }

            _currentPage = newPage;
            SetButtonRange(Mathf.Max(_currentPage-2,1));
            PageChanged();
        }

        public void RightClicked()
        {
            var newMin = Mathf.Min(_max, _currentPage + 1);
            if (newMin == _currentPage)
            {
                return;
            }

            _currentPage = newMin;
            SetButtonRange(Mathf.Min(Mathf.Max(_currentPage-2,1),_max-4));
            PageChanged();
        }

        private void PageChanged()
        {
            _onPageChanged?.Invoke(_currentPage);
        }
    }
}