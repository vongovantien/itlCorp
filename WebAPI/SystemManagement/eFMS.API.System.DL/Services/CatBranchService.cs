using AutoMapper;
using ITL.NetCore.Connection;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using System;
using System.Collections.Generic;
using System.Linq;
using SystemManagement.DL.Models;
using SystemManagement.DL.Models.Views;
using eFMS.API.System.Service.Models;

namespace SystemManagement.DL.Services
{
    public class CatBranchService : RepositoryBase<CatBranch, CatBranchModel>, ICatBranchService
    {

        ICatPlaceService _catPlaceService;
        
        public CatBranchService(
            IContextBase<CatBranch> repository, 
            ICatPlaceService catPlaceService, 
            IMapper mapper) : base(repository, mapper)
        {

            _catPlaceService = catPlaceService;
        }
        public List<vw_catBranch> GetByView()
        {
            return DataContext.DC.GetViewData<vw_catBranch>();
        }
        public string GetNewCode()
        {
            var rs= DataContext.DC.ExecuteFuncScalar("fn_GeneratePlaceCode");
            return rs.ToString();
        }
        public Boolean Exist(vw_catBranch vw_CatBranch)
        {
            return DataContext.Get(i => i.BranchId.Equals(vw_CatBranch.BranchID)).Count() > 0 ? true : false;
        }
        //public vw_catBranch save(vw_catBranch dr)
        //{
        //    HandleState State = new HandleState();
        //    if (dr == null)
        //    {
        //        return null;
        //    }

        //    if (dr.IsHub)
        //    {
        //        var DataBusiness = new vw_catBranch();
        //        var otherBranch = DataContext.Get(b =>
        //        (b.IsHub ?? false) && b.HubId.Equals(dr.HubID) && b.BranchId != dr.BranchID
        //        ).FirstOrDefault();

        //        if (otherBranch != null)
        //        {
        //            throw new Exception("Z_BRANCH_WITH_HUB_ROLE_EXISTED");
        //        }
        //    }

        //    //CatPlaceModel PlaceB = new CatPlaceModel();
        //    //CatBranchModel BranchB = new CatBranchModel();
        //    CatPlaceModel drPlace;
        //    CatBranchModel currentBranch;

        //    bool exist = Exist(dr);

        //    if (exist)
        //    {
        //        drPlace = _catPlaceService.First(t => t.Id == dr.BranchID);
        //        currentBranch = First(t => t.BranchId == dr.BranchID);
        //    }
        //    else
        //    {
        //        drPlace = new CatPlaceModel();
        //        currentBranch = new CatBranchModel();
        //    }
        //    drPlace.SetValues4PropertiesFromChildObject(dr, null);
        //    drPlace.DisplayName = dr.Code;
        //    currentBranch.Address = dr.Address;
        //    currentBranch.DistrictId = dr.DistrictID;
        //    currentBranch.ProvinceId = dr.ProvinceID;
        //    currentBranch.HubId = dr.HubID;
        //    currentBranch.IsHub = dr.IsHub;

        //    //Update objects
        //    if (exist)
        //    {
        //        drPlace.DatetimeModified = DateTime.Now;
        //        //drPlace.UserModified = MainModule.CurrentUser.ID;

        //        if (_catPlaceService.Update(drPlace, t => t.Id == drPlace.Id).Success)
        //            State = DataContext.Update(currentBranch, t => t.BranchId == currentBranch.BranchId, true);
        //    }
        //    else//Add new objects
        //    {
        //        Guid newID = Guid.NewGuid();
        //        drPlace.Id = newID;
        //        drPlace.PlaceTypeId = Enum.GetName(typeof(PlaceType), PlaceType.Branch);
        //        drPlace.DatetimeCreated = DataUtils.GetDate();
        //        drPlace.UserCreated = MainModule.CurrentUser.ID;

        //        currentBranch.BranchId = newID;
        //        if (_catPlaceService.Add(drPlace).Success)
        //            State = DataContext.Add(currentBranch, true);
        //    }
        //    if (State.Success)
        //    {
        //        //Add zone code
        //        if (GridUtil.DataMode == DataModeType.AddNew)
        //        {
        //            catZoneCodeBusiness ZoneCodeB = new catZoneCodeBusiness();
        //            ZoneCodeB.Add(new catZoneCode()
        //            {
        //                Code = dr.Code,
        //                DistanceFrom = 0,
        //                DistanceTo = 0,
        //                IsSpecialZone = true,
        //                DatetimeCreated = DataUtils.GetDate(),
        //                UserCreated = MainModule.CurrentUser.ID,
        //                Type = Enum.GetName(typeof(ZoneCodeType), ZoneCodeType.Delivery),
        //                Description = "Deliver cargo at destination branch"
        //            }, true);
        //            ZoneCodeB = new catZoneCodeBusiness();
        //            catZoneCode addedZonceCode = ZoneCodeB.First(t => t.Code == dr.Code);
        //            if (addedZonceCode != null)
        //            {
        //                catDeliveryZoneCodeBusiness DeliveryZoneCodeB = new catDeliveryZoneCodeBusiness();
        //                List<vw_catBranch> lBranch = (from b in DeliveryZoneCodeB.DC.GetTable<vw_catBranch>()
        //                                              where b.BranchID != currentBranch.BranchID
        //                                              select b).ToList();
        //                if (lBranch.Count > 0)
        //                {
        //                    foreach (var b in lBranch)
        //                    {
        //                        DeliveryZoneCodeB.Add(new catDeliveryZoneCode()
        //                        {
        //                            OriginBranchID = b.BranchID,
        //                            ToPlace = currentBranch.BranchID,
        //                            ZoneID = addedZonceCode.ID,
        //                            UserModified = MainModule.CurrentUser.ID,
        //                            DatetimeModified = DataUtils.GetDate(),
        //                            IsRAS = false
        //                        }, true);

        //                        catZoneCode dZone = ZoneCodeB.First(t => t.Code == b.Code);
        //                        if (dZone != null)
        //                        {
        //                            DeliveryZoneCodeB.Add(new catDeliveryZoneCode()
        //                            {
        //                                OriginBranchID = currentBranch.BranchID,
        //                                ToPlace = b.BranchID,
        //                                ZoneID = dZone.ID,
        //                                UserModified = MainModule.CurrentUser.ID,
        //                                DatetimeModified = DataUtils.GetDate(),
        //                                IsRAS = false
        //                            }, true);
        //                        }
        //                    }
        //                }
        //            }
        //        }

        //        SetSuccess(MainModule.LanguageMNMain.Get(LanguageText.Z_SAVE_SUCCESSFULLY));
        //        //Browse();
        //        return;
        //    }
        //    return null;
        //}

    }
    }

