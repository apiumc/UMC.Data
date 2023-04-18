using System;
using System.Collections.Generic;
using System.Text;

namespace UMC.Security
{
    public class AuthorizeType
    {
        public const byte UserAllow = 1;
        public const byte UserDeny = 2;
        public const byte RoleAllow = 3;
        public const byte RoleDeny = 4;
        public const byte OrganizeAllow = 5;
        public const byte OrganizeDeny = 6;

    }
}
