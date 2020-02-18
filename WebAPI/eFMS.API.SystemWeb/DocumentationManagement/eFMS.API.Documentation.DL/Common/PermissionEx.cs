using eFMS.API.Common.Globals;
using eFMS.API.Documentation.DL.Models;
using eFMS.IdentityServer.DL.UserManager;
using System.Collections.Generic;
using System.Linq;

namespace eFMS.API.Documentation.DL.Common
{
    public static class PermissionEx
    {
        public static ICurrentUser GetUserMenuPermission(ICurrentUser curUser, Menu menu = Menu.acctAP)
        {

            if (curUser.UserPermissions.Count > 0)
            {
                curUser.UserMenuPermission = curUser.UserPermissions.Where(x => x.MenuId == menu.ToString()).FirstOrDefault();
            }
            return curUser;
        }

        public static PermissionRange GetPermissionRange(string permissionRange)
        {
            PermissionRange result = PermissionRange.All;
            switch (permissionRange)
            {
                case Constants.PERMISSION_RANGE_ALL:
                    result = PermissionRange.All;
                    break;
                case Constants.PERMISSION_RANGE_OWNER:
                    result = PermissionRange.Owner;
                    break;
                case Constants.PERMISSION_RANGE_GROUP:
                    result = PermissionRange.Group;
                    break;
                case Constants.PERMISSION_RANGE_DEPARTMENT:
                    result = PermissionRange.Department;
                    break;
                case Constants.PERMISSION_RANGE_OFFICE:
                    result = PermissionRange.Office;
                    break;
                case Constants.PERMISSION_RANGE_COMPANY:
                    result = PermissionRange.Company;
                    break;
            }
            return result;
        }
        public static int GetPermissionItemOpsToUpdate(ModelUpdate model, PermissionRange permissionRange, ICurrentUser currentUser, List<string> authorizeUserIds)
        {
            int code = 200;
            switch (permissionRange)
            {
                case PermissionRange.Owner:
                    if (model.BillingOpsId != currentUser.UserID && !authorizeUserIds.Contains(model.BillingOpsId))
                    {
                        code = 403;
                    }
                    break;
                case PermissionRange.Group:
                    if ((model.GroupId != currentUser.GroupId && model.DepartmentId != currentUser.DepartmentId && model.OfficeId != currentUser.OfficeID)
                        && !authorizeUserIds.Contains(model.BillingOpsId))
                    {
                        code = 403;
                    }
                    break;
                case PermissionRange.Department:
                    if (model.DepartmentId != currentUser.DepartmentId && model.OfficeId != currentUser.OfficeID && !authorizeUserIds.Contains(model.BillingOpsId))
                    {
                        code = 403;
                    }
                    break;
                case PermissionRange.Office:
                    if (model.OfficeId != currentUser.OfficeID && !authorizeUserIds.Contains(model.BillingOpsId))
                    {
                        code = 403;
                    }
                    break;
                case PermissionRange.Company:
                    if (model.CompanyId != currentUser.CompanyID && !authorizeUserIds.Contains(model.BillingOpsId))
                    {
                        code = 403;
                    }
                    break;
            }
            return code;
        }

        public static int GetPermissionToDelete(ModelUpdate model, PermissionRange permissionRange, ICurrentUser currentUser)
        {
            int code = 0;
            switch (permissionRange)
            {
                case PermissionRange.Owner:
                    if (model.BillingOpsId != currentUser.UserID)
                    {
                        code = 403;
                    }
                    break;
                case PermissionRange.Group:
                    if (model.GroupId != currentUser.GroupId && model.DepartmentId == currentUser.DepartmentId)
                    {
                        code = 403;
                    }
                    break;
                case PermissionRange.Department:
                    if (model.DepartmentId != currentUser.DepartmentId)
                    {
                        code = 403;
                    }
                    break;
                case PermissionRange.Office:
                    if (model.OfficeId != currentUser.OfficeID)
                    {
                        code = 403;
                    }
                    break;
                case PermissionRange.Company:
                    if (model.CompanyId != currentUser.CompanyID)
                    {
                        code = 403;
                    }
                    break;
            }
            return code;
        }

        public static int GetPermissionToUpdateShipmentDocumentation(ModelUpdate model, PermissionRange permissionRange, ICurrentUser currentUser, List<string> authorizeUserIds)
        {
            int code = 0;
            switch (permissionRange)
            {
                case PermissionRange.Owner:
                    if (model.PersonInCharge != currentUser.UserID && !authorizeUserIds.Contains(model.PersonInCharge))
                    {
                        code = 403;
                    }
                    break;
                case PermissionRange.Group:
                    if ((model.GroupId != currentUser.GroupId && model.DepartmentId == currentUser.DepartmentId)
                        && !authorizeUserIds.Contains(model.PersonInCharge))
                    {
                        code = 403;
                    }
                    break;
                case PermissionRange.Department:
                    if (model.DepartmentId != currentUser.DepartmentId && !authorizeUserIds.Contains(model.PersonInCharge))
                    {
                        code = 403;
                    }
                    break;
                case PermissionRange.Office:
                    if (model.OfficeId != currentUser.OfficeID && !authorizeUserIds.Contains(model.PersonInCharge))
                    {
                        code = 403;
                    }
                    break;
                case PermissionRange.Company:
                    if (model.CompanyId != currentUser.CompanyID && !authorizeUserIds.Contains(model.PersonInCharge))
                    {
                        code = 403;
                    }
                    break;
            }
            return code;
        }

        public static int GetPermissionToDeleteShipmentDocumentation(ModelUpdate model, PermissionRange permissionRange, ICurrentUser currentUser)
        {
            int code = 0;
            switch (permissionRange)
            {
                case PermissionRange.Owner:
                    if (model.PersonInCharge != currentUser.UserID)
                    {
                        code = 403;
                    }
                    break;
                case PermissionRange.Group:
                    if (model.GroupId != currentUser.GroupId && model.DepartmentId == currentUser.DepartmentId)
                    {
                        code = 403;
                    }
                    break;
                case PermissionRange.Department:
                    if (model.DepartmentId != currentUser.DepartmentId)
                    {
                        code = 403;
                    }
                    break;
                case PermissionRange.Office:
                    if (model.OfficeId != currentUser.OfficeID)
                    {
                        code = 403;
                    }
                    break;
                case PermissionRange.Company:
                    if (model.CompanyId != currentUser.CompanyID)
                    {
                        code = 403;
                    }
                    break;
            }
            return code;
        }

        public static int GetPermissionToUpdateHblDocument(ModelUpdate model, PermissionRange permissionRange, ICurrentUser currentUser, List<string> authorizeUserIds)
        {
            int code = 0;
            switch (permissionRange)
            {
                case PermissionRange.Owner:
                    if (model.SaleManId != currentUser.UserID && !authorizeUserIds.Contains(model.SaleManId) && model.UserCreated != currentUser.UserID)
                    {
                        code = 403;
                    }
                    break;
                case PermissionRange.Group:
                    if ((model.GroupId != currentUser.GroupId && model.DepartmentId == currentUser.DepartmentId)
                        && !authorizeUserIds.Contains(model.SaleManId) && model.UserCreated != currentUser.UserID)
                    {
                        code = 403;
                    }
                    break;
                case PermissionRange.Department:
                    if (model.DepartmentId != currentUser.DepartmentId && !authorizeUserIds.Contains(model.SaleManId) && model.UserCreated != currentUser.UserID)
                    {
                        code = 403;
                    }
                    break;
                case PermissionRange.Office:
                    if (model.OfficeId != currentUser.OfficeID && !authorizeUserIds.Contains(model.SaleManId) && model.UserCreated != currentUser.UserID)
                    {
                        code = 403;
                    }
                    break;
                case PermissionRange.Company:
                    if (model.CompanyId != currentUser.CompanyID && !authorizeUserIds.Contains(model.SaleManId) && model.UserCreated != currentUser.UserID)
                    {
                        code = 403;
                    }
                    break;
            }
            return code;
        }



        public static ICurrentUser GetUserMenuPermissionTransaction(string transactionType, ICurrentUser currentUser)
        {
            ICurrentUser _user = GetUserMenuPermission(currentUser, Menu.docSeaFCLImport);//Set default

            if (transactionType == TermData.InlandTrucking)
            {
                _user = PermissionEx.GetUserMenuPermission(currentUser, Menu.docInlandTrucking);
            }
            else if (transactionType == TermData.AirExport)
            {
                _user = PermissionEx.GetUserMenuPermission(currentUser, Menu.docAirExport);
            }
            else if (transactionType == TermData.AirImport)
            {
                _user = PermissionEx.GetUserMenuPermission(currentUser, Menu.docAirImport);
            }
            else if (transactionType == TermData.SeaConsolExport)
            {
                _user = PermissionEx.GetUserMenuPermission(currentUser, Menu.docSeaConsolExport);
            }
            else if (transactionType == TermData.SeaConsolImport)
            {
                _user = PermissionEx.GetUserMenuPermission(currentUser, Menu.docSeaConsolImport);
            }
            else if (transactionType == TermData.SeaFCLExport)
            {
                _user = PermissionEx.GetUserMenuPermission(currentUser, Menu.docSeaFCLExport);
            }
            else if (transactionType == TermData.SeaFCLImport)
            {
                _user = PermissionEx.GetUserMenuPermission(currentUser, Menu.docSeaFCLImport);
            }
            else if (transactionType == TermData.SeaLCLExport)
            {
                _user = PermissionEx.GetUserMenuPermission(currentUser, Menu.docSeaLCLExport);
            }
            else if (transactionType == TermData.SeaLCLImport)
            {
                _user = PermissionEx.GetUserMenuPermission(currentUser, Menu.docSeaLCLImport);
            }

            return _user;
        }
    }
}
