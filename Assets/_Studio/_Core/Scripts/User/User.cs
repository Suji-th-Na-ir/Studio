using System;
using mixpanel;
using PlayShifu.Terra;

namespace Terra.Studio
{
    public class User
    {
        public string UserId => userId;
        public string ProjectName => projectName;
        public string ProjectId => projectId;

        private const string CORRECT_PASSWORD = "Studio@12345";
        private const string USER_NAME_PREF = "UserName";
        private const string USER_ID_PREF = "UserId";
        private const string PASSWORD_PREF = "Password";

        private string userName;
        private string userId;
        private string password;
        private string projectId;
        private string projectName;

        public User()
        {
            USER_NAME_PREF.TryGetPrefString(out userName);
            USER_ID_PREF.TryGetPrefString(out userId);
            PASSWORD_PREF.TryGetPrefString(out password);
            projectName = SystemOp.Resolve<System>().ConfigSO.SceneDataToLoad.name;
        }

        public User UpdateUserName(string userName)
        {
            this.userName = userName;
            USER_NAME_PREF.SetPref(userName);
            return this;
        }

        public User UpdateUserId(string userId)
        {
            this.userId = userId;
            USER_ID_PREF.SetPref(userName);
            return this;
        }

        public User UpdatePassword(string password)
        {
            this.password = password;
            PASSWORD_PREF.SetPref(userName);
            return this;
        }

        public User UpdateProjectId(string projectId)
        {
            this.projectId = projectId;
            return this;
        }

        public User UpdateProjectName(string projectName)
        {
            this.projectName = projectName;
            return this;
        }

        public void AttempLocalLogin(Action<bool> isLoginSuccessful)
        {
            var isSuccessful = password.Equals(CORRECT_PASSWORD);
            if (!isSuccessful)
            {
                isLoginSuccessful?.Invoke(false);
                return;
            }
            USER_NAME_PREF.SetPref(userName);
            PASSWORD_PREF.SetPref(password);
            isLoginSuccessful?.Invoke(true);
        }

        public void AttemptCloudLogin(Action<bool> isCloudLoginSuccessful, Action<bool, string> onCloudLoginResponseReceived = null)
        {
            var shouldDoCloudLogin = SystemOp.Resolve<System>().ConfigSO.DoCloudLogin;
            if (!shouldDoCloudLogin)
            {
                isCloudLoginSuccessful?.Invoke(true);
                return;
            }
            new LoginAPI(userName, password).DoRequest((status, response) =>
            {
                isCloudLoginSuccessful?.Invoke(status);
                onCloudLoginResponseReceived?.Invoke(status, response);
            });
        }

        public void GetProjectDetails(Action<bool, string> onProjectDetailsReceived)
        {
            new GetProjectDetailsAPI().DoRequest(onProjectDetailsReceived);
        }

        public void CreateNewProject(Action<bool, string> onProjectCreated)
        {
            new CreateProjectAPI(projectName).DoRequest(onProjectCreated);
        }

        public void UploadSaveDataToCloud(string data, Action<bool, string> onProjectSaved)
        {
            new SaveProjectAPI(data).DoRequest(onProjectSaved);
        }

        public void PublishProject(Action<bool, string> onPublished)
        {
            new PublishProjectAPI().DoRequest(onPublished);
        }

        public void LogLoginEvent(bool isAutoLoggedIn)
        {
            if (Helper.IsInUnityEditor()) return;
            var source = isAutoLoggedIn ? "auto" : "manual";
            var val = new Value()
            {
                { "user_name", userName },
                { "source", source }
            };
            Mixpanel.Track("LoginSuccessful", val);
            Mixpanel.Register("UserName", userName);
            Mixpanel.People.Set("UserName", userName);
        }
    }
}