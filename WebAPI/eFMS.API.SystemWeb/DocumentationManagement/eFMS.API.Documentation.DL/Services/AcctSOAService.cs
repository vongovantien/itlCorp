using AutoMapper;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.Service.Contexts;
using eFMS.API.Documentation.Service.Models;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using System;
using System.Linq;

namespace eFMS.API.Documentation.DL.Services
{
    public class AcctSOAService : RepositoryBase<AcctSoa, AcctSoaModel>, IAcctSOAService
    {
        private readonly ICurrentUser currentUser;
        public AcctSOAService(IContextBase<AcctSoa> repository, IMapper mapper, ICurrentUser user) : base(repository, mapper)
        {
            currentUser = user;
        }

        public HandleState AddSOA(AcctSoaModel model)
        {
            try
            {
                eFMSDataContext dc = (eFMSDataContext)DataContext.DC;
                var soa = mapper.Map<AcctSoa>(model);
                soa.Soano = CreateSoaNo(dc);
                var hs = dc.AcctSoa.Add(soa);

                var surcharge = dc.CsShipmentSurcharge.Where(x => model.SurchargeIds != null 
                                                               && model.SurchargeIds.Contains(x.Id) 
                                                               && (x.Soano == null || x.Soano == "")).ToList();

                if (surcharge.Count() > 0)
                {
                    //Update SOANo to CsShipmentSurcharge
                    surcharge.ForEach(a =>
                        {
                            a.Soano = soa.Soano;
                            a.UserModified = currentUser.UserID;
                        a.DatetimeModified = DateTime.Now;
                        }
                    );
                }

                dc.SaveChanges();
                return new HandleState();
            }
            catch (Exception ex)
            {
                var hs = new HandleState(ex.Message);
                return hs;
            }
        }

        private string CreateSoaNo(eFMSDataContext dc)
        {
            var prefix = (DateTime.Now.Year.ToString()).Substring(2, 2);
            string stt;
            //Lấy ra dòng cuối cùng của table acctSOA
            var rowLast = dc.AcctSoa.LastOrDefault();
            if (rowLast == null)
            {
                stt = "00001";
            }
            else
            {
                var soaCurrent = rowLast.Soano;
                var prefixCurrent = soaCurrent.Substring(0, 2);
                //Reset về 1 khi qua năm mới
                if (prefixCurrent != prefix)
                {
                    stt = "00001";
                }
                else
                {
                    stt = (Convert.ToInt32(soaCurrent.Substring(2, 5)) + 1).ToString();
                    stt = stt.PadLeft(5, '0');
                }
            }
            return prefix + stt;
        }

    }
}
