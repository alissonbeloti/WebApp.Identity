﻿using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.AspNetCore.Identity;

namespace WebAPI.Dominio
{
    public class Role: IdentityRole<int>
    {
        public List<UserRole> UserRoles { get; set; }
    }
}
