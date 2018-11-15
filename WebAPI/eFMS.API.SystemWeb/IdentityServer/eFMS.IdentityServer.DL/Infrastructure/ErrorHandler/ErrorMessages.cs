using System;

namespace eFMS.API.System.DL.Infrastructure.ErrorHandler
{
    public class ErrorHandler : IErrorHandler
    {
        public string GetMessage(ErrorMessagesEnum message)
        {
            switch (message)
            {
                case ErrorMessagesEnum.EntityNull:
                    return "The entity passed is null {0}. Additional information: {1}";

                case ErrorMessagesEnum.ModelValidation:
                    return "The request data is not correct. Additional information: {0}";

                case ErrorMessagesEnum.ObjectDoesNotExists:
                    return "The object doesn't not exists";

                case ErrorMessagesEnum.CannotCreate:
                    return "Cannot create object";

                case ErrorMessagesEnum.CannotDelete:
                    return "Cannot delete object";

                case ErrorMessagesEnum.CannotUpdate:
                    return "Cannot update object";

                case ErrorMessagesEnum.NotValidInformations:
                    return "Invalid informations";

                case ErrorMessagesEnum.CannotRetrieveToken:
                    return "Cannot retrieve token";
                default:
                    throw new ArgumentOutOfRangeException(nameof(message), message, null);
            }

        }
    }
}
