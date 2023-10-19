using System;
using System.Collections.Generic;

namespace Terra.Studio
{
    [Serializable]
    public class APIResponse
    {
        public int status;
        public string message;
        public string data;
    }

    public abstract class StudioAPI
    {
        protected const char PATH_SEPERATOR = '/';
        protected const char ARGUMENT_SEPERATOR = '?';
        protected const string DATA_KEY = "data";
        protected const string USER_NAME_KEY = "username";
        protected const string TIME_STAMP_KEY = "timestamp";
        protected const string PROJECT_NAME_KEY = "projectname";

        public abstract RequestType RequestType { get; }
        public virtual string Parameters { get; protected set; }
        public virtual Dictionary<string, string> FormData { get; protected set; }
        public virtual string URL => GetURL();

        protected abstract string Route { get; }
        protected const string DOMAIN = "http://game-assets-api.letsterra.com/studio";

        protected virtual string GetURL()
        {
            var url = string.Concat(DOMAIN, PATH_SEPERATOR, Route);
            if (!string.IsNullOrEmpty(Parameters))
            {
                url = string.Concat(url, ARGUMENT_SEPERATOR, Parameters);
            }
            UnityEngine.Debug.Log($"Hitting the URL: {url}");
            return url;
        }

        public virtual void DoRequest(Action<bool, string> callback)
        {
            SystemOp.Resolve<NetworkManager>().DoRequest(this, callback);
        }
    }

    public class LoginAPI : StudioAPI
    {
        public override RequestType RequestType => RequestType.Post;
        protected override string Route => "login";

        public LoginAPI(string userName)
        {
            FormData = new()
            {
                { USER_NAME_KEY, userName }
            };
        }
    }

    public class CreateProjectAPI : StudioAPI
    {
        public override RequestType RequestType => RequestType.Post;
        protected override string Route => "project/create";

        public CreateProjectAPI(string projectName)
        {
            var userName = SystemOp.Resolve<User>().UserName;
            FormData = new()
            {
                { USER_NAME_KEY, userName },
                { PROJECT_NAME_KEY, projectName }
            };
        }
    }

    public class GetProjectDetailsAPI : StudioAPI
    {
        [Serializable]
        public struct Data
        {
            public string content;
            public string timestamp;
        }

        public override RequestType RequestType => RequestType.Get;
        protected override string Route => "project/info";

        public GetProjectDetailsAPI()
        {
            var userName = SystemOp.Resolve<User>().UserName;
            var projectName = SystemOp.Resolve<User>().ProjectName;
            Parameters = $"{USER_NAME_KEY}={userName}&{PROJECT_NAME_KEY}={projectName}";
        }
    }

    public class SaveProjectAPI : StudioAPI
    {
        public override RequestType RequestType => RequestType.Post;
        protected override string Route => "project/save";

        public SaveProjectAPI(string data)
        {
            FormData = new()
            {
                { USER_NAME_KEY, SystemOp.Resolve<User>().UserName },
                { PROJECT_NAME_KEY, SystemOp.Resolve<User>().ProjectName },
                { DATA_KEY, data },
                { TIME_STAMP_KEY, SystemOp.Resolve<SaveSystem>().CheckAndGetFreshSaveDateTimeIfLastSavedTimestampNotPresent() }
            };
        }
    }
}