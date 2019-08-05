using AutoMapper;
using eFMS.API.Operation.DL.Helper;
using eFMS.API.Operation.DL.IService;
using eFMS.API.Operation.DL.Models;
using eFMS.API.Operation.DL.Models.Criteria;
using eFMS.API.Operation.DL.Models.Ecus;
using eFMS.API.Operation.Service.Contexts;
using eFMS.API.Operation.Service.Models;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace eFMS.API.Operation.DL.Services
{
    public class EcusConnectionService : RepositoryBase<SetEcusconnection, SetEcusConnectionModel>, IEcusConnectionService
    {
        //private ICatAreaApiService catAreaApi;
        public EcusConnectionService(IContextBase<SetEcusconnection> repository, IMapper mapper) : base(repository, mapper)
        {
            //catAreaApi = apiService;
        }

        public SetEcusConnectionModel GetConnectionDetails(int id)
        {
            eFMSDataContext dc = (eFMSDataContext)DataContext.DC;
            SetEcusConnectionModel EcusConnection = new SetEcusConnectionModel();
            var con = dc.SetEcusconnection.Where(x => x.Id == id).FirstOrDefault();
            if (con == null)
            {
                return null;
            }
            else
            {
                EcusConnection = mapper.Map<SetEcusConnectionModel>(con);
                var user = dc.SysUser.Where(x => x.Id == con.UserId).FirstOrDefault();
                var user_created = dc.SysUser.Where(x => x.Id == con.UserCreated).FirstOrDefault();
                var user_modified = dc.SysUser.Where(x => x.Id == con.UserModified).FirstOrDefault();
                EcusConnection.Username = user?.Username;
                EcusConnection.UserCreatedName = user_created?.Username;
                EcusConnection.UserModifiedName = user_modified?.Username;

            }
            return EcusConnection;
        }

        public List<SetEcusConnectionModel> GetConnections()
        {
            List<SetEcusConnectionModel> returnList = new List<SetEcusConnectionModel>();
            eFMSDataContext dc = (eFMSDataContext)DataContext.DC;
            var cons = DataContext.Get().ToList();
            var query = (from con in cons
                         join user in dc.SysUser on con.UserId equals user.Id into users
                         from u in users
                         join em in dc.SysEmployee on u.EmployeeId equals em.Id

                         select new { con, u, em }
                );
            if (query == null)
            {
                return returnList;
            }
            foreach (var item in query)
            {
                SetEcusConnectionModel ecus = mapper.Map<SetEcusConnectionModel>(item.con);
                ecus.Username = item.u.Username;
                ecus.Fullname = item.em.EmployeeNameEn;
                returnList.Add(ecus);
            }
            return returnList;
        }

        public List<SetEcusConnectionModel> Paging(SetEcusConnectionCriteria criteria, int pageNumber, int pageSize, out int totalItems)
        {
            var list = Query(criteria);
            totalItems = list.Count;
            if (pageSize > 1)
            {
                if (pageNumber < 1)
                {
                    pageNumber = 1;
                }
                list = list.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
            }
            return list;
        }

        private List<SetEcusConnectionModel> Query(SetEcusConnectionCriteria criteria)
        {
            List<SetEcusConnectionModel> list = GetConnections();
            if (criteria.All == null)
            {
                list = list.Where(x => (x.Username ?? "").IndexOf(criteria.Username ?? "", StringComparison.OrdinalIgnoreCase) > -1
                && (x.Name ?? "").IndexOf(criteria.Name ?? "", StringComparison.OrdinalIgnoreCase) > -1
                && (x.ServerName ?? "").IndexOf(criteria.ServerName ?? "", StringComparison.OrdinalIgnoreCase) > -1
                && (x.Dbname ?? "").IndexOf(criteria.Dbname ?? "", StringComparison.OrdinalIgnoreCase) > -1
                && (x.Fullname ?? "").IndexOf(criteria.Fullname ?? "", StringComparison.OrdinalIgnoreCase) > -1
                ).ToList();
            }
            else
            {
                list = list.Where(x => (x.Username ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1
                 || (x.Name ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1
                 || (x.ServerName ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1
                 || (x.Dbname ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1
                 || (x.Fullname ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1
               ).ToList();
            }

            return list;
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
                              FROM[ECUS5VNACCS].[dbo].[DTOKHAIMD]
                                    INNER JOIN[ECUS5VNACCS].[dbo].[DTOKHAIMD_VNACCS2]
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
