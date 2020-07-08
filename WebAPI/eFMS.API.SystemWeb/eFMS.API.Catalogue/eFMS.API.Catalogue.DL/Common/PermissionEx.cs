using eFMS.API.Catalogue.DL.Models;
using eFMS.API.Common.Globals;
using eFMS.IdentityServer.DL.UserManager;

namespace eFMS.API.Catalogue.DL.Common
{
    public static class PermissionEx
    {
        public static int GetPermissionToUpdate(ModelUpdate model, PermissionRange permissionRange, ICurrentUser currentUser,int? flagDetail)
        {
            int code = 0;
            switch (permissionRange)
            {
                case PermissionRange.None:
                    code = 403;
                    break;
                case PermissionRange.Owner:
                    if (model.PartnerGroup.Contains("CUSTOMER"))
                    {
                        if (model.Salemans.FindAll(x => x.SaleManId == currentUser.UserID).Count == 0 && model.UserCreator != currentUser.UserID)
                        {
                            code = 403;
                        }
                    }
                    else if(flagDetail != null)
                    {
                        if (model.UserCreator != currentUser.UserID) code = 403;
                    }
            
                    break;
                case PermissionRange.Group:
                    if ((model.GroupId != currentUser.GroupId
                        && model.DepartmentId != currentUser.DepartmentId
                        && model.OfficeId != currentUser.OfficeID
                        && model.CompanyId != currentUser.CompanyID)
                        && model.Salemans.FindAll(x => x.SaleManId == currentUser.UserID).Count == 0 && model.UserCreator != currentUser.UserID)
                    {
                        code = 403;
                    }
                    break;
                case PermissionRange.Department:
                    if (model.DepartmentId != currentUser.DepartmentId
                        && model.OfficeId != currentUser.OfficeID
                        && model.CompanyId != currentUser.CompanyID
                        && model.Salemans.FindAll(x => x.SaleManId == currentUser.UserID).Count == 0 && model.UserCreator != currentUser.UserID)
                    {
                        code = 403;
                    }
                    break;
                case PermissionRange.Office:
                    if (model.OfficeId != currentUser.OfficeID
                       && model.Salemans.FindAll(x => x.SaleManId == currentUser.UserID).Count == 0 && model.UserCreator != currentUser.UserID)
                    {
                        code = 403;
                    }
                    break;
                case PermissionRange.Company:
                    if (model.CompanyId != currentUser.CompanyID
                        )
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
                    if (model.PartnerGroup.Contains("CUSTOMER"))
                    {
                        if (model.Salemans.FindAll(x => x.SaleManId == currentUser.UserID).Count == 0 && model.UserCreator != currentUser.UserID )
                        {
                            code = 403;
                        }
                    }
                    else 
                    {
                        if (model.UserCreator != currentUser.UserID) code = 403;
                    }
                    break;
                case PermissionRange.Group:
                    if (model.GroupId != currentUser.GroupId && model.DepartmentId != currentUser.DepartmentId)
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
    }
}
