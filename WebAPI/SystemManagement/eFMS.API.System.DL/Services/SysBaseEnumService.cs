using AutoMapper;
using ITL.NetCore.Common;
using ITL.NetCore.Connection;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using SystemManagement.DL.Models;
using SystemManagement.DL.Models.Views;
using SystemManagementAPI.Service.Models;

namespace SystemManagement.DL.Services
{
    public class SysBaseEnumService : RepositoryBase<SysBaseEnum, SysBaseEnumModel>, ISysBaseEnumService
    {
        ISysBaseEnumDetailService _SysBaseEnumDetail;
        public SysBaseEnumService(IContextBase<SysBaseEnum> repository, ISysBaseEnumDetailService SysBaseEnumDetail,
            IMapper mapper) : base(repository, mapper)
        {
            _SysBaseEnumDetail = SysBaseEnumDetail;
        }
        public List<SysBaseEnumModel> GetSysBaseEnums()
        {
            List<SysBaseEnum>  sysBaseEnums= DataContext.Get().ToList();
            List<SysBaseEnumDetail> sysBaseEnumDetails = new List<SysBaseEnumDetail>();
            List<SysBaseEnumModel> rs = new List<SysBaseEnumModel>();
            
            sysBaseEnums.ForEach(i =>
                _SysBaseEnumDetail.Get(u => u.BaseEnumKey == i.Key).OrderBy(x => x.Stt).ToList().ForEach(d =>
                    i.SysBaseEnumDetail.Add(mapper.Map<SysBaseEnumDetailModel, SysBaseEnumDetail>(d))
                    )
                );
            sysBaseEnums.ForEach(i =>
                rs.Add(mapper.Map<SysBaseEnum, SysBaseEnumModel>(i))
            );
            return rs;
        }
        public HandleState PostSysBaseEnums(SysBaseEnumModel sysBaseEnumModel)
        {
            var rs=Add(sysBaseEnumModel);
            sysBaseEnumModel.SysBaseEnumDetail.ToList().ForEach(i =>
                _SysBaseEnumDetail.Add(mapper.Map<SysBaseEnumDetail, SysBaseEnumDetailModel>(i))
            );
            return rs;
        }
        public HandleState PutSysBaseEnums(SysBaseEnumModel sysBaseEnumModel, Expression<Func<SysBaseEnumModel, bool>> predicate)
        {
            var rs = Update(sysBaseEnumModel, predicate);
            //sysBaseEnumModel.SysBaseEnumDetail.ToList().ForEach(i =>
            //    rs=_SysBaseEnumDetail.Update(mapper.Map<SysBaseEnumDetail, SysBaseEnumDetailModel>(i), d => d.BaseEnumKey == i.BaseEnumKey && d.Id == i.Id)
            //);
            return rs;
        }
        public override string ToString()
        {
            return base.ToString();
        }
        public Dictionary<int, string> Authorization()
        {
            Dictionary<int, string> keyValuePairs = new Dictionary<int, string>();
            DNTDataContext dC = (DNTDataContext)DataContext.DC;
            dC.SysAuthorization.ToList().GroupBy(i => new { i.Id, i.Description }).Select(sl => sl.FirstOrDefault()).ToList().ForEach(
                i =>
                {
                    keyValuePairs.Add(i.Id, i.Description);
                }
                );
            return keyValuePairs;
        }
        public Dictionary<string, string> None()
        {
            Dictionary<string, string> keyValuePairs = new Dictionary<string, string>();
            keyValuePairs.Add("", "None");
            return keyValuePairs;
        }
        public Dictionary<string, string> YesNo()
        {
            Dictionary<string, string> keyValuePairs = new Dictionary<string, string>();
            keyValuePairs.Add("0", "No");
            keyValuePairs.Add("1", "Yes");
            return keyValuePairs;
        }
        public List<vw_datatype> vw_Datatypes()
        {
            return DataContext.DC.GetViewData<vw_datatype>();
        }
        public List<vw_catDistrict> vw_catDistrict()
        {
            return DataContext.DC.GetViewData<vw_catDistrict>();
        }
        public List<vw_catHub> vw_catHub()
        {
            return DataContext.DC.GetViewData<vw_catHub>();
        }
        public List<sp_getBaseEnum> getBaseEnum(string id)
        {
            var parameters = new[]{
            new SqlParameter(){ ParameterName="@id", Value=id },
        };
            return DataContext.DC.ExecuteProcedure<sp_getBaseEnum>(parameters);
        }
        public async System.Threading.Tasks.Task<object> databaseInfo()
        {
            
            var rs= await DataContext.DC.ExecuteScalarAsync("SELECT DB_NAME() AS [Current Database];  ",System.Data.CommandType.Text);
            return rs;

        }

        public SysBaseEnumModel getFirst(string id)
        {
            SysBaseEnum rs = DataContext.First(i=>i.Key==id);
            _SysBaseEnumDetail.Get(i => i.BaseEnumKey == rs.Key).ToList().ForEach(
                u =>
                {
                    rs.SysBaseEnumDetail.Add(mapper.Map<SysBaseEnumDetailModel, SysBaseEnumDetail>(u));
                }
                );
            return mapper.Map<SysBaseEnum, SysBaseEnumModel>(rs);

        }
    }
}
