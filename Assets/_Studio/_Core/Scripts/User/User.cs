using System;

namespace Terra.Studio
{
    public class User
    {
        public string UserName => userName;
        public string ProjectName => projectName;
        public string ProjectId => projectId;

        private const string CORRECT_PASSWORD = "Studio@12345";
        private const string USER_NAME_PREF = "UserName";
        private const string PASSWORD_PREF = "Password";

        private string userName;
        private string password;
        private string projectId;
        private string projectName;

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
            new GetProjectDetailsAPI(true).DoRequest(onProjectDetailsReceived);
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
    }
}