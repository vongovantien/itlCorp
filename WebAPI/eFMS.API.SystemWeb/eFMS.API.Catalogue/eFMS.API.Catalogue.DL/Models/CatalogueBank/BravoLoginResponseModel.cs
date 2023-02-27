namespace eFMS.API.Catalogue.DL.Models.CatalogueBank
{
    public class BravoLoginResponseModel
    {
        public string TokenKey { get; set; }
        public string Success { get; set; }
        public string Message { get; set; }
    }

    public class BravoLoginModel
    {
        public string UserName { get; set; }
        public string Password { get; set; }
    }
}
