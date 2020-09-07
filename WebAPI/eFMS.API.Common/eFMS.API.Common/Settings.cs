namespace eFMS.API.Common
{
    public class Settings
    {
        public string MongoConnection;
        public string MongoDatabase;
        public string eFMSConnection;
        public string RedisConnection;
    }
    public class WebUrl
    {
        public string Url;
    }
    public class ApiUrl
    {
        public string Url;
    }

    public class AuthenticationSetting
    {
        public string Authority;
        public string RequireHttpsMetadata;
        public string ApiName;
        public string ApiSecret;
        public string ApiKey;
    }
}
