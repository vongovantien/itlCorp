namespace DbHelper
{
    public class DbHelper
    {
        public static string ConnectionString { get; set; }
        public static string MongoDBConnectionString { get; set; }
        public static string AWSS3BucketName { get; set; }
        public static string AWSS3AccessKeyId { get; set; }
        public static string AWSS3SecretAccessKey { get; set; }
        public static string AWSS3DomainApi { get; set; }
    }
}