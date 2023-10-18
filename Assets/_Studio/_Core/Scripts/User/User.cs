using System;

namespace Terra.Studio
{
    public class User
    {
        private const string CORRECT_PASSWORD = "Studio@12345";
        private const string USER_NAME_PREF = "UserName";
        private const string PASSWORD_PREF = "Password";

        private string userName;
        private string password;

        public User()
        {
            USER_NAME_PREF.TryGetPrefString(out userName);
            PASSWORD_PREF.TryGetPrefString(out password);
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

        public void Login(Action<bool> isLoginSuccessful)
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
    }
}
