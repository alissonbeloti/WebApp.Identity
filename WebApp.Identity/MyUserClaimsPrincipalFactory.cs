using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace WebApp.Identity
{
    public class MyUserClaimsPrincipalFactory: UserClaimsPrincipalFactory<MyUser>
    {
        public MyUserClaimsPrincipalFactory(UserManager<MyUser> userManager, IOptions<IdentityOptions> options)
            : base(userManager, options)
        {

        }

        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(MyUser user)
        {
            var identity = await base.GenerateClaimsAsync(user);
            identity.AddClaim(new Claim("Member", user.Member));
            return identity;
        }
    }
}
