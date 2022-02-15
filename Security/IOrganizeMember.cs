using System;
namespace UMC.Security
{
    public interface IOrganizeMember
    {
        bool IsOrganizeMember(string organizeName);
    }
}
