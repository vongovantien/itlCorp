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
using eFMS.API.Setting.DL.Models.Criteria;

namespace eFMS.API.Setting.DL.Services
{
    public class EcusConnectionService : RepositoryBase<SetEcusconnection, SetEcusConnectionModel>, IEcusConnectionService
    {
        public EcusConnectionService(IContextBase<SetEcusconnection> repository, IMapper mapper) : base(repository, mapper)
        {
        }

        public SetEcusConnectionModel getConnectionDetails(int connection_id)
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
                var user_created = dc.SysUser.Where(x => x.Id == con.UserCreated).FirstOrDefault();
                var user_modified = dc.SysUser.Where(x => x.Id == con.UserModified).FirstOrDefault();
                EcusConnection.username = user?.Username;
                EcusConnection.userCreatedName = user_created?.Username;
                EcusConnection.userModifiedName = user_modified?.Username;
                
            }
            return EcusConnection;
        }

        public List<SetEcusConnectionModel> getConnections()
        {
            List<SetEcusConnectionModel> returnList = new List<SetEcusConnectionModel>();
            eFMSDataContext dc = (eFMSDataContext)DataContext.DC;
            var cons = dc.SetEcusconnection.ToList();
            var query = (from con in cons
                         join user in dc.SysUser on con.UserId equals user.Id into users
                         from u in users
                         join em in dc.SysEmployee on u.EmployeeId equals em.Id

                         select new {con,u,em}
                );
            if (query == null)
            {
                return returnList;
            }
            foreach(var item in query)
            {
                SetEcusConnectionModel ecus = mapper.Map<SetEcusConnectionModel>(item.con);
                ecus.username = item.u.Username;
                ecus.fullname = item.em.EmployeeNameEn;
                returnList.Add(ecus);
            }

            return returnList;
        }

        public List<SetEcusConnectionModel> Paging(SetEcusConnectionCriteria criteria, int page_num, int page_size,out int total_item)
        {
            var list = Query(criteria);
            total_item = list.Count;
            if (page_size > 1)
            {
                if (page_num < 1)
                {
                    page_num = 1;
                }
                list = list.Skip((page_num - 1) * page_size).Take(page_size).ToList();
            }
            return list;
        }

        private List<SetEcusConnectionModel> Query(SetEcusConnectionCriteria criteria)
        {
            List<SetEcusConnectionModel> list = getConnections();
            if (criteria.All == null)
            {
                list = list.Where(x => ((x.username ?? "").Contains(criteria.username ?? "")
                && ((x.Name??"").Contains(criteria.Name??"")
                && ((x.ServerName??"").Contains(criteria.ServerName??""))
                && ((x.Dbname??"").Contains(criteria.Dbname??"")))
                )).ToList();
            }
            else
            {
                list = list.Where(x => ((x.username ?? "").Contains(criteria.All ?? "")
                 || ((x.Name ?? "").Contains(criteria.All ?? "")
                 || ((x.ServerName ?? "").Contains(criteria.All ?? ""))
                 || ((x.Dbname ?? "").Contains(criteria.All ?? "")))
               )).ToList();
            }

            return list;
        }

       
    }
}
