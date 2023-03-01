namespace eFMS.API.Provider.Infrasture.API.Catalogue
{
    public static class CatCountryAPI
    {
        public static string GetAll(string baseUri) => $"{baseUri}/CatCountry/getAll";
    }
}
