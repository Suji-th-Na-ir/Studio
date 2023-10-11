#if UNITY_EDITOR
#define DEBUG
#endif

using System;
using UnityEngine;
using UnityEngine.UI;
using PlayShifu.Terra;
using System.Collections;
using mixpanel;

namespace Terra.Studio
{
    public class PasswordManager : MonoBehaviour
    {
        [SerializeField] private CanvasGroup m_refCanvasGroup;

        [SerializeField] private InputField nameInputField;
        [SerializeField] private Image m_refNameInputFieldBorder;
        [SerializeField] private InputField passwordInputField;
        [SerializeField] private Image m_refPasswordInputFieldBorder;

        [SerializeField] private Text feedbackText;

        private const string correctPassword = "Studio@12345";

        private string m_strName;
        private string m_strPassword;

        private bool m_bIsCheckPasswordInProgress;

        [SerializeField] private GameObject m_refNotLoggedInScreenGO;
        [SerializeField] private GameObject m_refLoggingYouInScreenGO;

        public event Action OnCorrectPasswordEntered;

        private void Awake()
        {
            SystemOp.Register(this);
            feedbackText.text = "";
        }

        private void Start () {
            if (PlayerPrefs.HasKey ("UserName")) {
                m_strName = PlayerPrefs.GetString ("UserName");
            }
            if (PlayerPrefs.HasKey ("Password")) {
                m_strPassword = PlayerPrefs.GetString ("Password");
            }
            if (m_strPassword == correctPassword) {
                m_refLoggingYouInScreenGO.SetActive (true);
                m_refNotLoggedInScreenGO.SetActive (false);
                LogLoginEvent ("auto");
                OnCorrectPasswordEntered?.Invoke ();
            } else {
                m_refLoggingYouInScreenGO.SetActive (false);
                m_refNotLoggedInScreenGO.SetActive (true);
            }
            //yield return StartCoroutine (ShowCorrectPasswordAnim ());
        }

        private void Update()
        {
            if (!m_bIsCheckPasswordInProgress && Input.GetKeyDown(KeyCode.Return))
            {
                CheckPassword();
            }
        }

        public void CheckPassword()
        {
            m_bIsCheckPasswordInProgress = true;
            m_strName = nameInputField.text;
            m_strPassword = passwordInputField.text;
            if (string.IsNullOrEmpty(nameInputField.text)) {
                LogLoginFailedEvent ("incorrect name");
                m_bIsCheckPasswordInProgress = false;
                StartCoroutine (ShowIncorrectNameAnim ("Please enter your name!"));
                return;
            }
            if (m_strPassword == correctPassword)
            {
                LogLoginEvent ("manual");
                PlayerPrefs.SetString ("UserName", m_strName);
                PlayerPrefs.SetString ("Password", m_strPassword);
                StartCoroutine (ShowCorrectPasswordAnim());
            }
            else
            {
                LogLoginFailedEvent ("incorrect password");
                m_bIsCheckPasswordInProgress = false;
                StartCoroutine (ShowIncorrectPasswordAnim("Incorrect Password. Please try again!"));
            }
        }

        private IEnumerator ShowCorrectPasswordAnim()
        {
            float timer = 0f;
            Color toCol = Color.green;
            Color fromCol = m_refPasswordInputFieldBorder.color;
            float speed = 3f;
            feedbackText.text = "Correct Password!";
            feedbackText.color = new Color(0, 0, 0, 0);
            Color textFromCol = feedbackText.color;
            Color textToCol = Color.green;
            textToCol.a = 1f;
            do
            {
                m_refNameInputFieldBorder.color = Color.Lerp(fromCol, toCol, timer);
                m_refPasswordInputFieldBorder.color = Color.Lerp(fromCol, toCol, timer);
                feedbackText.color = Color.Lerp(textFromCol, textToCol, timer);
                timer += Time.deltaTime * speed;
                yield return new WaitForEndOfFrame();
            } while (timer <= 1f);
            OnCorrectPasswordEntered?.Invoke();
        }

        private void LogLoginEvent (string a_strSource) {
            Value val = new Value ();
            val.Add ("username", m_strName);
            val.Add ("source", a_strSource);
            Mixpanel.Track ("LoginSuccessful", val);
        }

        private void LogLoginFailedEvent (string a_strReason) {
            Value val = new Value ();
            val.Add ("fail_reason", a_strReason);
            Mixpanel.Track ("LoginFailed", val);
        }

        private IEnumerator ShowIncorrectPasswordAnim(string a_strErrorValue)
        {
            float timer = 0f;
            Color toCol = Helper.GetColorFromHex("#E83811");
            Color fromCol = m_refPasswordInputFieldBorder.color;
            float speed = 3f;
            feedbackText.text = a_strErrorValue;
            feedbackText.color = new Color(0, 0, 0, 0);
            Color textFromCol = feedbackText.color;
            Color textToCol = Helper.GetColorFromHex("#E83811");
            textToCol.a = 1f;
            do
            {
                m_refPasswordInputFieldBorder.color = Color.Lerp(fromCol, toCol, timer);
                feedbackText.color = Color.Lerp(textFromCol, textToCol, timer);
                timer += Time.deltaTime * speed;
                yield return new WaitForEndOfFrame();
            } while (timer <= 1f);
            yield return new WaitForSeconds(2f);
            yield return ResetPasswordColors();
        }

        private IEnumerator ShowIncorrectNameAnim (string a_strErrorValue) {
            float timer = 0f;
            Color toCol = Helper.GetColorFromHex ("#E83811");
            Color fromCol = m_refNameInputFieldBorder.color;
            float speed = 3f;
            feedbackText.text = a_strErrorValue;
            feedbackText.color = new Color (0, 0, 0, 0);
            Color textFromCol = feedbackText.color;
            Color textToCol = Helper.GetColorFromHex ("#E83811");
            textToCol.a = 1f;
            do {
                m_refNameInputFieldBorder.color = Color.Lerp (fromCol, toCol, timer);
                feedbackText.color = Color.Lerp (textFromCol, textToCol, timer);
                timer += Time.deltaTime * speed;
                yield return new WaitForEndOfFrame ();
            } while (timer <= 1f);
            yield return new WaitForSeconds (2f);
            yield return ResetNameColors ();
        }

        private IEnumerator ResetNameColors () {
            float timer = 0f;
            Color toCol = Color.white;
            Color fromCol = m_refNameInputFieldBorder.color;
            float speed = 3f;
            Color textFromCol = feedbackText.color;
            Color textToCol = new Color (0, 0, 0, 0);
            textToCol.a = 1f;
            do {
                m_refNameInputFieldBorder.color = Color.Lerp (fromCol, toCol, timer);
                feedbackText.color = Color.Lerp (textFromCol, textToCol, timer);
                timer += Time.deltaTime * speed;
                yield return new WaitForEndOfFrame ();
            } while (timer <= 1f);
            feedbackText.text = "";
            feedbackText.color = new Color (0, 0, 0, 0);
        }

        private IEnumerator ResetPasswordColors()
        {
            float timer = 0f;
            Color toCol = Color.white;
            Color fromCol = m_refPasswordInputFieldBorder.color;
            float speed = 3f;
            Color textFromCol = feedbackText.color;
            Color textToCol = new Color(0, 0, 0, 0);
            textToCol.a = 1f;
            do
            {
                m_refPasswordInputFieldBorder.color = Color.Lerp(fromCol, toCol, timer);
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