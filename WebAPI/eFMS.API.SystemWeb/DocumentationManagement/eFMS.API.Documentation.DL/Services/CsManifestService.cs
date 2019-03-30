using AutoMapper;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.Service.Models;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Documentation.DL.Services
{
    public class CsManifestService : RepositoryBase<CsManifest, CsManifestModel>, ICsManifestService
    {
        public CsManifestService(IContextBase<CsManifest> repository, IMapper mapper) : base(repository, mapper)
        {
        }

        public HandleState AddNewManifest(CsManifestEditModel model)
        {
            try
            {
                var manifest = mapper.Map<CsManifest>(model);
                manifest.CreatedDate = DateTime.Now;
                var hs = DataContext.Add(manifest);
                if (hs.Success)
                {
                    foreach(var item in model.CsTransactionDetails)
                    {
                        item.ManifestRefNo = manifest.RefNo;
                        item.DatetimeModified = DateTime.Now;
                        item.UserModified = manifest.UserCreated;
                        ((eFMSDataContext)DataContext.DC).CsTransactionDetail.Update(item);
                    }
                    ((eFMSDataContext)DataContext.DC).SaveChanges();
                }

                return new HandleState();
            }
            catch (Exception ex)
            {
                var hs = new HandleState(ex.Message);
                return hs;
            }
        }
    }
}
