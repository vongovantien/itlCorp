using AutoMapper;
using eFMS.API.Setting.DL.IService;
using eFMS.API.Setting.DL.Models;
using eFMS.API.Setting.Service.Models;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eFMS.API.Setting.DL.Services
{
    public class TariffService : RepositoryBase<SetTariff, SetTariffModel>, ITariffService
    {
        private readonly ICurrentUser currentUser;
        private readonly IContextBase<SetTariffDetail> setTariffDetailRepo;
        public TariffService(IContextBase<SetTariff> repository, IMapper mapper, ICurrentUser user, IContextBase<SetTariffDetail> setTariffDetail) : base(repository, mapper)
        {
            currentUser = user;
            setTariffDetailRepo = setTariffDetail;
        }

        /// <summary>
        /// * Check tồn tại tariff. Check theo các field: 
        /// - Tariff Name (Không được trùng tên), 
        /// - Effective Date - Expried Date
        /// - Tariff Type, Product Service, Cargo Type,  Service Mode
        /// * Check list tariff detail không được phép trống
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public HandleState CheckExistsDataTariff(TariffModel model)
        {
            try
            {
                if (model == null)
                {
                    return new HandleState("Tariff is not null");
                }
                if (model.setTariff == null || model.setTariffDetails == null)
                {
                    return new HandleState("List tariff detail is not null");
                }

                //Trường hợp Insert (Id of tariff is null or empty)
                if (model.setTariff.Id == Guid.Empty)
                {
                    var tariffNameExists = DataContext.Get(x => x.TariffName == model.setTariff.TariffName).Any();
                    if (tariffNameExists)
                    {
                        return new HandleState("Tariff name already exists");
                    }

                    //Check theo bộ 4
                    var tariff = DataContext.Get(x => x.TariffType == model.setTariff.TariffType
                                                    && x.ProductService == model.setTariff.ProductService
                                                    && x.CargoType == model.setTariff.CargoType
                                                    && x.ServiceMode == model.setTariff.ServiceMode);
                    if (tariff.Any())
                    {
                        //Check nằm trong khoảng EffectiveDate - ExpiredDate
                        tariff = tariff
                            .Where(x => model.setTariff.EffectiveDate.Date >= x.EffectiveDate.Date
                                     && model.setTariff.ExpiredDate.Date <= x.ExpiredDate.Date);
                        if (tariff.Any())
                        {
                            return new HandleState("Tariff already exists");
                        }
                    }
                }
                else //Trường hợp Update (Id of tariff is not null & not empty)
                {
                    var tariffNameExists = DataContext.Get(x => x.Id != model.setTariff.Id
                                                             && x.TariffName == model.setTariff.TariffName).Any();
                    if (tariffNameExists)
                    {
                        return new HandleState("Tariff name already exists");
                    }

                    //Check theo bộ 4
                    var tariff = DataContext.Get(x => x.Id != model.setTariff.Id
                                                    && x.TariffType == model.setTariff.TariffType
                                                    && x.ProductService == model.setTariff.ProductService
                                                    && x.CargoType == model.setTariff.CargoType
                                                    && x.ServiceMode == model.setTariff.ServiceMode);
                    if (tariff.Any())
                    {
                        //Check nằm trong khoảng EffectiveDate - ExpiredDate
                        tariff = tariff
                            .Where(x => model.setTariff.EffectiveDate.Date >= x.EffectiveDate.Date
                                     && model.setTariff.ExpiredDate.Date <= x.ExpiredDate.Date);
                        if (tariff.Any())
                        {
                            return new HandleState("Tariff already exists");
                        }
                    }
                }

                //Check list tariff detail không được phép trống
                if (model.setTariffDetails.Count == 0)
                {
                    return new HandleState("Please add tariff to create new OPS tariff");
                }

                return new HandleState();
            }
            catch (Exception ex)
            {
                return new HandleState(ex.Message);
            }
        }

        /// <summary>
        /// Add tariff & list tariff detail
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public HandleState AddTariff(TariffModel model)
        {
            try
            {
                var checkData = CheckExistsDataTariff(model);
                if (!checkData.Success) return checkData;

                var userCurrent = "admin";//currentUser.UserID;
                var today = DateTime.Now;
                //Insert SetTariff
                var tariff = mapper.Map<SetTariff>(model.setTariff);
                tariff.Id = model.setTariff.Id = Guid.NewGuid();
                tariff.UserCreated = tariff.UserModified = userCurrent;
                tariff.DatetimeCreated = tariff.DatetimeModified = today;
                var hs = DataContext.Add(tariff);
                if (hs.Success)
                {
                    //Insert list SetTariffDetail
                    var tariffDetails = mapper.Map<List<SetTariffDetail>>(model.setTariffDetails);
                    tariffDetails.ForEach(r =>
                    {
                        r.Id = Guid.NewGuid();
                        r.TariffId = tariff.Id;
                        r.UserCreated = r.UserModified = userCurrent;
                        r.DatetimeCreated = r.DatetimeModified = today;
                    });
                    var hs2 = setTariffDetailRepo.Add(tariffDetails);
                }
                return hs;
            }
            catch (Exception ex)
            {
                return new HandleState(ex.Message);
            }
        }

        /// <summary>
        /// Update tariff & list tariff model
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public HandleState UpdateTariff(TariffModel model)
        {
            try
            {
                var userCurrent = "admin";//currentUser.UserID;
                var hs = new HandleState();
                return hs;
            }
            catch (Exception ex)
            {
                return new HandleState(ex.Message);
            }
        }

        /// <summary>
        /// Delete tariff & list tariff model
        /// </summary>
        /// <param name="idTariff"></param>
        /// <returns></returns>
        public HandleState DeleteTariff(Guid idTariff)
        {
            try
            {
                var userCurrent = "admin";//currentUser.UserID;
                var hs = new HandleState();
                return hs;
            }
            catch (Exception ex)
            {
                return new HandleState(ex.Message);
            }
        }
    }
}
