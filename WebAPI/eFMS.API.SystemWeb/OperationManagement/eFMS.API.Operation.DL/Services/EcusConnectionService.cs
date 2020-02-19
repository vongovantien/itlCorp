using AutoMapper;
using eFMS.API.Operation.DL.Helper;
using eFMS.API.Operation.DL.IService;
using eFMS.API.Operation.DL.Models;
using eFMS.API.Operation.DL.Models.Criteria;
using eFMS.API.Operation.DL.Models.Ecus;
using eFMS.API.Operation.Service.Models;
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
        public EcusConnectionService(IContextBase<SetEcusconnection> repository, 
            ICacheServiceBase<SetEcusconnection> cacheService, 
            IMapper mapper,
            IContextBase<SysUser> userRepo,
            IContextBase<SysEmployee> employeeRepo) : base(repository, cacheService, mapper)
        {
            userRepository = userRepo;
            employeeRepository = employeeRepo;
        }

        public SetEcusConnectionModel GetConnectionDetails(int id)
        {
            var data = Get(x => x.Id == id);
            if (data == null) return null;
            var result = data.FirstOrDefault();
            var users = userRepository.Get();
            result.Username = users.FirstOrDefault(x => x.Id == result.UserId)?.Username;
            result.UserCreatedName = users.FirstOrDefault(x => x.Id == result.UserCreated)?.Username;
            result.UserModifiedName = users.FirstOrDefault(x => x.Id == result.UserModified)?.Username;
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
                         join em in employees on user.EmployeeId equals em.Id

                         select new { con, user, em }
                );
            if (query == null)
            {
                return returnList;
            }
            foreach (var item in query)
            {
                SetEcusConnectionModel ecus = item.con;
                ecus.Username = item.user.Username;
                ecus.Fullname = item.em.EmployeeNameEn;
                returnList.Add(ecus);
            }
            return returnList;
        }

        public IQueryable<SetEcusConnectionModel> Paging(SetEcusConnectionCriteria criteria, int pageNumber, int pageSize, out int totalItems)
        {
            var list = Query(criteria);
            if(list == null)
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
                list = list.Skip((pageNumber - 1) * pageSize).Take(pageSize);
            }
            return list;
        }

        private IQueryable<SetEcusConnectionModel> Query(SetEcusConnectionCriteria criteria)
        {
            IQueryable<SetEcusConnectionModel> results = null;
            var list = GetConnections();
            if (criteria.All == null)
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
            string queryString = @"SELECT TOP (1000) DTOKHAIMD.[_DToKhaiMDID] AS DToKhaiMDID
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
                              FROM " + database + @".[dbo].[DTOKHAIMD]
                                    INNER JOIN " + database + @".[dbo].[DTOKHAIMD_VNACCS2]
                                    ON DTOKHAIMD._DToKhaiMDID = DTOKHAIMD_VNACCS2._DTOKHAIMDID
                              WHERE NAMDK = YEAR(GETDATE()) AND (MONTH(GETDATE()) - MONTH(NGAY_DK)) < 4";

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
                return null;
            }
            return null;
        }
    }
}
