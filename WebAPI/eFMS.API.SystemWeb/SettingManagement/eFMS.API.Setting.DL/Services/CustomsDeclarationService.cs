using AutoMapper;
using eFMS.API.Setting.DL.IService;
using eFMS.API.Setting.DL.Models;
using eFMS.API.Setting.Service.Contexts;
using eFMS.API.Setting.Service.Models;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Setting.DL.Services
{
    public class CustomsDeclarationService : RepositoryBase<CustomsDeclaration, CustomsDeclarationModel>, ICustomsDeclarationService
    {
        private readonly IEcusConnectionService ecusCconnectionService;
        public CustomsDeclarationService(IContextBase<CustomsDeclaration> repository, IMapper mapper, IEcusConnectionService ecusCconnection) : base(repository, mapper)
        {
            ecusCconnectionService = ecusCconnection;
        }

        public HandleState ImportClearancesFromEcus()
        {
            string userId = "admin";
            eFMSDataContext dc = (eFMSDataContext)DataContext.DC;
            var connections = dc.SetEcusconnection.Where(x => x.UserId == userId);
            foreach (var item in connections)
            {
                var clearanceEcus = ecusCconnectionService.GetDataEcusByUser(item.UserId, item.ServerName, item.Dbusername, item.Dbpassword, item.Dbname);
                foreach(var clearance in clearanceEcus)
                {
                    var type = "Export";
                    var typeCode = string.Empty;
                    if (clearance.XorN.Contains("N"))
                    {
                        type = "Import";
                    }
                    //if(clearance.MA_HIEU_PTVC == )
                    switch (clearance.MA_HIEU_PTVC)
                    {
                        case "1":
                            break;
                    }
                    var newItem = new CustomsDeclaration {
                        IdfromEcus = clearance.DToKhaiMDID,
                        ClearanceNo = clearance.SOTK.ToString(),
                        FirstClearanceNo = clearance.SOTK_DAU_TIEN,
                        ClearanceDate = clearance.NGAY_DK,
                        ServiceType = clearance.MA_DV,
                        PortCodeCk = clearance.MA_CK,
                        PortCodeNn = clearance.MA_CANGNN,
                        ExportCountryId = clearance.NUOC_XK,
                        ImportcountryId = clearance.NUOC_NK,
                        Pcs = (int)clearance.SO_KIEN,
                        UnitId = clearance.DVT_KIEN,
                        ContQuantity = (int)clearance.SO_CONTAINER,
                        GrossWeight = clearance.TR_LUONG,
                        Route = clearance.PLUONG,
                        Type = type
                    };
                    dc.CustomsDeclaration.Add(newItem);
                }
            }
            dc.SaveChanges();
            throw new NotImplementedException();
        }
    }
}
