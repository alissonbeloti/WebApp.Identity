using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Identity;

namespace WebApp.Identity
{
    public class DoesNotContainsPasswordValidator<TUser> : IPasswordValidator<TUser>
        where TUser : class
    {
        public async Task<IdentityResult> ValidateAsync(UserManager<TUser> manager, TUser user, string password)
        {
            var userName = await manager.GetUserNameAsync(user);

            if (userName == password)
            {
                return IdentityResult.Failed(new IdentityError
                {
                    Description = "Password deve ser diferente de UserName."
                });
            }
            if (password.Contains("password") || password.Contains("senha"))
            {
                return IdentityResult.Failed(new IdentityError
                {
                    Description = "Senha não pode ser password"
                });
            }
            return IdentityResult.Success;
        }
    }
}
