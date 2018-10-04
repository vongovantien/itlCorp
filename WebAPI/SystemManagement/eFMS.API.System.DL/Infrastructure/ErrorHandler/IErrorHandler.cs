namespace eFMS.API.System.DL.Infrastructure.ErrorHandler
{
    public interface IErrorHandler
    {
        string GetMessage(ErrorMessagesEnum message);
    }


    public enum ErrorMessagesEnum
    {
        EntityNull = 1,
        ModelValidation = 2,
        ObjectDoesNotExists = 3,
        AuthWrongCredentials = 4,
        CannotCreate = 5,
        CannotDelete = 6,
        CannotUpdate = 7,
        NotValidInformations = 8,
        CannotRetrieveToken = 9
    }

}
