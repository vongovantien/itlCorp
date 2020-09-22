using eFMS.API.System.DL.Models;
using eFMS.IdentityServer.DL.IService;
using eFMS.IdentityServer.DL.Models;
using eFMS.IdentityServer.Helpers;
using IdentityServer4.Models;
using IdentityServer4.Validation;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace eFMS.IdentityServer
{
    public class ResourceOwnerPasswordValidator : IResourceOwnerPasswordValidator
    {
        private readonly IAuthenUserService authenUser;
        private readonly IHttpContextAccessor _contextAccessor;

        public ResourceOwnerPasswordValidator(IAuthenUserService service,
            IHttpContextAccessor contextAccessor)
        {
            authenUser = service;
            _contextAccessor = contextAccessor;
        }

        public Task ValidateAsync(ResourceOwnerPasswordValidationContext context)
        {

            if (context.UserName == "partner.bravo")
            {
                context.Result = ValidatePartnerUser(context);
            }
            else
            {
                var messageError = string.Empty;

                RSAHelper cryption = new RSAHelper();
                PermissionInfo userPermissionInfo;
                string password = cryption.Decrypt(context.Password);

                string officeId = _contextAccessor.HttpContext.Request.Headers["officeId"];
                string deptId = _contextAccessor.HttpContext.Request.Headers["departmentId"];

                if (!string.IsNullOrEmpty(officeId))
                {
                    if (string.IsNullOrEmpty(deptId))
                    {
                        userPermissionInfo = new PermissionInfo
                        {
                            OfficeID = new Guid(officeId),
                            DepartmentID = null,
                            GroupID = null
                        };
                    }
                    else
                    {
                        short? departmentId = null;
                        if (deptId == "null")
                        {
                            departmentId = null;
                        }
                        else
                        {
                            departmentId = Int16.Parse(deptId);

                        }
                        int groupId = Int16.Parse(_contextAccessor.HttpContext.Request.Headers["groupId"]);
                        userPermissionInfo = new PermissionInfo
                        {
                            OfficeID = new Guid(officeId),
                            DepartmentID = departmentId,
                            GroupID = groupId
                        };
                    }
                }
                else
                {
                    userPermissionInfo = null;
                }

                Guid companyId = new Guid(_contextAccessor.HttpContext.Request.Headers["companyId"]);
                if (companyId == Guid.Empty)
                {
                    messageError = "Company invalid";
                    return Task.FromResult(new GrantValidationResult(TokenRequestErrors.InvalidGrant, messageError));
                }

                var result = authenUser.Login(context.UserName, password, companyId, out LoginReturnModel modelReturn, userPermissionInfo);

                switch (result)
                {
                    case -2:
                        messageError = "Username or password incorrect !";
                        break;
                    case -3:
                        messageError = "Not found company of this user name !";
                        break;
                    case -4:
                        messageError = "Not found office of this user name !";
                        break;
                    default:
                        break;
                }

                if (messageError != String.Empty)
                {
                    context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant, messageError);
                }

                else
                {
                    List<Claim> list_Claim = new List<Claim>();
                    list_Claim.Add(new Claim("userId", modelReturn.idUser));
                    //list_Claim.Add(new Claim("workplaceId", modelReturn.workplaceId));
                    list_Claim.Add(new Claim("email", modelReturn.email));
                    list_Claim.Add(new Claim("companyId", modelReturn.companyId.ToString()));
                    list_Claim.Add(new Claim("officeId", modelReturn.officeId.ToString()));
                    list_Claim.Add(new Claim("departmentId", (modelReturn.departmentId ?? null).ToString()));
                    list_Claim.Add(new Claim("groupId", modelReturn.groupId.ToString()));
                    list_Claim.Add(new Claim("nameEn", modelReturn.NameEn.ToString()));
                    list_Claim.Add(new Claim("nameVn", modelReturn.NameVn.ToString()));


                    context.Result = new GrantValidationResult(
                        subject: modelReturn.idUser,
                          authenticationMethod: "custom",
                          claims: list_Claim
                        );
                }
            }
            
            return Task.FromResult(context.Result);
        }

        public GrantValidationResult ValidatePartnerUser(ResourceOwnerPasswordValidationContext context)
        {
            int resultAuthenPartner = authenUser.ValidateAuthPartner(context.UserName, context.Password, out LoginReturnModel modelReturn);

            if (resultAuthenPartner == 1)
            {
                List<Claim> list_Claim = new List<Claim>();
                list_Claim.Add(new Claim("userId", modelReturn.idUser));
                list_Claim.Add(new Claim("email", ""));
                list_Claim.Add(new Claim("companyId", Guid.Empty.ToString()));
                list_Claim.Add(new Claim("officeId", Guid.Empty.ToString()));
                list_Claim.Add(new Claim("departmentId", ""));
                list_Claim.Add(new Claim("groupId", ""));
                list_Claim.Add(new Claim("nameEn", ""));
                list_Claim.Add(new Claim("nameVn",""));
                context.Result = new GrantValidationResult(subject: modelReturn.idUser,authenticationMethod: "custom", claims: list_Claim);
            }
            else
            {
                context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant, "Username or password incorrect !");
            }

            return context.Result;
        }
    }
}
