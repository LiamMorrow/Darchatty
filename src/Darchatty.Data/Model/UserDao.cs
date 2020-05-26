using System;
using Microsoft.AspNetCore.Identity;

namespace Darchatty.Data.Model
{
    // Note: we use strings for the ID, but the invariant is the ID will always be a GUID.  We could use GUID fully, but then UserManager and friends wouldn't work
    public class UserDao : IdentityUser
    {
        public string DisplayName { get; set; } = null!;
    }
}
