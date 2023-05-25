using eFMS.API.Report.DL.IService;
using eFMS.API.Report.DL.Models;
using eFMS.API.Report.Service.Models;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Connection;
using ITL.NetCore.Connection.EF;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace eFMS.API.Report.DL.Services
{
    public class EDocReportService: IEDocReportService
    {
        private readonly IContextBase<OpsTransaction> opsRepository;
        private readonly IContextBase<CsTransactionDetail> detailRepository;
        private readonly IContextBase<SysImageDetail> imageDetailRepository;
        private readonly IContextBase<SysAttachFileTemplate> sysattachRepository;
        private readonly IContextBase<CsTransaction> tranRepository;
        private readonly IContextBase<CatPartner> catPartnerRepo;
        private readonly IContextBase<CustomsDeclaration> customsDeclarationRepo;
        private readonly IContextBase<SysUser> sysUserRepo;
        private readonly ICurrentUser currentUser;
        private eFMSDataContextDefault DC => (eFMSDataContextDefault)opsRepository.DC;

        public EDocReportService(
              IContextBase<OpsTransaction> ops,
            IContextBase<CsTransactionDetail> detail,
                IContextBase<SysImageDetail> imageDetailRepo,
            IContextBase<CsTransaction> tranRepo,
            IContextBase<SysAttachFileTemplate> sysattachRepo,
             IContextBase<CatPartner> catPartner,
            ICurrentUser user,
            IContextBase<SysUser> sysUser,
            IContextBase<CustomsDeclaration> customsDeclarationRepository
            )
        {
            opsRepository = ops;
            detailRepository = detail;
            catPartnerRepo = catPartner;
            currentUser = user;
            sysUserRepo = sysUser;
            imageDetailRepository = imageDetailRepo;
            tranRepository = tranRepo;
            sysattachRepository = sysattachRepo;
            customsDeclarationRepo = customsDeclarationRepository;
        }
        public List<EDocReportResult> QueryDataEDocsReport(GeneralReportCriteria criteria)
        {
            var dataDocumentation = EdocReportDocumentation(criteria);
            return dataDocumentation.ToList();
        }

        private IQueryable<EDocReportResult> EdocReportDocumentation(GeneralReportCriteria criteria)
        {
            var dataShipment = GetDataGeneralReport(criteria);
            var listjob = dataShipment.GroupBy(x => x.JobNo).Select(x => x.FirstOrDefault().JobNo);
            var lstOPS = listjob.Where(x => (x.Contains("LOG")||x.Contains("TKI")) && !x.Contains("RLOG")).ToList();
            var lstCS = listjob.Where(x => !x.Contains("LOG")).ToList();
            var jobOps = opsRepository.Get(x => lstOPS.Contains(x.JobNo));
            var jobCs = tranRepository.Get(x => lstCS.Contains(x.JobNo));
            var jobDTCs = detailRepository.Get();
            var edoc = imageDetailRepository.Get();
            var partner = catPartnerRepo.Get();
            var clearance = customsDeclarationRepo.Get();
            var docType = sysattachRepository.Get();
            var user = sysUserRepo.Get();
            var currUser = currentUser.UserName;
            var cdJob = from cs in jobCs
                        join cd in jobDTCs on cs.Id equals cd.JobId into gr2
                        from g2 in gr2.DefaultIfEmpty()
                        join ed in edoc on g2.JobId equals ed.JobId
                        join pa in partner on g2.CustomerId equals pa.Id into gr3
                        from g3 in gr3.DefaultIfEmpty()
                        join cl in clearance on g2.Hwbno equals cl.Hblid into gr4
                        from g4 in gr4.DefaultIfEmpty()
                        join doc in docType on ed.DocumentTypeId equals doc.Id into gr5
                        from g5 in gr5.DefaultIfEmpty()
                        join us in user on cs.UserCreated equals us.Id into gr6
                        from g6 in gr6.DefaultIfEmpty()
                        select new EDocReportResult()
                        {
                            creator = g6.Username,
                            createDate = cs.DatetimeCreated,
                            attachPerson = ed.UserCreated,
                            aliasName = ed.SystemFileName,
                            attachTime = ed.DatetimeCreated,
                            codeCus = g3.AccountNo,
                            customer = g3.PartnerNameEn,
                            customNo = g4.ClearanceNo,
                            documentType = g5.NameEn,
                            HBL = g2.Hwbno,
                            MBL = g2.Mawb,
                            jobNo = cs.JobNo,
                            realFileName = ed.UserFileName,
                            require = g5.Required,
                            taxCode = g3.TaxCode,
                            userExport = currUser,
                        };
            var opsJob = from ops in jobOps
                         join ed in edoc on ops.Id equals ed.JobId 
                         join pa in partner on ops.CustomerId equals pa.Id into gr2
                         from g2 in gr2.DefaultIfEmpty()
                         join cl in clearance on ops.JobNo equals cl.JobNo into gr3
                         from g3 in gr3.DefaultIfEmpty()
                         join doc in docType on ed.DocumentTypeId equals doc.Id into gr4
                         from g4 in gr4.DefaultIfEmpty()
                         join us in user on ops.UserCreated equals us.Id into gr5
                         from g5 in gr5.DefaultIfEmpty()
                         select new EDocReportResult()
                         {
                             creator = g5.Username,
                             createDate = ops.DatetimeCreated,
                             attachPerson = ed.UserCreated,
                             aliasName = ed.SystemFileName,
                             attachTime = ed.DatetimeCreated,
                             codeCus = g2.AccountNo,
                             customer = g2.PartnerNameEn,
                             customNo = g3.ClearanceNo,
                             documentType = g4.NameEn,
                             HBL = ops.Hwbno,
                             MBL = ops.Mblno,
                             jobNo = ops.JobNo,
                             realFileName = ed.UserFileName,
                             require = g4.Required,
                             taxCode = g2.TaxCode,
                             userExport = currUser,
                         };
            var dataList = cdJob.Concat(opsJob);
            return dataList;
        }
        private List<sp_GetDataGeneralReport> GetDataGeneralReport(GeneralReportCriteria criteria)
        {
            var parameters = new[]{
                new SqlParameter(){ ParameterName = "@serviceDateFrom", Value = criteria.ServiceDateFrom },
                new SqlParameter(){ ParameterName = "@serviceDateTo", Value = criteria.ServiceDateTo },
                new SqlParameter(){ ParameterName = "@createdDateFrom", Value = criteria.CreatedDateFrom },
                new SqlParameter(){ ParameterName = "@createdDateTo", Value = criteria.CreatedDateTo },
                new SqlParameter(){ ParameterName = "@customerId", Value = criteria.CustomerId },
                new SqlParameter(){ ParameterName = "@service", Value = criteria.Service },
                new SqlParameter(){ ParameterName = "@currency", Value = criteria.Currency },
                new SqlParameter(){ ParameterName = "@jobId", Value = criteria.JobId },
                new SqlParameter(){ ParameterName = "@mawb", Value = criteria.Mawb },
                new SqlParameter(){ ParameterName = "@hawb", Value = criteria.Hawb },
                new SqlParameter(){ ParameterName = "@officeId", Value = criteria.OfficeId },
                new SqlParameter(){ ParameterName = "@departmentId", Value = criteria.DepartmentId },
                new SqlParameter(){ ParameterName = "@groupId", Value = criteria.GroupId },
                new SqlParameter(){ ParameterName = "@personalInCharge", Value = criteria.PersonInCharge },
                new SqlParameter(){ ParameterName = "@salesMan", Value = criteria.SalesMan },
                new SqlParameter(){ ParameterName = "@creator", Value = criteria.Creator },
                new SqlParameter(){ ParameterName = "@carrierId", Value = criteria.CarrierId },
                new SqlParameter(){ ParameterName = "@agentId", Value = criteria.AgentId },
                new SqlParameter(){ ParameterName = "@pol", Value = criteria.Pol },
                new SqlParameter(){ ParameterName = "@pod", Value = criteria.Pod }
            };
            //var list = ((eFMSDataContext)DataContext.DC).ExecuteProcedure<sp_GetDataGeneralReport>(parameters);
            var list = DC.ExecuteProcedure<sp_GetDataGeneralReport>(parameters);
            return list;
        }
    }
}
