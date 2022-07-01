using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eFMS.API.ReportData.Models.Common.Enums
{
    public enum ARTypeEnum
    {
        TrialOrOffical = 1,
        Guarantee = 2,
        Other = 3,
        NoAgreement = 4
    }
    public enum OverDueDayEnum
    {
        All,
        Over1_15,
        Over16_30,
        Over30,
    }
}
