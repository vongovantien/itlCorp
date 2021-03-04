using AutoMapper;
using eFMS.API.Common.Globals;
using eFMS.API.Common.Helpers;
using eFMS.API.Common.Models;
using eFMS.API.Infrastructure.Extensions;
using eFMS.API.Operation.DL.Helper;
using eFMS.API.Operation.DL.IService;
using eFMS.API.Operation.DL.Models;
using eFMS.API.Operation.DL.Models.Criteria;
using eFMS.API.Operation.DL.Models.Ecus;
using eFMS.API.Operation.Service.Models;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.Caching;
using ITL.NetCore.Connection.EF;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace eFMS.API.Operation.DL.Services
{
    public class EcusConnectionService : RepositoryBaseCache<SetEcusconnection, SetEcusConnectionModel>, IEcusConnectionService
    {
        private readonly IContextBase<SysUser> userRepository;
        private readonly IContextBase<SysEmployee> employeeRepository;
        private readonly ICurrentUser currentUser;

        public EcusConnectionService(IContextBase<SetEcusconnection> repository, 
            ICacheServiceBase<SetEcusconnection> cacheService, 
            IMapper mapper,
            ICurrentUser user,
            IContextBase<SysUser> userRepo,
            IContextBase<SysEmployee> employeeRepo) : base(repository, cacheService, mapper)
        {
            userRepository = userRepo;
            employeeRepository = employeeRepo;
            currentUser = user;
        }

        public SetEcusConnectionModel GetConnectionDetails(int id)
        {
            ICurrentUser _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.settingEcusConnection);
            var permissionRangeWrite = PermissionExtention.GetPermissionRange(currentUser.UserMenuPermission.Write);

            var data = Get(x => x.Id == id);
            if (data == null) return null;

            var result = data.FirstOrDefault();

            var users = userRepository.Get();

            result.Username = users.FirstOrDefault(x => x.Id == result.UserId)?.Username;
            result.UserCreatedName = users.FirstOrDefault(x => x.Id == result.UserCreated)?.Username;
            result.UserModifiedName = users.FirstOrDefault(x => x.Id == result.UserModified)?.Username;

            BaseUpdateModel baseModel = new BaseUpdateModel
            {
                UserCreated = result.UserCreated,
                CompanyId = result.CompanyId,
                DepartmentId = result.DepartmentId,
                OfficeId = result.OfficeId,
                GroupId = result.GroupId
            };
            result.Permission = new PermissionAllowBase
            {
                AllowUpdate = PermissionExtention.GetPermissionDetail(permissionRangeWrite, baseModel, currentUser),
            };

            return result;
        }

        public List<SetEcusConnectionModel> GetConnections()
        {
            List<SetEcusConnectionModel> returnList = new List<SetEcusConnectionModel>();
            var cons = Get().ToList();
            var users = userRepository.Get();
            var employees = employeeRepository.Get();
            var query = (from con in cons
                         join user in users on con.UserId equals user.Id
                         join em in employees on user.EmployeeId equals em.Id into grpEmployees
                         from employee in grpEmployees.DefaultIfEmpty()
                         select new { con, user.Username, employee?.EmployeeNameEn }
                );
            if (query == null)
            {
                return returnList;
            }
            foreach (var item in query)
            {
                SetEcusConnectionModel ecus = item.con;
                ecus.Username = item.Username;
                ecus.Fullname = item.EmployeeNameEn;
                returnList.Add(ecus);
            }
            return returnList;
        }

        public IQueryable<SetEcusConnectionModel> Paging(SetEcusConnectionCriteria criteria, int pageNumber, int pageSize, out int totalItems)
        {
            ICurrentUser _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.settingEcusConnection);
            var rangeSearch = PermissionExtention.GetPermissionRange(currentUser.UserMenuPermission.List);

            if(rangeSearch == PermissionRange.None)
            {
                totalItems = 0;
                return null;
            }

            IQueryable<SetEcusConnectionModel> list = QueryPermission(criteria, rangeSearch);

            if (list == null)
            {
                totalItems = 0;
                return null;
            }
            totalItems = list.Count();
            if (pageSize > 1)
            {
                if (pageNumber < 1)
                {
                    pageNumber = 1;
                }
                list = list.Skip((pageNumber - 1) * pageSize).Take(pageSize).OrderByDescending(x => x.DatetimeModified);
            }
            return list;

        }

        private IQueryable<SetEcusConnectionModel> Query(SetEcusConnectionCriteria criteria)
        {
            IQueryable<SetEcusConnectionModel> results = null;
            var list = GetConnections();
            if (string.IsNullOrEmpty(criteria.All))
            {
                results = list.Where(x => (x.Username ?? "").IndexOf(criteria.Username ?? "", StringComparison.OrdinalIgnoreCase) > -1
                    && (x.Name ?? "").IndexOf(criteria.Name ?? "", StringComparison.OrdinalIgnoreCase) > -1
                    && (x.ServerName ?? "").IndexOf(criteria.ServerName ?? "", StringComparison.OrdinalIgnoreCase) > -1
                    && (x.Dbname ?? "").IndexOf(criteria.Dbname ?? "", StringComparison.OrdinalIgnoreCase) > -1
                    && (x.Fullname ?? "").IndexOf(criteria.Fullname ?? "", StringComparison.OrdinalIgnoreCase) > -1
                    )?.AsQueryable();
            }
            else
            {
                results = list.Where(x => (x.Username ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1
                     || (x.Name ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1
                     || (x.ServerName ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1
                     || (x.Dbname ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1
                     || (x.Fullname ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1
                    )?.AsQueryable();
            }
            return results;
        }

        private IQueryable<SetEcusConnectionModel> QueryPermission(SetEcusConnectionCriteria criteria, PermissionRange range)
        {
            var list = Query(criteria);
            IQueryable<SetEcusConnectionModel> data = null;
            if (list == null)
            {
                return list;
            }
            switch (range)
            {
                case PermissionRange.Owner:
                    data = list.Where(x => x.UserCreated == currentUser.UserID);
                    break;
                case PermissionRange.Group:
                    data = list.Where(x => x.UserCreated == currentUser.UserID
                    || x.GroupId == currentUser.GroupId
                    && x.DepartmentId == currentUser.DepartmentId
                    && x.OfficeId == currentUser.OfficeID
                    && x.CompanyId == currentUser.CompanyID);
                    break;
                case PermissionRange.Department:
                    data = list.Where(x => x.UserCreated == currentUser.UserID || x.DepartmentId == currentUser.DepartmentId && x.OfficeId == currentUser.OfficeID
                    && x.CompanyId == currentUser.CompanyID);
                    break;
                case PermissionRange.Office:
                    data = list.Where(x => x.UserCreated == currentUser.UserID || x.OfficeId == currentUser.OfficeID && x.CompanyId == currentUser.CompanyID);
                    break;
                case PermissionRange.Company:
                    data = list.Where(x => x.UserCreated == currentUser.UserID || x.CompanyId == currentUser.CompanyID);
                    break;
                case PermissionRange.All:
                    data = list;
                    break;
            }
            return data;
        }

        public List<DTOKHAIMD> GetDataEcusByUser(string userId, string serverName, string dbusername, string dbpassword, string database)
        {
            List<DTOKHAIMD> results = null;
            if (DataContext.Any(x => x.UserId == userId))
            {
                results = GetDataFromEcus(serverName, dbusername, dbpassword, database);
            }
            return results;
        }

        private List<DTOKHAIMD> GetDataFromEcus(string serverName, string dbusername, string dbpassword, string database)
        {
            string queryString = @"DECLARE @eoMonth DATETIME = DATEADD(MONTH, 1, DATEADD(DAY, -(DAY(GETDATE())), GETDATE()))
                                   SELECT TOP (1000) DTOKHAIMD.[_DToKhaiMDID] AS DToKhaiMDID
                                  ,[_XorN] AS XorN
                                  ,[SOTK]
                                  ,[SOTK_DAU_TIEN]
                                  ,[NGAY_DK]
                                  ,[MA_DV]
                                  ,[VAN_DON]
                                  ,[MA_CK]
                                  ,[MA_CANGNN]
                                  ,[NUOC_XK]
                                  ,[NUOC_NK]
                                  ,[TR_LUONG]
                                  ,[SO_KIEN]
                                  ,[DVT_KIEN]
                                  ,[SO_CONTAINER]
                                  ,[PLUONG]
	                              ,DTOKHAIMD_VNACCS2.[MA_HIEU_PTVC]
                                  ,DV_DT
                                  ,_Ten_DV_L1
                              FROM " + database + @".[dbo].[DTOKHAIMD]
                                    INNER JOIN " + database + @".[dbo].[DTOKHAIMD_VNACCS2]
                                    ON DTOKHAIMD._DToKhaiMDID = DTOKHAIMD_VNACCS2._DTOKHAIMDID
                              WHERE DATEADD(MONTH, -3, @eoMonth) < NGAY_DK AND NGAY_DK <= @eoMonth
                              ORDER by NGAY_DK DESC";

            string connectionString = @"Server=" + serverName + ",1433; Database=" + database + "; User ID=" + dbusername + "; Password=" + dbpassword;
            try
            {
                var data = Helper.Helper.ExecuteDataSet(connectionString, queryString);
                if (data != null)
                {
                    DataTable dt = data.Tables[0];
                    return dt.DataTableToList<DTOKHAIMD>();
                }
            }
            catch (Exception ex)
            {
                string logErr = String.Format("Lỗi query Ecus {0} {1} {2} \n {3}", serverName, dbusername, database, ex.ToString());
                new LogHelper("ECUS", logErr);
                return null;
            }
            return null;
        }

        public bool CheckAllowPermissionAction(int id, PermissionRange range)
        {
            var detail = DataContext.Get(x => x.Id == id)?.FirstOrDefault();
            if (detail == null) return false;

            BaseUpdateModel baseModel = new BaseUpdateModel
            {
                UserCreated = detail.UserCreated,
                CompanyId = detail.CompanyId,
                DepartmentId = detail.DepartmentId,
                OfficeId = detail.OfficeId,
                GroupId = detail.GroupId
            };
            int code = PermissionExtention.GetPermissionCommonItem(baseModel, range, currentUser);

            if (code == 403) return false;

            return true;
        }
    }
}
