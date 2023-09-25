#if UNITY_EDITOR
#define DEBUG
#endif

using System;
using UnityEngine;
using UnityEngine.UI;
using PlayShifu.Terra;
using System.Collections;

namespace Terra.Studio
{
    public class PasswordManager : MonoBehaviour
    {
        [SerializeField] private CanvasGroup m_refCanvasGroup;
        [SerializeField] private InputField passwordInputField;
        [SerializeField] private Text feedbackText;
        [SerializeField] private Image m_refInputFieldBorder;
        private const string correctPassword = "Terra@12345";

        public event Action OnCorrectPasswordEntered;

        private void Awake()
        {
            SystemOp.Register(this);
            feedbackText.text = "";
        }

#if DEBUG
        private IEnumerator Start()
        {
            yield return StartCoroutine(ShowCorrectPasswordAnim());
        }
#endif

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                CheckPassword();
            }
        }

        public void CheckPassword()
        {
            string enteredPassword = passwordInputField.text;
            if (enteredPassword == correctPassword)
            {
                StartCoroutine(ShowCorrectPasswordAnim());
            }
            else
            {
                StartCoroutine(ShowIncorrectPasswordAnim("Incorrect Password. Please try again!"));
            }
        }

        private IEnumerator ShowCorrectPasswordAnim()
        {
            float timer = 0f;
            Color toCol = Color.green;
            Color fromCol = m_refInputFieldBorder.color;
            float speed = 3f;
            feedbackText.text = "Correct Password!";
            feedbackText.color = new Color(0, 0, 0, 0);
            Color textFromCol = feedbackText.color;
            Color textToCol = Color.green;
            textToCol.a = 1f;
            do
            {
                m_refInputFieldBorder.color = Color.Lerp(fromCol, toCol, timer);
                feedbackText.color = Color.Lerp(textFromCol, textToCol, timer);
                timer += Time.deltaTime * speed;
                yield return new WaitForEndOfFrame();
            } while (timer <= 1f);
            OnCorrectPasswordEntered?.Invoke();
        }

        private IEnumerator ShowIncorrectPasswordAnim(string a_strErrorValue)
        {
            float timer = 0f;
            Color toCol = Helper.GetColorFromHex("#E83811");
            Color fromCol = m_refInputFieldBorder.color;
            float speed = 3f;
            feedbackText.text = a_strErrorValue;
            feedbackText.color = new Color(0, 0, 0, 0);
            Color textFromCol = feedbackText.color;
            Color textToCol = Helper.GetColorFromHex("#E83811");
            textToCol.a = 1f;
            do
            {
                m_refInputFieldBorder.color = Color.Lerp(fromCol, toCol, timer);
                feedbackText.color = Color.Lerp(textFromCol, textToCol, timer);
                timer += Time.deltaTime * speed;
                yield return new WaitForEndOfFrame();
            } while (timer <= 1f);
            yield return new WaitForSeconds(2f);
            yield return ResetColors();
        }

        private IEnumerator ResetColors()
        {
            float timer = 0f;
            Color toCol = Color.white;
            Color fromCol = m_refInputFieldBorder.color;
            float speed = 3f;
            Color textFromCol = feedbackText.color;
            Color textToCol = new Color(0, 0, 0, 0);
            textToCol.a = 1f;
            do
            {
                m_refInputFieldBorder.color = Color.Lerp(fromCol, toCol, timer);
                feedbackText.color = Color.Lerp(textFromCol, textToCol, timer);
                timer += Time.deltaTime * speed;
                yield return new WaitForEndOfFrame();
            } while (timer <= 1f);
            feedbackText.text = "";
            feedbackText.color = new Color(0, 0, 0, 0);
        }

        internal void FuckOff()
        {
            StartCoroutine(FuckOffAndGoAway());
        }

        private IEnumerator FuckOffAndGoAway()
        {
            float timer = 0f;
            float speed = 0.5f;
            float alphaFrom = m_refCanvasGroup.alpha;
            float alphaTo = 0f;
            do
            {
                m_refCanvasGroup.alpha = Mathf.Lerp(alphaFrom, alphaTo, timer);
                timer += Time.deltaTime * speed;
                yield return new WaitForEndOfFrame();
            } while (timer <= 1f);
            Destroy(gameObject);
        }

        private void OnDestroy()
        {
            SystemOp.Unregister(this);
        }
    }
}