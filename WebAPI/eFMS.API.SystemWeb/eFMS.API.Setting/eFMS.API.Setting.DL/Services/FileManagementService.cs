﻿using AutoMapper;
using eFMS.API.Common.Globals;
using eFMS.API.Setting.DL.IService;
using eFMS.API.Setting.DL.Models;
using eFMS.API.Setting.DL.Models.Criteria;
using eFMS.API.Setting.Service.Models;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace eFMS.API.Setting.DL.Services
{
    public class FileManagementService : RepositoryBase<SysImage, SysImageModel>, IFileManagementService
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly ICurrentUser currentUser;
        private readonly IContextBase<AcctSoa> acctSOARepo;
        private readonly IContextBase<AcctSettlementPayment> acctSettleRepo;
        private readonly IContextBase<AcctAdvancePayment> acctAdvanceRepo;

        public FileManagementService(IStringLocalizer<LanguageSub> localizer, IContextBase<SysImage> repository, IContextBase<AcctSoa> acctSOA, IContextBase<AcctSettlementPayment> accSettle, IContextBase<AcctAdvancePayment> acctAdvance, IMapper mapper, ICurrentUser user) : base(repository, mapper)
        {
            stringLocalizer = localizer;
            currentUser = user;
            acctSOARepo = acctSOA;
            acctSettleRepo = accSettle;
            acctAdvanceRepo = acctAdvance;
        }
        public List<SysImageModel> Get(string folderName, int page, int size, out int rowsCount)
        {
            var data = DataContext.Where(s => s.Folder == folderName).OrderByDescending(s => s.DatetimeModified).ToList().GroupBy(x=>x.ObjectId).Select(x=>x.FirstOrDefault());

            var items = mapper.Map<List<SysImageModel>>(data);

            if (!string.IsNullOrEmpty(folderName))
            {
                switch (folderName)
                {
                    case "SOA":
                        foreach (var item in items)
                        {
                            var folderNames = acctSOARepo.Get().Where(s => s.Id == item.ObjectId).Select(s => s.Soano).FirstOrDefault();
                            item.folderName = folderNames;
                        }
                        break;
                    case "Settlement":
                        foreach (var item in items)
                        {
                            var folderNames = acctSettleRepo.Get().Where(s => s.Id.ToString() == item.ObjectId).Select(s => s.SettlementNo).FirstOrDefault();
                            item.folderName = folderNames;
                        }
                        break;
                    case "Advance":
                        foreach (var item in items)
                        {
                            var folderNames = acctAdvanceRepo.Get().Where(s => s.Id.ToString() == item.ObjectId).Select(s => s.AdvanceNo).FirstOrDefault();
                            item.folderName = folderNames;
                        }
                        break;
                    default:
                        break;
                }
            }
            items = items.Where(s => s.folderName != null).ToList();
            if (items == null)
            {
                rowsCount = 0;
                return null;
            }

            //Phân trang
            var _totalItem = items.ToList().Count();
            rowsCount = (_totalItem > 0) ? _totalItem : 0;
            if (size > 0)
            {
                if (page < 1)
                {
                    page = 1;
                }
                items = items.Skip((page - 1) * size).Take(size).ToList();
            }
            
            
            return items;
        }

        public List<SysImageModel> Search(SysImageCriteria criteria, int page, int size, out int rowsCount)
        {
            var data = DataContext.Get();
            if (!string.IsNullOrEmpty(criteria.Name))
            {
              data = data.Where(x => x.Name.ToLower().Contains(criteria.Name.ToLower()));
            }
            if (!string.IsNullOrEmpty(criteria.ObjectId))
            {
                data = data.Where(x => x.ObjectId == criteria.ObjectId);
            }
            if (!string.IsNullOrEmpty(criteria.Folder))
            {
                data = data.Where(x => x.Folder == criteria.Folder);
            }
            if (criteria.DateType != null)
            {
                if (criteria.FromDate.HasValue && criteria.ToDate.HasValue)
                {
                    switch (criteria.DateType)
                    {
                        case "CreateDate":
                            data = data.Where(x => x.DateTimeCreated.Value.Date >= criteria.FromDate.Value.Date &&
                            x.DateTimeCreated.Value.Date <= criteria.ToDate.Value.Date);
                            break;
                        case "ModifiedDate":
                            data = data.Where(x => x.DatetimeModified.Value.Date >= criteria.FromDate.Value.Date
                            && x.DatetimeModified.Value.Date <= criteria.ToDate.Value.Date);
                            break;
                        default:
                            break;
                    }
                }
            }

            if (data == null)
            {
                rowsCount = 0;
                return null;
            }

            //Phân trang
            var _totalItem = data.Select(s => s.Id).Count();
            rowsCount = (_totalItem > 0) ? _totalItem : 0;
            if (size > 0)
            {
                if (page < 1)
                {
                    page = 1;
                }
                data = data.Skip((page - 1) * size).Take(size);
            }

            data=data.OrderBy(x => x.Name);

            var result = mapper.Map<List<SysImageModel>>(data);

            return result;
        }
    }
}
