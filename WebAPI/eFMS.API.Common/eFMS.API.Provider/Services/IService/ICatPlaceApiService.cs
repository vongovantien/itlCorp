using eFMS.API.Provider.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace eFMS.API.Provider.Services.IService
{
    public interface ICatPlaceApiService
    {
        Task<List<CatPlaceApiModel>> GetPlaces();
    }
}
