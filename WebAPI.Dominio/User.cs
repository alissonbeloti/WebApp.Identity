using System.Collections.Generic;

using Microsoft.AspNetCore.Identity;

namespace WebAPI.Dominio
{
    public class User: IdentityUser<int>
    {
        public string NomeCompleto { get; set; }
        public string Member { get; set; } = "Member";
        public string OrgId { get; set; }

        public List<UserRole> UserRoles { get; set; }
    }
}
