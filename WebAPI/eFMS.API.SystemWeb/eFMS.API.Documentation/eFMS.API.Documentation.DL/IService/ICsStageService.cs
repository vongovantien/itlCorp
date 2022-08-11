using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Documentation.DL.IService
{
    public class ICsStageService
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly IContextBase<CatDepartment> departmentRepository;
        private readonly ICurrentUser currentUser;
        public CatStageService(IContextBase<CatStage> repository,
            ICacheServiceBase<CatStage> cacheService,
            IMapper mapper,
            IStringLocalizer<CatalogueLanguageSub> localizer,
            IContextBase<CatDepartment> departmentRepo,
            ICurrentUser currUser) : base(repository, cacheService, mapper)
        {
            currentUser = currUser;
            stringLocalizer = localizer;
            departmentRepository = departmentRepo;
            SetChildren<OpsStageAssigned>("Id", "StageId");
        }
    }
}
