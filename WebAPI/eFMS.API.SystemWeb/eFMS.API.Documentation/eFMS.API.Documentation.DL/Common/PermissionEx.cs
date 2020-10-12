using eFMS.API.Common.Globals;
using eFMS.API.Common.Models;
using eFMS.API.Documentation.DL.Models;
using eFMS.API.Infrastructure.Extensions;
using eFMS.IdentityServer.DL.UserManager;
using System;
using System.Collections.Generic;
using System.Linq;

namespace eFMS.API.Documentation.DL.Common
{
    public static class PermissionEx
    {
        public static int GetPermissionItemOpsToUpdate(ModelUpdate model, PermissionRange permissionRange, ICurrentUser currentUser, List<string> authorizeUserIds)
        {
            int code = 403;
            switch (permissionRange)
            {
                case PermissionRange.All:
                    code = 200;
                    break;
                case PermissionRange.Owner:
                    if (model.BillingOpsId == currentUser.UserID 
                        || model.SaleManId == currentUser.UserID
                        || authorizeUserIds.Contains(model.BillingOpsId))
                    {
                        code = 200;
                    }
                    break;
                case PermissionRange.Group:
                    if (model.BillingOpsId == currentUser.UserID
                        || model.SaleManId == currentUser.UserID
                        || (model.GroupId == currentUser.GroupId
                            && model.DepartmentId == currentUser.DepartmentId
                            && model.OfficeId == currentUser.OfficeID
                            && model.CompanyId == currentUser.CompanyID)
                        || authorizeUserIds.Contains(model.BillingOpsId))
                    {
                        code = 200;
                    }
                    break;
                case PermissionRange.Department:
                    if (model.BillingOpsId == currentUser.UserID
                        || model.SaleManId == currentUser.UserID
                        || (model.DepartmentId == currentUser.DepartmentId 
                            && model.OfficeId == currentUser.OfficeID 
                            && model.CompanyId == currentUser.CompanyID)
                        || authorizeUserIds.Contains(model.BillingOpsId))
                    {
                        code = 200;
                    }
                    break;
                case PermissionRange.Office:
                    if (model.BillingOpsId == currentUser.UserID
                        || model.SaleManId == currentUser.UserID
                        || (model.OfficeId == currentUser.OfficeID 
                            && model.CompanyId == currentUser.CompanyID)
                        || authorizeUserIds.Contains(model.BillingOpsId))
                    {
                        code = 200;
                    }
                    break;
                case PermissionRange.Company:
                    if (model.BillingOpsId == currentUser.UserID
                        || model.SaleManId == currentUser.UserID
                        || model.CompanyId == currentUser.CompanyID
                        || authorizeUserIds.Contains(model.BillingOpsId))
                    {
                        code = 200;
                    }
                    break;
            }
            return code;
        }

        public static int GetPermissionToDelete(ModelUpdate model, PermissionRange permissionRange, ICurrentUser currentUser)
        {
            int code = 403;
            switch (permissionRange)
            {
                case PermissionRange.All:
                    code = 200;
                    break;
                case PermissionRange.Owner:
                    if (model.BillingOpsId == currentUser.UserID || model.SaleManId == currentUser.UserID)
                    {
                        code = 200;
                    }
                    break;
                case PermissionRange.Group:
                    if (model.GroupId == currentUser.GroupId
                        && model.DepartmentId == currentUser.DepartmentId
                        && model.OfficeId == currentUser.OfficeID
                        && model.CompanyId == currentUser.CompanyID)
                    {
                        code = 200;
                    }
                    break;
                case PermissionRange.Department:
                    if (model.DepartmentId == currentUser.DepartmentId
                        && model.OfficeId == currentUser.OfficeID
                        && model.CompanyId == currentUser.CompanyID)
                    {
                        code = 200;
                    }
                    break;
                case PermissionRange.Office:
                    if (model.OfficeId == currentUser.OfficeID
                        && model.CompanyId == currentUser.CompanyID)
                    {
                        code = 200;
                    }
                    break;
                case PermissionRange.Company:
                    if (model.CompanyId == currentUser.CompanyID)
                    {
                        code = 200;
                    }
                    break;
            }
            return code;
        }

        public static int GetPermissionToUpdateShipmentDocumentation(ModelUpdate model, PermissionRange permissionRange, ICurrentUser currentUser, List<string> authorizeUserIds)
        {
            int code = 403;
            switch (permissionRange)
            {
                case PermissionRange.All:
                    code = 200;
                    break;
                case PermissionRange.Owner:
                    if (model.PersonInCharge == currentUser.UserID
                        || model.UserCreated == currentUser.UserID
                        || authorizeUserIds.Contains(model.PersonInCharge)
                        || model.SalemanIds.Contains(currentUser.UserID)
                        )
                    {
                        code = 200;
                    }
                    break;
                case PermissionRange.Group:
                    if ((model.GroupId == currentUser.GroupId
                        && model.DepartmentId == currentUser.DepartmentId
                        && model.OfficeId == currentUser.OfficeID
                        && model.CompanyId == currentUser.CompanyID)
                        || authorizeUserIds.Contains(model.PersonInCharge)
                        || model.SalemanIds.Contains(currentUser.UserID)
                        )
                    {
                        code = 200;
                    }
                    break;
                case PermissionRange.Department:
                    if ((model.DepartmentId == currentUser.DepartmentId
                        && model.OfficeId == currentUser.OfficeID
                        && model.CompanyId == currentUser.CompanyID)
                        || authorizeUserIds.Contains(model.PersonInCharge)
                        || model.SalemanIds.Contains(currentUser.UserID)
                        )
                    {
                        code = 200;
                    }
                    break;
                case PermissionRange.Office:
                    if ((model.OfficeId == currentUser.OfficeID
                        && model.CompanyId == currentUser.CompanyID)
                        || authorizeUserIds.Contains(model.PersonInCharge)
                        || model.SalemanIds.Contains(currentUser.UserID)
                        )
                    {
                        code = 200;
                    }
                    break;
                case PermissionRange.Company:
                    if (model.CompanyId == currentUser.CompanyID
                        || authorizeUserIds.Contains(model.PersonInCharge)
                        || model.SalemanIds.Contains(currentUser.UserID)
                        )
                    {
                        code = 200;
                    }
                    break;
            }
            return code;
        }

        public static int GetPermissionToDeleteShipmentDocumentation(ModelUpdate model, PermissionRange permissionRange, ICurrentUser currentUser)
        {
            int code = 403;
            switch (permissionRange)
            {
                case PermissionRange.All:
                    code = 200;
                    break;
                case PermissionRange.Owner:
                    if (model.PersonInCharge == currentUser.UserID 
                        || model.UserCreated == currentUser.UserID)
                    {
                        code = 200;
                    }
                    break;
                case PermissionRange.Group:
                    if (model.GroupId == currentUser.GroupId
                        && model.DepartmentId == currentUser.DepartmentId
                        && model.OfficeId == currentUser.OfficeID
                        && model.CompanyId == currentUser.CompanyID)
                    {
                        code = 200;
                    }
                    break;
                case PermissionRange.Department:
                    if (model.DepartmentId == currentUser.DepartmentId
                        && model.OfficeId == currentUser.OfficeID
                        && model.CompanyId == currentUser.CompanyID)
                    {
                        code = 200;
                    }
                    break;
                case PermissionRange.Office:
                    if (model.OfficeId == currentUser.OfficeID
                        && model.CompanyId == currentUser.CompanyID)
                    {
                        code = 200;
                    }
                    break;
                case PermissionRange.Company:
                    if (model.CompanyId == currentUser.CompanyID)
                    {
                        code = 200;
                    }
                    break;
            }
            return code;
        }

        public static int GetPermissionToDeleteHbl(ModelUpdate model, PermissionRange permissionRange, ICurrentUser currentUser)
        {
            int code = 403;
            switch (permissionRange)
            {
                case PermissionRange.All:
                    code = 200;
                    break;
                case PermissionRange.Owner:
                    if (model.SaleManId == currentUser.UserID || model.UserCreated == currentUser.UserID) 
                    {
                        code = 200;
                    }
                    break;
                case PermissionRange.Group:
                    if (model.GroupId == currentUser.GroupId
                        && model.DepartmentId == currentUser.DepartmentId
                        && model.OfficeId == currentUser.OfficeID
                        && model.CompanyId == currentUser.CompanyID)
                    {
                        code = 200;
                    }
                    break;
                case PermissionRange.Department:
                    if (model.DepartmentId == currentUser.DepartmentId
                        && model.OfficeId == currentUser.OfficeID
                        && model.CompanyId == currentUser.CompanyID)
                    {
                        code = 200;
                    }
                    break;
                case PermissionRange.Office:
                    if (model.OfficeId == currentUser.OfficeID
                        && model.CompanyId == currentUser.CompanyID)
                    {
                        code = 200;
                    }
                    break;
                case PermissionRange.Company:
                    if (model.CompanyId == currentUser.CompanyID)
                    {
                        code = 200;
                    }
                    break;
            }
            return code;
        }

        public static int GetPermissionToUpdateHbl(ModelUpdate model, PermissionRange permissionRange, ICurrentUser currentUser, List<string> authorizeUserIds)
        {
            int code = 403;
            switch (permissionRange)
            {
                case PermissionRange.All:
                    code = 200;
                    break;
                case PermissionRange.Owner:
                    if (model.SaleManId == currentUser.UserID
                        || authorizeUserIds.Contains(model.SaleManId)
                        || model.UserCreated == currentUser.UserID
                        || model.SaleManId == currentUser.UserID)
                    {
                        code = 200;
                    }
                    break;
                case PermissionRange.Group:
                    if ((model.GroupId == currentUser.GroupId
                        && model.DepartmentId == currentUser.DepartmentId
                        && model.OfficeId == currentUser.OfficeID
                        && model.CompanyId == currentUser.CompanyID)
                        || authorizeUserIds.Contains(model.SaleManId)
                        || model.UserCreated == currentUser.UserID
                        || model.SaleManId == currentUser.UserID)
                    {
                        code = 200;
                    }
                    break;
                case PermissionRange.Department:
                    if ((model.DepartmentId == currentUser.DepartmentId
                        && model.OfficeId == currentUser.OfficeID
                        && model.CompanyId == currentUser.CompanyID)
                        || authorizeUserIds.Contains(model.SaleManId)
                        || model.UserCreated == currentUser.UserID
                        || model.SaleManId == currentUser.UserID)
                    {
                        code = 200;
                    }
                    break;
                case PermissionRange.Office:
                    if ((model.OfficeId == currentUser.OfficeID
                        && model.CompanyId == currentUser.CompanyID)
                        || authorizeUserIds.Contains(model.SaleManId)
                        || model.UserCreated == currentUser.UserID
                        || model.SaleManId == currentUser.UserID)
                    {
                        code = 200;
                    }
                    break;
                case PermissionRange.Company:
                    if (model.CompanyId == currentUser.CompanyID
                        || authorizeUserIds.Contains(model.SaleManId)
                        || model.UserCreated == currentUser.UserID
                        || model.SaleManId == currentUser.UserID)
                    {
                        code = 200;
                    }
                    break;
            }
            return code;
        }

        public static ICurrentUser GetUserMenuPermissionTransaction(string transactionType, ICurrentUser currentUser)
        {
            ICurrentUser _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.docSeaFCLImport);//Set default

            if (transactionType == TermData.InlandTrucking)
            {
                _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.docInlandTrucking);
            }
            else if (transactionType == TermData.AirExport)
            {
                _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.docAirExport);
            }
            else if (transactionType == TermData.AirImport)
            {
                _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.docAirImport);
            }
            else if (transactionType == TermData.SeaConsolExport)
            {
                _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.docSeaConsolExport);
            }
            else if (transactionType == TermData.SeaConsolImport)
            {
                _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.docSeaConsolImport);
            }
            else if (transactionType == TermData.SeaFCLExport)
            {
                _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.docSeaFCLExport);
            }
            else if (transactionType == TermData.SeaFCLImport)
            {
                _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.docSeaFCLImport);
            }
            else if (transactionType == TermData.SeaLCLExport)
            {
                _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.docSeaLCLExport);
            }
            else if (transactionType == TermData.SeaLCLImport)
            {
                _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.docSeaLCLImport);
            }

            return _user;
        }

        public static PermissionAllowBase GetSpecialActions(PermissionAllowBase detailPermission, List<SpecialAction> specialActions)
        {
            if (specialActions.Count > 0)
            {
                detailPermission.AllowLock = (bool)specialActions.FirstOrDefault(x => x.Action.Contains("LockShipment"))?.IsAllow;
                detailPermission.AllowUpdateCharge = (bool)specialActions.FirstOrDefault(x => x.Action.Contains("UpdateCharge"))?.IsAllow;
                detailPermission.AllowAssignStage = (bool)specialActions.FirstOrDefault(x => x.Action.Contains("AssignStage"))?.IsAllow;
            }
            return detailPermission;
        }
    }
}
