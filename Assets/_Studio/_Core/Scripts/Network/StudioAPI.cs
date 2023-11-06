using System;
using Newtonsoft.Json;
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

    [Serializable]
    public struct ProjectData
    {
        public int ParamValue;
        public string ProjectId;
        public readonly StudioState State => (StudioState)ParamValue;
    }

    public abstract class StudioAPI
    {
        protected const char PATH_SEPERATOR = '/';
        protected const char ARGUMENT_SEPERATOR = '?';
        protected const string DATA_KEY = "data";
        protected const string USER_NAME_KEY = "username";
        protected const string USER_ID_KEY = "userId";
        protected const string TIME_STAMP_KEY = "timestamp";
        protected const string PROJECT_NAME_KEY = "projectname";
        protected const string PASSWORD_NAME_KEY = "password";
        protected const string PROJECT_ID_KEY = "projectId";
        private const int DEFAULT_TIMEOUT = 20;

        public abstract RequestType RequestType { get; }
        public virtual int Timeout => DEFAULT_TIMEOUT;
        public virtual string Parameters { get; protected set; }
        public virtual Dictionary<string, string> FormData { get; protected set; }
        public virtual string URL => GetURL();

        protected abstract string Route { get; }

        protected virtual string DOMAIN => "https://game-assets-api.letsterra.com/studio";

        protected virtual string GetURL()
        {
            var url = string.Concat(DOMAIN, PATH_SEPERATOR, Route);
            if (!string.IsNullOrEmpty(Parameters))
            {
                url = string.Concat(url, ARGUMENT_SEPERATOR, Parameters);
            }
            return url;
        }

        public virtual void DoRequest(Action<bool, string> callback)
        {
            SystemOp.Resolve<NetworkManager>().DoRequest(this, (status, response) =>
            {
                var content = string.Empty;
                if (string.IsNullOrEmpty(response))
                {
                    status = false;
                }
                else
                {
                    try
                    {
                        var unpackedData = JsonConvert.DeserializeObject<APIResponse>(response);
                        status = unpackedData.status == 200;
                        content = unpackedData.data;
                    }
                    catch
                    {
                        status = false;
                    }
                }
                callback?.Invoke(status, content);
            });
        }
    }

    public class LoginAPI : StudioAPI
    {
        [Serializable]
        public struct Data
        {
            public string Username;
            public string ProjectId;
            public string UserId;
            public bool IsAutoLoggedIn;
        }

        public override RequestType RequestType => RequestType.Post;
        protected override string Route => "login";

        public LoginAPI(string userName, string password)
        {
            FormData = new() { { USER_NAME_KEY, userName }, { PASSWORD_NAME_KEY, password } };
        }
    }

    public class CreateProjectAPI : StudioAPI
    {
        public override RequestType RequestType => RequestType.Post;
        protected override string Route => "project/create";

        public CreateProjectAPI(string projectName)
        {
            var userId = SystemOp.Resolve<User>().UserId;
            FormData = new() { { USER_ID_KEY, userId }, { PROJECT_NAME_KEY, projectName } };
        }
    }

    public class GetProjectDetailsAPI : StudioAPI
    {
        [Serializable]
        public struct Data
        {
            public string content;
            public string timestamp;
            public string id;
            public string projectname;
        }

        public override int Timeout => 30;
        public override RequestType RequestType => RequestType.Get;
        protected override string Route => "project/info";

        public GetProjectDetailsAPI()
        {
            var userId = SystemOp.Resolve<User>().UserId;
            var projectId = SystemOp.Resolve<User>().ProjectId;
            Parameters = $"{USER_ID_KEY}={userId}&{PROJECT_ID_KEY}={projectId}";
        }
    }

    public class SaveProjectAPI : StudioAPI
    {
        public override int Timeout => 30;
        public override RequestType RequestType => RequestType.Post;
        protected override string Route => "project/save";

        public SaveProjectAPI(string data)
        {
            var timeStamp = SystemOp
                .Resolve<SaveSystem>()
                .CheckAndGetFreshSaveDateTimeIfLastSavedTimestampNotPresent();
            FormData = new()
            {
                { USER_ID_KEY, SystemOp.Resolve<User>().UserId },
                { PROJECT_ID_KEY, SystemOp.Resolve<User>().ProjectId },
                { DATA_KEY, data },
                { TIME_STAMP_KEY, timeStamp }
            };
        }
    }

    public class GetPublishDataAPI : StudioAPI
    {
        public override RequestType RequestType => RequestType.Get;
        protected override string Route => "published/info";

        public GetPublishDataAPI()
        {
            var projectId = SystemOp.Resolve<User>().ProjectId;
            Parameters = $"{PROJECT_ID_KEY}={projectId}";
        }
    }

    public class PublishProjectAPI : StudioAPI
    {
        public override RequestType RequestType => RequestType.Post;
        protected override string Route => "project/publish";

        public PublishProjectAPI()
        {
            FormData = new()
            {
                { USER_ID_KEY, SystemOp.Resolve<User>().UserId },
                { PROJECT_ID_KEY, SystemOp.Resolve<User>().ProjectId }
            };
        }
    }
    
    public class AssetsWindowAPI : StudioAPI
    {
        protected override string DOMAIN => "https://game-assets-api.letsterra.com";
        public override RequestType RequestType => RequestType.Get;
        protected override string Route => "getAllModels?type=assets_gltf";
    }
}
