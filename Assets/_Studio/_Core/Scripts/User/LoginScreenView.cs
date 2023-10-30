using System;
using mixpanel;
using UnityEngine;
using UnityEngine.UI;
using PlayShifu.Terra;
using System.Collections;

namespace Terra.Studio
{
    public class LoginScreenView : View
    {
        [SerializeField]
        private Canvas parentCanvas;

        [SerializeField]
        private CanvasGroup m_refCanvasGroup;

        [SerializeField]
        private InputField nameInputField;

        [SerializeField]
        private Image m_refNameInputFieldBorder;

        [SerializeField]
        private InputField passwordInputField;

        [SerializeField]
        private Image m_refPasswordInputFieldBorder;

        [SerializeField]
        private Text feedbackText;

        private string m_strName;
        private string m_strPassword;

        private bool m_bIsCheckPasswordInProgress;

        [SerializeField]
        private GameObject m_refNotLoggedInScreenGO;

        [SerializeField]
        private GameObject m_refLoggingYouInScreenGO;

        public event Action OnLoginSuccessful;

        private void Awake()
        {
            SystemOp.Register(this);
        }

        public override void Init()
        {
            feedbackText.text = string.Empty;
            OnLocalLoginFailed();
            SystemOp
                .Resolve<User>()
                .AttempLocalLogin(
                    (status) =>
                    {
                        if (status)
                        {
                            LogLoginEvent("auto");
                            OnLocalLoginSuccessful();
                        }
                    }
                );
        }

        public override void Flush()
        {
            Destroy(parentCanvas.gameObject);
        }

        private void OnLocalLoginSuccessful()
        {
            m_refLoggingYouInScreenGO.SetActive(true);
            m_refNotLoggedInScreenGO.SetActive(false);
            SystemOp
                .Resolve<User>()
                .AttemptCloudLogin(
                    (status) =>
                    {
                        OnLoginSuccessful?.Invoke();
                    }
                );
        }

        private void OnLocalLoginFailed()
        {
            m_refLoggingYouInScreenGO.SetActive(false);
            m_refNotLoggedInScreenGO.SetActive(true);
        }

        private void OnManualLoginUnsuccessful()
        {
            LogLoginFailedEvent("incorrect password");
            m_bIsCheckPasswordInProgress = false;
            StartCoroutine(ShowIncorrectPasswordAnim("Incorrect Password. Please try again!"));
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
            if (string.IsNullOrEmpty(nameInputField.text))
            {
                LogLoginFailedEvent("incorrect name");
                m_bIsCheckPasswordInProgress = false;
                StartCoroutine(ShowIncorrectNameAnim("Please enter your name!"));
                return;
            }
            SystemOp.Resolve<User>().UpdateUserName(m_strName).UpdatePassword(m_strPassword);
            SystemOp
                .Resolve<User>()
                .AttempLocalLogin(
                    (status) =>
                    {
                        if (status)
                        {
                            LogLoginEvent("manual");
                            OnLocalLoginSuccessful();
                        }
                        else
                        {
                            OnManualLoginUnsuccessful();
                        }
                    }
                );
        }

        private void LogLoginEvent(string a_strSource)
        {
            Value val = new() { { "username", SystemOp.Resolve<User>().UserName }, { "source", a_strSource } };
            Mixpanel.Track("LoginSuccessful", val);
        }

        private void LogLoginFailedEvent(string a_strReason)
        {
            Value val = new() { { "fail_reason", a_strReason } };
            Mixpanel.Track("LoginFailed", val);
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

        private IEnumerator ShowIncorrectNameAnim(string a_strErrorValue)
        {
            float timer = 0f;
            Color toCol = Helper.GetColorFromHex("#E83811");
            Color fromCol = m_refNameInputFieldBorder.color;
            float speed = 3f;
            feedbackText.text = a_strErrorValue;
            feedbackText.color = new Color(0, 0, 0, 0);
            Color textFromCol = feedbackText.color;
            Color textToCol = Helper.GetColorFromHex("#E83811");
            textToCol.a = 1f;
            do
            {
                m_refNameInputFieldBorder.color = Color.Lerp(fromCol, toCol, timer);
                feedbackText.color = Color.Lerp(textFromCol, textToCol, timer);
                timer += Time.deltaTime * speed;
                yield return new WaitForEndOfFrame();
            } while (timer <= 1f);
            yield return new WaitForSeconds(2f);
            yield return ResetNameColors();
        }

        private IEnumerator ResetNameColors()
        {
            float timer = 0f;
            Color toCol = Color.white;
            Color fromCol = m_refNameInputFieldBorder.color;
            float speed = 3f;
            Color textFromCol = feedbackText.color;
            Color textToCol = new Color(0, 0, 0, 0);
            textToCol.a = 1f;
            do
            {
                m_refNameInputFieldBorder.color = Color.Lerp(fromCol, toCol, timer);
                feedbackText.color = Color.Lerp(textFromCol, textToCol, timer);
                timer += Time.deltaTime * speed;
                yield return new WaitForEndOfFrame();
            } while (timer <= 1f);
            feedbackText.text = "";
            feedbackText.color = new Color(0, 0, 0, 0);
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

        private void OnDestroy()
        {
            SystemOp.Unregister(this);
        }
    }
}
