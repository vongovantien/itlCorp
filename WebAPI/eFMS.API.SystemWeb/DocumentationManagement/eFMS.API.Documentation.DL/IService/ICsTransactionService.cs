﻿using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.DL.Models.Criteria;
using eFMS.API.Documentation.Service.Models;
using eFMS.API.Documentation.Service.ViewModels;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eFMS.API.Documentation.DL.IService
{
    public interface ICsTransactionService : IRepositoryBase<CsTransaction, CsTransactionModel>
    {
        IQueryable<vw_csTransaction> Query(CsTransactionCriteria criteria);
        IQueryable<vw_csTransaction> Paging(CsTransactionCriteria criteria, int page, int size, out int rowsCount);
        HandleState AddCSTransaction(CsTransactionEditModel model);
        HandleState UpdateCSTransaction(CsTransactionEditModel model);
    }
}
