using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.AspNetCore.Identity;

namespace WebAPI.Dominio
{
    public class UserRole: IdentityUserRole
    {
        public User User { get; set; }
        public Role Role { get; set; }
    }
}
