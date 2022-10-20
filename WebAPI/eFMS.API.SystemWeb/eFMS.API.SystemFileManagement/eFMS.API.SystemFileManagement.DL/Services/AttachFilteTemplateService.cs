﻿using AutoMapper;
using eFMS.API.SystemFileManagement.DL.IService;
using eFMS.API.SystemFileManagement.DL.Models;
using eFMS.API.SystemFileManagement.Service.Models;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace eFMS.API.SystemFileManagement.DL.Services
{
    public class AttachFilteTemplateService : RepositoryBase<SysAttachFileTemplate, SysAttachFileTemplateModel>, IAttachFileTemplateService
    {
        public AttachFilteTemplateService(IContextBase<SysAttachFileTemplate> repository, IMapper mapper) : base(repository, mapper)
        {
        }

        public async Task<HandleState> Import(List<SysAttachFileTemplate> list)
        {
            if (list.Count == 0)
            {
                return new HandleState();
            }

            var hs = await DataContext.AddAsync(list);

            return hs;
        }

        public async Task<List<SysAttachFileTemplate>> GetDocumentType(string transactionType)
        {
            var lst = await DataContext.GetAsync(x => x.TransactionType == transactionType);
            return lst;
        }
    }
}
