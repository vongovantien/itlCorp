﻿namespace eFMS.API.Provider.Infrasture.API.Catalogue
{
    public static class CatStageAPI
    {
        public static string GetAll(string baseUri) => $"{baseUri}/CatStage/GetAll";
        public static string Get(string baseUri) => $"{baseUri}/CatStage/query";
    }
}
