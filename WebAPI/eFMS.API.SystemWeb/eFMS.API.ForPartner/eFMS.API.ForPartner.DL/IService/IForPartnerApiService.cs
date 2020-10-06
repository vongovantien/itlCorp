

namespace eFMS.API.ForPartner.DL.IService
{
    public interface IForPartnerApiService
    {
        bool ValidateApiKey(string apiKey);
        bool ValidateHashString(object body, string apiKey, string hash);
    }
}
