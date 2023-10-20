using System;

namespace Terra.Studio
{
    public class User
    {
        public string UserName { get { return userName; } }
        public string ProjectName { get { return projectName; } }

        private const string CORRECT_PASSWORD = "Studio@12345";
        private const string USER_NAME_PREF = "UserName";
        private const string PASSWORD_PREF = "Password";

        private string userName;
        private string password;
        private readonly string projectName;

        public User()
        {
            USER_NAME_PREF.TryGetPrefString(out userName);
            PASSWORD_PREF.TryGetPrefString(out password);
            projectName = SystemOp.Resolve<System>().ConfigSO.SceneDataToLoad.name;
        }

        public User UpdateUserName(string userName)
        {
            this.userName = userName;
            return this;
        }

        public User UpdatePassword(string password)
        {
            this.password = password;
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

        public void AttemptCloudLogin(Action<bool> isCloudLoginSuccessful)
        {
            var shouldDoCloudLogin = SystemOp.Resolve<System>().ConfigSO.DoCloudLogin;
            if (!shouldDoCloudLogin)
            {
                isCloudLoginSuccessful?.Invoke(true);
                return;
            }
            new LoginAPI(userName).DoRequest((status, response) =>
            {
                isCloudLoginSuccessful?.Invoke(status);
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
    }
}