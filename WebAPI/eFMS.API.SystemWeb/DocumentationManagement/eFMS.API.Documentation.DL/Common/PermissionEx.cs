﻿using eFMS.API.Common.Globals;
using eFMS.API.Documentation.DL.Models;
using eFMS.API.Infrastructure.Extensions;
using eFMS.API.Infrastructure.Models;
using eFMS.IdentityServer.DL.Models;
using eFMS.IdentityServer.DL.UserManager;
using System.Collections.Generic;
using System.Linq;

namespace eFMS.API.Documentation.DL.Common
{
    public static class PermissionEx
    {
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
                case PermissionRange.None:
                    code = 403;
                    break;
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
                case PermissionRange.None:
                    code = 403;
                    break;
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
                    if (model.SaleManId != currentUser.UserID && !authorizeUserIds.Contains(model.SaleManId))
                    {
                        code = 403;
                    }
                    break;
                case PermissionRange.Group:
                    if ((model.GroupId != currentUser.GroupId && model.DepartmentId == currentUser.DepartmentId)
                        && !authorizeUserIds.Contains(model.SaleManId))
                    {
                        code = 403;
                    }
                    break;
                case PermissionRange.Department:
                    if (model.DepartmentId != currentUser.DepartmentId && !authorizeUserIds.Contains(model.SaleManId))
                    {
                        code = 403;
                    }
                    break;
                case PermissionRange.Office:
                    if (model.OfficeId != currentUser.OfficeID && !authorizeUserIds.Contains(model.SaleManId))
                    {
                        code = 403;
                    }
                    break;
                case PermissionRange.Company:
                    if (model.CompanyId != currentUser.CompanyID && !authorizeUserIds.Contains(model.SaleManId))
                    {
                        code = 403;
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
                if (specialActions.Any(x => x.Action.Contains("Lock")))
                {
                    detailPermission.AllowLock = true;
                }
                if (specialActions.Any(x => x.Action.Contains("Add Charge")))
                {
                    detailPermission.AllowAddCharge = true;
                }
                if (specialActions.Any(x => x.Action.Contains("Update Charge")))
                {
                    detailPermission.AllowUpdateCharge = true;
                }
            }
            return detailPermission;
        }
    }
}
