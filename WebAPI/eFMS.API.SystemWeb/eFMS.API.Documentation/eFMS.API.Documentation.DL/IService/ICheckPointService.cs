
using ITL.NetCore.Common;
using System;

namespace eFMS.API.Documentation.DL.IService
{
    public interface ICheckPointService
    {
        HandleState ValidateCheckPointPartner(string partnerId, Guid HblId, string transactionType);
        bool ValidateCheckPointCashContractPartner(string partnerId, Guid HblId, string transactionType);
        bool ValidateCheckPointOfficialTrialContractPartner(string partnerId, Guid HblId, string transactionType);
    }
}
