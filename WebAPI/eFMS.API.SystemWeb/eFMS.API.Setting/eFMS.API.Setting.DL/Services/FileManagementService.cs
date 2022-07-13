using AutoMapper;
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
        public List<SysImageViewModel> Get(string folderName, List<string> keyWords, int page, int size, out int rowsCount)
        {
            var data = DataContext.Where(s => s.Folder == folderName).OrderByDescending(s => s.DatetimeModified).ToList().GroupBy(x => x.ObjectId).Select(x => x.FirstOrDefault());

            //Phân trang
            var _totalItem = data.Count();
            rowsCount = (_totalItem > 0) ? _totalItem : 0;
            if (size > 0)
            {
                if (page < 1)
                {
                    page = 1;
                }

                var items = mapper.Map<List<SysImageViewModel>>(data);

                if (!string.IsNullOrEmpty(folderName))
                {
                    switch (folderName)
                    {
                        case "SOA":
                            foreach (var item in items)
                            {
                                var folderNames = acctSOARepo.Where(s => s.Id == item.ObjectId).FirstOrDefault();
                                if (folderNames != null)
                                {
                                    item.FolderName = folderNames?.Soano;
                                }
                                else
                                {
                                    continue;
                                }
                            }
                            break;
                        case "Settlement":
                            foreach (var item in items)
                            {
                                var sm = acctSettleRepo.Where(s => s.Id.ToString() == item.ObjectId).FirstOrDefault();
                                if (sm != null)
                                {
                                    item.FolderName = sm?.SettlementNo;
                                }
                                else
                                {
                                    continue;
                                }
                            }
                            break;
                        case "Advance":
                            foreach (var item in items)
                            {
                                var folderNames = acctAdvanceRepo.Where(s => s.Id.ToString() == item.ObjectId).FirstOrDefault();
                                if (folderNames != null)
                                {
                                    item.FolderName = folderNames?.AdvanceNo;
                                }
                                else
                                {
                                    continue;
                                }
                            }
                            break;
                        default:
                            break;
                    }
                }
                items = items.Where(x => x.FolderName != null).ToList();
                if (keyWords.Count > 0)
                {
                    List<SysImageViewModel> results = new List<SysImageViewModel>();
                    foreach (var kw in keyWords)
                    {
                        var item = items.Where(x => x.FolderName.Contains(kw)).OrderByDescending(s => s.FolderName).FirstOrDefault();
                        results.Add(item);
                    }
                    rowsCount = results.Count();
                    return results;
                }

                items = items.Skip((page - 1) * size).Take(size).ToList();


                if (items == null)
                {
                    rowsCount = 0;
                    return null;
                }
                return items;   
            }
            return new List<SysImageViewModel>();
        }
        public List<SysImageViewModel> GetDetail(string folderName, string objectId)
        {
            var data = DataContext.Where(s => s.ObjectId == objectId && s.Folder == folderName).ToList();
            var result = mapper.Map<List<SysImageViewModel>>(data);
            return result;
        }
    }
}
