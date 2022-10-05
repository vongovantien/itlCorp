﻿using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.DL.Models.Criteria;
using eFMS.API.Documentation.Service.Models;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using System;
using System.Collections.Generic;
using System.Linq;

namespace eFMS.API.Documentation.DL.IService
{
    public interface ICsMawbcontainerService : IRepositoryBase<CsMawbcontainer, CsMawbcontainerModel>
    {
        IQueryable<CsMawbcontainerModel> Query(CsMawbcontainerCriteria criteria);
        HandleState Update(List<CsMawbcontainerModel> list, Guid? masterId, Guid? housebillId);
        List<object> ListContOfHB(Guid JobId);
        HandleState Importcontainer(List<CsMawbcontainerImportModel> data);
        List<CsMawbcontainerImportModel> CheckValidContainerImport(List<CsMawbcontainerImportModel> list, Guid? mblid, Guid? hblid);
        List<CsMawbcontainerImportModel> CheckValidGoodsImport(List<CsMawbcontainerImportModel> list, Guid? mblid, Guid? hblid);
        HandleState ValidateContainerList(List<CsMawbcontainerModel> csMawbcontainers, Guid? mblId, Guid? hblId);
        HandleState UpdateMasterBill(List<CsMawbcontainerModel> containers, Guid masterId);
        HandleState UpdateHouseBill(List<CsMawbcontainerModel> containers, Guid housebillId);
    }
}
