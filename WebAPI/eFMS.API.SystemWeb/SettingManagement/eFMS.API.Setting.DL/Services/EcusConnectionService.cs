using eFMS.API.Setting.DL.Common;
using eFMS.API.Setting.DL.IService;
using eFMS.API.Setting.DL.Models;
using eFMS.API.Setting.Service;
using eFMS.API.Setting.Service.Contexts;
using eFMS.API.Setting.Service.Models;
using ITL.NetCore.Connection.EF;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Linq;
using System.Collections;
using eFMS.API.Setting.Models;
using ITL.NetCore.Connection.BL;
using AutoMapper;
using System.Data.SqlClient;
using System.Data;
using eFMS.API.Setting.DL.Models.Ecus;
using eFMS.API.Setting.DL.Helpers;

namespace eFMS.API.Setting.DL.Services
{
    public class EcusConnectionService : RepositoryBase<SetEcusconnection, SetEcusConnectionModel>, IEcusConnectionService
    {
        public EcusConnectionService(IContextBase<SetEcusconnection> repository, IMapper mapper) : base(repository, mapper)
        {
        }

        public SetEcusConnectionModel GetConnectionDetails(int connection_id)
        {
            eFMSDataContext dc = (eFMSDataContext)DataContext.DC;
            SetEcusConnectionModel EcusConnection = new SetEcusConnectionModel();
            var con = dc.SetEcusconnection.Where(x => x.Id == connection_id).FirstOrDefault();
            if (con == null)
            {
                return null;
            }
            else
            {
                EcusConnection = mapper.Map<SetEcusConnectionModel>(con);
                var user = dc.SysUser.Where(x => x.Id == con.UserId).FirstOrDefault();
                EcusConnection.Username = user?.Username;
                
            }
            return EcusConnection;
        }

        public List<SetEcusConnectionModel> GetConnections()
        {
            List<SetEcusConnectionModel> returnList = new List<SetEcusConnectionModel>();
            eFMSDataContext dc = (eFMSDataContext)DataContext.DC;
            var cons = dc.SetEcusconnection.ToList();
            var query = (from con in cons
                         join user in dc.SysUser on con.UserId equals user.Id
                         select new {con,user}
                );
            if (query == null)
            {
                return returnList;
            }
            foreach(var item in query)
            {
                SetEcusConnectionModel ecus = mapper.Map<SetEcusConnectionModel>(item.con);
                ecus.Username = item.user.Username;
                returnList.Add(ecus);
            }
            return returnList;
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
            string queryString = "SELECT * FROM [ECUS5VNACCS].[dbo].[DTOKHAIMD]";
            string connectionString = @"Server=" + serverName + ",1433; Database=" + database + "; User ID=" + dbusername + "; Password=" + dbpassword;
            var data = Helpers.Helper.ExecuteDataSet(connectionString, queryString);
            if (data != null)
            {
                DataTable dt = data.Tables[0];
                return dt.DataTableToList<DTOKHAIMD>();
            }
            return null;
        }
    }
}
