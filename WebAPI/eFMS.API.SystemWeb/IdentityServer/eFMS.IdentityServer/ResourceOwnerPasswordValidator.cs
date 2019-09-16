using eFMS.API.System.DL.Models;
using eFMS.IdentityServer.DL.IService;
using eFMS.IdentityServer.DL.Services;
using eFMS.IdentityServer.Service.Models;
using IdentityServer4.Models;
using IdentityServer4.Validation;
using ITL.NetCore.Connection.EF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace eFMS.IdentityServer
{
    public class ResourceOwnerPasswordValidator : IResourceOwnerPasswordValidator
    {
        private readonly IAuthenUserService authenUser;
        public ResourceOwnerPasswordValidator(IAuthenUserService service)
        {
            authenUser = service;
        }

        public Task ValidateAsync(ResourceOwnerPasswordValidationContext context)
        {
            var result = authenUser.Login(context.UserName, context.Password,out LoginReturnModel modelReturn);
            var messageError = String.Empty;
            if(result == -2)
            {
                messageError = "Username or password incorrect !";
            }
            if(messageError != String.Empty)
            {
                context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant, messageError);
            }
            else
            {
                List<Claim> list_Claim = new List<Claim>();
                list_Claim.Add(new Claim("userId", modelReturn.idUser));
                list_Claim.Add(new Claim("workplaceId", modelReturn.workplaceId));
                list_Claim.Add(new Claim("email", modelReturn.email));
                context.Result = new GrantValidationResult(
                    subject: modelReturn.idUser,
                      authenticationMethod: "custom",
                      claims: list_Claim
                    );
            }

     

            return Task.FromResult(context.Result);
        }

      
    }
}
