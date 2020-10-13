namespace eFMS.API.ForPartner.Infrastructure.Extensions
{
    public class CustomError
    {
        public string Error { get; }

        public CustomError(string message)
        {
            Error = message;
        }
    }
}
